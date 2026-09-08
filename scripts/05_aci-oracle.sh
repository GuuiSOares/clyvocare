#!/bin/bash
rm=562673
location="eastus"
resourceGroup="rg-clyvocare"
acrName="clyvocare$rm"
aciName="oracle-clyvo"
imageName="oracle-clyvo"
tag="v1"
keyVaultName="kv-clyvo-$rm"

az provider register --namespace Microsoft.ContainerInstance

az container create \
  --resource-group "$resourceGroup" \
  --name "$aciName" \
  --location "$location" \
  --image "$acrName.azurecr.io/$imageName:$tag" \
  --cpu 2 \
  --memory 4 \
  --os-type Linux \
  --dns-name-label oracle-clyvo-$rm \
  --ports 1521 \
  --registry-login-server "$acrName.azurecr.io" \
  --registry-username $(az keyvault secret show --vault-name "$keyVaultName" --name acr-username --query value -o tsv) \
  --registry-password $(az keyvault secret show --vault-name "$keyVaultName" --name acr-password --query value -o tsv) \
  --environment-variables \
    ORACLE_PASSWORD=$(az keyvault secret show --vault-name "$keyVaultName" --name oracle-password --query value -o tsv) \
    APP_USER=$(az keyvault secret show --vault-name "$keyVaultName" --name oracle-app-user --query value -o tsv) \
    APP_USER_PASSWORD=$(az keyvault secret show --vault-name "$keyVaultName" --name oracle-app-password --query value -o tsv) \
  --restart-policy Always

sleep 120
az container show --resource-group "$resourceGroup" --name "$aciName" --query "{FQDN:ipAddress.fqdn,State:instanceView.state}" -o table
az container logs --resource-group "$resourceGroup" --name "$aciName"
