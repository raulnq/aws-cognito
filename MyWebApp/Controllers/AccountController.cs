using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;

namespace MyWebApp.Controllers;

public class AccountController : Controller
{
    public AccountController()
    {

    }

    [HttpGet]

    public IActionResult Logout()
    {
        return SignOut(CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
    }

    [HttpGet]
    public IActionResult Loggedout()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Login(string returnUrl="/")
    {
        var props = new AuthenticationProperties()
        {
            RedirectUri = returnUrl,
        };
        return Challenge(props, OpenIdConnectDefaults.AuthenticationScheme);

    }
}