#!/bin/bash
rm=562673
location="eastus"
resourceGroup="rg-clyvocare"

if ! az group show --name "$resourceGroup" &>/dev/null; then
  az group create --name "$resourceGroup" --location "$location"
fi
