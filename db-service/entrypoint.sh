#!/bin/bash

set -e

# Inicia o SQL Server em background
/opt/mssql/bin/sqlservr &

# Aguarda o SQL Server estar pronto (máx 150s)
echo "⏳ Aguardando SQL Server iniciar..."
for i in {1..30}; do
  if /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -Q "SELECT 1" -C > /dev/null 2>&1; then
    echo "✅ SQL Server pronto!"
    break
  fi
  echo "   Tentativa $i/30 — aguardando 5s..."
  sleep 5
done

# Verifica se o banco TargetComex já existe
DB_EXISTS=$(/opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "$MSSQL_SA_PASSWORD" \
  -Q "SET NOCOUNT ON; SELECT COUNT(*) FROM sys.databases WHERE name = 'TargetComex'" \
  -h -1 -C | tr -d '[:space:]')

# Garante que é um número válido
if [[ ! "$DB_EXISTS" =~ ^[0-9]+$ ]]; then
  DB_EXISTS=0
fi

if [ "$DB_EXISTS" -eq "0" ]; then
  echo "🗄️  Banco TargetComex não encontrado. Executando init.sql..."
  /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P "$MSSQL_SA_PASSWORD" \
    -i /docker-entrypoint-initdb.d/init.sql -C

  echo "🌱 Estrutura criada. Populando com dados de teste (popular_bd.sql)..."
  /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P "$MSSQL_SA_PASSWORD" \
    -i /docker-entrypoint-initdb.d/popular_bd.sql -C

  echo "🎉 Banco TargetComex inicializado com sucesso!"
else
  echo "ℹ️  Banco TargetComex já existe. Pulando inicialização."
fi

# Mantém o container rodando
wait