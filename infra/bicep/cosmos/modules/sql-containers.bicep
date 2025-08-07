targetScope = 'resourceGroup'

@description('Existing Azure Cosmos DB account name.')
param cosmosAccountName string

@description('Existing SQL database name to host the containers.')
param cosmosDatabaseName string

@description('Container definitions. Supports id and optional indexingPolicy.')
param containers array

resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2024-05-15' existing = {
  name: cosmosAccountName
}

resource sqlDatabase 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2024-05-15' existing = {
  parent: cosmosAccount
  name: cosmosDatabaseName
}

resource sqlContainers 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2024-05-15' = [for container in containers: {
  parent: sqlDatabase
  name: container.id
  properties: {
    resource: union({
      id: container.id
      partitionKey: {
        paths: [
          '/BrewerId'
          '/EntityType'
        ]
        kind: 'MultiHash'
        version: 2
      }
    }, contains(container, 'indexingPolicy') ? {
      indexingPolicy: container.indexingPolicy
    } : {})
    options: {}
  }
}]
