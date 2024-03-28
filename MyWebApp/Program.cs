using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie()
.AddOpenIdConnect(options =>
{
    options.ResponseType = OpenIdConnectResponseType.Code;
    options.MetadataAddress = $"https://cognito-idp.{configuration["AWS:Region"]}.amazonaws.com/{configuration["AWS:UserPoolId"]}/.well-known/openid-configuration";
    options.ClientId = configuration["AWS:UserPoolClientId"];
    options.ClientSecret = configuration["AWS:UserPoolClientSecret"];
    options.UsePkce = true;
    options.Events = new OpenIdConnectEvents()
    {
        OnRedirectToIdentityProviderForSignOut = OnRedirectToIdentityProviderForSignOut
    };
    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("email");
    options.Scope.Add("aws.cognito.signin.user.admin");
    options.Scope.Add("profile");
    options.Scope.Add("weatherapi/read");
    options.SaveTokens = true;

    Task OnRedirectToIdentityProviderForSignOut(RedirectContext context)
    {
        context.ProtocolMessage.Scope = "openid";
        context.ProtocolMessage.ResponseType = OpenIdConnectResponseType.Code;
        var cognitoDomain = $"https://{configuration["AWS:Domain"]}.auth.{configuration["AWS:Region"]}.amazoncognito.com" ;
        var clientId = configuration["AWS:UserPoolClientId"]; ;
        var logoutUrl = $"{context.Request.Scheme}://{context.Request.Host}{configuration["AWS:AppSignOutUrl"]}";
        context.ProtocolMessage.IssuerAddress = $"{cognitoDomain}/logout?client_id={clientId}&logout_uri={logoutUrl}&redirect_uri={logoutUrl}";
        context.Properties.Items.Remove(CookieAuthenticationDefaults.AuthenticationScheme);
        context.Properties.Items.Remove(OpenIdConnectDefaults.AuthenticationScheme);
        return Task.CompletedTask;
    }
});

builder.Services.AddOpenIdConnectAccessTokenManagement();

builder.Services.AddHttpClient("api", client =>
{
    client.BaseAddress = new Uri("https://localhost:7064/");
})
.AddUserAccessTokenHandler();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

