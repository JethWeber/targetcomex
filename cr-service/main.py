from fastapi import FastAPI, HTTPException, UploadFile, File
from fastapi.responses import JSONResponse
import pyodbc
import os
import numpy as np
from sklearn.neighbors import NearestNeighbors
from sklearn.preprocessing import StandardScaler
from typing import List, Dict
from transformers import AutoTokenizer, AutoModel
import torch
from torchvision import models, transforms
from PIL import Image
import io

app = FastAPI(
    title="Canal de Recomendação - Target Comex",
    description="API Multimodal para Recomendações da Concessionária TARGET",
    version="0.3.0"
)

# Configuração do banco (pega das env vars do compose)
DB_SERVER = os.getenv("DB_SERVER", "target_comex_db")
DB_PORT = os.getenv("DB_PORT", "1433")
DB_DATABASE = os.getenv("DB_DATABASE", "TargetComex")
DB_USERNAME = os.getenv("DB_USERNAME", "sa")
DB_PASSWORD = os.getenv("DB_PASSWORD", "TargetComex2025!")

connection_string = (
    f"DRIVER={{ODBC Driver 18 for SQL Server}};"
    f"SERVER={DB_SERVER},{DB_PORT};"
    f"DATABASE={DB_DATABASE};"
    f"UID={DB_USERNAME};"
    f"PWD={DB_PASSWORD};"
    "Encrypt=yes;"
    "TrustServerCertificate=yes;"
)

# Mapeamentos categóricos (só M/F para Genero, conforme sua DB)
GENERO_MAP = {'M': 0, 'F': 1}
ESTADO_CIVIL_MAP = {
    'Solteiro': 0, 'Casado': 1, 'União de Facto': 2, 'Divorciado': 3, 'Viúvo': 4
}

# Tags de interesses (para vetor com peso maior)
INTERESSES_TAGS = ['família', 'economia', 'conforto', 'luxo', 'design', 'tecnologia', 'espaço', 'robustez', 'off-road']

def encode_interesses(interesses_str: str) -> List[float]:
    """Converte string de interesses em vetor numérico com peso maior (2 para match exato)."""
    if not interesses_str:
        return [0.0] * len(INTERESSES_TAGS)
    tags = [t.strip().lower() for t in interesses_str.split(',')]
    return [2.0 if tag in tags else 0.0 for tag in INTERESSES_TAGS]

def get_style_counts(user_id: int, cursor) -> np.ndarray:
    """Contagem ponderada de estilos (compra = 2× navegação)."""
    styles = ['Pick-up', 'Hatchback', 'SUV', 'Sedan']  # Expanda conforme sua tabela
    counts = []
    for style in styles:
        cursor.execute("""
            SELECT COUNT(*) FROM HistoricoCompras hc
            JOIN Veiculos v ON hc.VeiculoId = v.Id
            WHERE hc.UsuarioId = ? AND v.Estilo = ?
        """, (user_id, style))
        buy_count = cursor.fetchone()[0]

        cursor.execute("""
            SELECT COUNT(*) FROM HistoricoNavegacao hn
            JOIN Veiculos v ON hn.VeiculoId = v.Id
            WHERE hn.UsuarioId = ? AND v.Estilo = ?
        """, (user_id, style))
        nav_count = cursor.fetchone()[0]

        total = buy_count * 2 + nav_count
        counts.append(total)
    return np.array(counts)

def vectorize_user(user_row, cursor):
    """Vetor melhorado: filhos, renda, idade, genero, estado_civil, interesses (peso 2), estilos (histórico)"""
    renda_map = {'Baixa': 1, 'Média': 2, 'Alta': 3, 'Média-Alta': 2.5}
    idade = (2026 - int(str(user_row[3])[:4])) if user_row[3] else 30
    genero_code = GENERO_MAP.get(user_row[4], 0)  # Só M/F
    estado_civil_code = ESTADO_CIVIL_MAP.get(user_row[5], 0)
    interesses_vec = encode_interesses(user_row[6])
    style_vec = get_style_counts(user_row[0], cursor)

    return np.concatenate([
        [user_row[2] or 0],                    # NumeroFilhos
        [renda_map.get(user_row[7], 2)],       # FaixaRendaMensal
        [idade],
        [genero_code],
        [estado_civil_code],
        interesses_vec,                        # Peso 2 para match
        style_vec                              # Histórico de estilos
    ])

@app.get("/health")
async def health_check():
    return {"status": "healthy", "message": "Canal de Recomendação rodando!"}

@app.get("/test-db")
async def test_db_connection():
    try:
        conn = pyodbc.connect(connection_string)
        cursor = conn.cursor()
        cursor.execute("SELECT COUNT(*) FROM Usuarios")
        count = cursor.fetchone()[0]
        cursor.close()
        conn.close()
        return {"status": "success", "message": f"Conexão OK! {count} usuários encontrados no banco."}
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Erro ao conectar ao DB: {str(e)}")

@app.get("/recommend/{user_id}")
async def recommend(user_id: int):
    try:
        conn = pyodbc.connect(connection_string)
        cursor = conn.cursor()

        # Perfil do usuário alvo
        cursor.execute("""
            SELECT Id, Nome, NumeroFilhos, DataNascimento, Genero, EstadoCivil, InteressesPrincipais, FaixaRendaMensal
            FROM Usuarios WHERE Id = ?
        """, (user_id,))
        user = cursor.fetchone()
        if not user:
            raise HTTPException(status_code=404, detail="Usuário não encontrado")

        user_vector = vectorize_user(user, cursor)

        # Todos os outros usuários
        cursor.execute("""
            SELECT Id, Nome, NumeroFilhos, DataNascimento, Genero, EstadoCivil, InteressesPrincipais, FaixaRendaMensal
            FROM Usuarios WHERE Id != ?
        """, (user_id,))
        others = cursor.fetchall()

        if len(others) < 2:
            return {"message": "Não há usuários suficientes para recomendação colaborativa"}

        # Vetoriza todos
        X = np.array([vectorize_user(row, cursor) for row in others])

        # Normaliza (média 0, desvio padrão 1)
        scaler = StandardScaler()
        X_scaled = scaler.fit_transform(X)
        user_vector_scaled = scaler.transform(user_vector.reshape(1, -1))

        # KNN: até 3 vizinhos mais próximos
        n_neighbors = min(3, len(others))
        knn = NearestNeighbors(n_neighbors=n_neighbors, metric='euclidean')
        knn.fit(X_scaled)
        distances, indices = knn.kneighbors(user_vector_scaled)

        similar_user_ids = [others[i][0] for i in indices[0]]

        # Recomenda veículos que vizinhos compraram ou avaliaram bem
        placeholders = ','.join('?' * len(similar_user_ids))
        cursor.execute(f"""
            SELECT DISTINCT v.Marca, v.Modelo, v.Preco, v.Estilo
            FROM HistoricoCompras hc
            JOIN Veiculos v ON hc.VeiculoId = v.Id
            WHERE hc.UsuarioId IN ({placeholders})
            UNION
            SELECT DISTINCT v.Marca, v.Modelo, v.Preco, v.Estilo
            FROM Avaliacoes a
            JOIN Veiculos v ON a.VeiculoId = v.Id
            WHERE a.UsuarioId IN ({placeholders}) AND a.Nota >= 4
        """, similar_user_ids + similar_user_ids)

        recommendations = cursor.fetchall()
        cursor.close()
        conn.close()

        rec_list = [
            {"marca": r[0], "modelo": r[1], "preco": float(r[2]), "estilo": r[3]}
            for r in recommendations
        ]

        return {
            "user_id": user_id,
            "similar_users": similar_user_ids,
            "distances": distances[0].tolist(),
            "recommendations": rec_list or "Nenhuma recomendação colaborativa encontrada ainda"
        }

    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.get("/recommend-hybrid/{user_id}")
async def recommend_hybrid(user_id: int):
    conn = pyodbc.connect(connection_string)
    cursor = conn.cursor()

    # 1. Busca os interesses do usuário
    cursor.execute("SELECT InteressesPrincipais FROM Usuarios WHERE Id = ?", (user_id,))
    user = cursor.fetchone()
    if not user or not user.InteressesPrincipais:
        return {"error": "Usuário sem interesses cadastrados."}
    
    interesses = user.InteressesPrincipais # Ex: "família, economia"

    # 2. Transforma interesses em um "Vetor de Desejo" (Embedding)
    inputs = tokenizer(interesses, return_tensors="pt", truncation=True, padding=True)
    with torch.no_grad():
        user_vector = bert_model(**inputs).last_hidden_state.mean(dim=1).squeeze().numpy()

    # 3. Busca todos os veículos e seus embeddings
    cursor.execute("""
        SELECT v.Id, v.Marca, v.Modelo, v.Preco, f.EmbeddingTextual 
        FROM FeaturesMultimodais f
        JOIN Veiculos v ON f.VeiculoId = v.Id
    """)
    veiculos = cursor.fetchall()
    
    recommendations = []
    for v_id, marca, modelo, preco, emb_bin in veiculos:
        emb_veiculo = np.frombuffer(emb_bin, dtype=np.float32)
        
        # Similaridade de Cosseno entre Interesse do Usuário e Descrição do Carro
        score = np.dot(user_vector, emb_veiculo) / (np.linalg.norm(user_vector) * np.linalg.norm(emb_veiculo))
        
        recommendations.append({
            "veiculo": f"{marca} {modelo}",
            "preco": float(preco),
            "match_score": round(float(score) * 100, 2) # Porcentagem de match
        })

    # 4. Ordena pelos melhores matches
    recommendations = sorted(recommendations, key=lambda x: x['match_score'], reverse=True)

    return {
        "usuario_id": user_id,
        "interesses_analisados": interesses,
        "sugestoes_personalizadas": recommendations[:60]
    }

# Carrega modelos uma vez (global)
tokenizer = AutoTokenizer.from_pretrained('distilbert-base-uncased')
bert_model = AutoModel.from_pretrained('distilbert-base-uncased')
resnet = models.resnet18(pretrained=True)
resnet.eval()

# Transformação para imagens
image_transform = transforms.Compose([
    transforms.Resize(256),
    transforms.CenterCrop(224),
    transforms.ToTensor(),
    transforms.Normalize(mean=[0.485, 0.456, 0.406], std=[0.229, 0.224, 0.225])
])

@app.post("/embed-text")
async def embed_text(text: str):
    """Extrai embedding de texto com DistilBERT."""
    inputs = tokenizer(text, return_tensors="pt", truncation=True, padding=True, max_length=128)
    with torch.no_grad():
        outputs = bert_model(**inputs)
    embedding = outputs.last_hidden_state.mean(dim=1).squeeze().numpy().tolist()
    return {"embedding": embedding, "dimension": len(embedding)}

@app.post("/embed-image")
async def embed_image(file: UploadFile = File(...)):
    """Extrai embedding de imagem com ResNet18. Envie arquivo de imagem (jpg, png, webp)."""
    if not file.content_type.startswith('image/'):
        raise HTTPException(status_code=400, detail="Arquivo deve ser uma imagem (jpg, png, webp, etc.)")

    try:
        contents = await file.read()
        image = Image.open(io.BytesIO(contents)).convert('RGB')
        image_tensor = image_transform(image).unsqueeze(0)
        with torch.no_grad():
            embedding = resnet(image_tensor).squeeze().numpy().tolist()
        return {"embedding": embedding, "dimension": len(embedding)}
    except Exception as e:
        raise HTTPException(status_code=400, detail=f"Erro ao processar imagem: {str(e)}")

@app.get("/search-multimodal")
async def search_multimodal(query: str):
    # 1. Gera o embedding do que o usuário digitou
    inputs = tokenizer(query, return_tensors="pt", truncation=True, padding=True, max_length=128)
    with torch.no_grad():
        query_embedding = bert_model(**inputs).last_hidden_state.mean(dim=1).squeeze().numpy()

    # 2. Busca os embeddings de todos os veículos no banco
    conn = pyodbc.connect(connection_string)
    cursor = conn.cursor()
    cursor.execute("""
        SELECT v.Marca, v.Modelo, f.EmbeddingTextual 
        FROM FeaturesMultimodais f
        JOIN Veiculos v ON f.VeiculoId = v.Id
    """)
    rows = cursor.fetchall()
    
    results = []
    for marca, modelo, emb_bin in rows:
        # Converte o binário do SQL de volta para array numpy
        emb_veiculo = np.frombuffer(emb_bin, dtype=np.float32)
        
        # Cálculo de Similaridade de Cosseno (quão próximos estão os vetores)
        similarity = np.dot(query_embedding, emb_veiculo) / (np.linalg.norm(query_embedding) * np.linalg.norm(emb_veiculo))
        results.append({"veiculo": f"{marca} {modelo}", "score": float(similarity)})

    # Ordena pelo mais parecido
    results = sorted(results, key=lambda x: x['score'], reverse=True)
    return {"query": query, "top_matches": results}


@app.get("/recommend-hybrid/{user_id}")
async def recommend_hybrid(user_id: int):
    try:
        conn = pyodbc.connect(connection_string)
        cursor = conn.cursor()

        # 1. Pegar dados do Usuário (Interesses e Perfil)
        cursor.execute("SELECT Nome, InteressesPrincipais FROM Usuarios WHERE Id = ?", (user_id,))
        user_data = cursor.fetchone()
        if not user_data:
            raise HTTPException(status_code=404, detail="Usuário não encontrado")
        
        nome_usuario = user_data[0]
        interesses = user_data[1] or ""

        # 2. Gerar Vetor de Intenção do Usuário (IA - BERT)
        inputs = tokenizer(interesses, return_tensors="pt", truncation=True, padding=True)
        with torch.no_grad():
            user_intent_vector = bert_model(**inputs).last_hidden_state.mean(dim=1).squeeze().numpy()

        # 3. Buscar Veículos e seus Embeddings Multimodais
        cursor.execute("""
            SELECT v.Id, v.Marca, v.Modelo, v.Preco, v.Estilo, f.EmbeddingTextual 
            FROM FeaturesMultimodais f
            JOIN Veiculos v ON f.VeiculoId = v.Id
        """)
        veiculos = cursor.fetchall()
        
        final_recommendations = []
        for v_id, marca, modelo, preco, estilo, emb_bin in veiculos:
            # Converter binário para vetor numpy
            emb_veiculo = np.frombuffer(emb_bin, dtype=np.float32)
            
            # Cálculo de Similaridade de Cosseno (IA pura)
            cosine_sim = np.dot(user_intent_vector, emb_veiculo) / (
                np.linalg.norm(user_intent_vector) * np.linalg.norm(emb_veiculo)
            )
            
            # Converter para porcentagem amigável
            match_percent = round(float(cosine_sim) * 100, 1)

            final_recommendations.append({
                "veiculo_id": v_id,
                "nome": f"{marca} {modelo}",
                "preco": float(preco),
                "estilo": estilo,
                "match_score": f"{match_percent}%",
                "justificativa": f"Este veículo combina com seu interesse em '{interesses}'"
            })

        # 4. Ordenar do melhor para o pior
        final_recommendations = sorted(final_recommendations, key=lambda x: float(x['match_score'].replace('%','')), reverse=True)

        cursor.close()
        conn.close()

        return {
            "cliente": nome_usuario,
            "foco_da_ia": interesses,
            "top_sugestoes": final_recommendations[:60] # Top 5 resultados
        }

    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000, reload=False, timeout_keep_alive=60)