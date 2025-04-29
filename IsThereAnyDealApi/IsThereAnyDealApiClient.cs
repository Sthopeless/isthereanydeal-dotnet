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

        private string? _accessToken; // Placeholder for OAuth token

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

            var httpClient = new HttpClient() {
                 BaseAddress = new Uri(apiBaseUrl)
                 // Optionally add default headers like User-Agent
                 // httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("YourApp/1.0 (contact@example.com)");
            };

            var restClient = new RestClient(httpClient)
            {
                JsonSerializerSettings = DefaultJsonSerializerSettings
            };

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

        // Helper method to get the Authorization header string
        private string GetAuthorizationHeader()
        {
            if (string.IsNullOrWhiteSpace(_accessToken))
            {
                throw new InvalidOperationException("User is not authenticated. Access token is missing.");
            }
            return $"Bearer {_accessToken}";
        }

        // --- Public API Methods ---

        /// <summary>
        /// Gets information about shops.
        /// </summary>
        /// <param name="country">Two letter country code (ISO 3166-1 alpha-2).</param>
        /// <returns>A list of shops.</returns>
        public async Task<List<Shop>> GetShopsAsync(string country = "NL")
        {
            // No API key needed
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
                 return await _api.GetGameInfoAsync(gameId, _apiKey);
             }
             catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
             {
                 return null;
             }
             // Consider catching other specific API exceptions if needed
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
            return await _api.GetDealsListAsync(country, offset, limit, sort, nondeals, mature, shops, filter, _apiKey);
        }

        // --- OAuth Methods ---

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
            return await _api.GetUserInfoAsync(GetAuthorizationHeader());
        }

        /// <summary>
        /// Gets the user's waitlist. Requires OAuth.
        /// </summary>
        /// <returns>List of waitlisted games.</returns>
        public async Task<List<WaitlistGame>> GetWaitlistGamesAsync()
        {
            return await _api.GetWaitlistGamesAsync(GetAuthorizationHeader());
        }

        /// <summary>
        /// Adds games to the user's waitlist. Requires OAuth.
        /// </summary>
        /// <param name="gameIds">List of ITAD Game IDs.</param>
        public async Task AddGamesToWaitlistAsync(List<string> gameIds)
        {
            if (gameIds == null || gameIds.Count == 0) return;
            await _api.AddGamesToWaitlistAsync(gameIds, GetAuthorizationHeader());
        }

        /// <summary>
        /// Removes games from the user's waitlist. Requires OAuth.
        /// </summary>
        /// <param name="gameIds">List of ITAD Game IDs.</param>
        public async Task DeleteGamesFromWaitlistAsync(List<string> gameIds)
        {
             if (gameIds == null || gameIds.Count == 0) return;
             await _api.DeleteGamesFromWaitlistAsync(gameIds, GetAuthorizationHeader());
        }

        /// <summary>
        /// Gets the user's collection games (basic info). Requires OAuth.
        /// </summary>
        /// <returns>List of collection games.</returns>
        public async Task<List<CollectionGame>> GetCollectionGamesAsync()
        {
            return await _api.GetCollectionGamesAsync(GetAuthorizationHeader());
        }

        /// <summary>
        /// Adds games to the user's collection. Requires OAuth.
        /// </summary>
        /// <param name="gameIds">List of ITAD Game IDs.</param>
        public async Task AddGamesToCollectionAsync(List<string> gameIds)
        {
             if (gameIds == null || gameIds.Count == 0) return;
             await _api.AddGamesToCollectionAsync(gameIds, GetAuthorizationHeader());
        }

        /// <summary>
        /// Deletes games from the user's collection. Requires OAuth.
        /// </summary>
        /// <param name="gameIds">List of ITAD Game IDs.</param>
        public async Task DeleteGamesFromCollectionAsync(List<string> gameIds)
        {
             if (gameIds == null || gameIds.Count == 0) return;
             await _api.DeleteGamesFromCollectionAsync(gameIds, GetAuthorizationHeader());
        }

        /// <summary>
        /// Gets detailed copies from the user's collection. Requires OAuth.
        /// </summary>
        /// <returns>List of collection copies.</returns>
        public async Task<List<CollectionCopy>> GetCollectionCopiesAsync()
        {
            return await _api.GetCollectionCopiesAsync(GetAuthorizationHeader());
        }

        /// <summary>
        /// Adds new copies to the user's collection. Requires OAuth.
        /// </summary>
        /// <param name="copies">List of new copies to add.</param>
        public async Task AddCollectionCopiesAsync(List<NewCollectionCopy> copies)
        {
             if (copies == null || copies.Count == 0) return;
             await _api.AddCollectionCopiesAsync(copies, GetAuthorizationHeader());
        }

        /// <summary>
        /// Updates existing copies in the user's collection. Requires OAuth.
        /// </summary>
        /// <param name="copies">List of copies to update with changes.</param>
        public async Task UpdateCollectionCopiesAsync(List<UpdateCollectionCopy> copies)
        {
             if (copies == null || copies.Count == 0) return;
             await _api.UpdateCollectionCopiesAsync(copies, GetAuthorizationHeader());
        }

        /// <summary>
        /// Deletes copies from the user's collection. Requires OAuth.
        /// </summary>
        /// <param name="copyIds">List of copy IDs to delete.</param>
        public async Task DeleteCollectionCopiesAsync(List<int> copyIds)
        {
             if (copyIds == null || copyIds.Count == 0) return;
             await _api.DeleteCollectionCopiesAsync(copyIds, GetAuthorizationHeader());
        }

        /// <summary>
        /// Gets notes the user has saved for games. Requires OAuth.
        /// </summary>
        /// <returns>List of user notes.</returns>
        public async Task<List<UserNote>> GetUserNotesAsync()
        {
             return await _api.GetUserNotesAsync(GetAuthorizationHeader());
        }

        /// <summary>
        /// Sets or updates notes for games. Requires OAuth.
        /// </summary>
        /// <param name="notes">List of notes to set/update.</param>
        public async Task SetUserNotesAsync(List<UserNote> notes)
        {
            if (notes == null || notes.Count == 0) return;
            await _api.SetUserNotesAsync(notes, GetAuthorizationHeader());
        }

        /// <summary>
        /// Deletes notes for specific games. Requires OAuth.
        /// </summary>
        /// <param name="gameIds">List of ITAD game IDs whose notes should be deleted.</param>
        public async Task DeleteUserNotesAsync(List<string> gameIds)
        {
             if (gameIds == null || gameIds.Count == 0) return;
             await _api.DeleteUserNotesAsync(gameIds, GetAuthorizationHeader());
        }

        /// <summary>
        /// Gets user notifications. Requires OAuth.
        /// </summary>
        /// <returns>List of notifications.</returns>
        public async Task<List<Notification>> GetNotificationsAsync()
        {
             return await _api.GetNotificationsAsync(GetAuthorizationHeader());
        }

        // --- Start: Implemented TODOs ---

        /// <summary>
        /// Gets the detailed content of a specific waitlist notification. Requires OAuth.
        /// </summary>
        /// <param name="notificationId">The ID (UUID) of the notification.</param>
        /// <returns>Detailed notification information.</returns>
        public async Task<WaitlistNotificationDetail> GetWaitlistNotificationDetailAsync(string notificationId)
        {
            if (string.IsNullOrWhiteSpace(notificationId))
                throw new ArgumentException("Notification ID cannot be empty.", nameof(notificationId));

            return await _api.GetWaitlistNotificationDetailAsync(notificationId, GetAuthorizationHeader());
        }

        /// <summary>
        /// Marks a specific notification as read. Requires OAuth.
        /// </summary>
        /// <param name="notificationId">The ID (UUID) of the notification to mark as read.</param>
        public async Task MarkNotificationReadAsync(string notificationId)
        {
             if (string.IsNullOrWhiteSpace(notificationId))
                throw new ArgumentException("Notification ID cannot be empty.", nameof(notificationId));

            await _api.MarkNotificationReadAsync(notificationId, GetAuthorizationHeader());
        }

        /// <summary>
        /// Marks all notifications for the user as read. Requires OAuth.
        /// </summary>
        public async Task MarkAllNotificationsReadAsync()
        {
            await _api.MarkAllNotificationsReadAsync(GetAuthorizationHeader());
        }

        /// <summary>
        /// Links a profile to the user's ITAD account. Requires OAuth.
        /// </summary>
        /// <param name="profile">The profile details to link.</param>
        /// <returns>The response containing the profile token.</returns>
        public async Task<LinkProfileResponse> LinkProfileAsync(Profile profile)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            // Add more specific validation if needed (e.g., non-empty AccountId/AccountName)
            return await _api.LinkProfileAsync(profile, GetAuthorizationHeader());
        }

        /// <summary>
        /// Unlinks a profile from the user's ITAD account. Requires OAuth and the Profile Token.
        /// </summary>
        /// <param name="profileToken">The profile token obtained via the link endpoint.</param>
        public async Task UnlinkProfileAsync(string profileToken)
        {
            if (string.IsNullOrWhiteSpace(profileToken))
                throw new ArgumentException("Profile token cannot be empty.", nameof(profileToken));

            await _api.UnlinkProfileAsync(GetAuthorizationHeader(), profileToken);
        }

        /// <summary>
        /// Gets the user's collection categories/groups. Requires OAuth.
        /// </summary>
        /// <returns>List of collection groups.</returns>
        public async Task<List<CollectionGroup>> GetCollectionGroupsAsync()
        {
            return await _api.GetCollectionGroupsAsync(GetAuthorizationHeader());
        }

        /// <summary>
        /// Creates a new collection category/group. Requires OAuth.
        /// </summary>
        /// <param name="group">The details of the group to create.</param>
        /// <returns>The newly created collection group with its ID.</returns>
        public async Task<CollectionGroup> AddCollectionGroupAsync(NewCollectionGroup group)
        {
            if (group == null) throw new ArgumentNullException(nameof(group));
            if (string.IsNullOrWhiteSpace(group.Title))
                 throw new ArgumentException("Group title cannot be empty.", $"{nameof(group)}.{nameof(group.Title)}");

            return await _api.AddCollectionGroupAsync(group, GetAuthorizationHeader());
        }

        /// <summary>
        /// Updates one or more collection categories/groups. Requires OAuth.
        /// </summary>
        /// <param name="groups">List of groups to update with their changes.</param>
        /// <returns>The updated list of all collection groups.</returns>
        public async Task<List<CollectionGroup>> UpdateCollectionGroupsAsync(List<UpdateCollectionGroup> groups)
        {
             if (groups == null || groups.Count == 0) return await GetCollectionGroupsAsync(); // Or return empty list? Decide behavior.
             // Add validation for individual group updates if needed
             return await _api.UpdateCollectionGroupsAsync(groups, GetAuthorizationHeader());
        }

        /// <summary>
        /// Deletes collection categories/groups. Requires OAuth.
        /// </summary>
        /// <param name="groupIds">List of group IDs to delete.</param>
        public async Task DeleteCollectionGroupsAsync(List<int> groupIds)
        {
             if (groupIds == null || groupIds.Count == 0) return;
             await _api.DeleteCollectionGroupsAsync(groupIds, GetAuthorizationHeader());
        }

        /// <summary>
        /// Gets waitlist statistics for a specific game. Requires API Key.
        /// </summary>
        /// <param name="gameId">ITAD Game ID.</param>
        /// <param name="country">Country code for price context.</param>
        /// <param name="bucketPrice">Price bucket size for stats.</param>
        /// <param name="bucketCut">Cut bucket size for stats.</param>
        /// <returns>Waitlist statistics.</returns>
        public async Task<WaitlistStats> GetWaitlistStatsAsync(string gameId, string country = "US", int bucketPrice = 5, int bucketCut = 5)
        {
            EnsureApiKey();
             if (string.IsNullOrWhiteSpace(gameId))
                throw new ArgumentException("Game ID cannot be empty.", nameof(gameId));

            return await _api.GetWaitlistStatsAsync(gameId, country, bucketPrice, bucketCut, _apiKey);
        }

         /// <summary>
        /// Gets a ranked list of the most waitlisted games. Requires API Key.
        /// </summary>
        /// <param name="offset">Pagination offset.</param>
        /// <param name="limit">Pagination limit.</param>
        /// <returns>List of ranked games.</returns>
        public async Task<List<RankedGame>> GetMostWaitlistedGamesAsync(int offset = 0, int limit = 20)
        {
            EnsureApiKey();
            return await _api.GetMostWaitlistedGamesAsync(offset, limit, _apiKey);
        }

        /// <summary>
        /// Gets a ranked list of the most collected games. Requires API Key.
        /// </summary>
        /// <param name="offset">Pagination offset.</param>
        /// <param name="limit">Pagination limit.</param>
        /// <returns>List of ranked games.</returns>
        public async Task<List<RankedGame>> GetMostCollectedGamesAsync(int offset = 0, int limit = 20)
        {
            EnsureApiKey();
            return await _api.GetMostCollectedGamesAsync(offset, limit, _apiKey);
        }

        /// <summary>
        /// Gets a ranked list of the most popular games (Waitlisted + Collected). Requires API Key.
        /// </summary>
        /// <param name="offset">Pagination offset.</param>
        /// <param name="limit">Pagination limit.</param>
        /// <returns>List of ranked games.</returns>
        public async Task<List<RankedGame>> GetMostPopularGamesAsync(int offset = 0, int limit = 20)
        {
            EnsureApiKey();
            return await _api.GetMostPopularGamesAsync(offset, limit, _apiKey);
        }

        // --- End: Implemented TODOs ---

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
            if (string.IsNullOrWhiteSpace(profileToken))
                 throw new ArgumentException("Profile token cannot be empty.", nameof(profileToken));
            if (entries == null || entries.Count == 0) return new SyncResult();
            // Add validation for entries if needed
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
            if (string.IsNullOrWhiteSpace(profileToken))
                 throw new ArgumentException("Profile token cannot be empty.", nameof(profileToken));
            if (entries == null || entries.Count == 0) return new SyncResult(); // Return empty result for no input
            // Add validation for entries if needed
            return await _api.SyncCollectionAsync(entries, GetAuthorizationHeader(), profileToken);
        }

        // Remember to add error handling (try/catch ApiException), parameter validation,
        // and implement proper OAuth token management (refreshing tokens etc.).
    }
}