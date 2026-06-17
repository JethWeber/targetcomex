# TARGET COMEX — Documentação do Projecto

> Plataforma angolana de compra e venda de veículos com sistema de recomendação por IA.

---

## Visão Geral

O Target Comex é uma aplicação web full-stack composta por 4 serviços principais que correm em Docker. O sistema permite que clientes naveguem, pesquisem e recebam recomendações personalizadas de veículos com base no seu perfil e histórico de navegação/compras.

---

## Arquitectura

```
┌─────────────────────────────────────────────────────────┐
│                     DOCKER NETWORK                       │
│                                                         │
│  ┌──────────────┐     ┌──────────────┐                  │
│  │  Target.Ui   │────▶│  Target.Api  │                  │
│  │  Blazor SSR  │     │  ASP.NET 10  │                  │
│  │  :5001       │     │  :5000       │                  │
│  └──────────────┘     └──────┬───────┘                  │
│                              │                           │
│                    ┌─────────┴──────────┐               │
│                    │                    │               │
│             ┌──────▼──────┐    ┌────────▼──────┐       │
│             │  SQL Server  │    │  FastAPI (CR) │       │
│             │  :1433       │    │  :8000        │       │
│             └─────────────┘    └───────┬───────┘       │
│                                        │               │
│                                ┌───────▼───────┐       │
│                                │  CR Trainer   │       │
│                                │  (watcher)    │       │
│                                └───────────────┘       │
└─────────────────────────────────────────────────────────┘
```

### Fluxo de dados

```
UI (Blazor) ──▶ API .NET ──▶ SQL Server
                    │
                    └──▶ FastAPI (IA) ──▶ Modelos KNN
```

A UI **nunca** fala directamente com o FastAPI nem com o banco. Toda a comunicação passa pela API .NET.

---

## Serviços / Containers

| Container | Tecnologia | Porta | Descrição |
|---|---|---|---|
| `target_frontend` | Blazor Server (.NET 10) | 5001 | Interface do utilizador |
| `target_backend` | ASP.NET Core 10 | 5000 | API REST principal |
| `targetcomexcr` | FastAPI (Python) | 8000 | API de recomendação por IA |
| `targetcomexcr_trainer` | Python | — | Watcher que re-treina os modelos quando há novos veículos |
| `target_comex_db` | SQL Server 2022 (Developer) | 1433 | Banco de dados principal |

---

## Base de Dados — SQL Server (`TargetComex`)

### Tabelas

#### `Usuarios`
| Coluna | Tipo | Notas |
|---|---|---|
| Id | INT IDENTITY | PK |
| Nome | VARCHAR(150) | |
| Email | VARCHAR(150) | UNIQUE |
| SenhaHash | VARCHAR(255) | BCrypt |
| Role | VARCHAR(20) | `cliente` \| `admin` \| `vendedor` |
| Telefone | VARCHAR(20) | |
| DataNascimento | DATE | |
| Genero | CHAR(1) | `M` \| `F` |
| EstadoCivil | VARCHAR(50) | |
| NumeroFilhos | INT | |
| Profissao | VARCHAR(100) | |
| FaixaRendaMensal | VARCHAR(50) | |
| InteressesPrincipais | NVARCHAR(500) | separado por vírgulas |
| TipoDeUsoPretendido | NVARCHAR(200) | separado por vírgulas |
| DataCadastro | DATETIME | |

#### `Enderecos`
| Coluna | Tipo | Notas |
|---|---|---|
| Id | INT IDENTITY | PK |
| UsuarioId | INT | FK → Usuarios |
| Provincia | VARCHAR(100) | |
| Municipio | VARCHAR(100) | |
| Distrito | VARCHAR(100) | |
| Bairro | VARCHAR(100) | |
| RuaComplemento | VARCHAR(255) | |
| DataAtualizacao | DATETIME | |

#### `Veiculos`
| Coluna | Tipo | Notas |
|---|---|---|
| Id | INT IDENTITY | PK |
| Marca | VARCHAR(50) | |
| Modelo | VARCHAR(100) | |
| Ano | INT | |
| Descricao | NVARCHAR(MAX) | |
| ImagemUrl | VARCHAR(500) | caminho relativo ex: `/images/hilux.jpg` |
| Cor | VARCHAR(50) | |
| Estilo | VARCHAR(50) | `Pick-up` \| `Hatchback` \| `SUV` \| `Sedan` |
| Combustivel | VARCHAR(30) | `Gasolina` \| `Diesel` \| `Híbrido` |
| Quilometragem | INT | |
| Preco | DECIMAL(18,2) | em Kwanzas (AOA) |
| Disponivel | BIT | default 1 |

#### `HistoricoNavegacao`
| Coluna | Tipo | Notas |
|---|---|---|
| Id | INT IDENTITY | PK |
| UsuarioId | INT | FK → Usuarios |
| VeiculoId | INT | FK → Veiculos |
| DataVisualizacao | DATETIME | |

#### `HistoricoCompras`
| Coluna | Tipo | Notas |
|---|---|---|
| Id | INT IDENTITY | PK |
| UsuarioId | INT | FK → Usuarios |
| VeiculoId | INT | FK → Veiculos |
| ValorPago | DECIMAL(18,2) | |
| DataCompra | DATETIME | |

#### `Avaliacoes`
| Coluna | Tipo | Notas |
|---|---|---|
| Id | INT IDENTITY | PK |
| VeiculoId | INT | FK → Veiculos |
| UsuarioId | INT | FK → Usuarios |
| Nota | INT | 1 a 5 |
| Comentario | NVARCHAR(MAX) | |
| DataAvaliacao | DATETIME | |

#### `FeaturesMultimodais`
| Coluna | Tipo | Notas |
|---|---|---|
| Id | INT IDENTITY | PK |
| VeiculoId | INT | FK → Veiculos |
| EmbeddingVisual | VARBINARY(MAX) | gerado pelo trainer |
| EmbeddingTextual | VARBINARY(MAX) | gerado pelo trainer |
| DataProcessamento | DATETIME | |

---

## API .NET — `Target.Api` (porta 5000)

### Autenticação
JWT Bearer Token. A chave está hardcoded em desenvolvimento:
```
TARGETCOMEX_SUPER_SECRET_KEY_1234567890
```
Token expira em **5 horas**.

### Endpoints

#### Auth
| Método | Rota | Auth | Descrição |
|---|---|---|---|
| POST | `/api/Auth/login` | ❌ | Login, devolve JWT |
| POST | `/api/Auth/register` | ❌ | Registo de novo utilizador |
| POST | `/api/Auth/registrar` | ❌ | Alias de register |

#### Users
| Método | Rota | Auth | Descrição |
|---|---|---|---|
| GET | `/api/Users` | ✅ | Lista todos os utilizadores |
| GET | `/api/Users/{id}` | ✅ | Obtém utilizador por id |
| PUT | `/api/Users/{id}` | ✅ | Actualiza utilizador |
| DELETE | `/api/Users/{id}` | ✅ | Remove utilizador |

#### Veículos
| Método | Rota | Auth | Descrição |
|---|---|---|---|
| GET | `/api/Veiculos` | ❌ | Lista todos os veículos |
| GET | `/api/Veiculos/{id}` | ❌ | Obtém veículo por id |
| POST | `/api/Veiculos` | ✅ | Cria veículo |
| PUT | `/api/Veiculos/{id}` | ✅ | Actualiza veículo |
| DELETE | `/api/Veiculos/{id}` | ✅ | Remove veículo |

#### Recomendação
| Método | Rota | Auth | Descrição |
|---|---|---|---|
| GET | `/api/Recomendacao/usuario/{id}` | ✅ | Recomendações personalizadas para o utilizador |

> O controller `.NET` faz proxy para o FastAPI em `http://targetcomexcr:8000`. A UI nunca fala directamente com o FastAPI.

#### SMS
| Método | Rota | Auth | Descrição |
|---|---|---|---|
| POST | `/api/sms/enviar` | ✅ | Envia SMS |

---

## API de IA — `targetcomexcr` FastAPI (porta 8000)

Serviço Python com modelos de recomendação KNN. Acedido **apenas pelo Target.Api**, nunca directamente pela UI.

- Lê dados do SQL Server directamente para treino
- Guarda modelos no volume Docker `models-data` em `/app/models`
- O container `targetcomexcr_trainer` corre em modo `--watch` e re-treina automaticamente quando detecta novos veículos

---

## Frontend — `Target.Ui` Blazor Server (porta 5001)

### Serviços registados (`Program.cs`)

| Serviço | Descrição |
|---|---|
| `AuthService` | Gere o JWT em `localStorage` / `sessionStorage` |
| `ApiAuthorizationMessageHandler` | Injecto o Bearer token em todos os pedidos HTTP |
| `ApiClient` | Cliente tipado para todos os endpoints da API .NET |
| `HttpClient` ("Api") | HttpClient configurado com base URL e o handler de auth |

### ApiClient — métodos disponíveis

```csharp
// Auth
await Api.LoginAsync(request)
await Api.RegisterAsync(request)

// Utilizadores
await Api.GetUsuariosAsync()
await Api.GetUsuarioAsync(id)
await Api.UpdateUsuarioAsync(id, request)
await Api.DeleteUsuarioAsync(id)

// Veículos
await Api.GetVeiculosAsync()
await Api.GetVeiculoAsync(id)
await Api.CreateVeiculoAsync(request)
await Api.UpdateVeiculoAsync(id, request)
await Api.DeleteVeiculoAsync(id)

// Recomendação
await Api.GetRecomendacoesAsync(usuarioId)

// SMS
await Api.EnviarSmsAsync(request)
```

Todos os métodos devolvem `ApiResult<T>`:
```csharp
var result = await Api.GetVeiculosAsync();
if (result.Success)
    // usar result.Data
else
    // mostrar result.Error
```

### AuthService — métodos disponíveis

```csharp
await Auth.GetTokenAsync()           // lê JWT do storage
await Auth.SetTokenAsync(token, remember) // guarda JWT
await Auth.RemoveTokenAsync()        // apaga JWT
await Auth.IsAuthenticatedAsync()    // bool
await Auth.GetBearerTokenAsync()     // JWT ou null
await Auth.LogoutAsync()             // apaga e redireciona para /login
```

---

## Roles de utilizador

| Role | Permissões |
|---|---|
| `cliente` | Ver veículos, receber recomendações, gerir o próprio perfil |
| `vendedor` | Criar e editar veículos |
| `admin` | Acesso total |

---

## Variáveis de Ambiente relevantes

| Serviço | Variável | Valor em dev |
|---|---|---|
| target_backend | `ConnectionStrings__DefaultConnection` | `Server=target_comex_db;Database=TargetComex;User Id=sa;Password=TargetComex2025!;...` |
| target_backend | `AIServiceUrl` | `http://targetcomexcr:8000` |
| target_frontend | `ApiServiceUrl` | `http://target_backend:8080` |
| target_comex_db | `MSSQL_SA_PASSWORD` | `TargetComex2025!` |

---

## Estrutura de Pastas

```
targetcomex/
├── docker-compose.yml
├── db-service/               # Dockerfile + init.sql + popular_bd.sql
├── cr-service/               # FastAPI + modelos KNN + train.py
├── target-api/
│   └── Target.Api/
│       ├── Controllers/
│       │   ├── Auth/AuthController.cs
│       │   ├── UsersController.cs
│       │   ├── VeiculosController.cs
│       │   ├── RecomendacaoController.cs
│       │   └── SmsController.cs
│       ├── Models/
│       │   ├── Usuario.cs
│       │   ├── Endereco.cs
│       │   ├── Veiculo.cs
│       │   ├── Avaliacao.cs
│       │   ├── LoginRequest.cs
│       │   ├── RegisterRequest.cs
│       │   └── SmsRequest.cs
│       ├── Data/
│       │   └── AppDbContext.cs
│       └── Program.cs
└── target-ui/
    └── Target.Ui/
        ├── Components/       # páginas e componentes Razor
        ├── Services/
        │   ├── ApiClient.cs
        │   ├── AuthService.cs
        │   └── ApiAuthorizationMessageHandler.cs
        └── Program.cs
```

---

## Comandos úteis

```bash
# Subir tudo
docker compose up -d

# Reconstruir um serviço específico
docker compose up --build target-api

# Reiniciar a API
docker compose restart target-api

# Ver logs em tempo real
docker compose logs -f target-api

# Aceder ao SQL Server
docker exec -it target_comex_db /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "TargetComex2025!" -No

# Ver colunas de uma tabela
docker exec -it target_comex_db /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "TargetComex2025!" -No \
  -Q "USE TargetComex; SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Usuarios';"
```

---

## Notas importantes para desenvolvimento

1. **Migrações**: O projecto não usa EF Core Migrations. O schema é gerido manualmente via `init.sql`. Qualquer alteração ao modelo C# requer um `ALTER TABLE` manual no banco.

2. **Senha do admin de teste**: `admin@target.ao` com hash BCrypt de `TargetComex2025!` (verificar no `popular_bd.sql`).

3. **Moeda**: Todos os preços estão em **Kwanzas (AOA)**.

4. **Recomendação**: O algoritmo é KNN. O trainer re-treina automaticamente. Se as recomendações estiverem a falhar, verificar os logs do container `targetcomexcr`.

5. **JWT hardcoded**: Em produção, mover a chave JWT para variável de ambiente.