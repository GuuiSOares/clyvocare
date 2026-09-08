#!/bin/bash
resourceGroup="rg-clyvocare"
az group delete --name "$resourceGroup" --yes --no-wait
