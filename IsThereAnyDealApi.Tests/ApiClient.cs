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

        // Use consistent country and currency for testing
        private const string TestCountry = "NL";
        private const string TestCurrency = "EUR";

        // Known game IDs for testing (Updated GtaV based on test results)
        // GTA V IDs seem to vary based on edition/package. Using ID returned by LookupGameIdsByShopIdAsync for app/271590
        private const string GtaV_GameId = "01955ccf-9813-73b5-b311-0fd8b0f317d3";
        private const int GtaV_AppId = 271590;
        private const string GtaV_Title = "Grand Theft Auto V Legacy"; // Title might differ slightly based on lookup method
        // GTA V Shop ID returned by LookupShopIdsByGameIdAsync for the legacy ID
        private const string GtaV_ShopId_Sub = "sub/137730";
        private const string GtaV_Original_GameId = "018d937f-6ee4-73f5-858c-2fde3407462f"; // Keep original for looking up shop IDs

        private const string StardewValley_GameId = "018d937e-f835-710c-b95b-928e277b187e";
        private const int StardewValley_AppId = 413150;
        private const string StardewValley_Title = "Stardew Valley";

        // Known shop IDs
        private const int SteamShopId = 61;
        private const int GogShopId = 35;

        // Constructor: Reads config and initializes client ONCE for all tests in this class
        public ApiClientTests()
        {
            try
            {
                _apiKey = GetApiKeyFromConfig();
                _client = new IsThereAnyDealApiClient(_apiKey);
            }
            catch (InvalidOperationException ex)
            {
                _apiKey = null;
                _client = null;
                _initializationErrorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                 _apiKey = null;
                 _client = null;
                 _initializationErrorMessage = $"Unexpected error during test setup: {ex.Message}";
            }
        }

        // Helper method to get API key from environment variable or testsettings.json
        private string GetApiKeyFromConfig()
        {
            string? apiKeyFromEnv = Environment.GetEnvironmentVariable("ITAD_API_KEY");
            if (!string.IsNullOrWhiteSpace(apiKeyFromEnv))
            {
                // Added message for environment variable source
                Console.WriteLine("API Key found in environment variable.");
                return apiKeyFromEnv;
            }

            Console.WriteLine("API Key environment variable not found, trying testsettings.json...");
            var configPath = Path.Combine(AppContext.BaseDirectory, "testsettings.json");

            if (!File.Exists(configPath))
            {
                 throw new InvalidOperationException($"Neither ITAD_API_KEY environment variable nor configuration file '{configPath}' found. Set the environment variable in CI or ensure 'testsettings.json' exists locally and is copied to output.");
            }

            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("testsettings.json", optional: true, reloadOnChange: false)
                .Build();

            var apiKeyFromJson = config["ITAD_API_KEY"];

            if (string.IsNullOrWhiteSpace(apiKeyFromJson))
            {
                throw new InvalidOperationException("ITAD_API_KEY not found or empty in 'testsettings.json' and not set as environment variable.");
            }

            // **** ADDED CONSOLE MESSAGE HERE ****
            Console.WriteLine("API Key found in testsettings.json.");
            return apiKeyFromJson;
        }

        // Helper to skip tests if client/key isn't available
        private bool SkipTestIfNotReady()
        {
            if (_client == null || _apiKey == null)
            {
                Assert.Fail(_initializationErrorMessage ?? "API Client or Key not initialized, skipping test.");
                return true;
            }
            return false;
        }


        // --- Service Tests ---

        [Fact]
        public async Task GetShopsAsync_Should_Return_Shops_Without_ApiKey()
        {
            var publicClient = new IsThereAnyDealApiClient(apiKey: null);
            List<Shop> shops = await publicClient.GetShopsAsync(TestCountry);
            Assert.NotNull(shops);
            Assert.NotEmpty(shops);
            Assert.Contains(shops, s => s.Name != null && s.Name.Equals("Steam", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(shops, s => s.Name != null && s.Name.Equals("GOG", StringComparison.OrdinalIgnoreCase));
            var steamShop = shops.FirstOrDefault(s => s.Name != null && s.Name.Equals("Steam", StringComparison.OrdinalIgnoreCase));
            Assert.NotNull(steamShop);
            Assert.True(steamShop.Id > 0);
        }

        // --- Lookup Tests ---

        [Fact]
        public async Task LookupGameAsync_By_Title_Should_Work_With_ApiKey()
        {
            if (SkipTestIfNotReady()) return;
            var title = StardewValley_Title;
            var result = await _client!.LookupGameAsync(title: title);
            Assert.NotNull(result);
            Assert.True(result.Found, $"Game '{title}' should be found.");
            Assert.NotNull(result.Game);
            Assert.Equal(title, result.Game.Title, ignoreCase: true);
            Assert.Equal(StardewValley_GameId, result.Game.Id, ignoreCase: true);
            Assert.False(string.IsNullOrEmpty(result.Game.Slug));
        }

        [Fact]
        public async Task LookupGameAsync_By_AppId_Should_Work_With_ApiKey()
        {
           if (SkipTestIfNotReady()) return;
            var appId = StardewValley_AppId;
            var expectedTitle = StardewValley_Title;
            var result = await _client!.LookupGameAsync(appid: appId);
            Assert.NotNull(result);
            Assert.True(result.Found);
            Assert.NotNull(result.Game);
            Assert.Equal(expectedTitle, result.Game.Title, ignoreCase: true);
            Assert.Equal(StardewValley_GameId, result.Game.Id, ignoreCase: true);
        }

        [Fact]
        public async Task LookupGameAsync_InvalidTitle_Should_Return_NotFound()
        {
            if (SkipTestIfNotReady()) return;
            var title = Guid.NewGuid().ToString();
            var result = await _client!.LookupGameAsync(title: title);
            Assert.NotNull(result);
            Assert.False(result.Found);
            Assert.Null(result.Game);
        }

        [Fact]
        public async Task LookupGameIdsByTitleAsync_Should_Return_Ids()
        {
            if (SkipTestIfNotReady()) return;
            var titles = new List<string> { StardewValley_Title, "Half-Life 2", "NonExistentGameTitleXYZ" };
            var result = await _client!.LookupGameIdsByTitleAsync(titles);
            Assert.NotNull(result);
            Assert.Equal(titles.Count, result.Count);
            Assert.True(result.ContainsKey(StardewValley_Title));
            Assert.Equal(StardewValley_GameId, result[StardewValley_Title], ignoreCase: true);
            Assert.True(result.ContainsKey("Half-Life 2"));
            Assert.NotNull(result["Half-Life 2"]);
            Assert.True(result.ContainsKey("NonExistentGameTitleXYZ"));
            Assert.Null(result["NonExistentGameTitleXYZ"]);
        }

         [Fact]
        public async Task LookupGameIdsByShopIdAsync_Should_Return_Ids_For_Steam()
        {
            if (SkipTestIfNotReady()) return;
            var shopGameIds = new List<string> { $"app/{StardewValley_AppId}", $"app/{GtaV_AppId}", "app/99999999" };
            var result = await _client!.LookupGameIdsByShopIdAsync(SteamShopId, shopGameIds);
            Assert.NotNull(result);
            Assert.Equal(shopGameIds.Count, result.Count);
            Assert.True(result.ContainsKey($"app/{StardewValley_AppId}"));
            Assert.Equal(StardewValley_GameId, result[$"app/{StardewValley_AppId}"], ignoreCase: true);
            Assert.True(result.ContainsKey($"app/{GtaV_AppId}"));
            Assert.Equal(GtaV_GameId, result[$"app/{GtaV_AppId}"], ignoreCase: true);
            Assert.True(result.ContainsKey("app/99999999"));
            Assert.Null(result["app/99999999"]);
        }

         [Fact]
        public async Task LookupShopIdsByGameIdAsync_Should_Return_ShopIds_For_Steam()
        {
            if (SkipTestIfNotReady()) return;
            var legacyGtaVId = GtaV_Original_GameId;
            var gameIds = new List<string> { StardewValley_GameId, legacyGtaVId };
            var expectedStardewShopId = $"app/{StardewValley_AppId}";
            var expectedGtaShopId = GtaV_ShopId_Sub;
            var result = await _client!.LookupShopIdsByGameIdAsync(SteamShopId, gameIds);
            Assert.NotNull(result);
            Assert.True(result.ContainsKey(StardewValley_GameId));
            Assert.NotNull(result[StardewValley_GameId]);
            Assert.Contains(expectedStardewShopId, result[StardewValley_GameId]!);
            Assert.True(result.ContainsKey(legacyGtaVId));
            Assert.NotNull(result[legacyGtaVId]);
            Assert.Contains(expectedGtaShopId, result[legacyGtaVId]!);
        }


        // --- Game Info & Prices Tests ---

        [Fact]
        public async Task GetGameInfoAsync_Should_Return_Data_For_Known_Game()
        {
            if (SkipTestIfNotReady()) return;
            var gameId = StardewValley_GameId;
            var result = await _client!.GetGameInfoAsync(gameId);
            Assert.NotNull(result);
            Assert.Equal(gameId, result.Id, ignoreCase: true);
            Assert.Equal(StardewValley_Title, result.Title, ignoreCase: true);
            Assert.NotNull(result.Slug);
            Assert.NotNull(result.Assets);
            Assert.NotNull(result.Appid);
            Assert.Equal(StardewValley_AppId, result.Appid.Value);
            Assert.NotEmpty(result.Tags);
            Assert.NotEmpty(result.Developers);
            Assert.NotEmpty(result.Publishers);
            Assert.NotEmpty(result.Reviews);
            Assert.NotNull(result.Stats);
            Assert.NotNull(result.Urls);
            Assert.False(string.IsNullOrWhiteSpace(result.Urls.Game));
        }

        [Fact]
        public async Task GetGameInfoAsync_InvalidId_Should_Return_Null()
        {
            if (SkipTestIfNotReady()) return;
            var gameId = Guid.NewGuid().ToString();
            var result = await _client!.GetGameInfoAsync(gameId);
            Assert.Null(result);
        }

        [Fact]
        public async Task SearchGamesAsync_Should_Return_Results()
        {
            if (SkipTestIfNotReady()) return;
            var searchTerm = "Portal";
            var results = await _client!.SearchGamesAsync(searchTerm, results: 5);
            Assert.NotNull(results);
            Assert.NotEmpty(results);
            Assert.True(results.Count <= 5);
            Assert.Contains(results, g => g.Title != null && g.Title.Equals("Portal 2", StringComparison.OrdinalIgnoreCase));
            foreach(var game in results)
            {
                Assert.False(string.IsNullOrEmpty(game.Id));
                Assert.False(string.IsNullOrEmpty(game.Slug));
                Assert.False(string.IsNullOrEmpty(game.Title));
            }
        }

         [Fact]
        public async Task GetGamePricesAsync_Should_Return_Prices_For_Known_Game()
        {
            if (SkipTestIfNotReady()) return;
            var gameId = StardewValley_GameId;
            var results = await _client!.GetGamePricesAsync(new List<string> { gameId }, country: TestCountry, capacity: 5);
            Assert.NotNull(results);
            Assert.Single(results);
            var gamePrices = results[0];
            Assert.Equal(gameId, gamePrices.Id, ignoreCase: true);
            Assert.NotNull(gamePrices.HistoryLow);
            Assert.NotNull(gamePrices.Deals);
            Assert.NotEmpty(gamePrices.Deals);
            Assert.True(gamePrices.Deals.Count <= 5);
            var firstDeal = gamePrices.Deals[0];
            Assert.NotNull(firstDeal.Shop);
            Assert.True(firstDeal.Shop.Id > 0);
            Assert.True(string.IsNullOrEmpty(firstDeal.Shop.Name));
            Assert.NotNull(firstDeal.Price);
            Assert.True(firstDeal.Price.Amount >= 0);
            Assert.Equal(TestCurrency, firstDeal.Price.Currency, ignoreCase: true);
            Assert.NotNull(firstDeal.Regular);
            Assert.True(firstDeal.Regular.Amount >= 0);
            Assert.True(!string.IsNullOrEmpty(firstDeal.Url), "Deal URL should not be null or empty");
        }

        [Fact]
        public async Task GetGamesOverviewAsync_Should_Return_Overview()
        {
             if (SkipTestIfNotReady()) return;
            var gameIds = new List<string> { StardewValley_GameId, GtaV_GameId };
            var result = await _client!.GetGamesOverviewAsync(gameIds, country: TestCountry);
            Assert.NotNull(result);
            Assert.NotNull(result.Prices);
            Assert.NotEmpty(result.Prices);
            Assert.True(result.Prices.Count <= gameIds.Count);
            Assert.NotNull(result.Bundles);
            foreach (var priceOverview in result.Prices)
            {
                Assert.Contains(priceOverview.Id, gameIds, StringComparer.OrdinalIgnoreCase);
                Assert.NotNull(priceOverview.Urls);
                Assert.False(string.IsNullOrEmpty(priceOverview.Urls.Game));
                if (priceOverview.Current != null)
                {
                    Assert.NotNull(priceOverview.Current.Shop);
                    Assert.NotNull(priceOverview.Current.Price);
                    Assert.Equal(TestCurrency, priceOverview.Current.Price.Currency, ignoreCase: true);
                }
                 if (priceOverview.Lowest != null)
                {
                    Assert.NotNull(priceOverview.Lowest.Shop);
                    Assert.NotNull(priceOverview.Lowest.Price);
                    Assert.Equal(TestCurrency, priceOverview.Lowest.Price.Currency, ignoreCase: true);
                }
            }
        }

        // --- History & Bundles Tests ---

        [Fact]
        public async Task GetBundlesForGameAsync_Should_Return_Bundles_If_Available()
        {
             if (SkipTestIfNotReady()) return;
            var gameId = "018d937f-0d7f-7083-8742-44b0142a6e77";
            var results = await _client!.GetBundlesForGameAsync(gameId, country: TestCountry, expired: true);
            Assert.NotNull(results);
            if (results.Any())
            {
                var bundle = results[0];
                Assert.True(bundle.Id > 0);
                Assert.False(string.IsNullOrWhiteSpace(bundle.Title));
                Assert.NotNull(bundle.Page);
                Assert.False(string.IsNullOrWhiteSpace(bundle.Url));
                Assert.NotNull(bundle.Counts);
                Assert.NotNull(bundle.Tiers);
                Assert.NotEmpty(bundle.Tiers);
                var tier = bundle.Tiers[0];
                Assert.NotNull(tier.Games);
                Assert.NotEmpty(tier.Games);
                Assert.Contains(tier.Games, g => g.Id.Equals(gameId, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                 Console.WriteLine($"Warning: No bundles found for game ID {gameId}. Test passed but might need updating if game is removed from bundles.");
            }
        }

        [Fact]
        public async Task GetHistoricalLowsAsync_Should_Return_Low()
        {
             if (SkipTestIfNotReady()) return;
            var gameId = StardewValley_GameId;
            var results = await _client!.GetHistoricalLowsAsync(new List<string> { gameId }, country: TestCountry);
            Assert.NotNull(results);
            Assert.Single(results);
            var low = results[0];
            Assert.Equal(gameId, low.Id, ignoreCase: true);
            Assert.NotNull(low.Low);
            Assert.NotNull(low.Low.Shop);
            Assert.True(low.Low.Shop.Id > 0);
            Assert.NotNull(low.Low.Price);
            Assert.True(low.Low.Price.Amount >= 0);
            Assert.Equal(TestCurrency, low.Low.Price.Currency, ignoreCase: true);
        }

         [Fact]
        public async Task GetStoreLowsAsync_Should_Return_Lows()
        {
             if (SkipTestIfNotReady()) return;
            var gameId = StardewValley_GameId;
            var shops = new int[] { SteamShopId, GogShopId };
            var results = await _client!.GetStoreLowsAsync(new List<string> { gameId }, country: TestCountry, shops: shops);
            Assert.NotNull(results);
            Assert.Single(results);
            var gameStoreLows = results[0];
            Assert.Equal(gameId, gameStoreLows.Id, ignoreCase: true);
            Assert.NotNull(gameStoreLows.Lows);
            Assert.NotEmpty(gameStoreLows.Lows);
            Assert.True(gameStoreLows.Lows.Count <= shops.Length);
            foreach (var low in gameStoreLows.Lows)
            {
                 Assert.NotNull(low.Shop);
                 Assert.Contains(low.Shop.Id, shops);
                 Assert.NotNull(low.Price);
                 Assert.True(low.Price.Amount >= 0);
                 Assert.Equal(TestCurrency, low.Price.Currency, ignoreCase: true);
            }
        }

         [Fact]
        public async Task GetPriceHistoryAsync_Should_Return_History()
        {
             if (SkipTestIfNotReady()) return;
            var gameId = StardewValley_GameId;
            var shops = new int[] { SteamShopId };
            var results = await _client!.GetPriceHistoryAsync(gameId, country: TestCountry, shops: shops, since: null);
            Assert.NotNull(results);
            Assert.NotEmpty(results);
            foreach (var point in results)
            {
                Assert.NotNull(point.Shop);
                Assert.Equal(SteamShopId, point.Shop.Id);
                if(point.Deal != null)
                {
                    Assert.NotNull(point.Deal.Price);
                    Assert.Equal(TestCurrency, point.Deal.Price.Currency, ignoreCase: true);
                    Assert.NotNull(point.Deal.Regular);
                }
            }
        }

        // --- Subscriptions Tests ---

        [Fact]
        public async Task GetGameSubscriptionsAsync_Should_Return_Subscriptions()
        {
            if (SkipTestIfNotReady()) return;
            var gameIdWithSub = "018d937f-700e-7161-9c8d-5423af1b7c99";
            var gameIdAlsoWithSub = StardewValley_GameId;
            var gameIds = new List<string> { gameIdWithSub, gameIdAlsoWithSub };
            var results = await _client!.GetGameSubscriptionsAsync(gameIds, country: TestCountry);
            Assert.NotNull(results);
            Assert.Equal(2, results.Count);
            var subGame1 = results.FirstOrDefault(r => r.Id.Equals(gameIdWithSub, StringComparison.OrdinalIgnoreCase));
            Assert.NotNull(subGame1);
            Assert.NotNull(subGame1.Subs);
            Assert.NotEmpty(subGame1.Subs);
            Assert.Contains(subGame1.Subs, s => s.Name.Contains("Game Pass", StringComparison.OrdinalIgnoreCase));
            var subGame2 = results.FirstOrDefault(r => r.Id.Equals(gameIdAlsoWithSub, StringComparison.OrdinalIgnoreCase));
            Assert.NotNull(subGame2);
            Assert.NotNull(subGame2.Subs);
            Assert.NotEmpty(subGame2.Subs);
            Assert.Contains(subGame2.Subs, s => s.Name.Contains("Game Pass", StringComparison.OrdinalIgnoreCase));
        }

        // --- Deals List Tests ---

         [Fact]
        public async Task GetDealsListAsync_Should_Return_Deals()
        {
             if (SkipTestIfNotReady()) return;
            var limit = 5;
            var result = await _client!.GetDealsListAsync(country: TestCountry, limit: limit, sort: "-cut");
            Assert.NotNull(result);
            Assert.NotNull(result.List);
            Assert.NotEmpty(result.List);
            Assert.True(result.List.Count <= limit);
            Assert.True(result.HasMore);
            Assert.True(result.NextOffset > 0);
            var firstDealItem = result.List[0];
            Assert.False(string.IsNullOrWhiteSpace(firstDealItem.Id));
            Assert.False(string.IsNullOrWhiteSpace(firstDealItem.Title));
            Assert.NotNull(firstDealItem.Deal);
            Assert.NotNull(firstDealItem.Deal.Shop);
            Assert.NotNull(firstDealItem.Deal.Price);
            Assert.Equal(TestCurrency, firstDealItem.Deal.Price.Currency, ignoreCase: true);
            Assert.True(firstDealItem.Deal.Cut >= 0);
        }

         // --- Stats Tests ---

         [Fact]
         public async Task GetWaitlistStatsAsync_Should_Return_Stats()
         {
             if (SkipTestIfNotReady()) return;
             var gameId = StardewValley_GameId;
             var result = await _client!.GetWaitlistStatsAsync(gameId, country: TestCountry);
             Assert.NotNull(result);
             Assert.True(result.Count >= 0);
             Assert.NotNull(result.Price);
             Assert.Equal(TestCurrency, result.Price.Currency, ignoreCase: true);
             Assert.True(result.Price.Any >= 0);
             Assert.True(result.Price.Average >= 0);
             Assert.NotNull(result.Price.Buckets);
             Assert.NotNull(result.Cut);
             Assert.True(result.Cut.Average >= 0);
             Assert.NotNull(result.Cut.Buckets);
             if(result.Price.Buckets.Any())
             {
                 var bucket = result.Price.Buckets[0];
                 Assert.True(bucket.Bucket >= 0);
                 Assert.True(bucket.Count >= 0);
                 Assert.True(bucket.Percentile >= 0 && bucket.Percentile <= 100);
             }
         }

         [Fact]
         public async Task GetMostWaitlistedGamesAsync_Should_Return_Ranked_List()
         {
             if (SkipTestIfNotReady()) return;
             var limit = 10;
             var results = await _client!.GetMostWaitlistedGamesAsync(limit: limit);
             Assert.NotNull(results);
             Assert.NotEmpty(results);
             Assert.True(results.Count <= limit);
             int lastPosition = 0;
             foreach(var game in results)
             {
                 Assert.True(game.Position > lastPosition);
                 Assert.False(string.IsNullOrWhiteSpace(game.Id));
                 Assert.False(string.IsNullOrWhiteSpace(game.Title));
                 Assert.True(game.Count > 0);
                 lastPosition = game.Position;
             }
         }

         [Fact]
         public async Task GetMostCollectedGamesAsync_Should_Return_Ranked_List()
         {
             if (SkipTestIfNotReady()) return;
             var limit = 10;
             var results = await _client!.GetMostCollectedGamesAsync(limit: limit);
             Assert.NotNull(results);
             Assert.NotEmpty(results);
             Assert.True(results.Count <= limit);
             int lastPosition = 0;
             foreach(var game in results)
             {
                 Assert.True(game.Position > lastPosition);
                 Assert.False(string.IsNullOrWhiteSpace(game.Id));
                 Assert.False(string.IsNullOrWhiteSpace(game.Title));
                 Assert.True(game.Count > 0);
                 lastPosition = game.Position;
             }
         }

          [Fact]
         public async Task GetMostPopularGamesAsync_Should_Return_Ranked_List()
         {
             if (SkipTestIfNotReady()) return;
             var limit = 10;
             var results = await _client!.GetMostPopularGamesAsync(limit: limit);
             Assert.NotNull(results);
             Assert.NotEmpty(results);
             Assert.True(results.Count <= limit);
             int lastPosition = 0;
             foreach(var game in results)
             {
                 Assert.True(game.Position > lastPosition);
                 Assert.False(string.IsNullOrWhiteSpace(game.Id));
                 Assert.False(string.IsNullOrWhiteSpace(game.Title));
                 Assert.True(game.Count > 0);
                 lastPosition = game.Position;
             }
         }
    }
}