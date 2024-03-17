using Amazon.AspNetCore.Identity.Cognito;
using Amazon.CognitoIdentityProvider;
using Amazon.Extensions.CognitoAuthentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using MyWebApp.Models;

namespace MyWebApp.Controllers;

public class AccountController : Controller
{

    private readonly UserManager<CognitoUser> _userManager;
    private readonly SignInManager<CognitoUser> _signInManager;
    private readonly IAmazonCognitoIdentityProvider _provider;
    private readonly CognitoUserPool _pool;

    public AccountController(UserManager<CognitoUser> userManager,
        SignInManager<CognitoUser> signInManager, IAmazonCognitoIdentityProvider provider, CognitoUserPool pool)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _provider = provider;
        _pool = pool;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = _pool.GetUser(model.Email);
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Confirm");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("index", "home");
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult Confirm()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Confirm(ConfirmViewModel model)
    {
        if (ModelState.IsValid)
        {
            var userManager = _userManager as CognitoUserManager<CognitoUser>;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }
            var result = await userManager.ConfirmSignUpAsync(user, model.Code, true);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        return View(model);
    }
}