USE TargetComex;
GO

-- Primeiro, verifique quais Veiculos existem (para usar IDs corretos)
SELECT Id, Marca, Modelo, Estilo, Preco FROM Veiculos;
GO

-- Se não tiver os IDs 2 e 3, insira os veículos de teste
IF NOT EXISTS (SELECT 1 FROM Veiculos WHERE Id = 2)
INSERT INTO Veiculos (Marca, Modelo, Ano, Estilo, Preco, Disponivel)
VALUES ('VW', 'Golf GTI', 2022, 'Hatchback', 9500000.00, 1);

IF NOT EXISTS (SELECT 1 FROM Veiculos WHERE Id = 3)
INSERT INTO Veiculos (Marca, Modelo, Ano, Estilo, Preco, Disponivel)
VALUES ('Hyundai', 'i10', 2024, 'Hatchback', 6000000.00, 1);
GO

-- Agora insere a compra de Maria (ID 2) do Golf (ID 2)
INSERT INTO HistoricoCompras (UsuarioId, VeiculoId, ValorPago)
VALUES (2, 2, 9500000.00);
GO

-- Avaliação alta de João (ID 1) do i10 (ID 3)
INSERT INTO Avaliacoes (VeiculoId, UsuarioId, Nota, Comentario)
VALUES (3, 1, 5, 'Ótimo para cidade!');
GO

-- Confirmação
SELECT 'Dados adicionados!' AS Status;
SELECT 'Compras:' AS Compras, COUNT(*) FROM HistoricoCompras;
SELECT 'Avaliações:' AS Avaliacoes, COUNT(*) FROM Avaliacoes;
GO