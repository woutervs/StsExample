using System.Web.Http;
using StsExample.OAuth;

namespace StsExample.Controllers
{
    [RoutePrefix("api/external")]
    public class ExternalAuthenticationController : ApiController
    {
        [Route("discord")]
        [HttpGet]
        public IHttpActionResult Authenticate([FromUri] string returnUrl, [FromUri] string client)
        {
            return new ChallengeResult(Request, "Discord", returnUrl, client);
        }

        //[Route("callback")]
        //public IHttpActionResult Callback([FromUri] returnUrl)
        //{
        //    return Redirect(Url.)
        //}


    }
}