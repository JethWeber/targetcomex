"""
TARGET COMEX — Motor de Treino Multimodal
==========================================
- Gera embeddings textuais (DistilBERT) e visuais (ResNet18) para cada veículo
- Salva os modelos/artefactos em /app/models para serem carregados pelo main.py
- Roda automaticamente quando detecta novos veículos no BD
- Pode ser chamado manualmente: python train.py
"""

import os
import time
import pickle
import hashlib
import logging
import pyodbc
import numpy as np
import torch
from PIL import Image
from transformers import AutoTokenizer, AutoModel
from torchvision import models, transforms

# ─── Logging ────────────────────────────────────────────────────────────────
logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s [TRAIN] %(levelname)s — %(message)s",
    datefmt="%Y-%m-%d %H:%M:%S"
)
log = logging.getLogger(__name__)

# ─── Caminhos ────────────────────────────────────────────────────────────────
MODELS_DIR         = os.getenv("MODELS_DIR", "/app/models")
EMBEDDINGS_PATH    = os.path.join(MODELS_DIR, "vehicle_embeddings.pkl")
CATALOG_HASH_PATH  = os.path.join(MODELS_DIR, "catalog_hash.txt")

os.makedirs(MODELS_DIR, exist_ok=True)

# ─── Conexão BD ──────────────────────────────────────────────────────────────
DB_SERVER   = os.getenv("DB_SERVER",   "target_comex_db")
DB_PORT     = os.getenv("DB_PORT",     "1433")
DB_DATABASE = os.getenv("DB_DATABASE", "TargetComex")
DB_USERNAME = os.getenv("DB_USERNAME", "sa")
DB_PASSWORD = os.getenv("DB_PASSWORD", "TargetComex2025!")

CONNECTION_STRING = (
    f"DRIVER={{ODBC Driver 18 for SQL Server}};"
    f"SERVER={DB_SERVER},{DB_PORT};"
    f"DATABASE={DB_DATABASE};"
    f"UID={DB_USERNAME};"
    f"PWD={DB_PASSWORD};"
    "Encrypt=yes;TrustServerCertificate=yes;"
)

# ─── Transformação de imagem ──────────────────────────────────────────────────
IMAGE_TRANSFORM = transforms.Compose([
    transforms.Resize(256),
    transforms.CenterCrop(224),
    transforms.ToTensor(),
    transforms.Normalize(mean=[0.485, 0.456, 0.406],
                         std=[0.229, 0.224, 0.225])
])


# ─── Carregamento dos modelos de IA ──────────────────────────────────────────
def load_ai_models():
    log.info("Carregando DistilBERT...")
    tokenizer   = AutoTokenizer.from_pretrained("distilbert-base-uncased")
    bert_model  = AutoModel.from_pretrained("distilbert-base-uncased")
    bert_model.eval()

    log.info("Carregando ResNet18...")
    resnet = models.resnet18(weights=models.ResNet18_Weights.DEFAULT)
    # Remove a camada de classificação final → output: vetor de 512 dims
    resnet = torch.nn.Sequential(*list(resnet.children())[:-1])
    resnet.eval()

    return tokenizer, bert_model, resnet


# ─── Geração de embeddings ────────────────────────────────────────────────────
def embed_text(text: str, tokenizer, bert_model) -> np.ndarray:
    """Gera embedding textual via DistilBERT (768 dims)."""
    inputs = tokenizer(
        text or "",
        return_tensors="pt",
        truncation=True,
        padding=True,
        max_length=128
    )
    with torch.no_grad():
        output = bert_model(**inputs)
    return output.last_hidden_state.mean(dim=1).squeeze().numpy()


def embed_image(image_path: str, resnet) -> np.ndarray:
    """Gera embedding visual via ResNet18 (512 dims). Retorna zeros se falhar."""
    try:
        img = Image.open(image_path).convert("RGB")
        tensor = IMAGE_TRANSFORM(img).unsqueeze(0)
        with torch.no_grad():
            embedding = resnet(tensor).squeeze().numpy()
        return embedding
    except Exception as e:
        log.warning(f"Imagem não encontrada ou inválida ({image_path}): {e}")
        return np.zeros(512, dtype=np.float32)


# ─── Hash do catálogo ─────────────────────────────────────────────────────────
def get_catalog_hash(cursor) -> str:
    """Gera um hash dos IDs + descrições dos veículos para detectar mudanças."""
    cursor.execute("SELECT Id, Descricao, ImagemUrl FROM Veiculos ORDER BY Id")
    rows = cursor.fetchall()
    content = str(rows).encode("utf-8")
    return hashlib.md5(content).hexdigest()


def catalog_changed(current_hash: str) -> bool:
    """Retorna True se o catálogo mudou desde o último treino."""
    if not os.path.exists(CATALOG_HASH_PATH):
        return True
    with open(CATALOG_HASH_PATH, "r") as f:
        saved_hash = f.read().strip()
    return saved_hash != current_hash


def save_catalog_hash(current_hash: str):
    with open(CATALOG_HASH_PATH, "w") as f:
        f.write(current_hash)


# ─── Treino principal ─────────────────────────────────────────────────────────
def train():
    log.info("Conectando ao banco de dados...")
    try:
        conn   = pyodbc.connect(CONNECTION_STRING)
        cursor = conn.cursor()
    except Exception as e:
        log.error(f"Falha na conexão com o BD: {e}")
        return False

    try:
        current_hash = get_catalog_hash(cursor)

        if not catalog_changed(current_hash):
            log.info("Catálogo sem alterações. Treino ignorado. ✅")
            conn.close()
            return False

        log.info("Mudanças detectadas no catálogo. Iniciando treino...")

        tokenizer, bert_model, resnet = load_ai_models()

        cursor.execute("SELECT Id, Marca, Modelo, Descricao, ImagemUrl FROM Veiculos")
        veiculos = cursor.fetchall()
        log.info(f"{len(veiculos)} veículos encontrados para processar.")

        embeddings = {}

        for v_id, marca, modelo, descricao, imagem_url in veiculos:
            log.info(f"  Processando: {marca} {modelo} (ID {v_id})...")

            # Embedding textual — combina marca + modelo + descrição
            texto_completo = f"{marca} {modelo}. {descricao or ''}"
            emb_text = embed_text(texto_completo, tokenizer, bert_model)

            # Embedding visual
            image_path = f"/app/images/{imagem_url.lstrip('/')}" if imagem_url else ""
            emb_img = embed_image(image_path, resnet)

            # Embedding combinado (concatenação)
            emb_combined = np.concatenate([emb_text, emb_img])

            embeddings[v_id] = {
                "veiculo_id":    v_id,
                "nome":          f"{marca} {modelo}",
                "marca":         marca,
                "modelo":        modelo,
                "emb_textual":   emb_text,
                "emb_visual":    emb_img,
                "emb_combined":  emb_combined,
            }

            # Actualiza também a tabela FeaturesMultimodais no BD
            cursor.execute("""
                IF EXISTS (SELECT 1 FROM FeaturesMultimodais WHERE VeiculoId = ?)
                    UPDATE FeaturesMultimodais
                       SET EmbeddingVisual   = ?,
                           EmbeddingTextual  = ?,
                           DataProcessamento = GETDATE()
                     WHERE VeiculoId = ?
                ELSE
                    INSERT INTO FeaturesMultimodais (VeiculoId, EmbeddingVisual, EmbeddingTextual)
                    VALUES (?, ?, ?)
            """, (
                v_id,
                emb_img.astype(np.float32).tobytes(),
                emb_text.astype(np.float32).tobytes(),
                v_id,
                v_id,
                emb_img.astype(np.float32).tobytes(),
                emb_text.astype(np.float32).tobytes()
            ))

        conn.commit()

        # Salva embeddings em disco
        with open(EMBEDDINGS_PATH, "wb") as f:
            pickle.dump(embeddings, f)

        save_catalog_hash(current_hash)

        log.info(f"✅ Treino concluído! {len(embeddings)} veículos processados.")
        log.info(f"   Embeddings salvos em: {EMBEDDINGS_PATH}")
        conn.close()
        return True

    except Exception as e:
        log.error(f"Erro durante o treino: {e}")
        conn.close()
        return False


# ─── Watcher — detecta novos veículos automaticamente ────────────────────────
def watch(interval_seconds: int = 60):
    """
    Loop que verifica o catálogo periodicamente.
    Se detectar mudança, dispara o treino automaticamente.
    """
    log.info(f"🔍 Watcher iniciado. Verificando catálogo a cada {interval_seconds}s...")
    while True:
        try:
            train()
        except Exception as e:
            log.error(f"Erro no watcher: {e}")
        time.sleep(interval_seconds)


# ─── Entry point ─────────────────────────────────────────────────────────────
if __name__ == "__main__":
    import sys
    if "--watch" in sys.argv:
        watch(interval_seconds=60)
    else:
        train()