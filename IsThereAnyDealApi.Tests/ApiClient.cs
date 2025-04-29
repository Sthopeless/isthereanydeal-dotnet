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
                // GetApiKeyFromConfig now handles both env vars and the file
                _apiKey = GetApiKeyFromConfig();
                _client = new IsThereAnyDealApiClient(_apiKey);
            }
            catch (InvalidOperationException ex)
            {
                // Handle case where API key isn't found in either env var or file
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

        // Helper method to get API key from environment variable or testsettings.json
        private string GetApiKeyFromConfig()
        {
            // Prioritize Environment Variable (commonly used in CI/CD)
            string? apiKeyFromEnv = Environment.GetEnvironmentVariable("ITAD_API_KEY"); // Use the same name as your GitHub Secret
            if (!string.IsNullOrWhiteSpace(apiKeyFromEnv))
            {
                Console.WriteLine("Using API Key from environment variable."); // Optional: for logging in CI
                return apiKeyFromEnv;
            }

            // Fallback to JSON file (for local development)
            Console.WriteLine("API Key environment variable not found, trying testsettings.json...");
            // Ensure your testsettings.json file has "Copy to Output Directory" set to "Copy if newer" or "Copy always"
            // in its file properties in Visual Studio / Rider, or configure your build process accordingly.
            var configPath = Path.Combine(AppContext.BaseDirectory, "testsettings.json");

            if (!File.Exists(configPath))
            {
                 // Now this error is more relevant for local setup if env var is also missing
                 throw new InvalidOperationException($"Neither ITAD_API_KEY environment variable nor configuration file '{configPath}' found. Set the environment variable in CI or ensure 'testsettings.json' exists locally and is copied to output.");
            }

            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory) // Ensures it finds the file during test run
                .AddJsonFile("testsettings.json", optional: true, reloadOnChange: false) // Make optional: true as env var is primary now
                .Build();

            var apiKeyFromJson = config["ITAD_API_KEY"]; // Access the key (key name in JSON file)

            if (string.IsNullOrWhiteSpace(apiKeyFromJson))
            {
                // This exception means env var was missing AND file was missing or key was empty in file
                throw new InvalidOperationException("ITAD_API_KEY not found or empty in 'testsettings.json' and not set as environment variable.");
            }

            Console.WriteLine("Using API Key from testsettings.json.");
            return apiKeyFromJson;
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
            // Skip test if client initialization failed (API key couldn't be loaded)
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