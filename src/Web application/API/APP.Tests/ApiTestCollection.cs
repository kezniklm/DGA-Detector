using Xunit;

namespace APP.Tests;

[CollectionDefinition("APP.Tests")]
public class ApiTestCollection : ICollectionFixture<ApiApplicationFactory<Program>>
{
}
