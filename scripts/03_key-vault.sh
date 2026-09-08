#!/bin/bash
rm=562673
resourceGroup="rg-clyvocare"
location="eastus"
ORACLE_PASSWORD=ClyvoOra2026
APP_USER=clyvocare
APP_USER_PASSWORD=ClyvoApp2026
CONNECTIONSTRINGS='User Id=clyvocare;Password=ClyvoApp2026;Data Source=oracle-clyvo:1521/XEPDB1;'

acrName="clyvocare$rm"
ACRUSERNAME=$(az acr credential show --name "$acrName" --resource-group "$resourceGroup" --query username --output tsv)
ACRPASSWORD=$(az acr credential show --name "$acrName" --resource-group "$resourceGroup" --query passwords[0].value --output tsv)
keyVaultName="kv-clyvo-$rm"

az provider register --namespace Microsoft.KeyVault

if ! az keyvault show --name "$keyVaultName" --resource-group "$resourceGroup" &> /dev/null; then
  az keyvault create --name "$keyVaultName" --resource-group "$resourceGroup" --location "$location" --enable-rbac-authorization true
fi

az role assignment create \
  --assignee $(az account show --query user.name -o tsv) \
  --role "Key Vault Administrator" \
  --scope /subscriptions/$(az account show --query id -o tsv)/resourceGroups/$resourceGroup/providers/Microsoft.KeyVault/vaults/$keyVaultName

sleep 15

az keyvault secret set --vault-name "$keyVaultName" --name oracle-password --value "$ORACLE_PASSWORD"
az keyvault secret set --vault-name "$keyVaultName" --name oracle-app-user --value "$APP_USER"
az keyvault secret set --vault-name "$keyVaultName" --name oracle-app-password --value "$APP_USER_PASSWORD"
az keyvault secret set --vault-name "$keyVaultName" --name connection-strings --value "$CONNECTIONSTRINGS"
az keyvault secret set --vault-name "$keyVaultName" --name acr-username --value "$ACRUSERNAME"
az keyvault secret set --vault-name "$keyVaultName" --name acr-password --value "$ACRPASSWORD"
