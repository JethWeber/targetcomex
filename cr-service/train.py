import pyodbc
import os
import torch
import numpy as np
from PIL import Image
import requests
from io import BytesIO
from transformers import AutoTokenizer, AutoModel
from torchvision import models, transforms

# Configurações de conexão (mesmas do main.py)
DB_CONFIG = f"DRIVER={{ODBC Driver 18 for SQL Server}};SERVER={os.getenv('DB_SERVER', 'target_comex_db')};DATABASE=TargetComex;UID=sa;PWD=TargetComex2025!;Encrypt=yes;TrustServerCertificate=yes;"

# Carregar Modelos
tokenizer = AutoTokenizer.from_pretrained('distilbert-base-uncased')
bert_model = AutoModel.from_pretrained('distilbert-base-uncased')
resnet = models.resnet18(pretrained=True)
resnet.eval()

transform = transforms.Compose([
    transforms.Resize(256),
    transforms.CenterCrop(224),
    transforms.ToTensor(),
    transforms.Normalize(mean=[0.485, 0.456, 0.406], std=[0.229, 0.224, 0.225])
])

def get_embedding_text(text):
    inputs = tokenizer(text, return_tensors="pt", truncation=True, padding=True, max_length=128)
    with torch.no_grad():
        outputs = bert_model(**inputs)
    return outputs.last_hidden_state.mean(dim=1).squeeze().numpy().tobytes()

def get_embedding_image(image_path):
    # Aqui você pode adaptar para ler localmente ou via URL
    # Para teste, vamos simular um vetor vazio se a imagem não existir
    try:
        # Se for um path local no container:
        img = Image.open(image_path).convert('RGB')
        img_t = transform(img).unsqueeze(0)
        with torch.no_grad():
            embedding = resnet(img_t).squeeze().numpy().tobytes()
        return embedding
    except:
        return np.zeros(1000, dtype=np.float32).tobytes()

def process_catalog():
    conn = pyodbc.connect(DB_CONFIG)
    cursor = conn.cursor()
    
    cursor.execute("SELECT Id, Descricao, ImagemUrl FROM Veiculos")
    veiculos = cursor.fetchall()
    
    for v_id, desc, img_url in veiculos:
        print(f"Processando Veículo ID: {v_id}...")
        
        emb_text = get_embedding_text(desc or "")
        # No seu caso, garanta que a imagem exista no path ou use um placeholder
        emb_img = get_embedding_image(f"/app/images/{img_url}") 

        cursor.execute("""
            IF EXISTS (SELECT 1 FROM FeaturesMultimodais WHERE VeiculoId = ?)
                UPDATE FeaturesMultimodais SET EmbeddingVisual = ?, EmbeddingTextual = ? WHERE VeiculoId = ?
            ELSE
                INSERT INTO FeaturesMultimodais (VeiculoId, EmbeddingVisual, EmbeddingTextual) VALUES (?, ?, ?)
        """, (v_id, emb_img, emb_text, v_id, v_id, emb_img, emb_text))
    
    conn.commit()
    print("✅ Catálogo Multimodal atualizado com sucesso!")

if __name__ == "__main__":
    process_catalog()