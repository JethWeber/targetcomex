# Imagem oficial Microsoft SQL Server 2022 - Developer Edition (gratuita para dev/teste)
FROM mcr.microsoft.com/mssql/server:2022-latest

# Variáveis de ambiente obrigatórias
ENV ACCEPT_EULA=Y
ENV MSSQL_SA_PASSWORD=TargetComex2025!
ENV MSSQL_PID=Developer

# Locale para suporte a caracteres acentuados
ENV LC_ALL=C.UTF-8
ENV LANG=C.UTF-8

# Copia o entrypoint customizado
COPY entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

# Copia o script de inicialização
COPY init.sql /docker-entrypoint-initdb.d/init.sql

# Healthcheck (verifica se responde)
HEALTHCHECK --interval=15s --timeout=10s --start-period=60s --retries=10 \
    CMD /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P $MSSQL_SA_PASSWORD -Q "SELECT 1" || exit 1

# Usa o entrypoint customizado
ENTRYPOINT ["/entrypoint.sh"]