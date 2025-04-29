using RestEase;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using IsThereAnyDeal.Api.Models;

namespace IsThereAnyDeal.Api
{
    /// <summary>
    /// Client for interacting with the IsThereAnyDeal API v2.
    /// </summary>
    public sealed class IsThereAnyDealApiClient
    {
        private const string DefaultApiBaseUrl = "https://api.isthereanydeal.com";
        private readonly IIsThereAnyDealApi _api; // Uses the interface now defined in IIsThereAnyDealApi.cs
        private readonly string? _apiKey; // Store the API key

        // TODO: Add properties/methods for OAuth token management
        private string? _accessToken; // Example placeholder for OAuth token

        public static JsonSerializerSettings DefaultJsonSerializerSettings = new JsonSerializerSettings()
        {
            ContractResolver = new DefaultContractResolver
            {
                // NamingStrategy = new CamelCaseNamingStrategy() // Usually default
            },
            NullValueHandling = NullValueHandling.Ignore,
        };

        /// <summary>
        /// Creates a new IsThereAnyDeal API client.
        /// </summary>
        /// <param name="apiKey">Your ITAD API Key. Required for most non-user-specific endpoints.</param>
        /// <param name="apiBaseUrl">Optional base URL for the API.</param>
        public IsThereAnyDealApiClient(string? apiKey = null, string apiBaseUrl = DefaultApiBaseUrl)
        {
            _apiKey = apiKey;

            // 1. Create and configure the HttpClient
            var httpClient = new HttpClient() {
                 BaseAddress = new Uri(apiBaseUrl)
                 // Optionally add default headers like User-Agent
                 // httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("YourApp/1.0 (contact@example.com)");
            };

            // 2. Create the RestClient instance, passing the configured HttpClient
            var restClient = new RestClient(httpClient)
            {
                // 3. Set the custom JsonSerializerSettings on the RestClient instance
                JsonSerializerSettings = DefaultJsonSerializerSettings
            };

            // 4. Create the typed API interface implementation from the configured RestClient
            _api = restClient.For<IIsThereAnyDealApi>();
        }

        // Helper method to ensure API key is available when needed
        private void EnsureApiKey()
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                throw new InvalidOperationException("An API Key is required for this operation but was not provided during client initialization.");
            }
        }

        // Helper method to get the Authorization header string (replace with real token management)
        private string GetAuthorizationHeader()
        {
            if (string.IsNullOrWhiteSpace(_accessToken))
            {
                throw new InvalidOperationException("User is not authenticated. Access token is missing.");
            }
            return $"Bearer {_accessToken}";
        }

        // --- Public API Methods ---
        // These methods call the corresponding methods defined in IIsThereAnyDealApi

        /// <summary>
        /// Gets information about shops.
        /// </summary>
        /// <param name="country">Two letter country code (ISO 3166-1 alpha-2).</param>
        /// <returns>A list of shops.</returns>
        public async Task<List<Shop>> GetShopsAsync(string country = "NL")
        {
            // No API key needed for this endpoint per spec
            // TODO: Add try/catch for ApiException
            return await _api.GetShopsAsync(country);
        }

        /// <summary>
        /// Looks up an ITAD game using title or Steam AppID. Requires API Key.
        /// </summary>
        /// <param name="title">Game title.</param>
        /// <param name="appid">Steam AppID.</param>
        /// <returns>Lookup result.</returns>
        public async Task<GameLookupResult> LookupGameAsync(string? title = null, int? appid = null)
        {
            EnsureApiKey();
            if (string.IsNullOrWhiteSpace(title) && !appid.HasValue)
            {
                throw new ArgumentException("Either title or appid must be provided for game lookup.");
            }
            // TODO: Add try/catch for ApiException
            // Pass the stored _apiKey to the interface method where the [Query("key")] attribute is defined
            return await _api.LookupGameAsync(title, appid, _apiKey);
        }

        /// <summary>
        /// Looks up ITAD game IDs by their titles. Requires API Key.
        /// </summary>
        /// <param name="titles">A list of game titles.</param>
        /// <returns>A dictionary mapping requested titles to found ITAD Game IDs (string/UUID or null if not found).</returns>
        public async Task<Dictionary<string, string?>> LookupGameIdsByTitleAsync(List<string> titles)
        {
            EnsureApiKey();
            if (titles == null || titles.Count == 0) return new Dictionary<string, string?>();
            // TODO: Add try/catch for ApiException
            return await _api.LookupGameIdsByTitleAsync(titles, _apiKey);
        }

        /// <summary>
        /// Looks up ITAD game IDs by shop-specific IDs. Requires API Key.
        /// </summary>
        /// <param name="shopId">The ID of the shop.</param>
        /// <param name="shopGameIds">List of game IDs specific to the shop.</param>
        /// <returns>Dictionary mapping shop game ID to ITAD game ID.</returns>
        public async Task<Dictionary<string, string?>> LookupGameIdsByShopIdAsync(int shopId, List<string> shopGameIds)
        {
             EnsureApiKey();
             if (shopGameIds == null || shopGameIds.Count == 0) return new Dictionary<string, string?>();
             // TODO: Add try/catch for ApiException
             return await _api.LookupGameIdsByShopIdAsync(shopId, shopGameIds, _apiKey);
        }

        /// <summary>
        /// Looks up shop-specific game IDs by ITAD game IDs. Requires API Key.
        /// </summary>
        /// <param name="shopId">The ID of the shop.</param>
        /// <param name="gameIds">List of ITAD game IDs.</param>
        /// <returns>Dictionary mapping ITAD game ID to a list of shop-specific IDs.</returns>
        public async Task<Dictionary<string, List<string>?>> LookupShopIdsByGameIdAsync(int shopId, List<string> gameIds)
        {
            EnsureApiKey();
            if (gameIds == null || gameIds.Count == 0) return new Dictionary<string, List<string>?>();
            // TODO: Add try/catch for ApiException
            return await _api.LookupShopIdsByGameIdAsync(shopId, gameIds, _apiKey);
        }

        /// <summary>
        /// Gets detailed information for a specific game. Requires API Key.
        /// </summary>
        /// <param name="gameId">ITAD Game ID (UUID format).</param>
        /// <returns>Game information or null if not found.</returns>
        public async Task<GameInfo?> GetGameInfoAsync(string gameId)
        {
             EnsureApiKey();
             try
             {
                 // Pass the stored _apiKey to the interface method
                 return await _api.GetGameInfoAsync(gameId, _apiKey);
             }
             catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
             {
                 return null;
             }
        }

        /// <summary>
        /// Searches for games by title. Requires API Key.
        /// </summary>
        /// <param name="title">Search term.</param>
        /// <param name="results">Maximum number of results.</param>
        /// <returns>List of search results (basic game info).</returns>
        public async Task<List<Game>> SearchGamesAsync(string title, int results = 20)
        {
            EnsureApiKey();
            // TODO: Add try/catch for ApiException
            return await _api.SearchGamesAsync(title, results, _apiKey);
        }

        /// <summary>
        /// Gets current prices for a list of games. Requires API Key.
        /// </summary>
        /// <param name="gameIds">List of ITAD Game IDs.</param>
        /// <param name="country">ISO 3166-1 alpha-2 country code.</param>
        /// <param name="shops">Optional list of shop IDs to filter by.</param>
        /// <param name="vouchers">Allow prices with vouchers.</param>
        /// <param name="dealsOnly">Only include prices that are currently discounted.</param>
        /// <param name="capacity">Max number of prices per game (0 for no limit).</param>
        /// <returns>List of price information for each requested game.</returns>
        public async Task<List<GamePrices>> GetGamePricesAsync(
            List<string> gameIds,
            string country = "NL",
            int[]? shops = null,
            bool vouchers = true,
            bool dealsOnly = false,
            int capacity = 0)
        {
            EnsureApiKey();
            if (gameIds == null || gameIds.Count == 0) return new List<GamePrices>();
            // TODO: Add try/catch for ApiException
            return await _api.GetGamePricesAsync(gameIds, country, dealsOnly, vouchers, capacity, shops, _apiKey);
        }

         /// <summary>
        /// Gets overview (best current price, historical low, bundles) for games. Requires API Key.
        /// </summary>
        /// <param name="gameIds">List of ITAD Game IDs.</param>
        /// <param name="country">ISO 3166-1 alpha-2 country code.</param>
        /// <param name="shops">Optional list of shop IDs to filter current prices.</param>
        /// <param name="vouchers">Allow prices with vouchers.</param>
        /// <returns>Overview result.</returns>
        public async Task<GameOverviewResult> GetGamesOverviewAsync(
            List<string> gameIds,
            string country = "NL",
            int[]? shops = null,
            bool vouchers = true)
        {
            EnsureApiKey();
            if (gameIds == null || gameIds.Count == 0) return new GameOverviewResult(); // Return empty result
            // TODO: Add try/catch for ApiException
            return await _api.GetGamesOverviewAsync(gameIds, country, shops, vouchers, _apiKey);
        }

        /// <summary>
        /// Gets bundles containing a specific game. Requires API Key.
        /// </summary>
        /// <param name="gameId">ITAD Game ID.</param>
        /// <param name="country">Country code.</param>
        /// <param name="expired">Include expired bundles.</param>
        /// <returns>List of bundles.</returns>
        public async Task<List<Bundle>> GetBundlesForGameAsync(string gameId, string country = "NL", bool expired = false)
        {
            EnsureApiKey();
            // TODO: Add try/catch for ApiException
            return await _api.GetBundlesForGameAsync(gameId, country, expired, _apiKey);
        }

        /// <summary>
        /// Gets the all-time historical low price for games. Requires API Key.
        /// </summary>
        /// <param name="gameIds">List of ITAD Game IDs.</param>
        /// <param name="country">Country code.</param>
        /// <returns>List of historical low records.</returns>
        public async Task<List<GameHistoricalLow>> GetHistoricalLowsAsync(List<string> gameIds, string country = "NL")
        {
             EnsureApiKey();
             if (gameIds == null || gameIds.Count == 0) return new List<GameHistoricalLow>();
             // TODO: Add try/catch for ApiException
             return await _api.GetHistoricalLowsAsync(gameIds, country, _apiKey);
        }

        /// <summary>
        /// Gets the historical low price per store for games. Requires API Key.
        /// </summary>
        /// <param name="gameIds">List of ITAD Game IDs.</param>
        /// <param name="country">Country code.</param>
        /// <param name="shops">Optional list of shop IDs.</param>
        /// <returns>List of store low records.</returns>
        public async Task<List<GameStoreLow>> GetStoreLowsAsync(List<string> gameIds, string country = "NL", int[]? shops = null)
        {
            EnsureApiKey();
            if (gameIds == null || gameIds.Count == 0) return new List<GameStoreLow>();
            // TODO: Add try/catch for ApiException
            return await _api.GetStoreLowsAsync(gameIds, country, shops, _apiKey);
        }

        /// <summary>
        /// Gets the price history log for a game. Requires API Key.
        /// </summary>
        /// <param name="gameId">ITAD Game ID.</param>
        /// <param name="country">Country code.</param>
        /// <param name="shops">Optional list of shop IDs.</param>
        /// <param name="since">Optional date to fetch history from.</param>
        /// <returns>List of price history points.</returns>
        public async Task<List<PriceHistoryPoint>> GetPriceHistoryAsync(string gameId, string country = "NL", int[]? shops = null, DateTimeOffset? since = null)
        {
            EnsureApiKey();
            // TODO: Add try/catch for ApiException
            return await _api.GetPriceHistoryAsync(gameId, country, shops, since, _apiKey);
        }

        /// <summary>
        /// Gets subscription services a game is part of. Requires API Key.
        /// </summary>
        /// <param name="gameIds">List of ITAD Game IDs.</param>
        /// <param name="country">Country code.</param>
        /// <returns>List of game subscription info.</returns>
        public async Task<List<GameSubscriptions>> GetGameSubscriptionsAsync(List<string> gameIds, string country = "NL")
        {
             EnsureApiKey();
             if (gameIds == null || gameIds.Count == 0) return new List<GameSubscriptions>();
             // TODO: Add try/catch for ApiException
             return await _api.GetGameSubscriptionsAsync(gameIds, country, _apiKey);
        }

        /// <summary>
        /// Gets a list of current deals. Requires API Key.
        /// </summary>
        /// <param name="country">Country code.</param>
        /// <param name="offset">Offset for pagination.</param>
        /// <param name="limit">Limit for pagination.</param>
        /// <param name="sort">Sort order (e.g., "price", "-cut").</param>
        /// <param name="nondeals">Include games not on sale.</param>
        /// <param name="mature">Include mature games.</param>
        /// <param name="shops">Optional list of shop IDs.</param>
        /// <param name="filter">Optional filter string.</param>
        /// <returns>Paginated list of deals.</returns>
        public async Task<DealsListResult> GetDealsListAsync(
            string country = "NL",
            int offset = 0,
            int limit = 20,
            string? sort = null,
            bool nondeals = false,
            bool mature = false,
            int[]? shops = null,
            string? filter = null)
        {
            EnsureApiKey();
            // TODO: Add try/catch for ApiException
            return await _api.GetDealsListAsync(country, offset, limit, sort, nondeals, mature, shops, filter, _apiKey);
        }

        // --- OAuth Methods ---
        // TODO: Implement proper OAuth flow and token management

        /// <summary>
        /// Sets the current OAuth access token for user-specific requests.
        /// </summary>
        /// <param name="accessToken">The OAuth access token.</param>
        public void SetUserAccessToken(string? accessToken)
        {
            _accessToken = accessToken;
        }

        /// <summary>
        /// Gets basic info for the authenticated user. Requires OAuth.
        /// </summary>
        /// <returns>User info.</returns>
        public async Task<UserInfo> GetUserInfoAsync()
        {
            // TODO: Add try/catch for ApiException (e.g., 401 Unauthorized)
            return await _api.GetUserInfoAsync(GetAuthorizationHeader());
        }

        /// <summary>
        /// Gets the user's waitlist. Requires OAuth.
        /// </summary>
        /// <returns>List of waitlisted games.</returns>
        public async Task<List<WaitlistGame>> GetWaitlistGamesAsync()
        {
            // TODO: Add try/catch for ApiException
            return await _api.GetWaitlistGamesAsync(GetAuthorizationHeader());
        }

        /// <summary>
        /// Adds games to the user's waitlist. Requires OAuth.
        /// </summary>
        /// <param name="gameIds">List of ITAD Game IDs.</param>
        public async Task AddGamesToWaitlistAsync(List<string> gameIds)
        {
            if (gameIds == null || gameIds.Count == 0) return;
            // TODO: Add try/catch for ApiException
            await _api.AddGamesToWaitlistAsync(gameIds, GetAuthorizationHeader());
        }

        /// <summary>
        /// Removes games from the user's waitlist. Requires OAuth.
        /// </summary>
        /// <param name="gameIds">List of ITAD Game IDs.</param>
        public async Task DeleteGamesFromWaitlistAsync(List<string> gameIds)
        {
             if (gameIds == null || gameIds.Count == 0) return;
             // TODO: Add try/catch for ApiException
             await _api.DeleteGamesFromWaitlistAsync(gameIds, GetAuthorizationHeader());
        }

        /// <summary>
        /// Gets the user's collection games (basic info). Requires OAuth.
        /// </summary>
        /// <returns>List of collection games.</returns>
        public async Task<List<CollectionGame>> GetCollectionGamesAsync()
        {
            // TODO: Add try/catch for ApiException
            return await _api.GetCollectionGamesAsync(GetAuthorizationHeader());
        }

        /// <summary>
        /// Adds games to the user's collection. Requires OAuth.
        /// </summary>
        /// <param name="gameIds">List of ITAD Game IDs.</param>
        public async Task AddGamesToCollectionAsync(List<string> gameIds)
        {
             if (gameIds == null || gameIds.Count == 0) return;
             // TODO: Add try/catch for ApiException
             await _api.AddGamesToCollectionAsync(gameIds, GetAuthorizationHeader());
        }

        /// <summary>
        /// Deletes games from the user's collection. Requires OAuth.
        /// </summary>
        /// <param name="gameIds">List of ITAD Game IDs.</param>
        public async Task DeleteGamesFromCollectionAsync(List<string> gameIds)
        {
             if (gameIds == null || gameIds.Count == 0) return;
             // TODO: Add try/catch for ApiException
             await _api.DeleteGamesFromCollectionAsync(gameIds, GetAuthorizationHeader());
        }

        /// <summary>
        /// Gets detailed copies from the user's collection. Requires OAuth.
        /// </summary>
        /// <returns>List of collection copies.</returns>
        public async Task<List<CollectionCopy>> GetCollectionCopiesAsync()
        {
            // TODO: Add try/catch for ApiException
            return await _api.GetCollectionCopiesAsync(GetAuthorizationHeader());
        }

        /// <summary>
        /// Adds new copies to the user's collection. Requires OAuth.
        /// </summary>
        /// <param name="copies">List of new copies to add.</param>
        public async Task AddCollectionCopiesAsync(List<NewCollectionCopy> copies)
        {
             if (copies == null || copies.Count == 0) return;
             // TODO: Add try/catch for ApiException
             await _api.AddCollectionCopiesAsync(copies, GetAuthorizationHeader());
        }

        /// <summary>
        /// Updates existing copies in the user's collection. Requires OAuth.
        /// </summary>
        /// <param name="copies">List of copies to update with changes.</param>
        public async Task UpdateCollectionCopiesAsync(List<UpdateCollectionCopy> copies)
        {
             if (copies == null || copies.Count == 0) return;
             // TODO: Add try/catch for ApiException
             await _api.UpdateCollectionCopiesAsync(copies, GetAuthorizationHeader());
        }

        /// <summary>
        /// Deletes copies from the user's collection. Requires OAuth.
        /// </summary>
        /// <param name="copyIds">List of copy IDs to delete.</param>
        public async Task DeleteCollectionCopiesAsync(List<int> copyIds)
        {
             if (copyIds == null || copyIds.Count == 0) return;
             // TODO: Add try/catch for ApiException
             await _api.DeleteCollectionCopiesAsync(copyIds, GetAuthorizationHeader());
        }

        /// <summary>
        /// Gets notes the user has saved for games. Requires OAuth.
        /// </summary>
        /// <returns>List of user notes.</returns>
        public async Task<List<UserNote>> GetUserNotesAsync()
        {
            // TODO: Add try/catch for ApiException
             return await _api.GetUserNotesAsync(GetAuthorizationHeader());
        }

        /// <summary>
        /// Sets or updates notes for games. Requires OAuth.
        /// </summary>
        /// <param name="notes">List of notes to set/update.</param>
        public async Task SetUserNotesAsync(List<UserNote> notes)
        {
            if (notes == null || notes.Count == 0) return;
            // TODO: Add try/catch for ApiException
            await _api.SetUserNotesAsync(notes, GetAuthorizationHeader());
        }

        /// <summary>
        /// Deletes notes for specific games. Requires OAuth.
        /// </summary>
        /// <param name="gameIds">List of ITAD game IDs whose notes should be deleted.</param>
        public async Task DeleteUserNotesAsync(List<string> gameIds)
        {
             if (gameIds == null || gameIds.Count == 0) return;
             // TODO: Add try/catch for ApiException
             await _api.DeleteUserNotesAsync(gameIds, GetAuthorizationHeader());
        }

        /// <summary>
        /// Gets user notifications. Requires OAuth.
        /// </summary>
        /// <returns>List of notifications.</returns>
        public async Task<List<Notification>> GetNotificationsAsync()
        {
            // TODO: Add try/catch for ApiException
             return await _api.GetNotificationsAsync(GetAuthorizationHeader());
        }

        // Implement other notification methods...

        // --- Sync Methods ---
        // These require both OAuth and a Profile Token

        /// <summary>
        /// Syncs a list of games with the user's waitlist via a linked profile.
        /// </summary>
        /// <param name="profileToken">The profile token obtained via the link endpoint.</param>
        /// <param name="entries">List of waitlist entries to sync.</param>
        /// <returns>Sync result.</returns>
        public async Task<SyncResult> SyncWaitlistAsync(string profileToken, List<WaitlistSyncEntry> entries)
        {
            // TODO: Add validation for profileToken
            if (entries == null || entries.Count == 0) return new SyncResult();
            // TODO: Add try/catch for ApiException
            return await _api.SyncWaitlistAsync(entries, GetAuthorizationHeader(), profileToken);
        }

         /// <summary>
        /// Syncs a list of games with the user's collection via a linked profile.
        /// </summary>
        /// <param name="profileToken">The profile token obtained via the link endpoint.</param>
        /// <param name="entries">List of collection entries to sync.</param>
        /// <returns>Sync result.</returns>
        public async Task<SyncResult> SyncCollectionAsync(string profileToken, List<CollectionSyncEntry> entries)
        {
            // TODO: Add validation for profileToken
            if (entries == null || entries.Count == 0) return new SyncResult(); // Return empty result for no input
            // TODO: Add try/catch for ApiException
            return await _api.SyncCollectionAsync(entries, GetAuthorizationHeader(), profileToken);
        }

        // TODO: Add client methods for Profiles, Stats, remaining Notification endpoints etc.
        // Remember to add error handling (try/catch ApiException), parameter validation,
        // and implement proper OAuth token management (refreshing tokens etc.).
    }
}