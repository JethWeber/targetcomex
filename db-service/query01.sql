-- Drop o banco se existir
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'TargetComex')
    DROP DATABASE TargetComex;
GO

-- Confirmação (deve mostrar vazio ou sem TargetComex)
SELECT name FROM sys.databases;
GO