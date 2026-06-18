-- ============================================================
-- TARGET COMEX — Dados de Teste (Seed)
-- Executa APÓS init.sql ter criado a estrutura
-- ============================================================
-- Senhas:
--   Admin Target   → Target2025!
--   Todos os outros → User1-2026
-- ============================================================

USE TargetComex;
GO

-- ================== USUÁRIOS ==================
INSERT INTO Usuarios (Nome, Email, SenhaHash, Role, DataNascimento, Genero, EstadoCivil, NumeroFilhos, Profissao, FaixaRendaMensal, InteressesPrincipais, TipoDeUsoPretendido)
VALUES
    -- Admin  (senha: Target2025!)
    ('Admin Target',    'admin@target.ao',   '$2b$11$iXTtwU4/zlDAhJPb8071BuK7xdZNb6.ivDwQwWZgEyonQBPnLAp6m', 'admin',    '1985-03-15', 'M', 'Casado',    1, 'Gestor',     'Alta',       'tecnologia,design',            'Administrativo'),

    -- Clientes  (senha: User1-2026)
    ('João Silva',      'joao@target.ao',    '$2b$11$.N2psXeibzSXMAp8R8zFrOOohbULNQ0xtMcO43d.Ab0a79SDd4RT2', 'cliente',  '1988-05-12', 'M', 'Casado',    2, 'Professor',  'Média',      'família,economia,conforto',    'Uso diário familiar'),
    ('António Manuel',  'antonio@target.ao', '$2b$11$.N2psXeibzSXMAp8R8zFrOOohbULNQ0xtMcO43d.Ab0a79SDd4RT2', 'cliente',  '1975-08-20', 'M', 'Casado',    4, 'Comerciante','Média-Alta', 'espaço,robustez,off-road',     'Campo e cidade');
GO

-- ================== ENDEREÇOS ==================
INSERT INTO Enderecos (UsuarioId, Provincia, Municipio, Distrito, Bairro, RuaComplemento)
VALUES
    (1,  'Luanda',  'Luanda',  'Ingombota',  'Maianga',     'Rua Rainha Ginga 12'),
    (2,  'Luanda',  'Belas',   'Morro Bento', 'Talatona',   'Cond. Prestige Apt 302');
GO

-- ================== VEÍCULOS ==================
INSERT INTO Veiculos (Marca, Modelo, Ano, Descricao, ImagemUrl, Cor, Estilo, Combustivel, Quilometragem, Preco)
VALUES
    -- Pick-ups
    ('Toyota',     'Hilux SRV 4x4',      2023, 'Pick-up robusta ideal para off-road e trabalho pesado. Motor diesel potente, cabine dupla com conforto premium.',       '/images/hilux.jpg',    'Preto',    'Pick-up',   'Diesel',   15000, 18500000.00),
    ('Nissan',     'Navara NP300',       2023, 'Pick-up confiável com excelente custo-benefício. Cabine dupla, ar condicionado e sistema de entretenimento moderno.',    '/images/navara.jpg',   'Branco',   'Pick-up',   'Diesel',   8000,  15200000.00);
GO

-- ================== CONFIRMAÇÃO ==================
SELECT '✅ Banco TargetComex populado com sucesso!' AS Status;
SELECT COUNT(*) AS TotalUsuarios   FROM Usuarios;
SELECT COUNT(*) AS TotalVeiculos   FROM Veiculos;
GO