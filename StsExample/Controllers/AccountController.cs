using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using StsExample.Models.View;
using StsExample.Repositories;

namespace StsExample.Controllers
{
    [RoutePrefix("api/account")]
    public class AccountController : ApiController
    {
        private readonly AuthenticationRepository authenticationRepository;

        public AccountController()
        {
            authenticationRepository = new AuthenticationRepository();
        }

        [AllowAnonymous]
        [Route("register")]
        public async Task<IHttpActionResult> Register(UserViewModel userViewModel)
        {
            if (userViewModel == null)
            {
                ModelState.AddModelError("userViewModel", "Values cannot be null.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await authenticationRepository.RegisterUserAsync(userViewModel);

            var errorResult = GetErrorResult(result);

            return errorResult ?? Ok();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                authenticationRepository.Dispose();
            }

            base.Dispose(disposing);
        }

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }
    }
}
