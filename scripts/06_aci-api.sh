#!/bin/bash
rm=562673
location="eastus"
resourceGroup="rg-clyvocare"
acrName="clyvocare$rm"
aciName="api-clyvo"
aciNameOracle="oracle-clyvo"
imageName="api-clyvo"
tag="v1"
keyVaultName="kv-clyvo-$rm"
oracleURL=$(az container show --resource-group "$resourceGroup" --name "$aciNameOracle" --query ipAddress.fqdn --output tsv)
conn=$(az keyvault secret show --name connection-strings --vault-name "$keyVaultName" --query value -o tsv | sed "s/oracle-clyvo/$oracleURL/")

az provider register --namespace Microsoft.ContainerInstance

az container create \
  --resource-group "$resourceGroup" \
  --name "$aciName" \
  --location "$location" \
  --image "$acrName.azurecr.io/$imageName:$tag" \
  --cpu 1 \
  --memory 1 \
  --os-type Linux \
  --dns-name-label api-clyvo-$rm \
  --ports 8080 \
  --registry-login-server "$acrName.azurecr.io" \
  --registry-username $(az keyvault secret show --vault-name "$keyVaultName" --name acr-username --query value -o tsv) \
  --registry-password $(az keyvault secret show --vault-name "$keyVaultName" --name acr-password --query value -o tsv) \
  --environment-variables \
    ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
  --secure-environment-variables \
    "ConnectionStrings__DefaultConnection=${conn}" \
  --restart-policy Always

sleep 25

fqdndotnet=$(az container show --resource-group "$resourceGroup" --name "$aciName" --query ipAddress.fqdn --output tsv)
echo "FQDN da API: $fqdndotnet"
az container logs --resource-group "$resourceGroup" --name "$aciName"
