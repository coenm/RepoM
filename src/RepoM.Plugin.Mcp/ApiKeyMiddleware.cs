namespace RepoM.Plugin.Mcp;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

internal sealed class ApiKeyMiddleware
{
    private const string API_KEY_HEADER = "X-Api-Key";
    private readonly RequestDelegate _next;
    private readonly string _apiKey;

    public ApiKeyMiddleware(RequestDelegate next, string apiKey)
    {
        _next = next;
        _apiKey = apiKey;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(API_KEY_HEADER, out Microsoft.Extensions.Primitives.StringValues extractedApiKey) ||
            extractedApiKey.Count != 1 ||
            !string.Equals(extractedApiKey[0], _apiKey, System.StringComparison.Ordinal))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Invalid or missing API key.").ConfigureAwait(false);
            return;
        }

        await _next(context).ConfigureAwait(false);
    }
}
