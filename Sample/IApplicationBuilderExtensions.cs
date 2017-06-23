using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Sample
{
    public static class IApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseExternalAuth(this IApplicationBuilder application, IConfigurationRoot configuration)
        {
            var externalCookieScheme = application.ApplicationServices.GetRequiredService<IOptions<IdentityOptions>>().Value.Cookies.ExternalCookieAuthenticationScheme;
            // or use
            var facebookOptions = new FacebookOptions
            {
                AuthenticationScheme = "facebook",
                DisplayName = "Facebook",
                SignInScheme = externalCookieScheme,
                AppId = configuration["Auth:Facebook:AppId"],
                AppSecret = configuration["Auth:Facebook:AppSecret"],
                SaveTokens = true,
                Events = new OAuthEvents
                {
                    OnCreatingTicket = async context =>
                    {
                        var payload = await GetUserInfo(context);

                        JsonKeyClaim claims = new JsonKeyClaim(context.Identity, payload, context.Options.ClaimsIssuer);
                        claims.TryAddClaimByJsonKey("urn:facebook:photo", "picture");
                    }
                }
            };

            var gooleOptions = new GoogleOptions
            {
                AuthenticationScheme = "google",
                DisplayName = "Google",
                SignInScheme = externalCookieScheme,
                ClientId = configuration["Auth:Google:ClientId"],
                ClientSecret = configuration["Auth:Google:ClientSecret"],
                SaveTokens = true,
                Events = new OAuthEvents
                {
                    OnCreatingTicket = async context =>
                    {
                        var payload = await GetUserInfo(context);

                        JsonKeyClaim claims = new JsonKeyClaim(context.Identity, payload, context.Options.ClaimsIssuer);
                        claims.TryAddClaimByJsonSubKey("urn:google:photo", "image", "url");
                        claims.TryAddClaimByJsonKey(ClaimTypes.Locality, "language");
                        claims.TryAddClaimByJsonSubKey("urn:google:location", "location", "name");
                    }
                }
            };

            return application
                // Add external authentication middleware below. To configure them please see https://go.microsoft.com/fwlink/?LinkID=532715
                .UseFacebookAuthentication(facebookOptions)
                .UseGoogleAuthentication(gooleOptions);
        }

        private static async Task<JObject> GetUserInfo(OAuthCreatingTicketContext context)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

            var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"An error occurred when retrieving user information ({response.StatusCode}). Please check if the authentication information is correct and the corresponding Google+ API is enabled.");
            }

            return JObject.Parse(await response.Content.ReadAsStringAsync());
        }
    }
}