#!/bin/bash
set -euo pipefail
export MSYS_NO_PATHCONV=1
rm=562673
location="eastus"
resourceGroup="rg-clyvocare"
acrName="clyvocare$rm"
aciName="api-clyvo"
aciNameOracle="oracle-clyvo"
imageName="api-clyvo"
tag="v1"
keyVaultName="kv-clyvo-$rm"
ready=$(az container exec --resource-group "$resourceGroup" --name "$aciNameOracle" --exec-command '/bin/bash /oracle-ready.sh' 2>&1) || true
if ! grep -q CLYVOCARE_DATABASE_READY <<< "$ready"; then
  printf '%s\n' "$ready" >&2
  echo 'Oracle indisponivel. API nao sera implantada.' >&2
  exit 1
fi
oracleURL=$(az container show --resource-group "$resourceGroup" --name "$aciNameOracle" --query ipAddress.fqdn --output tsv)
conn=$(az keyvault secret show --name connection-strings --vault-name "$keyVaultName" --query value -o tsv | sed "s/oracle-clyvo/$oracleURL/")
registry_user=$(az keyvault secret show --vault-name "$keyVaultName" --name acr-username --query value -o tsv)
registry_password=$(az keyvault secret show --vault-name "$keyVaultName" --name acr-password --query value -o tsv)
digest=$(az acr repository show --name "$acrName" --image "$imageName:$tag" --query digest -o tsv)
[[ "$digest" == sha256:* ]] || { echo 'Digest da API invalido.' >&2; exit 1; }

az provider register --namespace Microsoft.ContainerInstance

if az container show --resource-group "$resourceGroup" --name "$aciName" &>/dev/null; then
  az container delete --resource-group "$resourceGroup" --name "$aciName" --yes --output none
fi

az container create \
  --resource-group "$resourceGroup" \
  --name "$aciName" \
  --location "$location" \
  --image "$acrName.azurecr.io/$imageName@$digest" \
  --cpu 1 \
  --memory 1 \
  --os-type Linux \
  --dns-name-label api-clyvo-$rm \
  --ports 8080 \
  --registry-login-server "$acrName.azurecr.io" \
  --registry-username "$registry_user" \
  --registry-password "$registry_password" \
  --environment-variables \
    ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
  --secure-environment-variables \
    "ConnectionStrings__DefaultConnection=${conn}" \
  --restart-policy Always \
  --output none

sleep 25

fqdndotnet=$(az container show --resource-group "$resourceGroup" --name "$aciName" --query ipAddress.fqdn --output tsv)
echo "FQDN da API: $fqdndotnet"
az container logs --resource-group "$resourceGroup" --name "$aciName"
for attempt in $(seq 1 30); do
  if curl --fail --silent --show-error --max-time 10 "http://$fqdndotnet:8080/health/ready"; then
    echo
    echo 'API pronta.'
    exit 0
  fi
  sleep 5
done
echo 'API nao ficou pronta dentro do prazo.' >&2
exit 1
