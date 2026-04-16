"""
TARGET COMEX — Canal de Recomendação Multimodal
================================================
Carrega embeddings pré-treinados do disco (/app/models).
NÃO re-treina na inicialização — usa artefactos gerados pelo train.py.
"""

import os
import pickle
import logging
import threading
import pyodbc
import numpy as np
from fastapi import FastAPI, HTTPException
from fastapi.responses import JSONResponse
from sklearn.neighbors import NearestNeighbors
from sklearn.preprocessing import StandardScaler
from transformers import AutoTokenizer, AutoModel
from typing import List
import torch
import datetime

# ─── Logging ─────────────────────────────────────────────────────────────────
logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s [API] %(levelname)s — %(message)s",
    datefmt="%Y-%m-%d %H:%M:%S"
)
log = logging.getLogger(__name__)

# ─── Caminhos ─────────────────────────────────────────────────────────────────
MODELS_DIR      = os.getenv("MODELS_DIR", "/app/models")
EMBEDDINGS_PATH = os.path.join(MODELS_DIR, "vehicle_embeddings.pkl")

# ─── Conexão BD ───────────────────────────────────────────────────────────────
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

# ─── Mapeamentos ──────────────────────────────────────────────────────────────
GENERO_MAP = {"M": 0, "F": 1}
ESTADO_CIVIL_MAP = {
    "Solteiro": 0, "Solteira": 0,
    "Casado": 1,   "Casada": 1,
    "União de Facto": 2,
    "Divorciado": 3, "Divorciada": 3,
    "Viúvo": 4,    "Viúva": 4
}
RENDA_MAP    = {"Baixa": 1, "Média": 2, "Média-Alta": 2.5, "Alta": 3}
ESTILOS      = ["Pick-up", "Hatchback", "SUV", "Sedan"]
INTERESSES_TAGS = [
    "família", "economia", "conforto", "luxo",
    "design", "tecnologia", "espaço", "robustez", "off-road"
]


# ─── Estado global da aplicação ───────────────────────────────────────────────
class AppState:
    embeddings: dict = {}          # {veiculo_id: {...}}
    tokenizer   = None
    bert_model  = None
    lock        = threading.Lock()

state = AppState()


# ─── FastAPI ──────────────────────────────────────────────────────────────────
app = FastAPI(
    title="Canal de Recomendação — Target Comex",
    description="API Multimodal de Recomendações para a Concessionária TARGET",
    version="1.0.0"
)


# ─── Inicialização ─────────────────────────────────────────────────────────────
@app.on_event("startup")
async def startup():
    log.info("🚀 Iniciando Canal de Recomendação...")
    _load_bert()
    _load_embeddings()
    log.info("✅ API pronta!")


def _load_bert():
    """Carrega DistilBERT para buscas textuais em tempo real."""
    log.info("Carregando DistilBERT para buscas...")
    state.tokenizer  = AutoTokenizer.from_pretrained("distilbert-base-uncased")
    state.bert_model = AutoModel.from_pretrained("distilbert-base-uncased")
    state.bert_model.eval()
    log.info("DistilBERT carregado. ✅")


def _load_embeddings():
    """Carrega embeddings pré-treinados do disco."""
    if not os.path.exists(EMBEDDINGS_PATH):
        log.warning(
            "⚠️  Arquivo de embeddings não encontrado. "
            "Execute: python train.py  (ou aguarde o watcher)"
        )
        state.embeddings = {}
        return

    with open(EMBEDDINGS_PATH, "rb") as f:
        state.embeddings = pickle.load(f)

    log.info(f"✅ {len(state.embeddings)} embeddings de veículos carregados de {EMBEDDINGS_PATH}")


def reload_embeddings():
    """Recarrega embeddings sem reiniciar a API (thread-safe)."""
    with state.lock:
        _load_embeddings()
    log.info("🔄 Embeddings recarregados.")


# ─── Helpers de BD ────────────────────────────────────────────────────────────
def get_conn():
    return pyodbc.connect(CONNECTION_STRING)


# ─── Vectorização de usuário ──────────────────────────────────────────────────
def encode_interesses(interesses_str: str) -> List[float]:
    if not interesses_str:
        return [0.0] * len(INTERESSES_TAGS)
    tags = [t.strip().lower() for t in interesses_str.split(",")]
    return [2.0 if tag in tags else 0.0 for tag in INTERESSES_TAGS]


def get_style_counts(user_id: int, cursor) -> np.ndarray:
    counts = []
    for estilo in ESTILOS:
        cursor.execute(
            "SELECT COUNT(*) FROM HistoricoCompras hc "
            "JOIN Veiculos v ON hc.VeiculoId = v.Id "
            "WHERE hc.UsuarioId = ? AND v.Estilo = ?", (user_id, estilo)
        )
        buys = cursor.fetchone()[0] or 0

        cursor.execute(
            "SELECT COUNT(*) FROM HistoricoNavegacao hn "
            "JOIN Veiculos v ON hn.VeiculoId = v.Id "
            "WHERE hn.UsuarioId = ? AND v.Estilo = ?", (user_id, estilo)
        )
        views = cursor.fetchone()[0] or 0
        counts.append(buys * 2 + views)
    return np.array(counts)


def vectorize_user(user_row, cursor) -> np.ndarray:
    try:
        renda_val          = RENDA_MAP.get(user_row[7], 2)
        idade              = (2026 - int(str(user_row[3])[:4])) if user_row[3] else 30
        genero_code        = GENERO_MAP.get(user_row[4], 0)
        estado_civil_code  = ESTADO_CIVIL_MAP.get(user_row[5], 0)
        interesses_vec     = encode_interesses(user_row[6] or "")
        style_vec          = get_style_counts(user_row[0], cursor)

        return np.concatenate([
            [user_row[2] or 0],   # NumeroFilhos
            [renda_val],
            [idade],
            [genero_code],
            [estado_civil_code],
            interesses_vec,       # 9 dims
            style_vec             # 4 dims  → total: 18 dims
        ])
    except Exception as e:
        log.warning(f"Erro ao vectorizar usuário {user_row[0]}: {e}")
        return np.zeros(18, dtype=np.float32)

def calc_age(data_nasc) -> int:
    """Calcula idade a partir do campo DataNascimento (DATE/str)."""
    try:
        if not data_nasc:
            return 30
        year = data_nasc.year if hasattr(data_nasc, "year") else int(str(data_nasc)[:4])
        current_year = datetime.datetime.utcnow().year
        age = current_year - year
        return max(18, min(90, age))
    except Exception:
        return 30

def user_has_behavior(cursor, user_id: int) -> bool:
    """Verifica se o usuário tem alguma interação na base."""
    cursor.execute("SELECT COUNT(1) FROM HistoricoCompras WHERE UsuarioId = ?", (user_id,))
    compras = cursor.fetchone()[0] or 0
    cursor.execute("SELECT COUNT(1) FROM Avaliacoes WHERE UsuarioId = ?", (user_id,))
    avaliacoes = cursor.fetchone()[0] or 0
    cursor.execute("SELECT COUNT(1) FROM HistoricoNavegacao WHERE UsuarioId = ?", (user_id,))
    navegacoes = cursor.fetchone()[0] or 0
    return (compras + avaliacoes + navegacoes) > 0

def profile_text_from_row(user_row) -> str:
    """
    Monta um texto único com as variáveis exigidas para similaridade:
    Genero, Idade, EstadoCivil, Profissao, FaixaRendaMensal, InteressesPrincipais e TipoDeUsoPretendido.
    """
    # ORDER BY no SQL deve manter este alinhamento:
    # (nome, genero, data_nascimento, estado_civil, profissao, faixa_renda, interesses, tipo_de_uso)
    nome, genero, data_nasc, estado_civil, profissao, faixa_renda, interesses, tipo_de_uso = user_row

    idade = calc_age(data_nasc)
    return (
        f"Genero: {genero or ''}. "
        f"Idade: {idade}. "
        f"EstadoCivil: {estado_civil or ''}. "
        f"Profissao: {profissao or ''}. "
        f"FaixaRendaMensal: {faixa_renda or ''}. "
        f"InteressesPrincipais: {interesses or ''}. "
        f"TipoDeUsoPretendido: {tipo_de_uso or ''}."
    )

def embed_text(text: str) -> np.ndarray:
    """Embedding textual curto (DistilBERT) para similaridade entre perfis."""
    inputs = state.tokenizer(
        text or "",
        return_tensors="pt",
        truncation=True,
        padding=True,
        max_length=128,
    )
    with torch.no_grad():
        vec = state.bert_model(**inputs).last_hidden_state.mean(dim=1).squeeze().numpy()
    return vec

def recommend_by_similar_profiles(cursor, user_id: int, max_candidates: int = 30, top_similar: int = 3, top_results: int = 5):
    """
    Fallback para usuários novos sem histórico/avaliações:
    1) encontra usuários com perfis similares usando as variáveis exigidas
    2) recomenda veículos com base no histórico (compras/avaliações) dos similares
    """
    cursor.execute(
        """
        SELECT Nome, Genero, DataNascimento, EstadoCivil, Profissao,
               FaixaRendaMensal, InteressesPrincipais, TipoDeUsoPretendido
        FROM Usuarios
        WHERE Id = ?
        """,
        (user_id,),
    )
    user_row = cursor.fetchone()
    if not user_row:
        raise HTTPException(status_code=404, detail="Utilizador não encontrado")

    user_name = user_row[0]
    foco = (user_row[6] or "") if len(user_row) > 6 else ""
    user_vec = embed_text(profile_text_from_row(user_row))
    user_vec_norm = np.linalg.norm(user_vec)

    # Mesmo que o usuário seja novo, tentamos sempre comparar com perfis existentes.
    cursor.execute(
        f"""
        SELECT TOP {max_candidates} Id, Genero, DataNascimento, EstadoCivil, Profissao,
               FaixaRendaMensal, InteressesPrincipais, TipoDeUsoPretendido, Nome
        FROM Usuarios
        WHERE Id <> ?
        ORDER BY DataCadastro DESC
        """,
        (user_id,),
    )
    candidates = cursor.fetchall()
    if not candidates:
        return {"cliente": user_name, "foco_da_ia": foco, "top_sugestoes": []}

    scored = []
    for row in candidates:
        # row: (Id, Genero, DataNascimento, EstadoCivil, Profissao, FaixaRendaMensal, InteressesPrincipais, TipoDeUsoPretendido, Nome)
        cand_id = row[0]
        cand_text = (
            f"Genero: {row[1] or ''}. "
            f"Idade: {calc_age(row[2])}. "
            f"EstadoCivil: {row[3] or ''}. "
            f"Profissao: {row[4] or ''}. "
            f"FaixaRendaMensal: {row[5] or ''}. "
            f"InteressesPrincipais: {row[6] or ''}. "
            f"TipoDeUsoPretendido: {row[7] or ''}."
        )
        cand_vec = embed_text(cand_text)
        denom = user_vec_norm * np.linalg.norm(cand_vec)
        if denom == 0:
            continue
        score = float(np.dot(user_vec, cand_vec) / denom)  # cosseno
        scored.append((cand_id, score))

    scored.sort(key=lambda x: x[1], reverse=True)
    similar_ids = [cid for cid, _ in scored[:top_similar]]
    if not similar_ids:
        return {"cliente": user_name, "foco_da_ia": foco, "top_sugestoes": []}

    placeholders = ",".join("?" * len(similar_ids))

    # Score do veículo baseado no histórico dos perfis similares.
    vehicle_scores: dict[int, dict] = {}

    def upsert_vehicle(vid, marca, modelo, preco, estilo, delta):
        if vid not in vehicle_scores:
            vehicle_scores[vid] = {
                "veiculo_id": vid,
                "nome": f"{marca} {modelo}".strip(),
                "preco": float(preco) if preco is not None else 0.0,
                "estilo": estilo,
                "score": 0.0,
            }
        vehicle_scores[vid]["score"] += float(delta)

    # Compras
    cursor.execute(
        f"""
        SELECT v.Id, v.Marca, v.Modelo, v.Preco, v.Estilo, COUNT(*) as cnt
        FROM HistoricoCompras hc
        JOIN Veiculos v ON hc.VeiculoId = v.Id
        WHERE hc.UsuarioId IN ({placeholders})
        GROUP BY v.Id, v.Marca, v.Modelo, v.Preco, v.Estilo
        """,
        similar_ids,
    )
    for vid, marca, modelo, preco, estilo, cnt in cursor.fetchall():
        upsert_vehicle(vid, marca, modelo, preco, estilo, delta=cnt * 1.0)

    # Avaliações altas (>= 4)
    cursor.execute(
        f"""
        SELECT v.Id, v.Marca, v.Modelo, v.Preco, v.Estilo, COUNT(*) as cnt
        FROM Avaliacoes a
        JOIN Veiculos v ON a.VeiculoId = v.Id
        WHERE a.UsuarioId IN ({placeholders}) AND a.Nota >= 4
        GROUP BY v.Id, v.Marca, v.Modelo, v.Preco, v.Estilo
        """,
        similar_ids,
    )
    for vid, marca, modelo, preco, estilo, cnt in cursor.fetchall():
        upsert_vehicle(vid, marca, modelo, preco, estilo, delta=cnt * 2.0)

    # Navegação (suave, só para não zerar recomendações)
    cursor.execute(
        f"""
        SELECT v.Id, v.Marca, v.Modelo, v.Preco, v.Estilo, COUNT(*) as cnt
        FROM HistoricoNavegacao hn
        JOIN Veiculos v ON hn.VeiculoId = v.Id
        WHERE hn.UsuarioId IN ({placeholders})
        GROUP BY v.Id, v.Marca, v.Modelo, v.Preco, v.Estilo
        """,
        similar_ids,
    )
    for vid, marca, modelo, preco, estilo, cnt in cursor.fetchall():
        upsert_vehicle(vid, marca, modelo, preco, estilo, delta=cnt * 0.5)

    if not vehicle_scores:
        return {"cliente": user_name, "foco_da_ia": foco, "top_sugestoes": []}

    max_score = max(v["score"] for v in vehicle_scores.values()) or 0.0
    justificativa = (
        "Recomendado com base em usuários com perfis similares "
        "(Genero/Idade/EstadoCivil/Profissao/FaixaRendaMensal/InteressesPrincipais/TipoDeUsoPretendido)."
    )

    ranked = sorted(vehicle_scores.values(), key=lambda x: x["score"], reverse=True)
    top = ranked[:top_results]
    suggestions = []
    for v in top:
        pct = 0.0 if max_score == 0 else (v["score"] / max_score) * 100.0
        suggestions.append({
            "veiculo_id": v["veiculo_id"],
            "nome": v["nome"],
            "preco": v["preco"],
            "match_score": f"{round(pct, 1)}%",
            "justificativa": justificativa,
        })

    return {
        "cliente": user_name,
        "foco_da_ia": foco,
        "top_sugestoes": suggestions,
    }


# ─── Endpoints ────────────────────────────────────────────────────────────────

@app.get("/health")
async def health():
    return {
        "status":             "healthy",
        "embeddings_loaded":  len(state.embeddings),
        "bert_loaded":        state.bert_model is not None,
        "message":            "Canal de Recomendação rodando!"
    }


@app.post("/reload-embeddings")
async def reload():
    """Recarrega embeddings do disco sem reiniciar a API."""
    reload_embeddings()
    return {"message": f"✅ {len(state.embeddings)} embeddings recarregados."}


@app.get("/recommend/{user_id}")
async def recommend(user_id: int):
    """Recomendação colaborativa — KNN baseado no perfil do utilizador."""
    conn = None
    try:
        conn   = get_conn()
        cursor = conn.cursor()

        cursor.execute(
            "SELECT Id, Nome, NumeroFilhos, DataNascimento, Genero, EstadoCivil, "
            "InteressesPrincipais, FaixaRendaMensal FROM Usuarios WHERE Id = ?",
            (user_id,)
        )
        user = cursor.fetchone()
        if not user:
            raise HTTPException(status_code=404, detail="Utilizador não encontrado")

        user_vector = vectorize_user(user, cursor)

        cursor.execute(
            "SELECT Id, Nome, NumeroFilhos, DataNascimento, Genero, EstadoCivil, "
            "InteressesPrincipais, FaixaRendaMensal FROM Usuarios WHERE Id != ?",
            (user_id,)
        )
        others = cursor.fetchall()

        if len(others) < 1:
            return {"message": "Utilizadores insuficientes para comparação."}

        X = np.array([vectorize_user(row, cursor) for row in others])
        scaler   = StandardScaler()
        X_scaled = scaler.fit_transform(X)
        u_scaled = scaler.transform(user_vector.reshape(1, -1))

        knn = NearestNeighbors(n_neighbors=min(3, len(others)), metric="euclidean")
        knn.fit(X_scaled)
        _, indices      = knn.kneighbors(u_scaled)
        similar_ids     = [others[i][0] for i in indices[0]]

        placeholders = ",".join("?" * len(similar_ids))
        cursor.execute(f"""
            SELECT DISTINCT v.Id, v.Marca, v.Modelo, v.Preco, v.Estilo
            FROM HistoricoCompras hc
            JOIN Veiculos v ON hc.VeiculoId = v.Id
            WHERE hc.UsuarioId IN ({placeholders})
            UNION
            SELECT DISTINCT v.Id, v.Marca, v.Modelo, v.Preco, v.Estilo
            FROM Avaliacoes a
            JOIN Veiculos v ON a.VeiculoId = v.Id
            WHERE a.UsuarioId IN ({placeholders}) AND a.Nota >= 4
        """, similar_ids + similar_ids)

        recs = cursor.fetchall()
        return {
            "user_id":       user_id,
            "similar_users": similar_ids,
            "recommendations": [
                {"id": r[0], "marca": r[1], "modelo": r[2],
                 "preco": float(r[3]), "estilo": r[4]}
                for r in recs
            ]
        }
    except HTTPException:
        raise
    except Exception as e:
        log.error(f"Erro em /recommend/{user_id}: {e}")
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        if conn: conn.close()


@app.get("/recommend-hybrid/{user_id}")
async def recommend_hybrid(user_id: int):
    """Recomendação híbrida — combina perfil KNN com similaridade de embeddings."""
    conn = None
    try:
        conn   = get_conn()
        cursor = conn.cursor()

        # Se o usuário ainda não tiver comportamento (compras/avaliações/navegação),
        # recomendamos via perfis similares (fallback por similaridade de perfil).
        if not user_has_behavior(cursor, user_id):
            return recommend_by_similar_profiles(cursor, user_id)

        if not state.embeddings:
            # Sem embeddings, ainda assim conseguimos recomendar com base em perfis similares.
            return recommend_by_similar_profiles(cursor, user_id)

        cursor.execute(
            "SELECT Nome, InteressesPrincipais FROM Usuarios WHERE Id = ?",
            (user_id,)
        )
        user_data = cursor.fetchone()
        if not user_data:
            raise HTTPException(status_code=404, detail="Utilizador não encontrado")

        interesses = user_data[1] or ""

        # Embedding do perfil via BERT
        inputs = state.tokenizer(
            interesses, return_tensors="pt",
            truncation=True, padding=True, max_length=128
        )
        with torch.no_grad():
            user_vector = state.bert_model(**inputs).last_hidden_state.mean(dim=1).squeeze().numpy()

        # Busca preços dos veículos para retornar
        cursor.execute("SELECT Id, Preco FROM Veiculos")
        precos = {row[0]: float(row[1]) for row in cursor.fetchall()}

        # Calcula similaridade coseno com cada veículo
        results = []
        for v_id, vdata in state.embeddings.items():
            emb = vdata["emb_textual"]
            norm = np.linalg.norm(user_vector) * np.linalg.norm(emb)
            if norm == 0:
                continue
            score = float(np.dot(user_vector, emb) / norm)
            results.append({
                "veiculo_id":   v_id,
                "nome":         vdata["nome"],
                "preco":        precos.get(v_id, 0),
                "match_score":  f"{round(score * 100, 1)}%",
                "justificativa": f"Combina com o seu interesse em '{interesses}'"
            })

        results.sort(key=lambda x: float(x["match_score"].replace("%", "")), reverse=True)

        return {
            "cliente":      user_data[0],
            "foco_da_ia":   interesses,
            "top_sugestoes": results[:5]
        }
    except HTTPException:
        raise
    except Exception as e:
        log.error(f"Erro em /recommend-hybrid/{user_id}: {e}")
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        if conn: conn.close()


@app.get("/search")
async def search(query: str):
    """Busca semântica de veículos por texto livre."""
    if not state.embeddings:
        raise HTTPException(
            status_code=503,
            detail="Embeddings não carregados. Execute python train.py primeiro."
        )
    try:
        inputs = state.tokenizer(
            query, return_tensors="pt",
            truncation=True, padding=True, max_length=128
        )
        with torch.no_grad():
            query_vec = state.bert_model(**inputs).last_hidden_state.mean(dim=1).squeeze().numpy()

        results = []
        for v_id, vdata in state.embeddings.items():
            emb  = vdata["emb_textual"]
            norm = np.linalg.norm(query_vec) * np.linalg.norm(emb)
            if norm == 0:
                continue
            score = float(np.dot(query_vec, emb) / norm)
            results.append({"veiculo": vdata["nome"], "score": round(score, 4)})

        results.sort(key=lambda x: x["score"], reverse=True)
        return {"query": query, "top_matches": results[:10]}
    except Exception as e:
        log.error(f"Erro em /search: {e}")
        raise HTTPException(status_code=500, detail=str(e))


if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)