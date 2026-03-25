public class CookieDelegatingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CookieDelegatingHandler> _logger;

    public CookieDelegatingHandler(IHttpContextAccessor httpContextAccessor, ILogger<CookieDelegatingHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context != null)
        {
            var cookie = context.Request.Headers["Cookie"].ToString();
            if (!string.IsNullOrEmpty(cookie))
            {
                request.Headers.Add("Cookie", cookie);
                _logger.LogDebug("Forwarded cookie: {Cookie}", cookie);
            }
            else
            {
                _logger.LogWarning("No cookie found in HttpContext.Request.Headers.");
            }
        }
        else
        {
            _logger.LogWarning("HttpContext is null, cannot forward cookie.");
        }

        return await base.SendAsync(request, cancellationToken);
    }
}