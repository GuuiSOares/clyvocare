#!/bin/bash
rm=562673
location="eastus"
resourceGroup="rg-clyvocare"
acrName="clyvocare$rm"

az provider register --namespace Microsoft.ContainerRegistry

az acr create \
  --resource-group "$resourceGroup" \
  --name "$acrName" \
  --sku Standard \
  --location "$location" \
  --public-network-enabled true \
  --admin-enabled true

LOGIN_SERVER=$(az acr show --name "$acrName" \
                           --resource-group "$resourceGroup" \
                           --query loginServer --output tsv)
echo "Login Server: $LOGIN_SERVER"

ADMIN_USERNAME=$(az acr credential show --name "$acrName" \
                                        --resource-group "$resourceGroup" \
                                        --query username --output tsv)
echo "Username: $ADMIN_USERNAME"
