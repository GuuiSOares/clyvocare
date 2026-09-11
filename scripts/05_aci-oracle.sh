#!/bin/bash
export MSYS_NO_PATHCONV=1
rm=562673
location="eastus"
resourceGroup="rg-clyvocare"
acrName="clyvocare$rm"
aciName="oracle-clyvo"
imageName="oracle-clyvo"
tag="v1"
keyVaultName="kv-clyvo-$rm"
storageAccountName="volclyvocare$rm"
file_share_name="oracle-clyvo-volume"
storage_key=$(az storage account keys list --resource-group "$resourceGroup" --account-name "$storageAccountName" --query "[0].value" --output tsv)

az provider register --namespace Microsoft.ContainerInstance

if az container show --resource-group "$resourceGroup" --name "$aciName" &>/dev/null; then
  az container delete --resource-group "$resourceGroup" --name "$aciName" --yes
fi

az container create \
  --resource-group "$resourceGroup" \
  --name "$aciName" \
  --location "$location" \
  --image "$acrName.azurecr.io/$imageName:$tag" \
  --cpu 2 \
  --memory 4 \
  --os-type Linux \
  --run-as-user 0 \
  --dns-name-label oracle-clyvo-$rm \
  --ports 1521 \
  --registry-login-server "$acrName.azurecr.io" \
  --registry-username $(az keyvault secret show --vault-name "$keyVaultName" --name acr-username --query value -o tsv) \
  --registry-password $(az keyvault secret show --vault-name "$keyVaultName" --name acr-password --query value -o tsv) \
  --azure-file-volume-account-name "$storageAccountName" \
  --azure-file-volume-account-key "$storage_key" \
  --azure-file-volume-share-name "$file_share_name" \
  --azure-file-volume-mount-path /opt/oracle/oradata \
  --environment-variables \
    ORACLE_PASSWORD=$(az keyvault secret show --vault-name "$keyVaultName" --name oracle-password --query value -o tsv) \
    APP_USER=$(az keyvault secret show --vault-name "$keyVaultName" --name oracle-app-user --query value -o tsv) \
    APP_USER_PASSWORD=$(az keyvault secret show --vault-name "$keyVaultName" --name oracle-app-password --query value -o tsv) \
  --restart-policy Always

sleep 180
az container show --resource-group "$resourceGroup" --name "$aciName" --query "{FQDN:ipAddress.fqdn,State:instanceView.state}" -o table
az container logs --resource-group "$resourceGroup" --name "$aciName"
