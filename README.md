# ClyvoCare — Sprint 3 DevOps (ACR + ACI)

**Integrantes**

- Geovanne Coneglian Passos — RM 562673
- Lucas Silva Gastão Pinheiro — RM 563960
- Guilherme Soares De Almeida — RM 563143

---

## 1. Descrição da solução

O **ClyvoCare** recebe leituras biométricas de pets (peso, temperatura e batimentos) de dispositivos IoT, associa cada animal ao tutor responsável e grava o histórico clínico.

| Tabela | Papel |
| --- | --- |
| `TB_CC_PET` | Animal monitorado (CORE) |
| `TB_CC_LOG_SAUDE` | Leitura de saúde do pet (CORE) |
| `TB_CC_USUARIO` | Tutor responsável pelo animal |

CRUD completo (incluir, alterar, excluir, consultar) em **Pet** e **Log de Saúde**, ligados por `ID_PET`.

## 2. Benefícios para o negócio

- Histórico contínuo de sinais vitais, sem depender só da consulta presencial.
- Temperatura fora de 30–45 °C é rejeitada na API, evitando lixo para modelo preditivo.
- Persistência na nuvem: o `SELECT` no Oracle prova o que a API gravou.
- Mesma imagem no ACR e no ACI.
- Segredos no Key Vault, não no código da API.

## 3. Arquitetura

![Arquitetura ACR + ACI](docs/arquitetura.png)

**Como funciona**

1. Azure CLI cria o Resource Group `rg-clyvocare`, o ACR `clyvocare562673` e o Key Vault `kv-clyvo-562673`.
2. `docker build` gera `oracle-clyvo` (Oracle XE + `script_bd.sql`) e `api-clyvo` (API .NET, usuário `appuser`).
3. `docker tag` + `docker push` enviam as imagens ao ACR.
4. ACI `oracle-clyvo` puxa a imagem do ACR e sobe o Oracle na porta 1521 (PDB `XEPDB1`).
5. ACI `api-clyvo` puxa a imagem do ACR, recebe a connection string do Key Vault (FQDN público do Oracle) e sobe na porta 8080.
6. A API responde em `http://<fqdn-api>:8080`, grava em `TB_CC_PET` e `TB_CC_LOG_SAUDE`. O `SELECT` no container Oracle prova a persistência.

---

## 4. How To — execução

Pré-requisitos: **Git Bash** (ou Azure Cloud Shell em Bash) e **Docker Desktop** no passo 4.5.

### 4.1 Clone

```bash
git clone https://github.com/GuuiSOares/clyvocare.git
cd clyvocare
ls
```

### 4.2 Login na Azure

```bash
az login
az account show
```

### 4.3 Permissão dos scripts

```bash
chmod +x scripts/*.sh
```

### 4.4 Criar recursos na Azure (CLI)

```bash
./scripts/00_resource-group.sh
./scripts/01_acr.sh
./scripts/03_key-vault.sh
```

### 4.5 Build, tag, push e run

```bash
./scripts/04_build-push.sh
```

Comandos equivalentes:

```bash
docker build -f docker/Dockerfile.oracle -t oracle-clyvo .
docker build -f docker/Dockerfile -t api-clyvo .
az acr login --name clyvocare562673
docker tag oracle-clyvo clyvocare562673.azurecr.io/oracle-clyvo:v1
docker tag api-clyvo clyvocare562673.azurecr.io/api-clyvo:v1
docker push clyvocare562673.azurecr.io/oracle-clyvo:v1
docker push clyvocare562673.azurecr.io/api-clyvo:v1
az acr repository list --name clyvocare562673 --output table
```

```bash
docker run -d --name oracle-clyvo -p 1521:1521 \
  -e ORACLE_PASSWORD=ClyvoOra2026 \
  -e APP_USER=clyvocare \
  -e APP_USER_PASSWORD=ClyvoApp2026 \
  oracle-clyvo

docker run -d --name api-clyvo -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="User Id=clyvocare;Password=ClyvoApp2026;Data Source=host.docker.internal:1521/XEPDB1;" \
  api-clyvo
```

### 4.6 Subir Oracle e API na ACI

```bash
./scripts/05_aci-oracle.sh
```

```bash
az container logs --resource-group rg-clyvocare --name oracle-clyvo
```

Aguarde `DATABASE IS READY TO USE` no log.

```bash
./scripts/06_aci-api.sh
```

```bash
fqdndotnet=$(az container show --resource-group rg-clyvocare --name api-clyvo --query ipAddress.fqdn --output tsv)
echo $fqdndotnet
```

- Swagger: `http://$fqdndotnet:8080/swagger`
- Live: `http://$fqdndotnet:8080/health/live`
- Ready: `http://$fqdndotnet:8080/health/ready`

### 4.7 CRUD + SELECT no Oracle

**Consulta**

```bash
curl -X GET http://$fqdndotnet:8080/api/Pets
curl -X GET http://$fqdndotnet:8080/api/LogsSaude
```

```bash
az container exec --resource-group rg-clyvocare --name oracle-clyvo --exec-command "sqlplus -s clyvocare/ClyvoApp2026@//localhost/XEPDB1"
```

```sql
SELECT * FROM TB_CC_PET;
SELECT * FROM TB_CC_LOG_SAUDE;
```

**Inserção**

```bash
curl -X POST http://$fqdndotnet:8080/api/Pets \
  -H "Content-Type: application/json" \
  -d '{"nome":"Bidu","especie":"Cachorro","dataNascimento":"2023-01-10","usuarioId":1}'

curl -X POST http://$fqdndotnet:8080/api/LogsSaude \
  -H "Content-Type: application/json" \
  -d '{"peso":12.50,"temperatura":38.60,"batimentosCardiacos":110,"observacoes":"Coleta IoT em repouso.","petId":1}'
```

```sql
SELECT * FROM TB_CC_PET;
SELECT * FROM TB_CC_LOG_SAUDE;
```

**Alteração**

```bash
curl -X PUT http://$fqdndotnet:8080/api/Pets/1 \
  -H "Content-Type: application/json" \
  -d '{"id":1,"nome":"Thor Atualizado","especie":"Cachorro","dataNascimento":"2022-04-15","usuarioId":1}'

curl -X PUT http://$fqdndotnet:8080/api/LogsSaude/1 \
  -H "Content-Type: application/json" \
  -d '{"peso":29.10,"temperatura":39.20,"batimentosCardiacos":118,"observacoes":"Febre leve detectada pelo sensor.","petId":1}'
```

```sql
SELECT * FROM TB_CC_PET;
SELECT * FROM TB_CC_LOG_SAUDE;
```

**Exclusão**

```bash
curl -X DELETE http://$fqdndotnet:8080/api/LogsSaude/3
curl -X DELETE http://$fqdndotnet:8080/api/Pets/3
```

```sql
SELECT * FROM TB_CC_PET;
SELECT * FROM TB_CC_LOG_SAUDE;
```

### 4.8 Logs dos containers

```bash
az container logs --resource-group rg-clyvocare --name oracle-clyvo
az container logs --resource-group rg-clyvocare --name api-clyvo
```

### 4.9 Encerrar recursos

```bash
./scripts/99_cleanup.sh
```
