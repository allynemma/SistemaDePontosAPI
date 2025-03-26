# SistemaDePontosAPI

Sistema de pontos inspirado no desafio do [Sistema de Ponto do Rafael Coelho](https://racoelho.com.br/listas/desafios/sistema-de-ponto).

## Descrição

O SistemaDePontosAPI é uma API desenvolvida em .NET 9 que permite gerenciar pontos de funcionários. A API oferece funcionalidades para registrar entradas e saídas, calcular horas trabalhadas e gerar relatórios.

## Funcionalidades

- Registro de entrada e saída de funcionários
- Cálculo de horas trabalhadas
- Geração de relatórios de ponto
- Autenticação e autorização de usuários

## Tecnologias Utilizadas

- ![.Net](https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white)
- ![MicrosoftSQLServer](https://img.shields.io/badge/Microsoft%20SQL%20Server-CC2927?style=for-the-badge&logo=microsoft%20sql%20server&logoColor=white)
- ![Swagger](https://img.shields.io/badge/-Swagger-%23Clojure?style=for-the-badge&logo=swagger&logoColor=white)

## Requisitos

- .NET 9 SDK
- SQL Server

## Como Executar

1. Clone o repositório:
   <br>
    `git clone https://github.com/allynemma/SistemaDePontosAPI.git`

2. Navegue até o diretório do projeto:
   <br>
   `cd SistemaDePontosAPI`

4.  Restaure as dependências:
    <br>
    `dotnet restore`

5. Atualize a string de conexão com o banco de dados no arquivo `appsettings.json`:
```"ConnectionStrings": {
  "DefaultConnection": "Server=SEU_SERVIDOR;Database=SEU_BANCO;User Id=SEU_USUARIO;Password=SUA_SENHA;"}
```


6. Atualize o banco de dados:
   <br>
   `dotnet ef database update`

7. Certifique-se que as dependencias da [mensageria](#Mensageria) está funcionando adequadamente.
   
8. Execute o projeto:
   <br>
   `dotnet run`

### Mensageria
1. Certifique-se de que você tenha o Apache Kafka 2.8.0 instalado (ou outra versão em que obtenha o Zookeeper)

2. Abra o cmd e navegue até a pasta de instalação do Kafka

3. Inicie o Zookeeper:
   `.\bin\windows\zookeeper-server-start.bat .\config\zookeeper.properties`

4. Deixe-o rodando e abra outro cmd e navegue também até a pasta de instalação.

5. Inicie o Kafka:
   `.\bin\windows\kafka-server-start.bat .\config\server.properties`

## Testes

Para executar os testes, siga os passos abaixo:

1. Clone o [repositório de testes](https://github.com/allynemma/SistemaDePontosTestes):
   <br>
   `git clone https://github.com/allynemma/SistemaDePontosTestes.git`

2. Navegue até o diretório de testes:
   <br>
   `cd SistemaDePontosTestes`

3. Restaure as dependências:
   <br>
`dotnet restore`

4. Execute os testes:
   <br>
`dotnet test`

## Contribuição

Contribuições são bem-vindas! Sinta-se à vontade para abrir issues e pull requests.

## Contato

Qualquer dúvida estou à disposição :)

- [LinkedIn](https://www.linkedin.com/in/allynealves/)
