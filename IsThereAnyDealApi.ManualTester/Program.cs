using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using IsThereAnyDeal.Api;
using IsThereAnyDeal.Api.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

class Program
{
    private static IsThereAnyDealApiClient? _client;
    private static string? _apiKey;
    private static string? _clientId;
    private static string? _clientSecret;

    // --- OAuth Configuration ---
    private const string YourRedirectUri = "http://localhost:6565/callback"; // MAKE SURE THIS MATCHES ITAD REGISTRATION
    private const string ItadAuthorizeUrl = "https://isthereanydeal.com/oauth/authorize/";
    private const string ItadTokenUrl = "https://isthereanydeal.com/oauth/token/";
    private const string DefaultScopes = "user_info wait_read coll_read notes_read";

    // Temporary storage for PKCE
    private static string? _pkceCodeVerifier;
    private static string? _currentAccessToken;
    private static DateTimeOffset? _accessTokenExpiry;

    static async Task Main(string[] args)
    {
        Console.WriteLine("IsThereAnyDeal API Manual Tester");
        Console.WriteLine("=================================");

        if (!InitializeClient())
        {
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
            return;
        }

        await RunMenu();

        Console.WriteLine("\nExiting tester. Press any key.");
        Console.ReadKey();
    }

    private static bool InitializeClient()
    {
        try
        {
            // --- Load Config ---
            var configPath = Path.Combine(AppContext.BaseDirectory, "testsettings.json");
            if (!File.Exists(configPath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ERROR: Configuration file '{configPath}' not found.");
                Console.WriteLine("Ensure 'testsettings.json' exists and is set to 'Copy to Output Directory'.");
                Console.ResetColor();
                return false;
            }

            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("testsettings.json", optional: false)
                .Build();

            _apiKey = config["ITAD_API_KEY"];
            _clientId = config["ITAD_CLIENT_ID"];
            _clientSecret = config["ITAD_CLIENT_SECRET"];

            // Validate config
            bool configError = false;
            if (string.IsNullOrWhiteSpace(_apiKey)) { Console.Error.WriteLine("ERROR: ITAD_API_KEY missing in testsettings.json"); configError = true; }
            if (string.IsNullOrWhiteSpace(_clientId)) { Console.Error.WriteLine("ERROR: ITAD_CLIENT_ID missing in testsettings.json"); configError = true; }
            if (string.IsNullOrWhiteSpace(_clientSecret)) { Console.Error.WriteLine("ERROR: ITAD_CLIENT_SECRET missing in testsettings.json"); configError = true; }

             if (YourRedirectUri.Contains("YOUR_REDIRECT_URI") || !YourRedirectUri.StartsWith("http://localhost")) // Basic check for placeholder or non-localhost
             {
                 Console.ForegroundColor = ConsoleColor.Yellow;
                 Console.WriteLine("WARNING: 'YourRedirectUri' constant in Program.cs needs to be updated with your actual registered localhost Redirect URI.");
                 Console.ResetColor();
             }

            if (configError) return false;
            // --- ---

            _client = new IsThereAnyDealApiClient(_apiKey);
            Console.WriteLine("API Client Initialized (API Key loaded).");
            Console.WriteLine($"OAuth Client ID: {_clientId}"); // Don't print secret
            return true;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"ERROR initializing client: {ex.Message}");
            Console.ResetColor();
            return false;
        }
    }

    private static async Task RunMenu()
    {
        bool exit = false;
        while (!exit && _client != null) // Ensure client is not null
        {
            Console.WriteLine("\nSelect API call to test:");
             Console.WriteLine($"--- OAuth (Token Set: {(!string.IsNullOrEmpty(_currentAccessToken))}, Expires: {_accessTokenExpiry?.ToString("o") ?? "N/A"}) ---");
            Console.WriteLine(" A) Prepare OAuth Request / Get Auth URL");
            Console.WriteLine(" B) Enter Redirect URL & Get Token");
            Console.WriteLine(" C) Get User Info (OAuth)");
            Console.WriteLine(" D) Get Waitlist Games (OAuth)");
            Console.WriteLine(" --- API Key Based ---");
            Console.WriteLine(" 1) Lookup Game by Title");
            Console.WriteLine(" 2) Lookup Game by Steam AppID");
            Console.WriteLine(" 3) Search Games by Title");
            Console.WriteLine(" 4) Get Game Info by ITAD ID");
            Console.WriteLine(" 5) Get Game Prices by ITAD ID");
            Console.WriteLine(" 6) Get Store Lows by ITAD ID");
            Console.WriteLine(" 7) Get Price History by ITAD ID");
            Console.WriteLine(" 8) Get Bundles for Game by ITAD ID");
            Console.WriteLine(" 9) Get Game Subscriptions by ITAD ID");
            Console.WriteLine("10) Get Shops (No API Key Needed)");
            Console.WriteLine("--- Other ---");
            Console.WriteLine(" 0) Exit");
            Console.Write("Enter selection: ");

            string? choice = Console.ReadLine()?.ToUpperInvariant();

            try
            {
                switch (choice)
                {
                     // OAuth
                    case "A":
                        GenerateAndShowAuthUrl();
                        break;
                    case "B":
                         await GetTokenFromCallbackUrl();
                         break;
                     case "C":
                         await TestGetUserInfo();
                         break;
                     case "D":
                         await TestGetWaitlist();
                         break;
                    case "1":
                        await TestLookupByTitle();
                        break;
                    case "2":
                        await TestLookupByAppId();
                        break;
                     case "3":
                         await TestSearchGames();
                         break;
                    case "4":
                        await TestGetGameInfo();
                        break;
                    case "5":
                        await TestGetGamePrices();
                        break;
                     case "6":
                         await TestGetStoreLows();
                         break;
                     case "7":
                         await TestGetPriceHistory();
                         break;
                     case "8":
                         await TestGetBundlesForGame();
                         break;
                     case "9":
                         await TestGetGameSubscriptions();
                         break;
                    case "10":
                        await TestGetShops();
                        break;
                    case "0":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Invalid choice.");
                        break;
                }
            }
            catch (Exception ex)
            {
                 Console.ForegroundColor = ConsoleColor.Red;
                 Console.WriteLine($"\n--- ERROR ---");
                 Console.WriteLine($"An error occurred: {ex.Message}");
                 Console.WriteLine($"---------------");
                 Console.ResetColor();
            }
             Console.WriteLine("\n------------------------------------");
        }
    }

    // --- PKCE Helpers ---

    private static string GeneratePkceVerifier(int length = 64)
    {
        const string possibleChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~";
        var randomBytes = RandomNumberGenerator.GetBytes(length);
        var chars = new char[length];
        for (int i = 0; i < length; i++)
        {
            chars[i] = possibleChars[randomBytes[i] % possibleChars.Length];
        }
        return new string(chars);
    }

    private static string GeneratePkceChallenge(string verifier)
    {
        using var sha256 = SHA256.Create();
        var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(verifier));
        // Base64Url encode
        return Convert.ToBase64String(challengeBytes)
            .TrimEnd('=') // Remove padding
            .Replace('+', '-') // URL-safe characters
            .Replace('/', '_'); // URL-safe characters
    }

    // --- OAuth Flow Steps ---

    private static void GenerateAndShowAuthUrl()
    {
        if (string.IsNullOrWhiteSpace(_clientId)) { Console.WriteLine("Client ID not loaded."); return; }

        _pkceCodeVerifier = GeneratePkceVerifier();
        string challenge = GeneratePkceChallenge(_pkceCodeVerifier);
        string state = Guid.NewGuid().ToString("N"); // Simple state generation, N format is compact

        Console.WriteLine($"\nGenerated PKCE Verifier (needed for step B): {_pkceCodeVerifier}"); // Keep track if needed for debugging

        var query = HttpUtility.ParseQueryString(string.Empty);
        query["response_type"] = "code";
        query["client_id"] = _clientId;
        query["redirect_uri"] = YourRedirectUri;
        query["scope"] = DefaultScopes;
        query["state"] = state;
        query["code_challenge"] = challenge;
        query["code_challenge_method"] = "S256";

        string authUrl = ItadAuthorizeUrl + "?" + query.ToString();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("\n--- Authorization URL ---");
        Console.WriteLine("1. Copy the URL below.");
        Console.WriteLine("2. Paste it into your browser.");
        Console.WriteLine("3. Log in to IsThereAnyDeal and authorize the application.");
        Console.WriteLine("4. After authorization, your browser will redirect.");
        Console.WriteLine("   Copy the FULL URL from the browser's address bar (it might show an error page).");
        Console.WriteLine("\nURL:");
        Console.WriteLine(authUrl);
         Console.WriteLine("-------------------------");
        Console.ResetColor();
         Console.WriteLine("Then use Option 'B' to paste the full redirect URL.");
    }

    private static async Task GetTokenFromCallbackUrl()
    {
        if (string.IsNullOrWhiteSpace(_clientId) || string.IsNullOrWhiteSpace(_clientSecret))
        { Console.WriteLine("Client ID or Secret not loaded."); return; }
         if (string.IsNullOrWhiteSpace(_pkceCodeVerifier))
        { Console.WriteLine("PKCE Verifier not generated. Use Option 'A' first."); return; }

        Console.Write("\nEnter the FULL redirect URL copied from the browser: ");
        string? fullUrl = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(fullUrl))
        {
            Console.WriteLine("URL cannot be empty.");
            return;
        }

        string? code = null;
        string? stateReceived = null;
        try
        {
            // Parse the URL to extract the code and state
            Uri uri = new Uri(fullUrl);
            var queryParams = HttpUtility.ParseQueryString(uri.Query);
            code = queryParams["code"];
            stateReceived = queryParams["state"];

             if (string.IsNullOrWhiteSpace(code))
             {
                  Console.ForegroundColor = ConsoleColor.Red;
                  Console.WriteLine("\nERROR: Could not find 'code' parameter in the pasted URL.");
                  Console.ResetColor();
                  return;
             }
             Console.WriteLine($"\nExtracted code: {code.Substring(0, Math.Min(code.Length, 10))}...");
        }
        catch (UriFormatException)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\nERROR: The pasted text was not a valid URL.");
            Console.ResetColor();
            return;
        }
        catch (Exception ex)
        {
             Console.ForegroundColor = ConsoleColor.Red;
             Console.WriteLine($"\nERROR parsing the URL: {ex.Message}");
             Console.ResetColor();
             return;
        }

        Console.WriteLine("Exchanging code for access token...");

        using var httpClient = new HttpClient();

        var parameters = new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "code", code },
            { "redirect_uri", YourRedirectUri },
            { "client_id", _clientId },
            { "code_verifier", _pkceCodeVerifier }
        };
        var content = new FormUrlEncodedContent(parameters);

        // Prepare Authorization header (Basic Auth for client credentials)
        var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}"));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);

        try
        {
            // Make POST request
            var response = await httpClient.PostAsync(ItadTokenUrl, content);
            string responseBody = await response.Content.ReadAsStringAsync();

             Console.WriteLine($"\nToken Endpoint Response Status: {response.StatusCode}");

             // Only try to parse if successful
            if (response.IsSuccessStatusCode)
            {
                 // Parse JSON response
                 var tokenData = JObject.Parse(responseBody);
                 _currentAccessToken = tokenData["access_token"]?.ToString();
                 string? refreshToken = tokenData["refresh_token"]?.ToString();
                 int expiresIn = tokenData["expires_in"]?.Value<int>() ?? 0;
                 string? scopeReceived = tokenData["scope"]?.ToString();

                if (!string.IsNullOrWhiteSpace(_currentAccessToken))
                {
                    _accessTokenExpiry = DateTimeOffset.UtcNow.AddSeconds(expiresIn);
                    _client?.SetUserAccessToken(_currentAccessToken);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n--- Access Token Obtained ---");
                    Console.WriteLine($"Access Token (start): {_currentAccessToken.Substring(0, Math.Min(_currentAccessToken.Length, 10))}...");
                    Console.WriteLine($"Refresh Token Present: {!string.IsNullOrWhiteSpace(refreshToken)}");
                    Console.WriteLine($"Expires In: {expiresIn} seconds (at {_accessTokenExpiry:o})");
                    Console.WriteLine($"Scope: {scopeReceived}");
                    Console.WriteLine("---------------------------");
                    Console.ResetColor();

                    // Clear verifier after successful use
                    _pkceCodeVerifier = null;
                }
                else
                {
                    Console.Error.WriteLine("ERROR: Access token was not found in the response.");
                    PrintResult(tokenData); // Print the JSON we got
                }
            }
            else // Handle non-success status codes
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"ERROR from Token Endpoint:");
                 try {
                     // Try to parse error JSON if possible
                     var errorData = JObject.Parse(responseBody);
                     PrintResult(errorData);
                 } catch {
                     // Otherwise print raw response
                     Console.WriteLine(responseBody);
                 }
                Console.ResetColor();
            }
        }
        catch (HttpRequestException httpEx)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"HTTP Error during token exchange: {httpEx.Message}");
             if(httpEx.StatusCode.HasValue) Console.WriteLine($"Status Code: {httpEx.StatusCode}");
            Console.ResetColor();
        }
         catch (Exception ex)
        {
             Console.ForegroundColor = ConsoleColor.Red;
             Console.WriteLine($"Error during token exchange: {ex.Message}");
             Console.ResetColor();
        }
    }

    // --- Helper Methods for Input ---

    private static string? PromptForGameId(string prompt = "Enter ITAD Game ID (UUID): ")
    {
        Console.Write(prompt);
        string? gameId = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(gameId) || !Guid.TryParse(gameId, out _))
        {
            Console.WriteLine("Invalid ITAD Game ID format.");
            return null;
        }
        return gameId;
    }

    private static string PromptForCountry(string defaultCountry = "NL")
    {
        Console.Write($"Enter Country Code (e.g., NL, PT, BE - defaults to {defaultCountry}): ");
        string? country = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(country) || country.Length != 2)
        {
            Console.WriteLine($"Using default country: {defaultCountry}");
            return defaultCountry;
        }
        return country.ToUpperInvariant();
    }

    // --- Test Execution Methods ---

     private static async Task TestGetUserInfo()
    {
        if (string.IsNullOrEmpty(_currentAccessToken)) { Console.WriteLine("Access Token not set. Use options A & B first."); return; }
        if (_accessTokenExpiry.HasValue && _accessTokenExpiry < DateTimeOffset.UtcNow) { Console.WriteLine("Access Token expired."); return; }

         Console.WriteLine("\nCalling GetUserInfoAsync()...");
         // Assuming GetUserInfoAsync exists and uses the token set via SetUserAccessToken
         var result = await _client!.GetUserInfoAsync();
         PrintResult(result);
    }

     private static async Task TestGetWaitlist()
    {
        if (string.IsNullOrEmpty(_currentAccessToken)) { Console.WriteLine("Access Token not set. Use options A & B first."); return; }
        if (_accessTokenExpiry.HasValue && _accessTokenExpiry < DateTimeOffset.UtcNow) { Console.WriteLine("Access Token expired."); return; }

         Console.WriteLine("\nCalling GetWaitlistGamesAsync()...");
         // Assuming GetWaitlistGamesAsync exists
         var result = await _client!.GetWaitlistGamesAsync();
         PrintResult(result);
    }

    private static async Task TestLookupByTitle()
    {
        Console.Write("Enter game title: ");
        string? title = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(title))
        {
            Console.WriteLine("Title cannot be empty.");
            return;
        }
        Console.WriteLine($"\nCalling LookupGameAsync(title: \"{title}\")...");
        var result = await _client!.LookupGameAsync(title: title);
        PrintResult(result);
    }

     private static async Task TestLookupByAppId()
    {
        Console.Write("Enter Steam AppID: ");
        string? input = Console.ReadLine();
        if (!int.TryParse(input, out int appId) || appId <= 0)
        {
            Console.WriteLine("Invalid AppID.");
            return;
        }
        Console.WriteLine($"\nCalling LookupGameAsync(appid: {appId})...");
        var result = await _client!.LookupGameAsync(appid: appId);
        PrintResult(result);
    }

     private static async Task TestSearchGames()
    {
        Console.Write("Enter search term: ");
        string? title = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(title))
        {
            Console.WriteLine("Search term cannot be empty.");
            return;
        }
        Console.WriteLine($"\nCalling SearchGamesAsync(title: \"{title}\")...");
        var result = await _client!.SearchGamesAsync(title); // Use default results limit
        PrintResult(result);
    }

    private static async Task TestGetGameInfo()
    {
        string? gameId = PromptForGameId();
        if (gameId == null) return;

        Console.WriteLine($"\nCalling GetGameInfoAsync(gameId: \"{gameId}\")...");
        var result = await _client!.GetGameInfoAsync(gameId);
         PrintResult(result); // Handles null result nicely
    }

     private static async Task TestGetGamePrices()
    {
        string? gameId = PromptForGameId();
        if (gameId == null) return;
        string country = PromptForCountry();

        Console.WriteLine($"\nCalling GetGamePricesAsync(ids: [\"{gameId}\"], country: \"{country}\")...");
        var result = await _client!.GetGamePricesAsync(new List<string> { gameId }, country);
        PrintResult(result);
    }

    private static async Task TestGetStoreLows()
    {
        string? gameId = PromptForGameId();
        if (gameId == null) return;
        string country = PromptForCountry();

        Console.WriteLine($"\nCalling GetStoreLowsAsync(ids: [\"{gameId}\"], country: \"{country}\")...");
        var result = await _client!.GetStoreLowsAsync(new List<string> { gameId }, country);
        PrintResult(result);
    }

    private static async Task TestGetPriceHistory()
    {
        string? gameId = PromptForGameId();
        if (gameId == null) return;
        string country = PromptForCountry();
        DateTimeOffset? since = null;

        Console.Write("Enter 'since' date (YYYY-MM-DD) or leave blank for recent: ");
        string? sinceInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(sinceInput) && DateTimeOffset.TryParseExact(sinceInput, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsedDate))
        {
            since = parsedDate;
            Console.WriteLine($"Using since date: {since:yyyy-MM-dd}");
        }
        else
        {
             Console.WriteLine("No valid 'since' date provided, fetching recent history.");
        }

        Console.WriteLine($"\nCalling GetPriceHistoryAsync(id: \"{gameId}\", country: \"{country}\", since: {since?.ToString("o") ?? "null"})...");
        var result = await _client!.GetPriceHistoryAsync(gameId, country, since: since);
        PrintResult(result);
    }

     private static async Task TestGetBundlesForGame()
    {
        string? gameId = PromptForGameId();
        if (gameId == null) return;
        string country = PromptForCountry();
        bool expired = false;

        Console.Write("Include expired bundles? (y/N): ");
        if (Console.ReadLine()?.Trim().Equals("y", StringComparison.OrdinalIgnoreCase) == true)
        {
            expired = true;
        }

        Console.WriteLine($"\nCalling GetBundlesForGameAsync(id: \"{gameId}\", country: \"{country}\", expired: {expired})...");
        var result = await _client!.GetBundlesForGameAsync(gameId, country, expired: expired);
        PrintResult(result);
    }

    private static async Task TestGetGameSubscriptions()
    {
        string? gameId = PromptForGameId();
        if (gameId == null) return;
        string country = PromptForCountry();

        Console.WriteLine($"\nCalling GetGameSubscriptionsAsync(ids: [\"{gameId}\"], country: \"{country}\")...");
        var result = await _client!.GetGameSubscriptionsAsync(new List<string> { gameId }, country);
        PrintResult(result);
    }

    private static async Task TestGetShops()
    {
        string country = PromptForCountry();
        Console.WriteLine($"\nCalling GetShopsAsync(country: \"{country}\")...");
        var result = await _client!.GetShopsAsync(country);
        PrintResult(result);
    }

    // Helper to pretty-print results as JSON
    private static void PrintResult(object? result)
    {
        Console.WriteLine("\n--- RESULT ---");
        if (result == null)
        {
            Console.WriteLine("(null)");
        }
        else
        {
            try
            {
                // Use Newtonsoft.Json for pretty printing
                string jsonResult = JsonConvert.SerializeObject(result, Formatting.Indented, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });
                Console.WriteLine(jsonResult);
            }
            catch (Exception ex)
            {
                 Console.ForegroundColor = ConsoleColor.DarkYellow;
                 Console.WriteLine($"Could not serialize result: {ex.Message}");
                 Console.ResetColor();
                 Console.WriteLine(result.ToString()); // Fallback to ToString()
            }
        }
         Console.WriteLine("--------------");
    }
}