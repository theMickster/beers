targetScope = 'subscription'

@description('Target resource group name for Cosmos container reconciliation.')
param resourceGroupName string

@description('Deployment location.')
param location string

@description('Existing Azure Cosmos DB account name.')
param cosmosAccountName string

@description('Existing SQL database name inside the Cosmos account.')
param cosmosDatabaseName string

@description('SQL containers to reconcile in the existing Cosmos SQL database.')
param containers array

resource targetRg 'Microsoft.Resources/resourceGroups@2024-03-01' existing = {
  name: resourceGroupName
}

module cosmosContainers './main.bicep' = {
  name: 'cosmos-container-reconcile-${uniqueString(resourceGroupName, cosmosAccountName, cosmosDatabaseName)}'
  scope: targetRg
  params: {
    location: location
    cosmosAccountName: cosmosAccountName
    cosmosDatabaseName: cosmosDatabaseName
    containers: containers
  }
}
