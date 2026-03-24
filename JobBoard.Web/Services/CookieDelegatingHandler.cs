namespace JobBoard.Web.Services;

public class CookieDelegatingHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var context = httpContextAccessor.HttpContext;

        if (context != null)
        {
            var cookie = context.Request.Headers["Cookie"].ToString();

            if (!string.IsNullOrEmpty(cookie))
            {
                request.Headers.Add("Cookie", cookie);
            }
        }

        if (request.Headers.TryGetValues("Cookie", out var cookies))
        {
            Console.WriteLine($"[Outgoing API Call] Cookies: {string.Join("; ", cookies)}");
        }
        else
        {
            Console.WriteLine("[Outgoing API Call] No cookies found in request headers.");
        }

        return base.SendAsync(request, cancellationToken);
    }
}