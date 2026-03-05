# CycoAi OAuth2 Integration Sample

[![Build and Test](https://github.com/CycoLand/cycoai-oauth2-sample/actions/workflows/build.yml/badge.svg)](https://github.com/CycoLand/cycoai-oauth2-sample/actions/workflows/build.yml)

Sample console application demonstrating how to integrate CycoAi authentication using the `CycoAi.OAuth2.Client` NuGet package.

## 🎯 What This Demonstrates

This sample shows how to:

- ✅ **Password flow authentication** - Standard username/password login
- ✅ **Device flow authentication** - For headless/CLI scenarios  
- ✅ **Automatic token refresh** - Keep users authenticated seamlessly
- ✅ **Call protected APIs** - Use bearer tokens to access user data
- ✅ **Error handling** - Graceful handling of auth failures
- ✅ **Cross-organization package access** - Using GitHub App for CI/CD

## 🚀 Quick Start

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
- CycoAi account ([register here](https://auth.cyco.ai/register.html))
- For local development: GitHub PAT with `read:packages` scope

### Run the Sample

```bash
# Clone the repository
git clone https://github.com/CycoLand/cycoai-oauth2-sample.git
cd cycoai-oauth2-sample

# Set GitHub token (for package restore)
export GITHUB_TOKEN="your_github_pat_here"  # Linux/macOS
set GITHUB_TOKEN=your_github_pat_here       # Windows CMD

# Restore and run
dotnet run --project src/CycoAiOAuth2Sample
```

### Interactive Menu

When you run the sample, you'll see:

```
╔════════════════════════════════════════════════════════════════╗
║       CycoAi OAuth2 Integration Sample                         ║
║  Demonstrates authentication flows using CycoAi.OAuth2.Client  ║
╚════════════════════════════════════════════════════════════════╝

Select an option:
  1. Password Flow Login
  2. Device Flow Login
  3. Refresh Access Token
  4. Call Protected API (/api/profile)
  5. Show Current Tokens
  6. Exit
```

## 📚 What You'll Learn

### 1. Password Flow (Option 1)

Shows how to authenticate users with email and password:

```csharp
var oauth2Client = new OAuth2Client("cycoai-sample", "https://auth.cyco.ai");
var result = await oauth2Client.LoginAsync(email, password);

if (result.IsSuccess)
{
    var accessToken = result.Value!.AccessToken;
    var refreshToken = result.Value.RefreshToken;
    // Use tokens...
}
```

### 2. Device Flow (Option 2)

Perfect for CLI tools - users authorize in browser:

```csharp
var deviceFlowClient = new DeviceFlowClient("cycoai-sample", "https://auth.cyco.ai");

// Step 1: Initiate flow
var deviceCode = await deviceFlowClient.InitiateDeviceFlowAsync();
Console.WriteLine($"Go to: {deviceCode.Value.VerificationUri}");
Console.WriteLine($"Enter code: {deviceCode.Value.UserCode}");

// Step 2: Poll for completion
var tokens = await deviceFlowClient.PollForCompletionAsync(
    deviceCode.Value.DeviceCode,
    deviceCode.Value.Interval,
    TimeSpan.FromSeconds(deviceCode.Value.ExpiresIn));
```

### 3. Token Refresh (Option 3)

Automatically get new access tokens without re-authentication:

```csharp
var result = await oauth2Client.RefreshTokenAsync(refreshToken, apiBaseUrl);
// Refresh tokens are single-use - you get a new one each time!
```

### 4. Calling Protected APIs (Option 4)

Use access tokens as Bearer tokens:

```csharp
using var httpClient = new HttpClient();
httpClient.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", accessToken);

var response = await httpClient.GetAsync("https://auth.cyco.ai/api/profile");
```

## 🔧 Integrating Into Your Own Project

### Step 1: Add the Package

```bash
dotnet add package CycoAi.OAuth2.Client --source "https://nuget.pkg.github.com/CycoAi/index.json"
```

### Step 2: Configure Package Source

Create `nuget.config` in your project root:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="CycoAi-GitHub" value="https://nuget.pkg.github.com/CycoAi/index.json" />
  </packageSources>
  <packageSourceCredentials>
    <CycoAi-GitHub>
      <add key="Username" value="github" />
      <add key="ClearTextPassword" value="%GITHUB_TOKEN%" />
    </CycoAi-GitHub>
  </packageSourceCredentials>
</configuration>
```

### Step 3: Request Client Registration

Contact CycoAi support to register your application's `client_id`.

### Step 4: Start Using!

```csharp
var client = new OAuth2Client("your-client-id", "https://auth.cyco.ai");
var result = await client.LoginAsync(email, password);
```

## 🔒 Security Notes

- **Client ID:** Public identifier (safe to commit in source code)
- **Access Tokens:** Short-lived (typically 1 hour), store securely
- **Refresh Tokens:** Long-lived, single-use, store encrypted
- **Package Code:** Contains no secrets (safe to inspect)

The security boundary is the **CycoAi IdentityService** (server), not the client package.

## 🏗️ CI/CD with GitHub App

This sample uses a **GitHub App** for cross-organization package access in CI/CD:

**Benefits over PATs:**
- ✅ Short-lived tokens (auto-rotate)
- ✅ Fine-grained permissions
- ✅ No user account dependency
- ✅ Better audit trail

See `.github/workflows/build.yml` for implementation.

## 📖 Additional Resources

- [CycoAi Authentication Service](https://auth.cyco.ai)
- [Register for CycoAi Account](https://auth.cyco.ai/register.html)
- [User Dashboard](https://auth.cyco.ai/dashboard.html)
- [Support](https://github.com/CycoAi/Cycodum/issues)

## 📄 License

MIT License - See [LICENSE](LICENSE) file for details.

---

**Questions?** Open an issue or reach out to the CycoAi team!
