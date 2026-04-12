# ============================================================
# TARGET COMEX — Serviço de Banco de Dados
# Base: SQL Server 2022 Developer Edition (Linux)
# ============================================================
FROM mcr.microsoft.com/mssql/server:2022-latest

# Root necessário para copiar ficheiros e dar permissões
USER root

# ─── Variáveis de ambiente do SQL Server ─────────────────────
# Estas são sobrescritas pelo docker-compose.yml em runtime,
# mas são necessárias aqui para o HEALTHCHECK funcionar no build
ENV ACCEPT_EULA=Y
ENV MSSQL_SA_PASSWORD=TargetComex2025!
ENV MSSQL_PID=Developer

# ─── Scripts de inicialização ────────────────────────────────
# O entrypoint.sh verifica se o banco existe:
#   - Se NÃO existe → executa init.sql depois popular_bd.sql
#   - Se JÁ existe  → ignora tudo (dados preservados)
COPY entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

# Scripts SQL copiados para a pasta de inicialização
# Ordem de execução controlada pelo entrypoint.sh
COPY init.sql       /docker-entrypoint-initdb.d/init.sql
COPY popular_bd.sql /docker-entrypoint-initdb.d/popular_bd.sql

# Volta ao utilizador mssql por segurança (least privilege)
USER mssql

# ─── Health Check ─────────────────────────────────────────────
# Outros serviços (cr-service, target-api) dependem deste check
# para só subirem quando o SQL Server estiver mesmo pronto
HEALTHCHECK --interval=15s --timeout=10s --start-period=60s --retries=10 \
    CMD /opt/mssql-tools18/bin/sqlcmd \
        -S localhost -U sa -P $MSSQL_SA_PASSWORD \
        -Q "SELECT 1" -C || exit 1

# ─── Entry Point ──────────────────────────────────────────────
ENTRYPOINT ["/entrypoint.sh"]