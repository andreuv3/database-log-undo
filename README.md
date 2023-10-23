# Mecanismo de log undo com checkpoints utilizando PostgreSQL

O objetivo deste projeto é implementar um mecanismo de log undo com checkpoints utilizando um SGBD, neste caso o PostgreSQL.

## Funcionalidades

O projeto é capaz de ler um [arquivo de log](src/entradaLog) e um [arquivo de metadados](src/metadado.json) e validar as informações no banco de dados através do mecanismo undo.

### Detalhes da implementação

O programa executa os seguintes passos:

1. Remove a tabela do banco de dados, caso exista;
2. Cria a tabela no banco dados baseado na definição do arquivo de metadados;
3. Insere as tuplas na tabela baseado no conteúdo do arquivo de metadados;
4. Faz um parse do arquivo de log;
5. Executa o mecanismo undo propriamente dito;
    - Verifica quais transações devem realizar undo e exibe seus ids;
    - Checa quais valores salvos na tabela estão inconsistentes e realiza a atualização.
6. Exibe os dados contidos na tabela após a aplicação do mecanismo undo;

## Arquivo de log

O arquivo de log deve conter todas as operações que foram executadas no banco de dados. Estas operações devem estar nos seguintes formatos:

Transação iniciada
```
<start ID_DA_TRANSACAO>
```

Transação commitada
```
<commit ID_DA_TRANSACAO>
```

Checkpoint iniciado
```
<START CKPT(IDS_DAS_TRANSACOES_EM_ABERTO)>
```

Observação: os ids devem ser separados por vírgulas.

Checkpoint finalizado
```
<END CKPT>
```

Operação
```
<ID_DA_TRANSACAO,ID_DA_TUPLA,COLUNA,VALOR_ANTIGO>
```

Um exemplo de arquivo de log:

```
<start T1>
<T1,1, A,20>
<start T2>
<commit T1>
<START CKPT(T2)>
<T2,2, B,20>
<commit T2>
<END CKPT>
<start T3>
<start T4>
<T4,1, B,55>
```

> O arquivo de log está localizado [aqui](src/entradaLog).

## Arquivo de metadados

O arquivo de metadados deve conter o nome da tabela, seguindo do nome das colunas e o valor (número inteiro) de cada coluna para cada tupla. Mais precisamente, deve obedecer o seguinte formato:

```
{
    "NOME_DA_TABELA": {
        "id":[1, 2, ..., n],
        "NOME_DA_COLUNA_1": [20, 20, ..., n],
        "NOME_DA_COLUNA_1": [55, 30, ..., n]
    }
}
```

Um exemplo do arquivo de metadados:

```
{
    "test": {
        "id":[1, 2],
        "A": [20, 20],
        "B": [55, 30]
    }
}
```

O exemplo acima equivale a uma tabela chamada "test" com os seguintes dados:

| id | A  | B  |
|----|----|----|
| 1  | 20 | 55 |
| 2  | 20 | 30 |

> O arquivo de metadados está localizado [aqui](src/metadado.json).

## Executando a aplicação

### 1. Dependências

Para executar o projeto você precisará instalar as seguintes ferramentas:

- [.NET 7.0](https://dotnet.microsoft.com/pt-br/download/dotnet/7.0);
- [PostgreSQL 15.3](https://www.postgresql.org/download/)

### 2. Projeto

1. Baixe o repositório do projeto:

```
git clone https://github.com/andreuv3/database-log-undo
```

2. Navegue até o diretório do projeto:

```
cd database-log-undo
```

3. Configure o arquivo de log localizado em ```src/entradaLog``` com o conteúdo desejado.

4. Configure o arquivo de metadados em ```src/metadado.json``` com o conteúdo desejado.

5. Configure o arquivo appsettings localizado em ```src/appsettings.json``` para utilizar a string de conexão desejada.

> Observação: você deverá criar o banco de dados configurado na string de conexão informada.

6. Faça o build do projeto

```
dotnet build
```

7. Navegue até o diretório src

```
cd src
```

8. Execute o projeto

```
dotnet run
```
