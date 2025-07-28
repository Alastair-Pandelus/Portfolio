using System.Web;

namespace Portfolio.Scrape.Api
{
    public class UrlDecodeHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.RequestUri = new Uri(HttpUtility.UrlDecode(request.RequestUri.ToString())); 

            var response = await base.SendAsync(request, cancellationToken);

            return response;
        }
    }
}
