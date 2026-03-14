-- Elimina o banco antigo se existir
USE master;
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'TargetComex')
    DROP DATABASE TargetComex;
GO

-- Cria o banco novo
CREATE DATABASE TargetComex;
GO

USE TargetComex;
GO

-- ================== USUÁRIOS (enriquecido para IA/KNN) ==================
CREATE TABLE Usuarios (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nome VARCHAR(150) NOT NULL,
    Email VARCHAR(150) UNIQUE NOT NULL,
    DataNascimento DATE NULL,
    Genero CHAR(1) NULL CHECK (Genero IN ('M','F')),
    EstadoCivil VARCHAR(50) NULL,
    NumeroFilhos INT NULL DEFAULT 0,
    Profissao VARCHAR(100) NULL,
    FaixaRendaMensal VARCHAR(50) NULL,
    InteressesPrincipais NVARCHAR(500) NULL,
    TipoDeUsoPretendido NVARCHAR(200) NULL,
    DataCadastro DATETIME DEFAULT GETDATE()
);
GO

-- ================== ENDEREÇO (simplificado, todos campos diretos) ==================
CREATE TABLE Enderecos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId INT NOT NULL FOREIGN KEY REFERENCES Usuarios(Id),
    Provincia VARCHAR(100) NULL,
    Municipio VARCHAR(100) NULL,
    Distrito VARCHAR(100) NULL,
    Bairro VARCHAR(100) NULL,
    RuaComplemento VARCHAR(255) NULL,
    DataAtualizacao DATETIME DEFAULT GETDATE()
);
GO

-- ================== VEÍCULOS ==================
CREATE TABLE Veiculos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Marca VARCHAR(50) NOT NULL,
    Modelo VARCHAR(100) NOT NULL,
    Ano INT NOT NULL,
    Descricao NVARCHAR(MAX) NULL,
    ImagemUrl VARCHAR(500) NULL,
    Cor VARCHAR(50) NULL,
    Estilo VARCHAR(50) NULL,
    Combustivel VARCHAR(30) NULL,
    Quilometragem INT NULL,
    Preco DECIMAL(18,2) NOT NULL,
    Disponivel BIT DEFAULT 1
);
GO

-- ================== INTERAÇÕES ==================
CREATE TABLE HistoricoNavegacao (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId INT NOT NULL FOREIGN KEY REFERENCES Usuarios(Id),
    VeiculoId INT NOT NULL FOREIGN KEY REFERENCES Veiculos(Id),
    DataVisualizacao DATETIME DEFAULT GETDATE()
);
GO

CREATE TABLE HistoricoCompras (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId INT NOT NULL FOREIGN KEY REFERENCES Usuarios(Id),
    VeiculoId INT NOT NULL FOREIGN KEY REFERENCES Veiculos(Id),
    DataCompra DATETIME DEFAULT GETDATE(),
    ValorPago DECIMAL(18,2) NOT NULL
);
GO

-- ================== AVALIAÇÕES E FEATURES ==================
CREATE TABLE Avaliacoes (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    VeiculoId INT NOT NULL FOREIGN KEY REFERENCES Veiculos(Id),
    UsuarioId INT NOT NULL FOREIGN KEY REFERENCES Usuarios(Id),
    Nota INT CHECK (Nota BETWEEN 1 AND 5),
    Comentario NVARCHAR(MAX) NULL,
    DataAvaliacao DATETIME DEFAULT GETDATE()
);
GO

CREATE TABLE FeaturesMultimodais (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    VeiculoId INT NOT NULL FOREIGN KEY REFERENCES Veiculos(Id),
    EmbeddingVisual VARBINARY(MAX) NULL,
    EmbeddingTextual VARBINARY(MAX) NULL
);
GO

-- ================== DADOS DE TESTE ==================
INSERT INTO Usuarios (Nome, Email, DataNascimento, Genero, EstadoCivil, NumeroFilhos, Profissao, FaixaRendaMensal, InteressesPrincipais)
VALUES 
    ('João Silva', 'joao@target.ao', '1988-05-12', 'M', 'Casado', 2, 'Professor', 'Média', 'família,economia,conforto'),
    ('Maria Antónia', 'maria@target.ao', '1992-11-03', 'F', 'Solteira', 0, 'Empresária', 'Alta', 'luxo,design,tecnologia'),
    ('António Manuel', 'antonio@target.ao', '1975-08-20', 'M', 'Casado', 4, 'Comerciante', 'Média-Alta', 'espaço,robustez,off-road');

INSERT INTO Enderecos (UsuarioId, Provincia, Municipio, Distrito, Bairro, RuaComplemento)
VALUES 
    (1, 'Luanda', 'Viana', 'Robadina', 'Kikuxi', 'Rua Principal 45'),
    (2, 'Luanda', 'Belas', 'Morro Bento', 'Talatona', 'Cond. Prestige Apt 302');

INSERT INTO Veiculos (Marca, Modelo, Ano, Descricao, ImagemUrl, Cor, Estilo, Combustivel, Quilometragem, Preco)
VALUES 
    ('Toyota', 'Hilux SRV 4x4', 2023, 'Pick-up robusta', '/hilux.jpg', 'Preto', 'Pick-up', 'Diesel', 15000, 18500000.00),
    ('Hyundai', 'i10', 2024, 'Compacto econômico', '/i10.jpg', 'Branco', 'Hatchback', 'Gasolina', 5000, 6000000.00);

-- Confirmação final
SELECT '✅ Banco TargetComex recriado com sucesso!' AS Status;
SELECT COUNT(*) AS Usuarios FROM Usuarios;
SELECT COUNT(*) AS Veiculos FROM Veiculos;
GO