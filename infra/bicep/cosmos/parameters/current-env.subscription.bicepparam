using '../deploy.bicep'

param resourceGroupName = 'Mick-West-US-3-CosmosDb'
param location = 'westus3'
param cosmosAccountName = 'mickcosmos'
param cosmosDatabaseName = 'PlatformDatabases'
param containers = [
  {
    id: 'Beers-Dev'
  }
  {
    id: 'Beers-QA'
  }
]
