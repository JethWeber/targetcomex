-- ============================================================
-- TARGET COMEX — Inicialização do Banco de Dados
-- Apenas DDL (estrutura). Dados de teste em popular_bd.sql
-- ============================================================

USE master;
GO


CREATE DATABASE TargetComex;
GO

USE TargetComex;
GO

-- Evita saltos grandes nos IDs IDENTITY (ex.: +1000) após restart do SQL Server 2017+
ALTER DATABASE SCOPED CONFIGURATION SET IDENTITY_CACHE = OFF;
GO

-- ================== USUÁRIOS ==================
CREATE TABLE Usuarios (
    Id                   INT IDENTITY(1,1) PRIMARY KEY,
    Nome                 VARCHAR(150)      NOT NULL,
    Email                VARCHAR(150)      UNIQUE NOT NULL,
    SenhaHash            VARCHAR(255)      NOT NULL DEFAULT 'changeme',
    Role                 VARCHAR(20)       NOT NULL DEFAULT 'cliente'
                                           CHECK (Role IN ('cliente', 'admin', 'vendedor')),
    DataNascimento       DATE              NULL,
    Genero               CHAR(1)           NULL CHECK (Genero IN ('M','F')),
    EstadoCivil          VARCHAR(50)       NULL,
    NumeroFilhos         INT               NULL DEFAULT 0,
    Profissao            VARCHAR(100)      NULL,
    FaixaRendaMensal     VARCHAR(50)       NULL,
    InteressesPrincipais NVARCHAR(500)     NULL,
    TipoDeUsoPretendido  NVARCHAR(200)     NULL,
    DataCadastro         DATETIME          DEFAULT GETDATE()
);
GO

-- ================== ENDEREÇOS ==================
CREATE TABLE Enderecos (
    Id               INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId        INT          NOT NULL REFERENCES Usuarios(Id),
    Provincia        VARCHAR(100) NULL,
    Municipio        VARCHAR(100) NULL,
    Distrito         VARCHAR(100) NULL,
    Bairro           VARCHAR(100) NULL,
    RuaComplemento   VARCHAR(255) NULL,
    DataAtualizacao  DATETIME     DEFAULT GETDATE()
);
GO

-- ================== VEÍCULOS ==================
CREATE TABLE Veiculos (
    Id            INT IDENTITY(1,1) PRIMARY KEY,
    Marca         VARCHAR(50)     NOT NULL,
    Modelo        VARCHAR(100)    NOT NULL,
    Ano           INT             NOT NULL,
    Descricao     NVARCHAR(MAX)   NULL,
    ImagemUrl     VARCHAR(500)    NULL,
    Cor           VARCHAR(50)     NULL,
    Estilo        VARCHAR(50)     NULL,  -- Pick-up | Hatchback | SUV | Sedan
    Combustivel   VARCHAR(30)     NULL,
    Quilometragem INT             NULL DEFAULT 0,
    Preco         DECIMAL(18,2)   NOT NULL,
    Disponivel    BIT             DEFAULT 1
);
GO

-- ================== HISTÓRICO DE NAVEGAÇÃO ==================
CREATE TABLE HistoricoNavegacao (
    Id               INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId        INT      NOT NULL REFERENCES Usuarios(Id),
    VeiculoId        INT      NOT NULL REFERENCES Veiculos(Id),
    DataVisualizacao DATETIME DEFAULT GETDATE()
);
GO

-- ================== HISTÓRICO DE COMPRAS ==================
CREATE TABLE HistoricoCompras (
    Id         INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId  INT           NOT NULL REFERENCES Usuarios(Id),
    VeiculoId  INT           NOT NULL REFERENCES Veiculos(Id),
    DataCompra DATETIME      DEFAULT GETDATE(),
    ValorPago  DECIMAL(18,2) NOT NULL
);
GO

-- ================== AVALIAÇÕES ==================
CREATE TABLE Avaliacoes (
    Id            INT IDENTITY(1,1) PRIMARY KEY,
    VeiculoId     INT           NOT NULL REFERENCES Veiculos(Id),
    UsuarioId     INT           NOT NULL REFERENCES Usuarios(Id),
    Nota          INT           CHECK (Nota BETWEEN 1 AND 5),
    Comentario    NVARCHAR(MAX) NULL,
    DataAvaliacao DATETIME      DEFAULT GETDATE()
);
GO

-- ================== FEATURES MULTIMODAIS (IA) ==================
CREATE TABLE FeaturesMultimodais (
    Id                INT IDENTITY(1,1) PRIMARY KEY,
    VeiculoId         INT           NOT NULL REFERENCES Veiculos(Id),
    EmbeddingVisual   VARBINARY(MAX) NULL,
    EmbeddingTextual  VARBINARY(MAX) NULL,
    DataProcessamento DATETIME      DEFAULT GETDATE()
);
GO

-- Confirmação
SELECT '✅ Estrutura do banco TargetComex criada com sucesso!' AS Status;
GO