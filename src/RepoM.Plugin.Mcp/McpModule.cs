namespace RepoM.Plugin.Mcp;

using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HostFiltering;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using RepoM.Core.Plugin;
using RepoM.Core.Repositories.Store;

[UsedImplicitly]
internal class McpModule : IModule
{
    private readonly IRepositoryStore _repositoryStore;
    private readonly IMcpConfiguration _configuration;
    private readonly ILogger _logger;
    private WebApplication? _app;

    public McpModule(
        IRepositoryStore repositoryStore,
        IMcpConfiguration configuration,
        ILogger logger)
    {
        _repositoryStore = repositoryStore ?? throw new ArgumentNullException(nameof(repositoryStore));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task StartAsync()
    {
        try
        {
            var builder = WebApplication.CreateSlimBuilder();

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenLocalhost(_configuration.Port);
            });

            builder.Logging.ClearProviders();

            // Restrict accepted Host header values to loopback names only (DNS rebinding protection).
            builder.Services.Configure<HostFilteringOptions>(options =>
            {
                options.AllowedHosts = ["localhost", "127.0.0.1", "[::1]",];
            });

            builder.Services.AddSingleton(_repositoryStore);
            builder.Services
                .AddMcpServer(options =>
                {
                    options.ServerInfo = new()
                    {
                        Name = "RepoM",
                        Version = typeof(McpModule).Assembly.GetName().Version?.ToString() ?? "0.0.0",
                    };
                })
                .WithHttpTransport()
                .WithToolsFromAssembly(typeof(McpModule).Assembly);

            _app = builder.Build();

            _app.UseHostFiltering();

            if (!string.IsNullOrWhiteSpace(_configuration.ApiKey))
            {
                _app.UseMiddleware<ApiKeyMiddleware>(_configuration.ApiKey);
            }

            _app.MapMcp();

            _ = Task.Run(async () =>
            {
                try
                {
                    await _app.RunAsync().ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // Expected during shutdown
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "MCP server encountered an error. {Message}", ex.Message);
                }
            });

            _logger.LogInformation("MCP server started on port {Port}.", _configuration.Port);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start MCP server. {Message}", ex.Message);
        }
    }

    public async Task StopAsync()
    {
        if (_app != null)
        {
            _logger.LogInformation("MCP server stopping.");

            try
            {
                await _app.StopAsync(CancellationToken.None).ConfigureAwait(false);
                await _app.DisposeAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping MCP server. {Message}", ex.Message);
            }

            _app = null;
        }
    }
}
