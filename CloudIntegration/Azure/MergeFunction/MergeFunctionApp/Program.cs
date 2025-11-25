// <copyright file="Program.cs" company="Progress Software Corporation">
// Copyright (c) Progress Software Corporation. All rights reserved.
// </copyright>

/// <summary>
/// Entry point for the Azure Functions application.
/// Configures the host builder with Functions Web Application support and Application Insights telemetry.
/// </summary>

using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Configure and build the Azure Functions host with isolated worker model
var host = new HostBuilder()
    .ConfigureFunctionsWebApplication() // Enable Functions Web Application for HTTP triggers
    .ConfigureServices(services =>
    {
        // Add Application Insights telemetry collection for monitoring
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .Build();

// Start the Azure Functions host
host.Run();