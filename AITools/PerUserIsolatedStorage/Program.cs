using System.Text;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.AI;
using Microsoft.IdentityModel.Tokens;
using Telerik.Documents.AI.AgentTools.Examples;

var builder = WebApplication.CreateBuilder(args);

// Add controllers
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Configure JWT Authentication
// For testing, we use a simple symmetric key. In production, use proper key management.
var jwtKey = builder.Configuration["Jwt:Key"] ?? "YourSuperSecretKeyForTestingPurposes12345!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "PerUserIsolatedStorage";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "PerUserIsolatedStorage";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// Register IChatClient - Configure with Azure OpenAI
var azureEndpoint = builder.Configuration["AzureOpenAI:Endpoint"];
var azureApiKey = builder.Configuration["AzureOpenAI:ApiKey"];
var azureDeployment = builder.Configuration["AzureOpenAI:DeploymentName"] ?? "gpt-4o";

if (!string.IsNullOrEmpty(azureEndpoint) && !string.IsNullOrEmpty(azureApiKey))
{
    builder.Services.AddSingleton<IChatClient>(sp =>
    {
        var client = new AzureOpenAIClient(
            new Uri(azureEndpoint),
            new AzureKeyCredential(azureApiKey));
        return client.GetChatClient(azureDeployment).AsIChatClient();
    });
}
else
{
    // Fallback: Register a placeholder that will throw a helpful error
    builder.Services.AddSingleton<IChatClient>(sp =>
        throw new InvalidOperationException(
            "Azure OpenAI not configured. Set 'AzureOpenAI:Endpoint', 'AzureOpenAI:ApiKey', and 'AzureOpenAI:DeploymentName' in appsettings.json."));
}

// Register background service for session cleanup
builder.Services.AddHostedService<SessionCleanupService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

