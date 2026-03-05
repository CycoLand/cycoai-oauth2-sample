using CycoAi.OAuth2.Client;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;

Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
Console.WriteLine("║       CycoAi OAuth2 Integration Sample                         ║");
Console.WriteLine("║  Demonstrates authentication flows using CycoAi.OAuth2.Client  ║");
Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
Console.WriteLine();

// Load configuration
var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var apiBaseUrl = config["CycoAi:ApiBaseUrl"] ?? "https://auth.cyco.ai";
var clientId = config["CycoAi:ClientId"] ?? "cycoai-sample";

Console.WriteLine($"API Base URL: {apiBaseUrl}");
Console.WriteLine($"Client ID: {clientId}");
Console.WriteLine();

// Initialize OAuth2 clients
var oauth2Client = new OAuth2Client(clientId, apiBaseUrl);
var deviceFlowClient = new DeviceFlowClient(clientId, apiBaseUrl);

// Storage for demonstration
string? accessToken = null;
string? refreshToken = null;

// Main menu loop
while (true)
{
    Console.WriteLine("\n" + new string('─', 64));
    Console.WriteLine("Select an option:");
    Console.WriteLine("  1. Password Flow Login");
    Console.WriteLine("  2. Device Flow Login");
    Console.WriteLine("  3. Refresh Access Token");
    Console.WriteLine("  4. Call Protected API (/api/profile)");
    Console.WriteLine("  5. Show Current Tokens");
    Console.WriteLine("  6. Exit");
    Console.Write("\nChoice: ");

    var choice = Console.ReadLine();

    try
    {
        switch (choice)
        {
            case "1":
                await DemoPasswordFlow();
                break;
            case "2":
                await DemoDeviceFlow();
                break;
            case "3":
                await DemoTokenRefresh();
                break;
            case "4":
                await DemoProtectedApiCall();
                break;
            case "5":
                ShowTokens();
                break;
            case "6":
                Console.WriteLine("\nGoodbye! 👋");
                return;
            default:
                Console.WriteLine("\n⚠️  Invalid choice. Please select 1-6.");
                break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n❌ Error: {ex.Message}");
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// Demo Functions
// ═══════════════════════════════════════════════════════════════════════════

async Task DemoPasswordFlow()
{
    Console.WriteLine("\n╔════════════════════════════════════════════════════════════════╗");
    Console.WriteLine("║  Demo 1: Password Flow Login                                   ║");
    Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
    Console.WriteLine("\nThis demonstrates the standard OAuth2 password grant flow.");
    Console.WriteLine("Use this for direct username/password authentication.\n");

    Console.Write("Email: ");
    var email = Console.ReadLine();

    Console.Write("Password: ");
    var password = ReadPassword();

    Console.WriteLine("\n\n🔄 Authenticating...");

    var result = await oauth2Client.LoginAsync(email!, password!);

    if (result.IsSuccess)
    {
        accessToken = result.Value!.AccessToken;
        refreshToken = result.Value.RefreshToken;

        Console.WriteLine("✅ Login successful!");
        Console.WriteLine($"\n📋 Token Info:");
        Console.WriteLine($"   Access Token: {accessToken[..20]}...");
        Console.WriteLine($"   Refresh Token: {refreshToken?[..20]}...");
        Console.WriteLine($"   Expires In: {result.Value.ExpiresIn} seconds");
        Console.WriteLine($"   Token Type: {result.Value.TokenType}");
    }
    else
    {
        Console.WriteLine($"❌ Login failed: {result.Error}");
    }
}

async Task DemoDeviceFlow()
{
    Console.WriteLine("\n╔════════════════════════════════════════════════════════════════╗");
    Console.WriteLine("║  Demo 2: Device Flow Login                                     ║");
    Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
    Console.WriteLine("\nThis demonstrates the OAuth2 device authorization grant flow.");
    Console.WriteLine("Perfect for CLI tools and headless environments.\n");

    Console.WriteLine("🔄 Initiating device flow...");

    var deviceCodeResult = await deviceFlowClient.InitiateDeviceFlowAsync();

    if (!deviceCodeResult.IsSuccess)
    {
        Console.WriteLine($"❌ Failed to initiate device flow: {deviceCodeResult.Error}");
        return;
    }

    var deviceCode = deviceCodeResult.Value!;

    Console.WriteLine("\n✅ Device flow initiated!");
    Console.WriteLine($"\n📱 Please complete authorization:");
    Console.WriteLine($"   1. Go to: {deviceCode.VerificationUri}");
    Console.WriteLine($"   2. Enter code: {deviceCode.UserCode}");
    Console.WriteLine($"\n⏱️  Code expires in {deviceCode.ExpiresIn} seconds");
    Console.WriteLine($"   Polling every {deviceCode.Interval} seconds...\n");

    var pollResult = await deviceFlowClient.PollForCompletionAsync(
        deviceCode.DeviceCode,
        deviceCode.Interval,
        TimeSpan.FromSeconds(deviceCode.ExpiresIn));

    if (pollResult.IsSuccess)
    {
        accessToken = pollResult.Value!.AccessToken;
        refreshToken = pollResult.Value.RefreshToken;

        Console.WriteLine("✅ Device flow completed successfully!");
        Console.WriteLine($"\n📋 Token Info:");
        Console.WriteLine($"   Access Token: {accessToken[..20]}...");
        Console.WriteLine($"   Refresh Token: {refreshToken?[..20]}...");
        Console.WriteLine($"   Expires In: {pollResult.Value.ExpiresIn} seconds");
    }
    else
    {
        Console.WriteLine($"❌ Device flow failed: {pollResult.Error}");
    }
}

async Task DemoTokenRefresh()
{
    Console.WriteLine("\n╔════════════════════════════════════════════════════════════════╗");
    Console.WriteLine("║  Demo 3: Token Refresh                                         ║");
    Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
    Console.WriteLine("\nThis demonstrates automatic token refresh using refresh tokens.");
    Console.WriteLine("Refresh tokens have longer lifetime than access tokens.\n");

    if (string.IsNullOrEmpty(refreshToken))
    {
        Console.WriteLine("❌ No refresh token available. Please login first (option 1 or 2).");
        return;
    }

    Console.WriteLine("🔄 Refreshing access token...");

    var result = await oauth2Client.RefreshTokenAsync(refreshToken, apiBaseUrl);

    if (result.IsSuccess)
    {
        accessToken = result.Value!.AccessToken;
        refreshToken = result.Value.RefreshToken; // New refresh token

        Console.WriteLine("✅ Token refreshed successfully!");
        Console.WriteLine($"\n📋 New Token Info:");
        Console.WriteLine($"   Access Token: {accessToken[..20]}...");
        Console.WriteLine($"   Refresh Token: {refreshToken?[..20]}...");
        Console.WriteLine($"   Expires In: {result.Value.ExpiresIn} seconds");
        Console.WriteLine("\n💡 Tip: Refresh tokens are single-use. A new one is issued each time.");
    }
    else
    {
        Console.WriteLine($"❌ Token refresh failed: {result.Error}");
        Console.WriteLine("   Your session may have expired. Please login again.");
        accessToken = null;
        refreshToken = null;
    }
}

async Task DemoProtectedApiCall()
{
    Console.WriteLine("\n╔════════════════════════════════════════════════════════════════╗");
    Console.WriteLine("║  Demo 4: Call Protected API                                    ║");
    Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
    Console.WriteLine("\nThis demonstrates calling a protected API endpoint using the");
    Console.WriteLine("access token as a Bearer token in the Authorization header.\n");

    if (string.IsNullOrEmpty(accessToken))
    {
        Console.WriteLine("❌ No access token available. Please login first (option 1 or 2).");
        return;
    }

    Console.WriteLine("🔄 Calling GET /api/profile...");

    using var httpClient = new HttpClient { BaseAddress = new Uri(apiBaseUrl) };
    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

    try
    {
        var response = await httpClient.GetAsync("/api/profile");

        Console.WriteLine($"\n📡 Response Status: {(int)response.StatusCode} {response.ReasonPhrase}");

        if (response.IsSuccessStatusCode)
        {
            var profile = await response.Content.ReadFromJsonAsync<ProfileResponse>();

            Console.WriteLine("\n✅ Profile retrieved successfully!");
            Console.WriteLine($"\n👤 User Profile:");
            Console.WriteLine($"   Email: {profile?.Email}");
            Console.WriteLine($"   Display Name: {profile?.DisplayName ?? "(not set)"}");
            Console.WriteLine($"   Avatar URL: {profile?.AvatarUrl ?? "(not set)"}");
            Console.WriteLine($"   GitHub Username: {profile?.GitHubUsername ?? "(not linked)"}");
            Console.WriteLine($"   Email Confirmed: {profile?.EmailConfirmed}");
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"❌ API call failed: {error}");

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                Console.WriteLine("\n💡 Tip: Access token may have expired. Try refreshing (option 3).");
            }
        }
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"❌ Network error: {ex.Message}");
        Console.WriteLine($"\n💡 Check that API URL is correct: {apiBaseUrl}");
    }
}

void ShowTokens()
{
    Console.WriteLine("\n╔════════════════════════════════════════════════════════════════╗");
    Console.WriteLine("║  Current Token Status                                          ║");
    Console.WriteLine("╚════════════════════════════════════════════════════════════════╝\n");

    if (string.IsNullOrEmpty(accessToken))
    {
        Console.WriteLine("❌ No tokens stored. Please login (option 1 or 2).");
    }
    else
    {
        Console.WriteLine("✅ Tokens available:");
        Console.WriteLine($"\n   Access Token: {accessToken[..20]}...");
        Console.WriteLine($"   Refresh Token: {(refreshToken != null ? refreshToken[..20] + "..." : "N/A")}");
        Console.WriteLine("\n💡 Note: In production, store tokens securely (not in memory).");
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// Helper Functions
// ═══════════════════════════════════════════════════════════════════════════

string ReadPassword()
{
    var password = "";
    ConsoleKeyInfo key;

    do
    {
        key = Console.ReadKey(intercept: true);

        if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
        {
            password += key.KeyChar;
            Console.Write("*");
        }
        else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
        {
            password = password[0..^1];
            Console.Write("\b \b");
        }
    } while (key.Key != ConsoleKey.Enter);

    return password;
}

// ═══════════════════════════════════════════════════════════════════════════
// Response Models (matches CycoAi API)
// ═══════════════════════════════════════════════════════════════════════════

record ProfileResponse(
    string Email,
    string? DisplayName,
    string? AvatarUrl,
    string? GitHubUsername,
    bool EmailConfirmed
);
