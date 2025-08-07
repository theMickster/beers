using '../main.bicep'

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
