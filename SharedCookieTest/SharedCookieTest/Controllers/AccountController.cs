using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;


namespace SharedCookieTest.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        public AccountController()
        {
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
			//ViewBag.ReturnUrl = returnUrl;
			SsoLogin();
			return RedirectToAction("Index", "Home");
        }

		private void SsoLogin()
		{
			try
			{
				var userEmail = "tony.stark@starkindustries.test";
				IList<Claim> claims = new List<Claim>
					{
						new Claim(ClaimTypes.Name, "tstark"),
						new Claim(ClaimTypes.Email, userEmail),
						new Claim(ClaimTypes.Role, "admin"),
						new Claim(ClaimTypes.NameIdentifier, userEmail),
						new Claim("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider", userEmail)
					};

				ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);

				var authManager = Request.GetOwinContext().Authentication;
				authManager.SignIn(new AuthenticationProperties { IsPersistent = true, IssuedUtc = DateTime.Now, ExpiresUtc = DateTime.Now.AddMinutes(2) }, claimsIdentity);
			}
			catch (Exception ex)
			{
				string msg = ex.Message;
			}
		}
	}
}