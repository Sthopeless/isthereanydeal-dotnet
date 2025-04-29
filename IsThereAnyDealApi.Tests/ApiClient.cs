using Xunit; // NuGet: xunit
using System;
using System.Threading.Tasks;
using System.Linq;
using System.IO; // Added for File operations
using Microsoft.Extensions.Configuration; // NuGet: Microsoft.Extensions.Configuration & Microsoft.Extensions.Configuration.Json
using IsThereAnyDeal.Api; // Reference your main project's namespace
using IsThereAnyDeal.Api.Models; // Reference your models namespace
using System.Collections.Generic;

// Define a namespace for your tests
namespace IsThereAnyDeal.Api.Tests
{
    public class ApiClientTests
    {
        private readonly IsThereAnyDealApiClient? _client; // Nullable if initialization fails
        private readonly string? _apiKey; // Nullable if initialization fails
        private readonly string? _initializationErrorMessage; // To store errors during setup

        // Constructor: Reads config and initializes client ONCE for all tests in this class
        public ApiClientTests()
        {
            try
            {
                _apiKey = GetApiKeyFromConfig(); // Load API key from testsettings.json
                _client = new IsThereAnyDealApiClient(_apiKey);
            }
            catch (InvalidOperationException ex)
            {
                // Handle case where file/key isn't found
                _apiKey = null;
                _client = null;
                _initializationErrorMessage = ex.Message; // Store error message
            }
            catch (Exception ex) // Catch other potential errors during setup
            {
                 _apiKey = null;
                 _client = null;
                 _initializationErrorMessage = $"Unexpected error during test setup: {ex.Message}";
            }
        }

        // Helper method to get API key from testsettings.json
        private string GetApiKeyFromConfig()
        {
            // Ensure your testsettings.json file has "Copy to Output Directory" set to "Copy if newer" or "Copy always"
            // in its file properties in Visual Studio / Rider, or configure your build process accordingly.
            var configPath = Path.Combine(AppContext.BaseDirectory, "testsettings.json");

            if (!File.Exists(configPath))
            {
                 throw new InvalidOperationException($"Configuration file '{configPath}' not found. Create 'testsettings.json' in the test project root with your ITAD_API_KEY and ensure it's copied to the output directory.");
            }

            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory) // Ensures it finds the file during test run
                .AddJsonFile("testsettings.json", optional: false, reloadOnChange: false)
                .Build();

            var apiKey = config["ITAD_API_KEY"]; // Access the key (key name in JSON file)

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("ITAD_API_KEY not found or empty in 'testsettings.json'.");
            }
            return apiKey;
        }


        // --- Test Methods ---

        [Fact]
        public async Task GetShopsAsync_Should_Return_Shops_Without_ApiKey()
        {
            // Arrange
            // Instantiate a separate client *without* an API key specifically for this test,
            // as the class constructor now tries to load one.
            // This ensures the test verifies the endpoint works publicly.
            var publicClient = new IsThereAnyDealApiClient(apiKey: null);

            // Act
            List<Shop> shops = await publicClient.GetShopsAsync("US"); // Example country code

            // Assert
            Assert.NotNull(shops);
            Assert.NotEmpty(shops);
            Assert.Contains(shops, s => s.Name != null && s.Name.Equals("Steam", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(shops, s => s.Name != null && s.Name.Equals("GOG", StringComparison.OrdinalIgnoreCase));
            var steamShop = shops.FirstOrDefault(s => s.Name != null && s.Name.Equals("Steam", StringComparison.OrdinalIgnoreCase));
            Assert.NotNull(steamShop);
            Assert.True(steamShop.Id > 0);
        }

        [Fact]
        public async Task LookupGameAsync_By_Title_Should_Work_With_ApiKey()
        {
            // Skip test if client initialization failed
            if (_client == null || _apiKey == null)
            {
                // Using Assert.Skip is cleaner if available in your xUnit version
                // Assert.Skip(_initializationErrorMessage ?? "API Client or Key not initialized.");
                // Fallback:
                Assert.Fail(_initializationErrorMessage ?? "API Client or Key not initialized, skipping test.");
                return; // Keep compiler happy
            }

            // Arrange
            var title = "Grand Theft Auto V"; // A known game

            // Act
            var result = await _client.LookupGameAsync(title: title);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Found, $"Game '{title}' should be found.");
            Assert.NotNull(result.Game);
            Assert.Equal(title, result.Game.Title, ignoreCase: true);
            Assert.False(string.IsNullOrEmpty(result.Game.Id)); // Should have an ID
        }

        // --- Add other tests requiring API Key below, using the same skip logic ---

        [Fact]
        public async Task LookupGameAsync_By_AppId_Should_Work_With_ApiKey()
        {
            if (_client == null || _apiKey == null)
            {
                Assert.Fail(_initializationErrorMessage ?? "API Client or Key not initialized, skipping test.");
                return;
            }

            // Arrange
            var appId = 271590; // Steam AppID for GTA V
            var expectedTitle = "Grand Theft Auto V Legacy";

            // Act
            var result = await _client.LookupGameAsync(appid: appId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Found);
            Assert.NotNull(result.Game);
            Assert.Equal(expectedTitle, result.Game.Title, ignoreCase: true);
        }

        // TODO: Add more integration tests here...
        // - Test GetGameInfoAsync with a known game ID
        // - Test GetGamePricesAsync
        // - Test endpoints with various parameters (different countries, shops, etc.)
    }
}