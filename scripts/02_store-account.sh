#!/bin/bash
rm=562673
location="eastus"
resourceGroup="rg-clyvocare"
storageAccountName="volclyvocare$rm"
file_share_name="oracle-clyvo-volume"

az provider register --namespace Microsoft.Storage

if ! az storage account show --name "$storageAccountName" --resource-group "$resourceGroup" &>/dev/null; then
  az storage account create \
    --resource-group "$resourceGroup" \
    --name "$storageAccountName" \
    --location "$location" \
    --sku Standard_LRS
else
  echo "A conta de armazenamento '$storageAccountName' ja existe"
fi

connection_string=$(az storage account show-connection-string --name "$storageAccountName" --resource-group "$resourceGroup" --query connectionString --output tsv)

if ! az storage share exists --name "$file_share_name" --account-name "$storageAccountName" --connection-string "$connection_string" | grep true; then
  az storage share create --name "$file_share_name" --account-name "$storageAccountName" --connection-string "$connection_string"
else
  echo "O compartilhamento de arquivos '$file_share_name' ja existe"
fi
