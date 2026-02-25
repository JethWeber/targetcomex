-- 1. Apagar banco antigo (se existir)
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'TargetComex')
    DROP DATABASE TargetComex;
GO

-- 2. Criar novo banco
CREATE DATABASE TargetComex;
GO

USE TargetComex;
GO

-- 3. Tabela Usuarios - enriquecida para IA / KNN
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Usuarios')
CREATE TABLE Usuarios (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nome VARCHAR(150) NOT NULL,
    Email VARCHAR(150) UNIQUE NOT NULL,
    SenhaHash VARCHAR(255) NULL,
    DataNascimento DATE NULL,                      -- para calcular idade
    Genero CHAR(1) NULL CHECK (Genero IN ('M','F','O','N')),  -- M=Masculino, F=Feminino, O=Outro, N=Prefiro não informar
    EstadoCivil VARCHAR(50) NULL,                  -- Solteiro, Casado, União de Facto, Divorciado, Viúvo
    NumeroFilhos INT NULL DEFAULT 0,
    Profissao VARCHAR(100) NULL,
    FaixaRendaMensal VARCHAR(50) NULL,             -- ex: 'Até 100.000 Kz', '100.001-300.000 Kz', 'Acima 500.000 Kz'
    InteressesPrincipais NVARCHAR(500) NULL,       -- ex: 'off-road, família, economia de combustível'
    TipoDeUsoPretendido NVARCHAR(200) NULL,        -- ex: 'uso diário na cidade', 'viagens longas', 'trabalho pesado'
    VeiculoAtual VARCHAR(150) NULL,                -- marca/modelo atual (texto livre ou futuro FK)
    DataCadastro DATETIME DEFAULT GETDATE(),
    UltimoAcesso DATETIME NULL
);
GO

-- 4. Tabela Enderecos - simplificada, todos campos diretos
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Enderecos')
CREATE TABLE Enderecos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId INT NOT NULL FOREIGN KEY REFERENCES Usuarios(Id),
    Provincia VARCHAR(100) NULL,                   -- ex: 'Luanda', 'Benguela', 'Huambo'
    Municipio VARCHAR(100) NULL,                   -- ex: 'Viana', 'Belas', 'Cacuaco'
    Distrito VARCHAR(100) NULL,                    -- ex: 'Robadina', 'Cazenga'
    Bairro VARCHAR(100) NULL,                      -- ex: 'Kikuxi', 'Grafanil'
    RuaComplemento VARCHAR(255) NULL,              -- Rua, nº, casa, andar, etc.
    CodigoPostal VARCHAR(20) NULL,                 -- opcional
    Latitude DECIMAL(9,6) NULL,                    -- para geolocalização futura (opcional)
    Longitude DECIMAL(9,6) NULL,
    DataAtualizacao DATETIME DEFAULT GETDATE()
);
GO

-- 5. Tabela Veiculos (mantida com foco em concessionária)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Veiculos')
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
    Disponivel BIT DEFAULT 1,
    DataCadastro DATETIME DEFAULT GETDATE()
);
GO

-- 6. Tabelas de interações (para histórico e KNN colaborativo)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'HistoricoNavegacao')
CREATE TABLE HistoricoNavegacao (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId INT NOT NULL FOREIGN KEY REFERENCES Usuarios(Id),
    VeiculoId INT NOT NULL FOREIGN KEY REFERENCES Veiculos(Id),
    DataVisualizacao DATETIME DEFAULT GETDATE(),
    DuracaoSegundos INT NULL
);
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'HistoricoCompras')
CREATE TABLE HistoricoCompras (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId INT NOT NULL FOREIGN KEY REFERENCES Usuarios(Id),
    VeiculoId INT NOT NULL FOREIGN KEY REFERENCES Veiculos(Id),
    DataCompra DATETIME DEFAULT GETDATE(),
    ValorPago DECIMAL(18,2) NOT NULL,
    FormaPagamento VARCHAR(50) NULL
);
GO

-- 7. Avaliações e Features Multimodais (mantidas)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Avaliacoes')
CREATE TABLE Avaliacoes (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    VeiculoId INT NOT NULL FOREIGN KEY REFERENCES Veiculos(Id),
    UsuarioId INT NOT NULL FOREIGN KEY REFERENCES Usuarios(Id),
    Nota INT CHECK (Nota BETWEEN 1 AND 5),
    Comentario NVARCHAR(MAX) NULL,
    DataAvaliacao DATETIME DEFAULT GETDATE()
);
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'FeaturesMultimodais')
CREATE TABLE FeaturesMultimodais (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    VeiculoId INT NOT NULL FOREIGN KEY REFERENCES Veiculos(Id),
    EmbeddingVisual VARBINARY(MAX) NULL,
    EmbeddingTextual VARBINARY(MAX) NULL,
    DataExtracao DATETIME DEFAULT GETDATE()
);
GO

-- 8. Índices úteis para KNN e relatórios
CREATE NONCLUSTERED INDEX IX_Usuarios_Perfil_KNN ON Usuarios(Profissao, FaixaRendaMensal, NumeroFilhos, Genero);
CREATE NONCLUSTERED INDEX IX_Enderecos_Localizacao ON Enderecos(Provincia, Municipio);
GO

-- 9. Dados de teste mínimos (adicione mais depois)
INSERT INTO Usuarios (Nome, Email, DataNascimento, Genero, EstadoCivil, NumeroFilhos, Profissao, FaixaRendaMensal, InteressesPrincipais, TipoDeUsoPretendido)
VALUES 
    ('João Silva', 'joao@target.ao', '1988-05-12', 'M', 'Casado', 2, 'Professor', 'Média (200-500 mil Kz)', 'família, economia, conforto', 'uso diário + viagens curtas'),
    ('Maria Antónia', 'maria@target.ao', '1992-11-03', 'F', 'Solteira', 0, 'Empresária', 'Alta (>800 mil Kz)', 'luxo, design, tecnologia', 'status e conforto'),
    ('António Manuel', 'antonio@target.ao', '1975-08-20', 'M', 'Casado', 4, 'Comerciante', 'Média-Alta', 'espaço, robustez, off-road', 'família grande + trabalho');

INSERT INTO Enderecos (UsuarioId, Provincia, Municipio, Distrito, Bairro, RuaComplemento)
VALUES 
    (1, 'Luanda', 'Viana', 'Robadina', 'Kikuxi', 'Rua Principal 45'),
    (2, 'Luanda', 'Belas', 'Morro Bento', 'Talatona', 'Cond. Prestige Apt 302'),
    (3, 'Benguela', 'Lobito', 'Zona Comercial', 'Companhia', 'Av. 14 de Abril');

-- Confirmação
SELECT 'Banco recriado com sucesso - pronto para IA multimodal e KNN!' AS Status;
SELECT COUNT(*) AS Usuarios_Cadastrados FROM Usuarios;
GO