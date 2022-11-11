dotnet publish
cd bin/Debug/net6.0/publish/
rm api.zip
zip -r api.zip .* *

# todo: update with resource group name and app name
az webapp deploy --resource-group vue-sample --name vue-test-application --src-path api.zip