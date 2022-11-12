# About
Running Selenium and Playwright in Azure.

## Use Cases
Following use cases describe how to run Selenium in Azure. Due to dependency on browser drivers, not all scenarios are supported.

### Azure Functions
Docker images for Azure Functions miss chrome dependencies, although it's possible to install them post deployment.

Since running selenium requires running a web service on a port, azure security restrictions are blocking it.

There are however options to run playwright - see [functions-headless-chromium](https://github.com/anthonychu/functions-headless-chromium/blob/main/playwright/screencast/index.js).

Due to security restrictions and differences in docker images between function run times (node, dotnet) it's recommended to use custom docker image on Premium Plan SKU.

Other links:
- [Issues with chromium dependencies](https://github.com/Azure/azure-functions-docker/issues/451)
- [App Service unsupported frameworks](https://github.com/projectkudu/kudu/wiki/Azure-Web-App-sandbox#unsupported-frameworks)
- [Selenium in docker](https://www.silverlining.cloud/docs/azure/selenium-azure-functions)

### Azure Container Apps
Due to it price competitiveness - Container Apps may be a viable solution.

### Alternative - running in Azure Devops
It is possible however to run selenium tests in Azure DevOps and report results to Azure using Application Insights.

The scenario described here, assumes that App Insights is used to report application failures (raise alerts, log issues), that may not be the case for every client.

Sample code is included in `Tests` directory.