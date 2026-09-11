#!/bin/bash
set -euo pipefail
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
registry_user=$(az keyvault secret show --vault-name "$keyVaultName" --name acr-username --query value -o tsv)
registry_password=$(az keyvault secret show --vault-name "$keyVaultName" --name acr-password --query value -o tsv)
oracle_password=$(az keyvault secret show --vault-name "$keyVaultName" --name oracle-password --query value -o tsv)
app_user=$(az keyvault secret show --vault-name "$keyVaultName" --name oracle-app-user --query value -o tsv)
app_password=$(az keyvault secret show --vault-name "$keyVaultName" --name oracle-app-password --query value -o tsv)
digest=$(az acr repository show --name "$acrName" --image "$imageName:$tag" --query digest -o tsv)
[[ "$digest" == sha256:* ]] || { echo 'Digest da imagem invalido.' >&2; exit 1; }

az provider register --namespace Microsoft.ContainerInstance

if ! az storage share exists --name "$file_share_name" --account-name "$storageAccountName" --account-key "$storage_key" --query exists -o tsv | grep -qi true; then
  az storage share create --name "$file_share_name" --account-name "$storageAccountName" --account-key "$storage_key"
fi

if az container show --resource-group "$resourceGroup" --name "$aciName" &>/dev/null; then
  az container delete --resource-group "$resourceGroup" --name "$aciName" --yes --output none
fi

echo "CONTAINER IMAGE: $acrName.azurecr.io/$imageName:$tag"
echo "FILE SHARE: $file_share_name -> /opt/oracle/oradata"

az container create \
  --resource-group "$resourceGroup" \
  --name "$aciName" \
  --location "$location" \
  --image "$acrName.azurecr.io/$imageName@$digest" \
  --cpu 2 \
  --memory 4 \
  --os-type Linux \
  --run-as-user 0 \
  --dns-name-label oracle-clyvo-$rm \
  --ports 1521 \
  --registry-login-server "$acrName.azurecr.io" \
  --registry-username "$registry_user" \
  --registry-password "$registry_password" \
  --azure-file-volume-account-name "$storageAccountName" \
  --azure-file-volume-account-key "$storage_key" \
  --azure-file-volume-share-name "$file_share_name" \
  --azure-file-volume-mount-path /opt/oracle/oradata \
  --secure-environment-variables \
    "ORACLE_PASSWORD=$oracle_password" \
    "APP_USER=$app_user" \
    "APP_USER_PASSWORD=$app_password" \
  --restart-policy Always \
  --output none

echo "Esperando o Oracle (ate 15 min)."
for i in $(seq 1 30); do
  sleep 30
  echo "----- ${i}/30 -----"
  az container show --resource-group "$resourceGroup" --name "$aciName" --query "{FQDN:ipAddress.fqdn,State:instanceView.state,Restarts:containers[0].instanceView.restartCount}" -o table
  logs=$(az container logs --resource-group "$resourceGroup" --name "$aciName" --container-name "$aciName" 2>/dev/null || true)
  printf '%s\n' "$logs" | tail -n 40
  state=$(az container show --resource-group "$resourceGroup" --name "$aciName" --query 'containers[0].instanceView.currentState.state' -o tsv)
  if [ "$state" = Terminated ]; then
    echo 'Oracle terminou com falha.' >&2
    exit 1
  fi
  if grep -q "DATABASE IS READY TO USE" <<< "$logs"; then
    result=$(az container exec --resource-group "$resourceGroup" --name "$aciName" --exec-command '/bin/bash /oracle-ready.sh' 2>&1) || true
    printf '%s\n' "$result"
    if grep -q CLYVOCARE_DATABASE_READY <<< "$result"; then
      exit 0
    fi
  fi
  if grep -q "DATABASE STARTUP FAILED" <<< "$logs"; then
    echo "STARTUP FAILED no log." >&2
    exit 1
  fi
  restarts=$(az container show --resource-group "$resourceGroup" --name "$aciName" --query "containers[0].instanceView.restartCount" -o tsv 2>/dev/null || echo 0)
  if [ "${restarts:-0}" -ge 3 ]; then
    echo "CrashLoop (restarts=$restarts)." >&2
    exit 1
  fi
done
echo 'Banco nao validado dentro do prazo. Nao execute o 06.' >&2
exit 1
