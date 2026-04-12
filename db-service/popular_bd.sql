-- ============================================================
-- TARGET COMEX — Dados de Teste (Seed)
-- Executa APÓS init.sql ter criado a estrutura
-- ============================================================

USE TargetComex;
GO

-- ================== USUÁRIOS ==================
-- Roles variados: 1 admin, 1 vendedor, resto clientes
-- Perfis diversificados para o KNN ter dados suficientes

INSERT INTO Usuarios (Nome, Email, SenhaHash, Role, DataNascimento, Genero, EstadoCivil, NumeroFilhos, Profissao, FaixaRendaMensal, InteressesPrincipais, TipoDeUsoPretendido)
VALUES
    -- Admin
    ('Admin Target',      'admin@target.ao',    'hashed_admin123',  'admin',    '1985-03-15', 'M', 'Casado',         1, 'Gestor',       'Alta',       'tecnologia,design',             'Administrativo'),

    -- Vendedor
    ('Maria Vendedora',   'maria@target.ao',    'hashed_maria123',  'vendedor', '1990-07-22', 'F', 'Solteira',       0, 'Vendedora',    'Média-Alta', 'conforto,tecnologia',           'Profissional'),

    -- Clientes — perfis distintos para alimentar o KNN
    ('João Silva',        'joao@target.ao',     'hashed_joao123',   'cliente',  '1988-05-12', 'M', 'Casado',         2, 'Professor',    'Média',      'família,economia,conforto',     'Uso diário familiar'),
    ('António Manuel',    'antonio@target.ao',  'hashed_antonio123','cliente',  '1975-08-20', 'M', 'Casado',         4, 'Comerciante',  'Média-Alta', 'espaço,robustez,off-road',      'Campo e cidade'),
    ('Carla Rodrigues',   'carla@target.ao',    'hashed_carla123',  'cliente',  '1995-01-30', 'F', 'Solteira',       0, 'Designer',     'Média',      'design,tecnologia,luxo',        'Uso urbano'),
    ('Pedro Lopes',       'pedro@target.ao',    'hashed_pedro123',  'cliente',  '1982-11-05', 'M', 'Divorciado',     1, 'Engenheiro',   'Alta',       'tecnologia,conforto,design',    'Executivo'),
    ('Ana Fernandes',     'ana@target.ao',      'hashed_ana123',    'cliente',  '1993-06-18', 'F', 'Casada',         2, 'Médica',       'Alta',       'conforto,família,espaço',       'Uso familiar premium'),
    ('Miguel Costa',      'miguel@target.ao',   'hashed_miguel123', 'cliente',  '1979-04-25', 'M', 'Casado',         3, 'Agricultor',   'Média',      'robustez,off-road,espaço',      'Trabalho rural'),
    ('Sofia Neto',        'sofia@target.ao',    'hashed_sofia123',  'cliente',  '1998-09-12', 'F', 'Solteira',       0, 'Estudante',    'Baixa',      'economia,design,tecnologia',    'Uso urbano econômico'),
    ('Carlos Baptista',   'carlos@target.ao',   'hashed_carlos123', 'cliente',  '1970-12-03', 'M', 'Casado',         5, 'Empresário',   'Alta',       'luxo,conforto,família,espaço',  'Família grande premium');
GO

-- ================== ENDEREÇOS ==================
INSERT INTO Enderecos (UsuarioId, Provincia, Municipio, Distrito, Bairro, RuaComplemento)
VALUES
    (1,  'Luanda', 'Luanda',  'Ingombota',   'Maianga',      'Rua Rainha Ginga 12'),
    (2,  'Luanda', 'Belas',   'Morro Bento',  'Talatona',    'Cond. Prestige Apt 302'),
    (3,  'Luanda', 'Viana',   'Robadina',     'Kikuxi',      'Rua Principal 45'),
    (4,  'Luanda', 'Cacuaco', 'Cacuaco',      'Sequele',     'Rua do Comércio 78'),
    (5,  'Luanda', 'Luanda',  'Sambizanga',   'Palanca',     'Rua da Arte 22'),
    (6,  'Luanda', 'Belas',   'Talatona',     'Alphaville',  'Av. Principal Bl 5 Apt 11'),
    (7,  'Luanda', 'Luanda',  'Ingombota',    'Alvalade',    'Rua dos Médicos 9'),
    (8,  'Malanje','Malanje', 'Malanje',      'Centro',      'Rua da Fazenda 100'),
    (9,  'Luanda', 'Luanda',  'Rangel',       'Rangel',      'Rua da Juventude 5'),
    (10, 'Luanda', 'Belas',   'Talatona',     'Benfica',     'Cond. Sol Nascente Casa 3');
GO

-- ================== VEÍCULOS ==================
-- Estilos: Pick-up | Hatchback | SUV | Sedan
-- Quantidade suficiente para o KNN e recomendações variadas

INSERT INTO Veiculos (Marca, Modelo, Ano, Descricao, ImagemUrl, Cor, Estilo, Combustivel, Quilometragem, Preco)
VALUES
    -- Pick-ups
    ('Toyota',    'Hilux SRV 4x4',     2023, 'Pick-up robusta ideal para off-road e trabalho pesado. Motor diesel potente, cabine dupla com conforto premium.',                    '/images/hilux.jpg',      'Preto',   'Pick-up',   'Diesel',   15000, 18500000.00),
    ('Ford',      'Ranger Wildtrak',   2022, 'Pick-up aventureira com tração 4x4, suspensão reforçada e tecnologia avançada. Ideal para terrenos difíceis.',                     '/images/ranger.jpg',     'Azul',    'Pick-up',   'Diesel',   22000, 16800000.00),
    ('Nissan',    'Navara NP300',      2023, 'Pick-up confiável com excelente custo-benefício. Cabine dupla, ar condicionado e sistema de entretenimento moderno.',               '/images/navara.jpg',     'Branco',  'Pick-up',   'Diesel',   8000,  15200000.00),

    -- Hatchbacks
    ('Hyundai',   'i10',               2024, 'Compacto econômico e ágil para uso urbano. Baixo consumo, fácil estacionamento e manutenção acessível.',                           '/images/i10.jpg',        'Branco',  'Hatchback', 'Gasolina', 5000,  6000000.00),
    ('Volkswagen','Polo',              2023, 'Hatchback sofisticado com tecnologia alemã. Excelente acabamento, conforto e eficiência no consumo de combustível.',                '/images/polo.jpg',       'Prata',   'Hatchback', 'Gasolina', 12000, 8500000.00),
    ('Toyota',    'Yaris',             2024, 'Hatchback moderno com design jovem e tecnologia Toyota Safety Sense. Econômico e confiável para o dia a dia.',                     '/images/yaris.jpg',      'Vermelho','Hatchback', 'Híbrido',  3000,  9200000.00),

    -- SUVs
    ('Toyota',    'Land Cruiser Prado',2022, 'SUV premium todo-terreno com sofisticação e potência. Interior luxuoso, 7 lugares, ideal para famílias exigentes.',                '/images/prado.jpg',      'Bege',    'SUV',       'Diesel',   35000, 42000000.00),
    ('Hyundai',   'Tucson',            2023, 'SUV moderno e tecnológico com design arrojado. Tração AWD, teto panorâmico e conectividade total.',                                '/images/tucson.jpg',     'Cinza',   'SUV',       'Gasolina', 18000, 19500000.00),
    ('Ford',      'Explorer',          2022, 'SUV espaçoso de 7 lugares com desempenho e conforto para toda a família. Sistema de entretenimento traseiro incluído.',             '/images/explorer.jpg',   'Preto',   'SUV',       'Gasolina', 28000, 28000000.00),
    ('Kia',       'Sportage',          2024, 'SUV compacto com design moderno, acabamento premium e ótima relação custo-benefício para famílias urbanas.',                       '/images/sportage.jpg',   'Branco',  'SUV',       'Gasolina', 5000,  17000000.00),

    -- Sedans
    ('Toyota',    'Camry',             2023, 'Sedan executivo com conforto e tecnologia híbrida. Interior espaçoso, silencioso e eficiente. Ideal para executivos.',             '/images/camry.jpg',      'Preto',   'Sedan',     'Híbrido',  10000, 22000000.00),
    ('Honda',     'Accord',            2022, 'Sedan sofisticado com motor turbo, acabamento premium e tecnologia Honda Sensing. Perfeito para uso executivo.',                   '/images/accord.jpg',     'Prata',   'Sedan',     'Gasolina', 20000, 19800000.00),
    ('Hyundai',   'Sonata',            2023, 'Sedan de design arrojado com motor turbo e teto solar. Conectividade total e sistemas de segurança avançados.',                   '/images/sonata.jpg',     'Azul',    'Sedan',     'Gasolina', 15000, 16500000.00);
GO

-- ================== HISTÓRICO DE NAVEGAÇÃO ==================
-- Simula comportamento de navegação dos clientes

INSERT INTO HistoricoNavegacao (UsuarioId, VeiculoId, DataVisualizacao) VALUES
    -- João (família/economia) navega em Hatchbacks e SUV compacto
    (3, 4, DATEADD(day, -10, GETDATE())),
    (3, 5, DATEADD(day, -8,  GETDATE())),
    (3, 10,DATEADD(day, -5,  GETDATE())),
    (3, 6, DATEADD(day, -3,  GETDATE())),

    -- António (off-road/robustez) navega em Pick-ups
    (4, 1, DATEADD(day, -15, GETDATE())),
    (4, 2, DATEADD(day, -12, GETDATE())),
    (4, 3, DATEADD(day, -7,  GETDATE())),
    (4, 7, DATEADD(day, -2,  GETDATE())),

    -- Carla (design/luxo/tecnologia) navega em Sedans e SUVs
    (5, 11,DATEADD(day, -9,  GETDATE())),
    (5, 12,DATEADD(day, -6,  GETDATE())),
    (5, 8, DATEADD(day, -4,  GETDATE())),

    -- Pedro (tecnologia/conforto) navega em Sedans executivos
    (6, 11,DATEADD(day, -14, GETDATE())),
    (6, 12,DATEADD(day, -10, GETDATE())),
    (6, 13,DATEADD(day, -6,  GETDATE())),

    -- Ana (conforto/família) navega em SUVs e Sedans
    (7, 7, DATEADD(day, -11, GETDATE())),
    (7, 9, DATEADD(day, -8,  GETDATE())),
    (7, 10,DATEADD(day, -4,  GETDATE())),

    -- Miguel (trabalho rural) navega em Pick-ups
    (8, 1, DATEADD(day, -20, GETDATE())),
    (8, 2, DATEADD(day, -18, GETDATE())),
    (8, 3, DATEADD(day, -10, GETDATE())),

    -- Sofia (economia/jovem) navega em Hatchbacks
    (9, 4, DATEADD(day, -7,  GETDATE())),
    (9, 5, DATEADD(day, -5,  GETDATE())),
    (9, 6, DATEADD(day, -2,  GETDATE())),

    -- Carlos (luxo/família grande) navega em SUVs premium e Sedans
    (10,7, DATEADD(day, -13, GETDATE())),
    (10,9, DATEADD(day, -9,  GETDATE())),
    (10,11,DATEADD(day, -5,  GETDATE()));
GO

-- ================== HISTÓRICO DE COMPRAS ==================
INSERT INTO HistoricoCompras (UsuarioId, VeiculoId, ValorPago, DataCompra) VALUES
    (3,  5,  8500000.00, DATEADD(month, -6, GETDATE())),  -- João comprou Polo
    (4,  1, 18500000.00, DATEADD(month, -3, GETDATE())),  -- António comprou Hilux
    (5,  13,16500000.00, DATEADD(month, -4, GETDATE())),  -- Carla comprou Sonata
    (6,  11,22000000.00, DATEADD(month, -2, GETDATE())),  -- Pedro comprou Camry
    (7,  10,17000000.00, DATEADD(month, -5, GETDATE())),  -- Ana comprou Sportage
    (8,  2,  16800000.00,DATEADD(month, -1, GETDATE())),  -- Miguel comprou Ranger
    (9,  4,  6000000.00, DATEADD(month, -7, GETDATE())),  -- Sofia comprou i10
    (10, 7,  42000000.00,DATEADD(month, -2, GETDATE()));  -- Carlos comprou Prado
GO

-- ================== AVALIAÇÕES ==================
INSERT INTO Avaliacoes (VeiculoId, UsuarioId, Nota, Comentario, DataAvaliacao) VALUES
    (5,  3,  5, 'Excelente! Consumo baixo e perfeito para a cidade. Recomendo a famílias.',           DATEADD(month, -5, GETDATE())),
    (1,  4,  5, 'Robusto e confiável. Vai onde nenhum outro vai. Perfeito para o campo.',             DATEADD(month, -2, GETDATE())),
    (13, 5,  4, 'Design moderno e espaçoso. Motor potente. Poderia ter melhor consumo.',              DATEADD(month, -3, GETDATE())),
    (11, 6,  5, 'Silencioso, confortável e elegante. O melhor sedan que já tive.',                    DATEADD(month, -1, GETDATE())),
    (10, 7,  5, 'Família adorou. Espaçoso, seguro e tecnológico. Valeu cada kwanza.',                 DATEADD(month, -4, GETDATE())),
    (2,  8,  4, 'Ótimo para trabalho pesado. Poderia ter melhor ar condicionado, mas no geral ótimo.',DATEADD(day,  -20, GETDATE())),
    (4,  9,  5, 'Perfeito para estudante! Econômico, fácil de estacionar e bonito.',                  DATEADD(month, -6, GETDATE())),
    (7,  10, 5, 'Luxo completo. Interior magnifico. Família ficou encantada.',                        DATEADD(month, -1, GETDATE())),
    (8,  5,  4, 'Bonito e tecnológico. Ótimo SUV para cidade.',                                      DATEADD(day,  -15, GETDATE())),
    (12, 6,  4, 'Muito confortável e potente. Boa alternativa ao Camry.',                             DATEADD(day,  -10, GETDATE()));
GO

-- ================== CONFIRMAÇÃO ==================
SELECT '✅ Banco TargetComex populado com sucesso!' AS Status;
SELECT COUNT(*) AS TotalUsuarios   FROM Usuarios;
SELECT COUNT(*) AS TotalVeiculos   FROM Veiculos;
SELECT COUNT(*) AS TotalNavegacao  FROM HistoricoNavegacao;
SELECT COUNT(*) AS TotalCompras    FROM HistoricoCompras;
SELECT COUNT(*) AS TotalAvaliacoes FROM Avaliacoes;
GO
