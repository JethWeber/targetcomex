# ============================================================
# TARGET COMEX — Serviço de Recomendação (cr-service)
# Base: Python 3.10 Slim
# ============================================================
FROM python:3.10-slim

WORKDIR /app

# ─── Dependências do sistema + Driver ODBC 18 (SQL Server) ───
# unixodbc é necessário para o pyodbc conectar ao SQL Server
RUN apt-get update && apt-get install -y \
    curl \
    gnupg \
    apt-transport-https \
    ca-certificates \
    unixodbc \
    unixodbc-dev \
    && curl -fsSL https://packages.microsoft.com/keys/microsoft.asc \
       | gpg --dearmor -o /usr/share/keyrings/microsoft-prod.gpg \
    && echo "deb [arch=amd64 signed-by=/usr/share/keyrings/microsoft-prod.gpg] \
       https://packages.microsoft.com/debian/12/prod bookworm main" \
       > /etc/apt/sources.list.d/mssql-release.list \
    && apt-get update \
    && ACCEPT_EULA=Y apt-get install -y msodbcsql18 \
    && apt-get clean && rm -rf /var/lib/apt/lists/*

# ─── Dependências Python ──────────────────────────────────────
# Timeout extendido para pacotes grandes como Torch (~500MB)
COPY requirements.txt .
RUN pip install --no-cache-dir --default-timeout=1000 -r requirements.txt

# ─── Download dos modelos de IA (feito em build time) ─────────
# Modelos ficam dentro da imagem — sem necessidade de internet
# em runtime. Hugging Face salva em /root/.cache/huggingface
# e PyTorch em /root/.cache/torch automaticamente.
RUN python -c "\
from transformers import AutoTokenizer, AutoModel; \
from torchvision import models; \
print('📥 Baixando DistilBERT...'); \
AutoTokenizer.from_pretrained('distilbert-base-uncased'); \
AutoModel.from_pretrained('distilbert-base-uncased'); \
print('📥 Baixando ResNet18...'); \
models.resnet18(weights=models.ResNet18_Weights.DEFAULT); \
print('✅ Modelos de IA prontos!')"

# ─── Código da aplicação ──────────────────────────────────────
COPY . .

# Pasta para embeddings gerados pelo train.py
RUN mkdir -p /app/models

EXPOSE 8000

# ─── Comando de inicialização ─────────────────────────────────
# Hot reload activo em desenvolvimento
# Em produção remover o --reload
CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "8000", "--reload"]