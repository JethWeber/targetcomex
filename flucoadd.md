
---

## ① Fluxo: Login

**O que falta:** `Login.razor` tem os campos e o botão mas o `@onclick` não chama nada — o `@code` termina sem lógica de submissão.

**Ficheiros a alterar:**
- `Login.razor` — adicionar `@inject HttpClient Http`, `@inject NavigationManager Nav`, chamar `POST /api/auth/login` com `{ email, senhaHash }`, guardar o token JWT (em `localStorage` via JS interop ou `ProtectedSessionStorage`).
- `Program.cs` (UI) — registar `HttpClient` apontando para a URL da API.

**Ligação UI → API:**
```
Login.razor → POST /api/auth/login → { token } → guardar token → Nav.NavigateTo("/")
```

O campo actual usa `telefone` como identificador, mas a API espera `email`. Tens de alinhar: ou a API aceita telefone ou o campo muda para email.

---

## ② Fluxo: Registar

**O que falta:** `Registrar.razor` tem o método `Submeter()` com um `TODO` e um `Task.Delay` simulado. Precisa de montar o DTO e chamar a API.

**Ficheiros a alterar:**
- `Registrar.razor` — substituir o bloco `Submeter()` por um `PostAsJsonAsync` para `POST /api/auth/register`, construindo o objecto `Usuario` a partir dos 5 steps.

**Mapeamento dos steps para o modelo `Usuario`:**

| Step | Campo Blazor | Campo API |
|------|-------------|-----------|
| 1 | `Conta.NomeCompleto` | `Nome` |
| 1 | `Conta.Telefone` | `Email` (ou campo a adicionar) |
| 1 | `Conta.Senha` | `SenhaHash` (enviado em texto, API faz hash) |
| 2 | `Perfil.DataNascimento` | `DataNascimento` |
| 2 | `Perfil.Genero` | `Genero` |
| 2 | `Perfil.EstadoCivil` | `EstadoCivil` |
| 3 | `Economico.Profissao` | `Profissao` |
| 3 | `Economico.FaixaRendaMensal` | `FaixaRendaMensal` |
| 4 | `Preferencias.TiposUso` | `TipoDeUsoPretendido` (join com vírgula) |
| 4 | `Preferencias.Interesses` | `InteressesPrincipais` (join com vírgula) |

O `Role` deve ser enviado como `"Cliente"` por defeito.

---

## ③ Fluxo: Catálogo de Veículos

**O que falta:** `Produto.razor` tem os dados hardcoded numa lista local. Precisa de chamar a API real.

**Ficheiros a alterar:**
- `Produto.razor` — substituir a lista `List<Veiculo> veiculos` por um `GET /api/veiculos` com `HttpClient` + Bearer token. O modelo `Veiculo` do Blazor (record com `ScoreIA`, `Motor`, `Potencia`, `Localizacao`) precisa de ser alinhado com o modelo da API (que não tem esses campos ainda).

**Duas opções:**
1. Adicionar os campos em falta ao modelo `Veiculo` da API e à tabela SQL.
2. Criar um DTO de resposta enriquecido que a API calcula (score IA vem do serviço de recomendação).

A filtragem (combustível, marca, preço, ano) pode continuar a ser feita no lado Blazor depois do fetch — não é necessário passar query params à API nesta fase.

---

## ④ Fluxo: Detalhe do Veículo + Avaliações

**O que falta:** `DetalheVeiculo.razor` também tem dados hardcoded. Os comentários são locais em memória — precisam de persistência.

**Ficheiros a criar na API:**
- `Controllers/AvaliacoesController.cs` com:
  - `GET /api/avaliacoes/veiculo/{id}` — lista avaliações de um veículo.
  - `POST /api/avaliacoes` — cria avaliação (body: `{ veiculoId, usuarioId, nota, comentario }`).

O modelo `Avaliacao` já existe. Basta adicionar o controller e registar o `DbSet` (já está em `AppDbContext`).

**Ligação:**
- `DetalheVeiculo.razor` chama `GET /api/veiculos/{VeiculoId}` no `OnParametersSetAsync`.
- O método `SubmeterComentario()` chama `POST /api/avaliacoes`.
- A lista de comentários vem de `GET /api/avaliacoes/veiculo/{VeiculoId}`.

---

## ⑤ Fluxo: Admin Dashboard

**O que falta:** `AdminDashboard.razor` tem CRUD de carros e utilizadores em memória. Precisa de ligar tudo à API real. As Reservas não existem na API.

**Ficheiros a criar na API:**
- `Models/Reserva.cs` — modelo com `Id, UsuarioId, VeiculoId, DataPedido, Estado, Preco`.
- Migration de base de dados para a tabela `Reservas`.
- `Controllers/ReservasController.cs`:
  - `GET /api/reservas` — lista todas (admin).
  - `POST /api/reservas` — cria reserva.
  - `PUT /api/reservas/{id}/estado` — muda estado (Pendente → Confirmada, etc.).

**Protecção:** Os endpoints de admin devem exigir `[Authorize(Roles = "Admin")]`. A `UsersController` já tem `[Authorize]` mas não filtra por role — adicionar.

**Ligação na UI:**
- `AdminDashboard.razor` chama `GET /api/veiculos` no load, `POST/PUT/DELETE /api/veiculos` nos botões do modal, `GET/DELETE /api/users` na aba utilizadores, `GET/PUT /api/reservas` na aba reservas.

---

## ⑥ Fluxo: Área do Cliente

**O que falta:** `ClienteArea.razor` usa um objecto `cliente` hardcoded. Precisa de ler o perfil real do utilizador autenticado e listar as suas reservas.

**Endpoint a adicionar na API:**
- `GET /api/users/me` — extrai o `userId` do JWT (claim `NameIdentifier`) e devolve o perfil do utilizador actual. Evita expor todos os users ao cliente.

**Ligação:**
- No `OnInitializedAsync` do `ClienteArea.razor`: chamar `GET /api/users/me` com o Bearer token → preencher `cliente`.
- A aba "Meus Pedidos" chama `GET /api/reservas?usuarioId={id}` (adicionar filtro ao endpoint).
- O botão "Salvar Alterações" chama `PUT /api/users/{id}`.
- O botão "Cancelar" chama `PUT /api/reservas/{id}/estado` com `{ estado: "Cancelado" }`.

---

## ⑦ Fluxo: Recomendação IA

**O que já existe:** `RecomendacaoController` e `RecommendationService` estão implementados e chamam o serviço Python em `recommend-hybrid/{userId}`.

**O que falta na UI:** nenhuma página chama este endpoint. Precisa de ser integrado em:
- `Produto.razor` — após o load dos veículos, chamar `GET /api/recomendacao/usuario/{userId}` e usar os IDs retornados para ordenar ou destacar cards com o score IA.
- `ClienteArea.razor` — o painel "Sugestão da IA" deve usar a resposta real em vez do texto hardcoded.

---

## Infraestrutura comum (UI)

Há duas coisas que precisam de ser criadas no projecto Blazor antes de qualquer fluxo funcionar:

**`Services/AuthService.cs`** — responsável por guardar e ler o JWT:
```csharp
// guarda token após login
// expõe GetToken() e GetUserId()
// injecta o Bearer header no HttpClient
```

**`Program.cs` (UI)** — registar o `HttpClient` com a base URL da API e um `DelegatingHandler` que adiciona o token automaticamente a cada pedido.

Sem isto, cada página teria de gerir o token individualmente — o que é frágil e repetitivo.

---

Em resumo, o agente precisa de actuar em três frentes em paralelo: criar os endpoints em falta na API (Avaliações, Reservas, `/me`), substituir todos os dados hardcoded nas páginas Blazor por chamadas HTTP reais, e criar a camada de autenticação partilhada no lado UI. Queres que comece por um fluxo específico com o código completo?