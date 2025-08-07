targetScope = 'resourceGroup'

@description('Deployment location for this resource group context.')
param location string = resourceGroup().location

@description('Existing Azure Cosmos DB account name.')
param cosmosAccountName string

@description('Existing SQL database name inside the Cosmos account.')
param cosmosDatabaseName string

@description('SQL containers to reconcile in the existing Cosmos SQL database.')
param containers array

module sqlContainers './modules/sql-containers.bicep' = {
  name: 'sql-containers-${uniqueString(resourceGroup().id, cosmosAccountName, cosmosDatabaseName)}'
  params: {
    cosmosAccountName: cosmosAccountName
    cosmosDatabaseName: cosmosDatabaseName
    containers: containers
  }
}

output deploymentLocation string = location
