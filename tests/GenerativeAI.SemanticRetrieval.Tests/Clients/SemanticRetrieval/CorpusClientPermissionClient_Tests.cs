using System.Diagnostics.CodeAnalysis;
using GenerativeAI.Clients;
using GenerativeAI.SemanticRetrieval.Tests;
using GenerativeAI.Tests.Base;
using GenerativeAI.Types;
using Shouldly;
using Xunit;

namespace GenerativeAI.Tests.Clients.SemanticRetrieval
{

    [TestCaseOrderer(
        typeof(PriorityOrderer))]
    public class CorpusPermissionClient_Tests : SemanticRetrieverTestBase
    {
        private const string TestCorpus = "corpora/test-corpus-id";
        private static string? _createdPermissionName;
        
        public CorpusPermissionClient_Tests(ITestOutputHelper output) : base(output)
        {
            Assert.SkipWhen(GitHubEnvironment(), "Github");
            Assert.SkipUnless(IsSemanticTestsEnabled, SemanticTestsDisabledMessage);
        }

        [Fact, TestPriority(1)]
        public async Task ShouldCreatePermissionAsync()
        {
            // Arrange
            var client = new CorpusPermissionClient(GetTestGooglePlatform());
            var newPermission = new Permission
            {
                GranteeType = GranteeType.USER,         // Example grantee type
                EmailAddress = "test-user@example.com", // Example email
                Role = Role.WRITER                      // Example role
            };

            // Act
            var result = await client.CreatePermissionAsync(TestCorpus, newPermission).ConfigureAwait(false);

            // Assert
            result.ShouldNotBeNull();
            result.Name.ShouldNotBeNullOrEmpty();

            // Keep track of the created permission name for subsequent tests
            _createdPermissionName = result.Name;

            Console.WriteLine($"Created Permission: Name={result.Name}, Role={result.Role}");
        }

        [Fact, TestPriority(2)]
        public async Task ShouldGetPermissionAsync()
        {
            // Arrange
            var client = new CorpusPermissionClient(GetTestGooglePlatform());
            _createdPermissionName.ShouldNotBeNullOrEmpty();

            // Act
            var result = await client.GetPermissionAsync(_createdPermissionName).ConfigureAwait(false);

            // Assert
            result.ShouldNotBeNull();
            result!.Name.ShouldBe(_createdPermissionName);
            //result.Role.ShouldNotBeNull();

            Console.WriteLine($"Retrieved Permission: Name={result.Name}, Role={result.Role}");
        }

        [Fact, TestPriority(3)]
        public async Task ShouldListPermissionsAsync()
        {
            // Arrange
            var client = new CorpusPermissionClient(GetTestGooglePlatform());
            const int pageSize = 10;

            // Act
            var result = await client.ListPermissionsAsync(TestCorpus, pageSize).ConfigureAwait(false);

            // Assert
            result.ShouldNotBeNull();
            result!.Permissions.ShouldNotBeNull();
            result.Permissions.Count.ShouldBeGreaterThan(0);
            result.Permissions.Count.ShouldBeLessThanOrEqualTo(pageSize);

            // Check each permission for required fields
            foreach (var permission in result.Permissions)
            {
                permission.Name.ShouldNotBeNullOrEmpty();
                //permission.Role.ShouldNotBeNull();
            }

            Console.WriteLine($"Listed {result.Permissions.Count} Permissions");
        }

        [Fact, TestPriority(4)]
        public async Task ShouldUpdatePermissionAsync()
        {
            // Arrange
            var client = new CorpusPermissionClient(GetTestGooglePlatform());
            _createdPermissionName.ShouldNotBeNullOrEmpty();

            var updatedPermission = new Permission
            {
                Role = Role.READER  // Example updated role
            };
            const string updateMask = "role"; // Example update mask

            // Act
            var result = await client.UpdatePermissionAsync(_createdPermissionName, updatedPermission, updateMask).ConfigureAwait(false);

            // Assert
            result.ShouldNotBeNull();
            result!.Name.ShouldBe(_createdPermissionName);
            result.Role.ShouldBe(Role.READER);

            Console.WriteLine($"Updated Permission: Name={result.Name}, Role={result.Role}");
        }

        [Fact, TestPriority(5)]
        public async Task ShouldDeletePermissionAsync()
        {
            // Arrange
            var client = new CorpusPermissionClient(GetTestGooglePlatform());
            _createdPermissionName.ShouldNotBeNullOrEmpty();

            // Act
            await client.DeletePermissionAsync(_createdPermissionName).ConfigureAwait(false);

            // Assert - optionally confirm via retrieval
            var getResult = await client.GetPermissionAsync(_createdPermissionName).ConfigureAwait(false);
            getResult.ShouldBeNull();

            Console.WriteLine($"Deleted Permission: Name={_createdPermissionName}");
        }
    }
}