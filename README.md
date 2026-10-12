# Target Comex

<p align="center"><strong>Comércio automóvel com recomendações personalizadas por inteligência artificial.</strong></p>

<p align="center"><a href="https://github.com/JethWeber/targetcomex">Repositório</a> · <a href="#arquitectura">Arquitectura</a> · <a href="#instalação">Instalação</a> · <a href="#api-rest">API REST</a> · <a href="#motor-de-recomendação">Motor de IA</a></p>

---

## Índice

- [Visão geral](#visão-geral)
- [Objectivos e funcionalidades](#objectivos-e-funcionalidades)
- [Tecnologias](#tecnologias)
- [Arquitectura](#arquitectura)
- [Estrutura do repositório](#estrutura-do-repositório)
- [Pré-requisitos](#pré-requisitos)
- [Instalação](#instalação)
- [Serviços e portas](#serviços-e-portas)
- [Configuração](#configuração)
- [Base de dados](#base-de-dados)
- [API REST](#api-rest)
- [Motor de recomendação](#motor-de-recomendação)
- [Frontend Blazor](#frontend-blazor)
- [Autenticação e perfis](#autenticação-e-perfis)
- [Comandos úteis](#comandos-úteis)
- [Resolução de problemas](#resolução-de-problemas)
- [Segurança e produção](#segurança-e-produção)
- [Estado e próximos passos](#estado-e-próximos-passos)
- [Contribuir](#contribuir)
- [Licença](#licença)

## Visão geral

**Target Comex** é uma plataforma web de comércio automóvel orientada ao contexto angolano. Reúne um catálogo de veículos, gestão de contas, uma API de negócio e um serviço independente de recomendação baseado em Machine Learning, com o objectivo de ajudar os utilizadores a descobrir veículos relevantes para o seu perfil e preferências.

A solução combina aplicações .NET, um serviço Python/FastAPI, SQL Server e Docker Compose. A arquitectura separa a interface, a lógica de negócio, a persistência de dados e o processamento das recomendações.

> **Estado do projecto:** o repositório contém a base técnica destas componentes, mas a documentação de desenvolvimento assinala fluxos da interface e integrações que ainda precisam de validação. A existência de um endpoint não garante que toda a experiência esteja concluída ou pronta para produção.

## Objectivos e funcionalidades

- Catálogo de veículos com marca, modelo, ano, descrição, imagens, combustível, quilometragem e preço em kwanzas.
- Registo e autenticação de utilizadores.
- Operações CRUD de utilizadores e veículos através da API.
- Galeria de imagens por veículo.
- Histórico de navegação e compras para apoiar personalização.
- Recomendações híbridas que combinam sinais de perfil/interacção e características do conteúdo.
- Persistência relacional em SQL Server.
- Treino separado que detecta alterações no catálogo e gera embeddings.
- Endpoint de SMS existente na API; a configuração do fornecedor e o envio real devem ser verificados.

## Tecnologias

| Componente | Tecnologias identificadas |
|---|---|
| Interface | Blazor, ASP.NET Core, C# |
| API de negócio | ASP.NET Core Web API, C# |
| Persistência .NET | Entity Framework Core |
| Base de dados | Microsoft SQL Server |
| Autenticação | JWT Bearer |
| Hash de palavras-passe | BCrypt |
| API de IA | Python, FastAPI, Uvicorn |
| Recomendação | scikit-learn, KNN e embeddings |
| Embeddings textuais | DistilBERT / Transformers |
| Embeddings visuais | ResNet-18 / torchvision |
| Processamento numérico | NumPy |
| Acesso Python ao SQL Server | pyodbc e Microsoft ODBC Driver 18 |
| Infraestrutura | Docker e Docker Compose |

### Versões observadas

- Interface: alvo .NET 10 (net10.0).
- API principal: alvo .NET 8 (net8.0) no projecto actualmente versionado.
- Dockerfile da API: SDK .NET 8.
- Dockerfile da interface: SDK .NET 10.
- Serviço de IA: Python 3.10.
- Base de dados: SQL Server, edição Developer.

A API e a interface usam versões diferentes do .NET. Confirma compatibilidade e estratégia de actualização antes de publicar; não assumas que todo o projecto corre em .NET 10.

## Arquitectura

~~~text
                         Navegador
                             |
                             v
                  +----------------------+
                  | Target.Ui             |
                  | Blazor / ASP.NET Core |
                  | localhost:5001        |
                  +----------+-----------+
                             |
                             v
                  +----------------------+
                  | Target.Api            |
                  | ASP.NET Core Web API  |
                  | localhost:5000        |
                  +----------+-----------+
                             |
                  +----------+-----------+
                  |                      |
                  v                      v
        +------------------+   +-----------------------+
        | SQL Server       |   | Serviço de IA         |
        | TargetComex      |   | FastAPI / Python      |
        | porta 1433       |   | localhost:8000        |
        +------------------+   +-----------+-----------+
                                           ^
                                           |
                                +----------+-----------+
                                | Trainer Python       |
                                | train.py --watch     |
                                +----------------------+
~~~

- **Target.Ui** — interface web; comunica com a API .NET, não directamente com a base de dados ou com o serviço interno de IA.
- **Target.Api** — autenticação, operações de negócio, acesso aos dados e integração com o serviço de recomendação.
- **cr-service API** — calcula recomendações e disponibiliza-as por HTTP.
- **cr-service trainer** — calcula embeddings textuais/visuais e guarda os artefactos do modelo.
- **db-service** — prepara o SQL Server e contém scripts de criação e dados de demonstração.

### Fluxo de recomendação

1. A interface pede recomendações à API .NET.
2. A API valida a existência do utilizador e chama o FastAPI.
3. O serviço de IA usa dados de perfil, interacções e embeddings disponíveis.
4. A API .NET normaliza os resultados e filtra veículos associados a reservas concluídas.

## Estrutura do repositório

Estrutura lógica dos componentes principais; não é uma listagem exaustiva de todos os ficheiros.

~~~text
targetcomex/
├── docker-compose.yml
├── DEscription.md
├── EndpointsTargetAPI.md
├── flucoadd.md
├── db-service/
│   ├── Dockerfile
│   ├── entrypoint.sh
│   ├── init.sql
│   ├── popular_bd.sql
│   └── query01.sql
├── cr-service/
│   ├── Dockerfile
│   ├── requirements.txt
│   ├── main.py
│   └── train.py
├── target-api/
│   └── Target.Api/
│       ├── Controllers/
│       ├── Data/
│       ├── Models/
│       ├── Services/
│       ├── Program.cs
│       ├── Target.Api.csproj
│       └── Dockerfile
└── target-ui/
    └── Target.Ui/
        ├── Components/
        ├── Services/
        ├── Program.cs
        ├── Target.Ui.csproj
        └── Dockerfile
~~~

### Documentação adicional

- **[DEscription.md](DEscription.md)** — arquitectura, tabelas, serviços e comandos.
- **[EndpointsTargetAPI.md](EndpointsTargetAPI.md)** — endpoints e integrações da interface.
- **[flucoadd.md](flucoadd.md)** — notas de desenvolvimento e fluxos a concluir ou rever.

## Pré-requisitos

- Git.
- Docker Engine e Docker Compose v2.
- Espaço em disco para SQL Server, imagens Docker, dependências e modelos de IA.
- Internet durante o primeiro build para descarregar pacotes Python e modelos pré-treinados.
- Opcionalmente, SDK .NET 8 e SDK .NET 10 para executar os projectos fora de Docker.

O build de IA pode demorar devido ao PyTorch, torchvision e downloads dos modelos. A execução sem Internet depende de as dependências e os modelos já estarem disponíveis localmente.

## Instalação

### 1. Clonar o repositório

~~~bash
git clone https://github.com/JethWeber/targetcomex.git
cd targetcomex
~~~

### 2. Rever os segredos

Antes de arrancar, revê o Compose e a configuração da aplicação. O código actualmente versionado contém valores sensíveis de desenvolvimento, incluindo credenciais e chave JWT codificadas. Não reutilizes esses valores em produção. Move os segredos para variáveis de ambiente ou um gestor de segredos e roda qualquer segredo real que tenha sido publicado no histórico Git.

### 3. Construir e iniciar

~~~bash
docker compose config
docker compose up --build -d
~~~

### 4. Confirmar estado e consultar logs

~~~bash
docker compose ps
docker compose logs --tail=100 target_comex_db
docker compose logs --tail=100 target-api
docker compose logs --tail=100 target-ui
docker compose logs --tail=100 targetcomexcr
docker compose logs --tail=100 targetcomexcr-trainer
~~~

### 5. Endereços locais

| Serviço | Endereço |
|---|---|
| Interface Blazor | http://localhost:5001 |
| API .NET | http://localhost:5000 |
| FastAPI | http://localhost:8000 |
| Documentação FastAPI | http://localhost:8000/docs |
| SQL Server | localhost, porta 1433 |

A documentação Swagger da API .NET está configurada no código; o caminho e a disponibilidade dependem do ambiente e do Program.cs. Confirma os logs se não estiver acessível.

### 6. Parar os serviços

~~~bash
docker compose down
~~~

O comando docker compose down -v também elimina volumes persistentes, incluindo os dados da base de dados e embeddings. Usa-o apenas se pretenderes apagar esses dados.

## Serviços e portas

| Serviço Compose | Responsabilidade | Porta anfitrião | Porta container |
|---|---|---:|---:|
| target-ui | Interface web | 5001 | 8080 |
| target-api | API de negócio | 5000 | 8080 |
| targetcomexcr | API de recomendação | 8000 | 8000 |
| targetcomexcr-trainer | Treino/watch | — | — |
| target_comex_db | SQL Server | 1433 | 1433 |

Os serviços comunicam pela rede Docker usando os nomes dos serviços. URLs internas de container não devem ser usadas directamente pelo browser; o browser precisa de uma URL acessível a partir da máquina do utilizador.

## Configuração

Variáveis relevantes no Compose e no serviço de IA:

| Serviço | Variável | Finalidade |
|---|---|---|
| SQL Server | ACCEPT_EULA | Aceitação dos termos |
| SQL Server | MSSQL_SA_PASSWORD | Credencial administrativa do SQL Server |
| API .NET | ConnectionStrings__DefaultConnection | Ligação à base de dados |
| API .NET | AIServiceUrl | URL interno do FastAPI |
| Interface | ApiServiceUrl | URL interno da API .NET |
| Serviço IA | DB_SERVER, DB_PORT | Endereço do SQL Server |
| Serviço IA | DB_DATABASE, DB_USERNAME, DB_PASSWORD | Base de dados e credenciais |
| Serviço IA | MODELS_DIR | Directório dos artefactos do modelo |

Boas práticas:

1. Usa um ficheiro .env local, excluído do Git, ou um secret manager.
2. Lê configuração via IConfiguration no .NET e os.getenv no Python.
3. Não comites palavras-passe, tokens ou chaves criptográficas.
4. Se um segredo real já entrou no Git, considera-o comprometido e roda-o; apagar o valor num commit posterior não o remove do histórico.

## Base de dados

A base de dados chama-se **TargetComex** e é criada através de scripts SQL.

| Tabela | Finalidade |
|---|---|
| Usuarios | Contas, hash da palavra-passe, role e perfil |
| Enderecos | Endereços associados aos utilizadores |
| Veiculos | Dados principais do catálogo automóvel |
| VeiculoImagens | Galeria de imagens por veículo |
| HistoricoNavegacao | Visualizações de veículos |
| HistoricoCompras | Histórico de compras |
| Reservas | Pedidos/reservas e estado |
| Avaliacoes | Classificações e comentários |
| FeaturesMultimodais | Embeddings textuais/visuais produzidos pelo trainer |

### Scripts SQL

- **db-service/init.sql** — cria a base de dados e as tabelas.
- **db-service/popular_bd.sql** — insere dados de demonstração.
- **db-service/query01.sql** — consultas auxiliares para desenvolvimento.

As alterações ao esquema são geridas manualmente pelos scripts SQL; não assumas que as migrações EF Core actualizam automaticamente a base de dados. Revê os scripts e o processo de actualização ao alterar modelos ou tabelas. Os dados de demonstração são apenas para testes, nunca para ambientes públicos com dados reais.

## API REST

As rotas seguintes reflectem os controllers encontrados. Confirma sempre os atributos de autorização e os contratos no código antes de integrar um cliente externo.

### Autenticação — AuthController

| Método | Rota | Acesso | Finalidade |
|---|---|---|---|
| POST | /api/Auth/login | Público | Validar credenciais e devolver JWT |
| POST | /api/Auth/register | Público | Registar utilizador |
| POST | /api/Auth/registrar | Público | Alias de registo |

Exemplo de login:

~~~http
POST /api/Auth/login
Content-Type: application/json
~~~

~~~json
{
  "email": "utilizador@exemplo.ao",
  "senha": "substituir-por-uma-palavra-passe"
}
~~~

Quando o login é válido, a API devolve um JWT. Os endpoints protegidos recebem-no no cabeçalho Authorization: Bearer <token>.

### Utilizadores — UsersController

| Método | Rota | Finalidade |
|---|---|---|
| GET | /api/Users | Listar utilizadores |
| GET | /api/Users/{id} | Consultar utilizador |
| PUT | /api/Users/{id} | Actualizar utilizador |
| DELETE | /api/Users/{id} | Remover utilizador |

O controller exige autenticação. A autorização por função e o isolamento dos dados de cada utilizador devem ser revistos antes de publicação pública.

### Veículos — VeiculosController

| Método | Rota | Acesso declarado | Finalidade |
|---|---|---|---|
| GET | /api/Veiculos | Público | Listar veículos disponíveis |
| GET | /api/Veiculos/{id} | Público | Consultar veículo |
| POST | /api/Veiculos | Autenticado | Criar veículo |
| PUT | /api/Veiculos/{id} | Autenticado | Actualizar veículo |
| DELETE | /api/Veiculos/{id} | Autenticado | Remover veículo |

O controller inclui a galeria de imagens e exclui da listagem veículos associados a reservas concluídas. Operações de escrita devem ter autorização por role, não apenas exigir um token válido.

### Recomendação — RecomendacaoController

| Método | Rota | Acesso | Finalidade |
|---|---|---|---|
| GET | /api/Recomendacao/usuario/{id} | Autenticado | Obter recomendações |

A API verifica se o utilizador existe, chama o serviço de IA, normaliza identificador/score e filtra veículos associados a reservas concluídas.

### SMS — SmsController

| Método | Rota | Finalidade |
|---|---|---|
| POST | /api/sms/enviar | Endpoint de envio de SMS |

Confirma no controller os requisitos de autenticação e a integração com o fornecedor antes de assumir que o envio está operacional.

### FastAPI — recomendação

| Método | Rota | Finalidade |
|---|---|---|
| GET | /recommend-hybrid/{user_id} | Calcular recomendações híbridas |
| GET | /docs | Documentação interactiva |
| GET | /openapi.json | Esquema OpenAPI, se exposto |

O FastAPI deve ser tratado como serviço interno. Não exponhas a porta 8000 publicamente sem avaliar autenticação e controlo de acesso.

## Motor de recomendação

O serviço cr-service tem duas partes: a API que serve recomendações e o processo que calcula os artefactos dos modelos.

### Componentes de IA identificados

- **DistilBERT** — gera embeddings textuais a partir da informação do veículo.
- **ResNet-18** — extrai características de imagens.
- **Embeddings multimodais** — o trainer concatena representações textuais e visuais.
- **KNN / vizinhos mais próximos** — usado para encontrar perfis ou itens semelhantes.
- **Sinais de perfil e interacção** — o código considera dados demográficos, interesses, histórico de navegação e compras.
- **Fusão híbrida** — combina sinais colaborativos e de conteúdo; os pesos definidos no código precisam de validação com dados reais.

### Processo de treino

O cr-service/train.py:

1. Liga-se ao SQL Server.
2. Calcula um hash do catálogo para detectar alterações.
3. Carrega os modelos pré-treinados.
4. Processa texto e imagens dos veículos.
5. Gera embeddings visuais e textuais.
6. Persiste features em FeaturesMultimodais.
7. Guarda os embeddings num ficheiro pickle no directório de modelos.

O container trainer executa python train.py --watch e verifica periodicamente se o catálogo mudou. O volume models-data partilha artefactos entre o trainer e a API.

### Imagens e limitações

O código de treino procura imagens em /app/images. Confirma que os caminhos guardados em ImagemUrl correspondem a ficheiros acessíveis dentro do container. Se uma imagem falhar, o código pode produzir um vector visual de zeros, reduzindo a informação disponível.

- O primeiro treino pode consumir bastante CPU, memória e disco.
- Se ainda não houver embeddings, a API pode iniciar sem recomendações úteis.
- Confirma o mecanismo de recarga dos embeddings depois de novo treino.
- Não carregues ficheiros pickle de origem desconhecida.
- A qualidade depende do catálogo, imagens e volume de interacções reais.
- Valida a interpretação dos scores antes de os apresentar como percentagens de compatibilidade.

## Frontend Blazor

A interface está em target-ui/Target.Ui e é organizada em componentes Razor. Os serviços centrais incluem:

- **ApiClient** — agrupa chamadas HTTP e devolve resultados tipados.
- **AuthService** — gere leitura, armazenamento e remoção do JWT.
- **ApiAuthorizationMessageHandler** — adiciona o token Bearer aos pedidos autenticados.
- **TokenStore** — mantém o token disponível em memória para os pedidos que dele necessitam.

A documentação descreve páginas de login, registo, catálogo, detalhe do veículo, área de cliente e administração. Os documentos de integração também registam partes que podem usar dados estáticos ou precisar de ligação aos endpoints reais. Usa EndpointsTargetAPI.md e flucoadd.md como guias de verificação e testa os fluxos na aplicação.

Boas práticas: nunca guardar palavras-passe no browser; tratar erros 401/403; não confiar em IDs enviados pelo cliente para determinar a identidade do utilizador; evitar expor dados pessoais em páginas públicas; manter DTOs alinhados com as respostas da API; mostrar estados de carregamento, lista vazia e erro.

## Autenticação e perfis

O código prevê os perfis cliente, vendedor e admin. A nomenclatura e capitalização devem ser consistentes entre base de dados, claims JWT e regras de autorização.

- **Cliente** — navegação pelo catálogo e recomendações.
- **Vendedor** — perfil previsto para gestão de veículos, sujeito a autorização explícita.
- **Administrador** — perfil previsto para administração.

O registo usa BCrypt para guardar o hash da palavra-passe e o login emite um JWT com identificador, email e role. A configuração actual é de desenvolvimento e não deve ser considerada pronta para produção.

**Autenticação não é autorização:** ter um token válido não deve permitir operações administrativas nem acesso aos dados de outros clientes.

## Comandos úteis

### Docker Compose

~~~bash
docker compose config
docker compose up --build -d
docker compose ps
docker compose logs -f target-api
docker compose logs -f target-ui
docker compose logs -f targetcomexcr
docker compose logs -f targetcomexcr-trainer
docker compose restart target-api
docker compose down
~~~

### API .NET

Na pasta target-api/Target.Api, com o SDK adequado instalado:

~~~bash
dotnet restore
dotnet build
dotnet run
~~~

### Interface Blazor

Na pasta target-ui/Target.Ui:

~~~bash
dotnet restore
dotnet build
dotnet run
~~~

### Treino Python

No container ou num ambiente Python 3.10 preparado:

~~~bash
python train.py
python train.py --watch
~~~

Para execução fora de Docker, configura o ODBC Driver, a ligação SQL Server, os caminhos das imagens e o directório dos modelos.

## Resolução de problemas

| Sintoma | O que verificar |
|---|---|
| SQL Server não fica saudável | Logs, credenciais, espaço em disco e tempo de inicialização |
| API não liga à base de dados | Connection string, nome do serviço Docker e estado do SQL Server |
| UI não chama a API | ApiServiceUrl, portas e diferença entre URLs internas e públicas |
| FastAPI não inicia | Dependências Python, ODBC Driver 18, logs e ligação SQL |
| Recomendações vazias | Veículos, embeddings, perfil e histórico de interacções |
| Novos veículos não entram nas recomendações | Logs do trainer, resultado do treino e recarga dos embeddings |
| Imagens não são processadas | ImagemUrl, ficheiros e montagem de /app/images |
| Porta ocupada | Processo que usa a porta e mapeamentos no Compose |
| Mudanças no esquema não aparecem | Scripts SQL e estratégia de actualização da base persistida |

Diagnóstico inicial:

~~~bash
docker compose ps
docker compose logs --tail=150 target-api
docker compose logs --tail=150 targetcomexcr
docker compose logs --tail=150 targetcomexcr-trainer
~~~

## Segurança e produção

Antes de expor o sistema publicamente, revê esta lista:

- [ ] Remover credenciais e chaves criptográficas hardcoded do código e do Compose.
- [ ] Rodar segredos reais expostos no histórico Git.
- [ ] Usar HTTPS e rever as opções de validação JWT.
- [ ] Configurar chave JWT forte via ambiente/secret manager.
- [ ] Aplicar autorização por role às operações administrativas e de escrita.
- [ ] Garantir que cada utilizador só consulta/altera os próprios dados, salvo permissões explícitas.
- [ ] Rever CORS e evitar políticas permissivas sem justificação.
- [ ] Proteger o serviço interno de IA e limitar portas expostas.
- [ ] Validar e limitar dados recebidos pela API.
- [ ] Evitar registar tokens, palavras-passe ou dados pessoais desnecessários.
- [ ] Configurar backups e testar recuperação da base de dados.
- [ ] Definir retenção e acesso aos dados pessoais e ao histórico de navegação.
- [ ] Rever dependências e versões vulneráveis.
- [ ] Separar dados de demonstração dos dados reais.
- [ ] Criar testes unitários, de integração e de autorização.

A aplicação lida com dados de perfil e preferências. Recolhe apenas o necessário, explica a finalidade aos utilizadores e restringe o acesso.

## Estado e próximos passos

A base técnica inclui a arquitectura distribuída, o esquema SQL, a API de negócio, o serviço FastAPI e a geração de embeddings. Os documentos existentes identificam áreas de integração que devem ser confirmadas.

Prioridades recomendadas:

1. **Segurança e configuração:** retirar segredos do código e centralizar configuração.
2. **Contratos UI/API:** alinhar DTOs e modelos, incluindo campos que possam existir apenas na interface.
3. **Testes ponta a ponta:** validar registo, login, catálogo, detalhe, área de cliente e administração.
4. **Autorização:** definir regras por role e validar a propriedade dos recursos.
5. **Interacções:** confirmar persistência de reservas, avaliações e histórico usados pela recomendação.
6. **Ciclo de vida da IA:** versionar artefactos, formalizar treino e garantir recarga após actualizações.
7. **Testes automatizados:** adicionar testes unitários, integração e contrato.
8. **Operação:** health checks, logging estruturado, métricas e backups.
9. **Produção:** HTTPS, domínio, deployment, monitorização e recuperação.

Esta lista é uma orientação baseada no código e na documentação consultados, não uma afirmação de que todos os itens estão ausentes. Confirma o estado actual antes de abrir tarefas ou marcar funcionalidades como concluídas.

## Contribuir

1. Cria uma branch para a alteração.
2. Mantém as mudanças focadas e documenta decisões importantes.
3. Não incluas segredos, dados reais de clientes ou modelos grandes sem necessidade.
4. Executa build e testes aplicáveis.
5. Actualiza a documentação quando alterares endpoints, variáveis, tabelas ou serviços.
6. Abre um Pull Request com objectivo, alterações, forma de testar e limitações conhecidas.

~~~bash
git switch -c feat/minha-melhoria
git add .
git commit -m "docs: actualizar documentação"
git push -u origin feat/minha-melhoria
~~~

## Licença

Não foi possível confirmar uma licença de distribuição explícita nos ficheiros consultados. Até existir um ficheiro LICENSE com os termos escolhidos pelo autor, não assumas que o código pode ser reutilizado, redistribuído ou explorado comercialmente sem autorização.

---

<p align="center"><strong>Target Comex</strong><br/>Tecnologia, dados e recomendações inteligentes para o comércio automóvel.</p>
<p align="center">Desenvolvido por <a href="https://github.com/JethWeber">JethWeber</a></p>
