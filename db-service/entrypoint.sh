#!/bin/bash

set -e

# Inicia o SQL Server em background
/opt/mssql/bin/sqlservr &

# Espera o SQL Server estar pronto para conexões (timeout de 2 minutos)
echo "Aguardando SQL Server iniciar..."
for i in {1..24}; do
  if /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P $MSSQL_SA_PASSWORD -Q "SELECT 1" > /dev/null 2>&1; then
    echo "SQL Server pronto!"
    break
  fi
  sleep 5
done

# Verifica se o banco TargetComex já existe (evita rodar init.sql toda vez)
DB_EXISTS=$(/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P $MSSQL_SA_PASSWORD -Q "SELECT COUNT(*) FROM sys.databases WHERE name = 'TargetComex'" -h -1 | tr -d ' ')

if [ "$DB_EXISTS" -eq "0" ]; then
  echo "Banco TargetComex não encontrado. Executando init.sql..."
  /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P $MSSQL_SA_PASSWORD -i /docker-entrypoint-initdb.d/init.sql
  echo "Inicialização do banco concluída!"
else
  echo "Banco TargetComex já existe. Pulando init.sql."
fi

# Mantém o container rodando (SQL Server continua em foreground)
wait