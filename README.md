# URL Shortener

![Status](https://img.shields.io/badge/status-running-green)
![Tech](https://img.shields.io/badge/.NET-8.0-purple)
![Tests](https://img.shields.io/badge/tests-unit%20%2B%20integration-blue)
![License](https://img.shields.io/badge/license-MIT-lightgrey)

API REST para encurtamento de URLs, construída com foco em performance, robustez, testabilidade e boas práticas modernas do ecossistema .NET.

## Como rodar o projeto

- Usando Docker (recomendado)

```bash
git clone https://github.com/heitor-nb/url-shortener.git
cd url-shortener

cp .env.example .env
# Copy-Item -Path ".env.example" -Destination ".env" (PowerShell)

docker compose up -d

# A aplicação estará disponível em http://localhost:8080/
# A documentação gerada pelo Swagger poderá ser acessada pelo caminho /swagger/index.html
```

## Features Principais

- **Clean Architecture** (Domain, Application, Infra, API)
- Autenticação com **JWT + Refresh Tokens** (armazenados em cookies HTTP-only)
- **CQRS + padrão Mediator**
- **Repository Pattern**
- **PostgreSQL + Entity Framework Core**
- **Background Service + Channel** para salvar logs de acesso em segundo plano
- Otimização do fluxo de redirecionamento
- **Testes unitários e de integração**
  - xUnit
  - NSubstitute
  - WebApplicationFactory
  - Sqlite InMemory para integração
- Dockerfile + Docker Compose
- Deploy em **VM Oracle Cloud** com **Nginx** como proxy reverso

## Demonstração

*(gif aqui)*

## Motivação

Este projeto foi criado com o objetivo de aplicar e demonstrar:

- Arquitetura limpa aplicada em APIs reais
- Boas práticas de autenticação e segurança
- CQRS com o padrão Mediator
- Técnicas modernas de processamento assíncrono
- Cobertura de testes (unitários + integração)
- Otimização prática de performance em cenários de I/O

## Destaque Técnico: Background Service + Channel

A decisão técnica mais impactante do projeto foi desacoplar o processo de salvar logs de acesso.

Na implementação inicial, a entidade de log de acesso à URL era persistida antes do redirecionamento, o que estava impactando negativamente a performance do endpoint.

### Como funciona a solução:

1. O usuário acessa a URL curta  
2. A API localiza a URL original e **redireciona imediatamente**  
3. O log não é salvo sincronicamente  
4. A API envia um objeto de log para um **Channel**  
5. Um **BackgroundService** consome o Channel e realiza o insert no banco

Resultado: redirecionamento perceptivelmente mais rápido.

## Stack

**Backend:**  
- .NET 8  
- ASP.NET Core   
- NetDevPack SimpleMediator  
- EF Core 
- xUnit 
- NSubstitute

**Banco de Dados** 
- PostgreSQL  
- Sqlite InMemory (testes)

**Infra:**  
- Docker
- Nginx

## Licença

Este projeto está licenciado sob a MIT License.
