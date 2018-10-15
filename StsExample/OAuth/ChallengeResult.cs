using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Owin.Security;

namespace StsExample.OAuth
{
    public class ChallengeResult : IHttpActionResult
    {
        private readonly HttpRequestMessage request;
        private readonly string provider;
        private readonly string redirectUrl;
        private readonly string client;

        public ChallengeResult(HttpRequestMessage request, string provider, string redirectUrl, string client)
        {
            this.request = request;
            this.provider = provider;
            this.redirectUrl = redirectUrl;
            this.client = client;
        }
        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            properties.Dictionary.Add("as:client_id", client);
            request.GetOwinContext().Authentication.Challenge(properties, provider);
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized) { RequestMessage = request };
            return Task.FromResult(response);
        }
    }
}