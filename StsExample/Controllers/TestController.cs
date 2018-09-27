using System.Net.Http;
using System.Web.Http;

namespace StsExample.Controllers
{
    public class TestController : ApiController
    {
        //[Authorize(Roles = "Administrator, User")]
        [Authorize]
        public IHttpActionResult Get()
        {
            var context = this.Request.GetOwinContext();

            return Ok("Hello world");
        }
    }
}
