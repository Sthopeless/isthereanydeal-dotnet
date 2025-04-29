# IsThereAnyDeal .NET Client Library

[![NuGet Version](https://img.shields.io/nuget/v/IsThereAnyDealApi.svg)](https://www.nuget.org/packages/IsThereAnyDealApi/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/IsThereAnyDealApi.svg)](https://www.nuget.org/packages/IsThereAnyDealApi/)

A .NET client library for interacting with the [IsThereAnyDeal API (v2)](https://api-docs.itad.rekt.net/). This library uses [RestEase](https://github.com/canton7/RestEase) and is compatible with .NET Standard 2.0+.

It provides easy access to ITAD features including:
* Game lookups (by title, Steam AppID, ITAD ID, etc.)
* Current game prices and deals across various stores
* Historical price information (overall lows, store lows, price history)
* Bundle information
* User waitlists, collections, notes, and notifications (via OAuth)
* Store information

## Installation

Install via [NuGet](https://www.nuget.org/packages/IsThereAnyDealApi/):

```bash
# .NET CLI
dotnet add package IsThereAnyDealApi

# Package Manager Console
Install-Package IsThereAnyDealApi
```

## Usage

### Quickstart

The library uses [RestEase](https://github.com/canton7/RestEase) to provide a typed client for the ITAD API.

Most public endpoints require an **API Key**, while user-specific endpoints (Waitlist, Collection, etc.) require **OAuth 2.0**.

#### Using an API Key

You obtain an API key by [registering your app](https://isthereanydeal.com/apps/my/) on the ITAD website.

```csharp
using IsThereAnyDeal.Api;
using IsThereAnyDeal.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Get your API Key from ITAD registration (e.g., load from config)
string? apiKey = Environment.GetEnvironmentVariable("ITAD_API_KEY"); // Or load from elsewhere

if (string.IsNullOrEmpty(apiKey))
{
    Console.WriteLine("API Key is required!");
    return;
}

var client = new IsThereAnyDealApiClient(apiKey);

// --- Example 1: Lookup a game by Steam AppID ---
int steamAppId = 570; // Dota 2
GameLookupResult? lookupResult = await client.LookupGameAsync(appid: steamAppId);

if (lookupResult != null && lookupResult.Found && lookupResult.Game != null)
{
    Console.WriteLine($"Found Game: {lookupResult.Game.Title} (ITAD ID: {lookupResult.Game.Id})");
    string gameId = lookupResult.Game.Id; // Use this ID for other calls

    // --- Example 2: Get current prices for the found game ---
    List<GamePrices>? pricesResult = await client.GetGamePricesAsync(new List<string> { gameId }, country: "US");
    GamePrices? gamePrices = pricesResult?.FirstOrDefault();

    if (gamePrices != null && gamePrices.Deals.Any())
    {
        Deal? bestDeal = gamePrices.Deals.OrderBy(d => d.Price.Amount).First();
        Console.WriteLine($"Current Best Deal: {bestDeal.Price.Amount} {bestDeal.Price.Currency} at {bestDeal.Shop.Name} ({bestDeal.Cut}% off)");
        Console.WriteLine($"Link: {bestDeal.Url}");
    }
    else
    {
        Console.WriteLine("No current deals found for this game.");
    }
}
else
{
    Console.WriteLine($"Game with AppID {steamAppId} not found.");
}
```

#### Using OAuth 2.0 (for User Data)

This library supports accessing user-specific endpoints (like Waitlist, Collection) via OAuth. ITAD uses the **Authorization Code flow with PKCE extension**.

This flow requires user interaction (browser redirect for login and authorization). Therefore, the **consuming application** (your app using this library) is responsible for:

1.  Handling the interactive OAuth flow (generating PKCE, redirecting the user, receiving the callback, exchanging the code for tokens).
2.  Obtaining the `access_token`.
3.  Providing the token to this library instance.

```csharp
// Assume 'myAccessToken' was obtained via the OAuth flow handled by your application
string myAccessToken = "USER_ACCESS_TOKEN_OBTAINED_VIA_OAUTH_FLOW";

// Initialize client (can be done with or without API key depending on needs)
var userClient = new IsThereAnyDealApiClient(apiKey); // API key might still be needed for other calls

// Set the user's token
userClient.SetUserAccessToken(myAccessToken);

// Now you can call OAuth-protected methods
try
{
    UserInfo userInfo = await userClient.GetUserInfoAsync();
    Console.WriteLine($"Authenticated as user: {userInfo.Username}");

    List<WaitlistGame> waitlist = await userClient.GetWaitlistGamesAsync();
    Console.WriteLine($"User has {waitlist.Count} games on their waitlist.");
    // ... call other methods like GetCollectionGamesAsync, etc.
}
catch (RestEase.ApiException apiEx)
{
    // Handle potential 401 Unauthorized or other API errors
    Console.WriteLine($"API Error: {apiEx.StatusCode} - {apiEx.ReasonPhrase}");
    // If 401, the access token might be expired or invalid - trigger refresh token flow
}
```

*(See the `IsThereAnyDealApi.ManualTester` project in this repository for an example of how to handle the OAuth flow semi-manually for testing purposes).*

### API Documentation

Refer to the official [IsThereAnyDeal API Documentation](https://api-docs.itad.rekt.net/) for details on all available endpoints, parameters, and response models. The models in this library (`IsThereAnyDeal.Api.Models`) correspond directly to the schemas defined there.

## Versioning Policy

This project aims to follow Semantic Versioning (SemVer).
* **MAJOR** version bumps for incompatible API changes.
* **MINOR** version bumps for adding functionality in a backward-compatible manner.
* **PATCH** version bumps for backward-compatible bug fixes.

## Contributing / Local Development

### Prerequisites

* .NET 6 SDK or later (Runtime supports .NET Standard 2.0+)
* VS Code, Visual Studio, or JetBrains Rider

### Setup

1.  Clone the repository.
2.  **Create Secrets File:** For running the `ManualTester` project or potential future integration tests, create a file named `testsettings.json` in the `IsThereAnyDealApi.ManualTester` project directory (ensure it's set to "Copy to Output Directory: Copy if newer"). Populate it with your credentials obtained from the [ITAD App Registration](https://isthereanydeal.com/apps/my/):
    ```json
    {
      "ITAD_CLIENT_ID": "YOUR_ITAD_APP_CLIENT_ID",
      "ITAD_CLIENT_SECRET": "YOUR_ITAD_APP_CLIENT_SECRET",
      "ITAD_API_KEY": "YOUR_ITAD_API_KEY"
    }
    ```
    *(**Note:** This file is included in `.gitignore` and should NOT be committed).*
3.  **Define Redirect URI:** In `IsThereAnyDealApi.ManualTester/Program.cs`, update the `YourRedirectUri` constant to match the URI you registered with ITAD (e.g., `http://localhost:1234/callback`).
4.  Build the solution:
    ```bash
    dotnet build IsThereAnyDeal.sln
    ```
5.  Run the Manual Tester (optional):
    ```bash
    dotnet run --project IsThereAnyDealApi.ManualTester
    ```
    *(Or use the launch configuration provided in `.vscode/launch.json`)*

Contributions are welcome! Please feel free to open an issue or submit a pull request.