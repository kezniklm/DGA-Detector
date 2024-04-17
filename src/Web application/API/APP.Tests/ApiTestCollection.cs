/**
 * @file ApiTestCollection.cs
 *
 * @brief Defines a collection for API tests using Xunit.
 *
 * This file contains the definition of the ApiTestCollection class, which serves as a collection for API tests using Xunit. It allows grouping of tests that require shared state or resources.
 *
 * The main purpose of this file is to define a collection for API tests. It utilizes Xunit's CollectionDefinition attribute to mark the collection and implements ICollectionFixture to provide a fixture for the collection.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 *
 */

using Xunit;

namespace APP.Tests;

/// <summary>
///     Contains the definition of a collection of tests for the API application.
/// </summary>
[CollectionDefinition("APP.Tests")]
public class ApiTestCollection : ICollectionFixture<ApiApplicationFactory<Program>>
{
}
