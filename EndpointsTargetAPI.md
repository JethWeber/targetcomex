# Target Comex — Endpoints para Alimentar a UI

> Mapeamento completo entre cada página/componente da UI e os endpoints necessários da API.

---

## Sumário rápido

| Página | Endpoints necessários | Estado |
|---|---|---|
| `/login` | `POST /api/Auth/login` | ✅ Existe |
| `/registrar` | `POST /api/Auth/register` | ✅ Existe |
| `/produtos` | `GET /api/Veiculos` | ✅ Existe |
| `/detalhes-veiculo/{id}` | `GET /api/Veiculos/{id}` · `POST /api/Avaliacoes` · `GET /api/Recomendacao/usuario/{id}` | ⚠️ Parcial |
| `/area-cliente` | `GET /api/Users/{id}` · `PUT /api/Users/{id}` · `GET /api/HistoricoCompras/{userId}` | ⚠️ Parcial |
| `/contacto` | `POST /api/sms/enviar` | ✅ Existe |
| `/admin` | `GET/POST/PUT/DELETE /api/Veiculos` · `GET/PUT/DELETE /api/Users` | ✅ Existe |
| `/sobre` · `/servicos` | Nenhum (conteúdo estático) | ✅ OK |

---

## 1. `/login`

**Ficheiro:** `Login.razor`

### Endpoint usado

```
POST /api/Auth/login
```

**Body enviado:**
```json
{
  "email": "string",
  "senha": "string"
}
```

**Resposta esperada:**
```json
{
  "token": "eyJhbGci..."
}
```

**Observação:** A UI guarda o token via `AuthService.SetTokenAsync(token, relembrar)`. Está a funcionar correctamente.

---

## 2. `/registrar`

**Ficheiro:** `Registrar.razor`

### Endpoint usado

```
POST /api/Auth/register
```

**Body enviado (construído no Submeter()):**
```json
{
  "nome": "string",
  "email": "string",
  "senha": "string",
  "telefone": "string",
  "dataNascimento": "2000-01-01T00:00:00",
  "genero": "M",
  "estadoCivil": "Solteiro(a)",
  "numeroFilhos": 0,
  "profissao": "string",
  "faixaRendaMensal": "string",
  "tiposUso": ["Uso pessoal", "Lazer / aventura"],
  "interessesPrincipais": ["Conforto", "Potência"],
  "provincia": "Luanda",
  "municipio": "Viana",
  "bairro": "string",
  "ruaComplemento": "string"
}
```

**Resposta esperada:**
```json
{
  "message": "Usuário criado com sucesso"
}
```

**Observação:** Após registo bem-sucedido, redireciona para `/login`.

---

## 3. `/produtos`

**Ficheiro:** `Produtos.razor`

### Endpoint necessário

```
GET /api/Veiculos
```

**Resposta esperada (array):**
```json
[
  {
    "id": 1,
    "marca": "Toyota",
    "modelo": "Corolla Cross",
    "ano": 2024,
    "combustivel": "Híbrido",
    "transmissao": "Automática",
    "motor": "2.0L Híbrido",
    "potencia": 196,
    "preco": 195000000,
    "quilometragem": 0,
    "imagemUrl": "/images/corolla.jpg",
    "disponivel": true
  }
]
```

### ⚠️ Problema actual

A página **usa dados hardcoded** (`List<Veiculo> veiculos = new() { ... }`). É necessário substituir por uma chamada real à API via `ApiClient`:

```csharp
// Substituir o OnInitialized() actual por:
protected override async Task OnInitializedAsync()
{
    var result = await Api.GetVeiculosAsync();
    if (result.Success)
        veiculos = result.Data; // mapear para o record local
    AplicarFiltros();
}
```

### Campos em falta no modelo `Veiculo` da API vs UI

| Campo na UI | Campo na BD | Estado |
|---|---|---|
| `Nome` (Marca + Modelo) | `Marca` + `Modelo` separados | ⚠️ Concatenar |
| `Motor` | ❌ Não existe na tabela `Veiculos` | ❌ **Falta na BD** |
| `Potencia` | ❌ Não existe na tabela `Veiculos` | ❌ **Falta na BD** |
| `Transmissao` | ❌ Não existe na tabela `Veiculos` | ❌ **Falta na BD** |
| `ScoreIA` | Calculado pelo FastAPI | ⚠️ Vem da Recomendação |
| `Localizacao` | ❌ Não existe na tabela `Veiculos` | ❌ **Falta na BD** |
| `Imagem` | `ImagemUrl` | ✅ Renomear |
| `Preco` | `Preco` (DECIMAL) | ✅ OK |
| `Combustivel` | `Combustivel` | ✅ OK |
| `Ano` | `Ano` | ✅ OK |

### ALTER TABLE necessário

```sql
ALTER TABLE Veiculos ADD Motor VARCHAR(100) NULL;
ALTER TABLE Veiculos ADD Potencia INT NULL;
ALTER TABLE Veiculos ADD Transmissao VARCHAR(30) NULL;
ALTER TABLE Veiculos ADD Localizacao VARCHAR(100) NULL;
```

---

## 4. `/detalhes-veiculo/{id}`

**Ficheiro:** `DetalhesVeiculo.razor`

### Endpoints necessários

#### 4.1 — Carregar o veículo

```
GET /api/Veiculos/{id}
```

**Resposta esperada:**
```json
{
  "id": 1,
  "marca": "BMW",
  "modelo": "M4 Competition",
  "ano": 2023,
  "combustivel": "Gasolina",
  "transmissao": "Automática",
  "motor": "3.0L Twin-Turbo",
  "potencia": 510,
  "preco": 420000000,
  "imagemUrl": "/images/bmw-m4.jpg",
  "descricao": "string"
}
```

#### 4.2 — Score IA (match do utilizador)

```
GET /api/Recomendacao/usuario/{usuarioId}
```

**Observação:** A UI mostra `ScoreIA%` por veículo. O endpoint de recomendação devolve uma lista de veículos recomendados com score — verificar o contrato de resposta do FastAPI.

#### 4.3 — Submeter avaliação/comentário

```
POST /api/Avaliacoes
```

**⚠️ Este endpoint NÃO EXISTE ainda na API .NET.**

**Body a enviar:**
```json
{
  "veiculoId": 1,
  "usuarioId": 3,
  "nota": 5,
  "comentario": "Excelente veículo!"
}
```

**Controller a criar:**

```csharp
[HttpPost]
[Authorize]
public IActionResult CriarAvaliacao([FromBody] AvaliacaoRequest request)
{
    var avaliacao = new Avaliacao
    {
        VeiculoId = request.VeiculoId,
        UsuarioId = request.UsuarioId,
        Nota = request.Nota,
        Comentario = request.Comentario,
        DataAvaliacao = DateTime.UtcNow
    };
    _context.Avaliacoes.Add(avaliacao);
    _context.SaveChanges();
    return Ok(avaliacao);
}
```

#### 4.4 — Listar avaliações do veículo

```
GET /api/Avaliacoes/veiculo/{veiculoId}
```

**⚠️ Este endpoint NÃO EXISTE ainda na API .NET.**

**Resposta esperada:**
```json
[
  {
    "id": 1,
    "veiculoId": 1,
    "usuarioId": 3,
    "nota": 5,
    "comentario": "Excelente veículo!",
    "dataAvaliacao": "2025-06-01T10:30:00"
  }
]
```

#### 4.5 — Registar visita (HistoricoNavegacao)

```
POST /api/HistoricoNavegacao
```

**⚠️ Este endpoint NÃO EXISTE ainda na API .NET.**

**Body a enviar (chamado no `OnParametersSetAsync`):**
```json
{
  "usuarioId": 3,
  "veiculoId": 1
}
```

**Importância:** Alimenta o modelo KNN do FastAPI. Sem isto, as recomendações não evoluem.

---

## 5. `/area-cliente`

**Ficheiro:** `AreaCliente.razor`

### Endpoints necessários

#### 5.1 — Carregar dados do utilizador autenticado

```
GET /api/Users/{id}
```

**Resposta esperada:**
```json
{
  "id": 3,
  "nome": "Aníbal Manuel",
  "email": "anibal@email.com",
  "telefone": "+244 923 456 789",
  "role": "cliente",
  "dataNascimento": "1990-05-15",
  "genero": "M",
  "estadoCivil": "Solteiro(a)",
  "profissao": "Engenheiro",
  "faixaRendaMensal": "300.000 – 600.000 AOA",
  "interessesPrincipais": "Conforto,Potência",
  "tipoDeUsoPretendido": "Uso pessoal,Lazer / aventura",
  "endereco": {
    "provincia": "Luanda",
    "municipio": "Talatona",
    "bairro": "Talatona",
    "ruaComplemento": "Rua 14"
  }
}
```

**Observação:** O `id` do utilizador vem do JWT — extrair com `AuthService.GetBearerTokenAsync()` e descodificar o claim `ClaimTypes.NameIdentifier`.

#### 5.2 — Actualizar dados do perfil

```
PUT /api/Users/{id}
```

**Body a enviar:**
```json
{
  "nome": "Aníbal Manuel",
  "telefone": "+244 923 456 789",
  "email": "anibal@email.com",
  "provincia": "Luanda",
  "municipio": "Talatona"
}
```

#### 5.3 — Histórico de compras (aba "Meus Pedidos")

```
GET /api/HistoricoCompras/usuario/{usuarioId}
```

**⚠️ Este endpoint NÃO EXISTE ainda na API .NET.**

**Resposta esperada:**
```json
[
  {
    "id": 1,
    "veiculoId": 2,
    "veiculo": {
      "marca": "BAIC",
      "modelo": "X55 II",
      "imagemUrl": "/images/baic-x55.jpg"
    },
    "valorPago": 22000000,
    "dataCompra": "2025-01-15T10:00:00"
  }
]
```

#### 5.4 — Recomendações da IA (painel geral)

```
GET /api/Recomendacao/usuario/{usuarioId}
```

Usado para preencher a sugestão da IA no painel geral da área cliente.

---

## 6. `/contacto`

**Ficheiro:** `Contacto.razor`

### Endpoint usado

```
POST /api/sms/enviar
```

**Body enviado:**
```json
{
  "to": "+244959288888",
  "message": "[Target Comex]\nNome: João\nTel: 923000000\nAssunto: Venda de Viatura\nShowroom: Morro Bento\nMensagem: Olá, quero informações."
}
```

**Observação:** Já está implementado. Verificar se o `SmsController` tem autenticação — se sim, a UI precisa de enviar o Bearer token.

---

## 7. `/admin`

**Ficheiro:** `Admin.razor`

### Endpoints usados (todos hardcoded actualmente — precisam de integração)

#### 7.1 — Listar veículos

```
GET /api/Veiculos
```

#### 7.2 — Criar veículo

```
POST /api/Veiculos
```

**Body:**
```json
{
  "marca": "Toyota",
  "modelo": "Corolla",
  "ano": 2024,
  "preco": 195000000,
  "combustivel": "Gasolina",
  "transmissao": "Automática",
  "motor": "2.0L",
  "potencia": 170,
  "quilometragem": 0,
  "disponivel": true
}
```

#### 7.3 — Editar veículo

```
PUT /api/Veiculos/{id}
```

#### 7.4 — Eliminar veículo

```
DELETE /api/Veiculos/{id}
```

#### 7.5 — Listar utilizadores

```
GET /api/Users
```

#### 7.6 — Eliminar utilizador

```
DELETE /api/Users/{id}
```

#### 7.7 — Gerir reservas

**⚠️ Não existe tabela nem endpoints de Reservas na API actual.**

A UI de admin tem uma aba completa de "RESERVAS" com estados (Pendente, Confirmada, Concluída, Cancelada). É necessário:

**Criar tabela:**
```sql
CREATE TABLE Reservas (
    Id          INT IDENTITY PRIMARY KEY,
    UsuarioId   INT NOT NULL FOREIGN KEY REFERENCES Usuarios(Id),
    VeiculoId   INT NOT NULL FOREIGN KEY REFERENCES Veiculos(Id),
    TipoPedido  VARCHAR(50),   -- 'Compra' | 'Test Drive'
    Showroom    VARCHAR(100),
    Estado      VARCHAR(30),   -- 'Pendente' | 'Confirmada' | 'Concluída' | 'Cancelada'
    DataReserva DATETIME DEFAULT GETDATE()
);
```

**Endpoints a criar:**
```
GET    /api/Reservas
GET    /api/Reservas/usuario/{usuarioId}
POST   /api/Reservas
PUT    /api/Reservas/{id}/estado
DELETE /api/Reservas/{id}
```

---

## 8. Endpoints em falta — Resumo de implementação

### 8.1 — Endpoints a criar na API .NET

| Endpoint | Método | Descrição | Urgência |
|---|---|---|---|
| `/api/Avaliacoes` | POST | Criar avaliação/comentário | 🔴 Alta |
| `/api/Avaliacoes/veiculo/{id}` | GET | Listar avaliações de um veículo | 🔴 Alta |
| `/api/HistoricoNavegacao` | POST | Registar visita a veículo | 🔴 Alta |
| `/api/HistoricoCompras/usuario/{id}` | GET | Histórico de compras do utilizador | 🔴 Alta |
| `/api/Reservas` | GET | Listar todas as reservas (admin) | 🟡 Média |
| `/api/Reservas/usuario/{id}` | GET | Reservas do utilizador | 🟡 Média |
| `/api/Reservas` | POST | Criar reserva | 🟡 Média |
| `/api/Reservas/{id}/estado` | PUT | Actualizar estado da reserva | 🟡 Média |

### 8.2 — Campos a adicionar à tabela `Veiculos`

```sql
ALTER TABLE Veiculos ADD Motor       VARCHAR(100) NULL;
ALTER TABLE Veiculos ADD Potencia    INT          NULL;
ALTER TABLE Veiculos ADD Transmissao VARCHAR(30)  NULL;
ALTER TABLE Veiculos ADD Localizacao VARCHAR(100) NULL;
```

### 8.3 — Modelo C# `Veiculo.cs` a actualizar

```csharp
public class Veiculo
{
    public int    Id           { get; set; }
    public string Marca        { get; set; }
    public string Modelo       { get; set; }
    public int    Ano          { get; set; }
    public string? Descricao   { get; set; }
    public string? ImagemUrl   { get; set; }
    public string? Cor         { get; set; }
    public string? Estilo      { get; set; }
    public string? Combustivel { get; set; }
    public int?   Quilometragem{ get; set; }
    public decimal Preco       { get; set; }
    public bool   Disponivel   { get; set; }

    // Novos campos:
    public string? Motor       { get; set; }
    public int?   Potencia     { get; set; }
    public string? Transmissao { get; set; }
    public string? Localizacao { get; set; }
}
```

### 8.4 — Modelos a criar

```csharp
// AvaliacaoRequest.cs
public class AvaliacaoRequest
{
    public int    VeiculoId  { get; set; }
    public int    UsuarioId  { get; set; }
    public int    Nota       { get; set; } // 1 a 5
    public string? Comentario { get; set; }
}

// HistoricoNavegacaoRequest.cs
public class HistoricoNavegacaoRequest
{
    public int UsuarioId { get; set; }
    public int VeiculoId { get; set; }
}

// ReservaRequest.cs
public class ReservaRequest
{
    public int    UsuarioId  { get; set; }
    public int    VeiculoId  { get; set; }
    public string TipoPedido { get; set; } // 'Compra' | 'Test Drive'
    public string? Showroom  { get; set; }
}
```

---

## 9. Fluxo de autenticação na UI

A UI usa `AuthService` para gerir o JWT. Para chamar endpoints protegidos, o `ApiAuthorizationMessageHandler` injeta automaticamente o Bearer token em todos os pedidos do `HttpClient("Api")`.

**Para obter o `usuarioId` do token JWT no frontend:**

```csharp
// Em qualquer componente que precise do ID do utilizador autenticado:
var token = await AuthService.GetBearerTokenAsync();
// Descodificar o claim NameIdentifier do JWT
// (ou criar um método GetUserIdAsync() no AuthService)
```

**Sugestão — adicionar ao `AuthService.cs`:**

```csharp
public async Task<int?> GetUserIdAsync()
{
    var token = await GetBearerTokenAsync();
    if (token == null) return null;

    var handler = new JwtSecurityTokenHandler();
    var jwt = handler.ReadJwtToken(token);
    var idClaim = jwt.Claims
        .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

    return idClaim != null ? int.Parse(idClaim.Value) : null;
}
```

---

## 10. ApiClient — métodos a adicionar

```csharp
// Avaliacoes
await Api.GetAvaliacoesVeiculoAsync(veiculoId)
await Api.CriarAvaliacaoAsync(request)

// Historico
await Api.RegistarNavegacaoAsync(request)
await Api.GetHistoricoComprasAsync(usuarioId)

// Reservas
await Api.GetReservasAsync()
await Api.GetReservasUsuarioAsync(usuarioId)
await Api.CriarReservaAsync(request)
await Api.AtualizarEstadoReservaAsync(id, estado)
```

---

*Documento gerado com base na análise das páginas: Home, Login, Registar, Produtos, Detalhes Veículo, Área Cliente, Contacto, Serviços, Sobre, Admin, NavMenu e Footer.*