#!/bin/bash
rm=562673
resourceGroup="rg-clyvocare"
acrName="clyvocare$rm"
tag="v1"

docker build -f docker/Dockerfile.oracle -t oracle-clyvo .
docker build -f docker/Dockerfile -t api-clyvo .

az acr login --name "$acrName"

LOGIN_SERVER=$(az acr show --name "$acrName" --resource-group "$resourceGroup" --query loginServer --output tsv)

docker tag oracle-clyvo "$LOGIN_SERVER/oracle-clyvo:$tag"
docker tag api-clyvo "$LOGIN_SERVER/api-clyvo:$tag"

docker push "$LOGIN_SERVER/oracle-clyvo:$tag"
docker push "$LOGIN_SERVER/api-clyvo:$tag"

az acr repository list --name "$acrName" --output table
az acr repository show-tags --name "$acrName" --repository oracle-clyvo
az acr repository show-tags --name "$acrName" --repository api-clyvo
