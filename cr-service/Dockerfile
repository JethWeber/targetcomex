FROM python:3.10-slim

WORKDIR /app

# Instala dependências essenciais + driver Microsoft ODBC 18
RUN apt-get update && apt-get install -y \
    curl \
    gnupg \
    apt-transport-https \
    ca-certificates \
    unixodbc \
    unixodbc-dev \
    && curl -fsSL https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor -o /usr/share/keyrings/microsoft-prod.gpg \
    && echo "deb [arch=amd64 signed-by=/usr/share/keyrings/microsoft-prod.gpg] https://packages.microsoft.com/debian/12/prod bookworm main" > /etc/apt/sources.list.d/mssql-release.list \
    && apt-get update \
    && ACCEPT_EULA=Y apt-get install -y msodbcsql18 \
    && apt-get clean && rm -rf /var/lib/apt/lists/*

# Copia os requisitos
COPY requirements.txt .

# Instalação com timeout extendido para evitar quedas em pacotes grandes (como o Torch)
RUN pip install --no-cache-dir --default-timeout=1000 -r requirements.txt

# Copia o código da aplicação
COPY . .

EXPOSE 8000

# Comando para rodar a API com Hot Reload
CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "8000", "--reload"]