using RestEase;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IsThereAnyDeal.Api.Models;

namespace IsThereAnyDeal.Api
{
    /// <summary>
    /// Defines the IsThereAnyDeal API endpoints using RestEase attributes.
    /// </summary>
    [Header("Accept", "application/json")]
    public interface IIsThereAnyDealApi
    {
        // --- Service ---
        [Get("/service/shops/v1")]
        Task<List<Shop>> GetShopsAsync([Query] string country = "NL");

        // --- Lookup ---
        [Get("/games/lookup/v1")]
        Task<GameLookupResult> LookupGameAsync([Query] string? title = null, [Query] int? appid = null, [Query("key")] string? apiKey = null);

        [Post("/lookup/id/title/v1")]
        Task<Dictionary<string, string?>> LookupGameIdsByTitleAsync([Body] List<string> titles, [Query("key")] string? apiKey = null);

        [Post("/lookup/id/shop/{shopId}/v1")]
        Task<Dictionary<string, string?>> LookupGameIdsByShopIdAsync([Path] int shopId, [Body] List<string> shopGameIds, [Query("key")] string? apiKey = null);

        [Post("/lookup/shop/{shopId}/id/v1")]
        Task<Dictionary<string, List<string>?>> LookupShopIdsByGameIdAsync([Path] int shopId, [Body] List<string> gameIds, [Query("key")] string? apiKey = null);

        // --- Game Info & Prices ---
        [Get("/games/info/v2")]
        Task<GameInfo?> GetGameInfoAsync([Query("id")] string gameId, [Query("key")] string? apiKey = null);

        [Get("/games/search/v1")]
        Task<List<Game>> SearchGamesAsync([Query] string title, [Query] int results = 20, [Query("key")] string? apiKey = null);

        [Post("/games/prices/v3")]
        Task<List<GamePrices>> GetGamePricesAsync(
            [Body] List<string> gameIds,
            [Query] string country = "NL",
            [Query] bool deals = false,
            [Query] bool vouchers = true,
            [Query] int capacity = 0,
            [Query(QuerySerializationMethod.Serialized)] int[]? shops = null, // Use Serialized for array params
            [Query("key")] string? apiKey = null);

        [Post("/games/overview/v2")]
        Task<GameOverviewResult> GetGamesOverviewAsync(
             [Body] List<string> gameIds,
             [Query] string country = "NL",
             [Query(QuerySerializationMethod.Serialized)] int[]? shops = null,
             [Query] bool vouchers = true,
             [Query("key")] string? apiKey = null);

        // --- History & Bundles ---
        [Get("/games/bundles/v2")]
        Task<List<Bundle>> GetBundlesForGameAsync([Query("id")] string gameId, [Query] string country = "NL", [Query] bool expired = false, [Query("key")] string? apiKey = null);

        [Post("/games/historylow/v1")]
        Task<List<GameHistoricalLow>> GetHistoricalLowsAsync([Body] List<string> gameIds, [Query] string country = "NL", [Query("key")] string? apiKey = null);

        [Post("/games/storelow/v2")]
        Task<List<GameStoreLow>> GetStoreLowsAsync([Body] List<string> gameIds, [Query] string country = "NL", [Query(QuerySerializationMethod.Serialized)] int[]? shops = null, [Query("key")] string? apiKey = null);

        [Get("/games/history/v2")]
        Task<List<PriceHistoryPoint>> GetPriceHistoryAsync([Query("id")] string gameId, [Query] string country = "NL", [Query(QuerySerializationMethod.Serialized)] int[]? shops = null, [Query] DateTimeOffset? since = null, [Query("key")] string? apiKey = null);

        // --- Subscriptions ---
        [Post("/games/subs/v1")]
        Task<List<GameSubscriptions>> GetGameSubscriptionsAsync([Body] List<string> gameIds, [Query] string country = "NL", [Query("key")] string? apiKey = null);

        // --- Deals List ---
        [Get("/deals/v2")]
        Task<DealsListResult> GetDealsListAsync(
            [Query] string country = "NL",
            [Query] int offset = 0,
            [Query] int limit = 20,
            [Query] string? sort = null,
            [Query] bool nondeals = false,
            [Query] bool mature = false,
            [Query(QuerySerializationMethod.Serialized)] int[]? shops = null,
            [Query] string? filter = null,
            [Query("key")] string? apiKey = null);

        // --- User Info (OAuth Required) ---
        [Get("/user/info/v2")]
        Task<UserInfo> GetUserInfoAsync([Header("Authorization")] string authorization);

        // --- Waitlist (OAuth Required) ---
        [Get("/waitlist/games/v1")]
        Task<List<WaitlistGame>> GetWaitlistGamesAsync([Header("Authorization")] string authorization);

        [Put("/waitlist/games/v1")]
        Task AddGamesToWaitlistAsync([Body] List<string> gameIds, [Header("Authorization")] string authorization);

        [Delete("/waitlist/games/v1")]
        Task DeleteGamesFromWaitlistAsync([Body] List<string> gameIds, [Header("Authorization")] string authorization);

        // --- Collection (OAuth Required) ---
        [Get("/collection/games/v1")]
        Task<List<CollectionGame>> GetCollectionGamesAsync([Header("Authorization")] string authorization);

        [Put("/collection/games/v1")]
        Task AddGamesToCollectionAsync([Body] List<string> gameIds, [Header("Authorization")] string authorization);

        [Delete("/collection/games/v1")]
        Task DeleteGamesFromCollectionAsync([Body] List<string> gameIds, [Header("Authorization")] string authorization);

        [Get("/collection/copies/v1")]
        Task<List<CollectionCopy>> GetCollectionCopiesAsync([Header("Authorization")] string authorization);

        [Post("/collection/copies/v1")]
        Task AddCollectionCopiesAsync([Body] List<NewCollectionCopy> copies, [Header("Authorization")] string authorization);

        [Patch("/collection/copies/v1")]
        Task UpdateCollectionCopiesAsync([Body] List<UpdateCollectionCopy> copies, [Header("Authorization")] string authorization);

        [Delete("/collection/copies/v1")]
        Task DeleteCollectionCopiesAsync([Body] List<int> copyIds, [Header("Authorization")] string authorization);

        // --- Collection Groups (OAuth Required) --- NEWLY ADDED ---
        [Get("/collection/groups/v1")]
        Task<List<CollectionGroup>> GetCollectionGroupsAsync([Header("Authorization")] string authorization);

        [Post("/collection/groups/v1")]
        Task<CollectionGroup> AddCollectionGroupAsync([Body] NewCollectionGroup group, [Header("Authorization")] string authorization);

        [Patch("/collection/groups/v1")]
        Task<List<CollectionGroup>> UpdateCollectionGroupsAsync([Body] List<UpdateCollectionGroup> groups, [Header("Authorization")] string authorization);

        [Delete("/collection/groups/v1")]
        Task DeleteCollectionGroupsAsync([Body] List<int> groupIds, [Header("Authorization")] string authorization);
        // --- END NEW Collection Groups ---

        // --- User Notes (OAuth Required) ---
        [Get("/user/notes/v1")]
        Task<List<UserNote>> GetUserNotesAsync([Header("Authorization")] string authorization);

        [Put("/user/notes/v1")]
        Task SetUserNotesAsync([Body] List<UserNote> notes, [Header("Authorization")] string authorization);

        [Delete("/user/notes/v1")]
        Task DeleteUserNotesAsync([Body] List<string> gameIds, [Header("Authorization")] string authorization);

        // --- Notifications (OAuth Required) ---
        [Get("/notifications/v1")]
        Task<List<Notification>> GetNotificationsAsync([Header("Authorization")] string authorization);

        // --- NEWLY ADDED Notification Endpoints ---
        [Get("/notifications/waitlist/v1")]
        Task<WaitlistNotificationDetail> GetWaitlistNotificationDetailAsync([Query("id")] string notificationId, [Header("Authorization")] string authorization);

        [Put("/notifications/read/v1")]
        Task MarkNotificationReadAsync([Query("id")] string notificationId, [Header("Authorization")] string authorization);

        [Put("/notifications/read/all/v1")]
        Task MarkAllNotificationsReadAsync([Header("Authorization")] string authorization);
        // --- END NEW Notification Endpoints ---

        // --- Profiles (OAuth Required) --- NEWLY ADDED ---
        [Put("/profiles/link/v1")]
        Task<LinkProfileResponse> LinkProfileAsync([Body] Profile profile, [Header("Authorization")] string authorization);

        [Delete("/profiles/link/v1")]
        Task UnlinkProfileAsync([Header("Authorization")] string authorization, [Header("ITAD-Profile")] string profileToken);
        // --- END NEW Profiles ---

        // --- Sync (OAuth + Profile Token Required) ---
        [Put("/profiles/sync/waitlist/v1")]
        Task<SyncResult> SyncWaitlistAsync([Body] List<WaitlistSyncEntry> entries, [Header("Authorization")] string authorization, [Header("ITAD-Profile")] string profileToken);

        [Put("/profiles/sync/collection/v1")]
        Task<SyncResult> SyncCollectionAsync([Body] List<CollectionSyncEntry> entries, [Header("Authorization")] string authorization, [Header("ITAD-Profile")] string profileToken);

        // --- Stats (API Key Required) --- NEWLY ADDED ---
        [Get("/stats/waitlist/v1")]
        Task<WaitlistStats> GetWaitlistStatsAsync([Query("id")] string gameId, [Query] string country = "US", [Query] int bucket_price = 5, [Query] int bucket_cut = 5, [Query("key")] string? apiKey = null);

        [Get("/stats/most-waitlisted/v1")]
        Task<List<RankedGame>> GetMostWaitlistedGamesAsync([Query] int offset = 0, [Query] int limit = 20, [Query("key")] string? apiKey = null);

        [Get("/stats/most-collected/v1")]
        Task<List<RankedGame>> GetMostCollectedGamesAsync([Query] int offset = 0, [Query] int limit = 20, [Query("key")] string? apiKey = null);

        [Get("/stats/most-popular/v1")]
        Task<List<RankedGame>> GetMostPopularGamesAsync([Query] int offset = 0, [Query] int limit = 20, [Query("key")] string? apiKey = null);
        // --- END NEW Stats ---
    }
}