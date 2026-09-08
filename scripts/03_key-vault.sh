#!/bin/bash
rm=562673
resourceGroup="rg-clyvocare"
location="eastus"
ORACLE_PASSWORD=ClyvoOra2026
APP_USER=clyvocare
APP_USER_PASSWORD=ClyvoApp2026
CONNECTIONSTRINGS='User Id=clyvocare;Password=ClyvoApp2026;Data Source=//oracle-clyvo:1521/XEPDB1;'

acrName="clyvocare$rm"
ACRUSERNAME=$(az acr credential show --name "$acrName" --resource-group "$resourceGroup" --query username --output tsv)
ACRPASSWORD=$(az acr credential show --name "$acrName" --resource-group "$resourceGroup" --query passwords[0].value --output tsv)
keyVaultName="kv-clyvo-$rm"
objectId=$(az ad signed-in-user show --query id -o tsv)

az provider register --namespace Microsoft.KeyVault

if ! az keyvault show --name "$keyVaultName" --resource-group "$resourceGroup" &> /dev/null; then
  az keyvault create \
    --name "$keyVaultName" \
    --resource-group "$resourceGroup" \
    --location "$location" \
    --enable-rbac-authorization false
else
  az keyvault update \
    --name "$keyVaultName" \
    --resource-group "$resourceGroup" \
    --enable-rbac-authorization false
fi

az keyvault set-policy \
  --name "$keyVaultName" \
  --object-id "$objectId" \
  --secret-permissions get list set delete

sleep 20

az keyvault secret set --vault-name "$keyVaultName" --name oracle-password --value "$ORACLE_PASSWORD"
az keyvault secret set --vault-name "$keyVaultName" --name oracle-app-user --value "$APP_USER"
az keyvault secret set --vault-name "$keyVaultName" --name oracle-app-password --value "$APP_USER_PASSWORD"
az keyvault secret set --vault-name "$keyVaultName" --name connection-strings --value "$CONNECTIONSTRINGS"
az keyvault secret set --vault-name "$keyVaultName" --name acr-username --value "$ACRUSERNAME"
az keyvault secret set --vault-name "$keyVaultName" --name acr-password --value "$ACRPASSWORD"
