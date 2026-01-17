using '../main.bicep'

param location = 'westus3'
param cosmosAccountName = 'mickcosmos'
param cosmosDatabaseName = 'PlatformDatabases'
param containers = [
  {
    id: 'Beers-Dev'
    indexingPolicy: {
      indexingMode: 'consistent'
      automatic: true
      includedPaths: [
        {
          path: '/*'
        }
      ]
      excludedPaths: [
        {
          path: '/"_etag"/?'
        }
      ]
      compositeIndexes: [
        [
          {
            path: '/Rating/reviews'
            order: 'ascending'
          }
          {
            path: '/Name'
            order: 'ascending'
          }
        ]
        [
          {
            path: '/CreatedDate'
            order: 'ascending'
          }
          {
            path: '/Name'
            order: 'ascending'
          }
        ]
      ]
    }
  }
  {
    id: 'Beers-QA'
    indexingPolicy: {
      indexingMode: 'consistent'
      automatic: true
      includedPaths: [
        {
          path: '/*'
        }
      ]
      excludedPaths: [
        {
          path: '/"_etag"/?'
        }
      ]
      compositeIndexes: [
        [
          {
            path: '/Rating/reviews'
            order: 'ascending'
          }
          {
            path: '/Name'
            order: 'ascending'
          }
        ]
        [
          {
            path: '/CreatedDate'
            order: 'ascending'
          }
          {
            path: '/Name'
            order: 'ascending'
          }
        ]
      ]
    }
  }
]
