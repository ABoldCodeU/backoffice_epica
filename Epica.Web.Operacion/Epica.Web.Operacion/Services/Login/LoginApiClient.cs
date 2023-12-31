﻿using Epica.Web.Operacion.Config;
using Epica.Web.Operacion.Helpers;
using Epica.Web.Operacion.Models.Request;
using Epica.Web.Operacion.Services.UserResolver;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace Epica.Web.Operacion.Services.Login
{
    public class LoginApiClient: ApiClientBase, ILoginApiClient
    {
        public LoginApiClient(
            HttpClient httpClient,
            ILogger<LoginApiClient> logger,
            IOptions<UrlsConfig> config,
            IHttpContextAccessor httpContext,
            IConfiguration configuration,
            IUserResolver userResolver) : base(httpClient, logger, config, httpContext, configuration, userResolver)
        {
        }

        public async Task<object> GetCredentialsAsync(LoginRequest loginRequest, UserContextService userContextService)
        {
            MensajeResponse mensajeResponse = new MensajeResponse();
            LoginResponse loginResponse = new LoginResponse();

            try
            {
                var uri = Urls.Transaccion + UrlsConfig.LoginOperations.GetCredentials();
                var jsonRequest = JsonConvert.SerializeObject(loginRequest);
                var httpContent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                var response = await ApiClient.PostAsync(uri, httpContent);

                if (response.IsSuccessStatusCode)
                {
                    response.EnsureSuccessStatusCode();
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    loginResponse = JsonConvert.DeserializeObject<LoginResponse>(jsonResponse)!;

                    loginResponse.IsAuthenticated = true;
                    loginResponse.DireccionIp = loginRequest.Ip;
                    loginResponse.NombreDispositivo = loginRequest.DispositivoAcceso;
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, loginResponse.Usuario!)
                    };
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                    userContextService.SetLoginResponse(loginResponse);
                    return loginResponse;
                }
                else if (response.StatusCode == HttpStatusCode.GatewayTimeout)
                {
                    mensajeResponse = new MensajeResponse
                    {
                        Codigo = "504",
                        message = "Error 504: Gateway Timeout",
                        Error = true
                    };
                    return mensajeResponse;
                }
            }
            catch (Exception ex)
            {
                mensajeResponse = new MensajeResponse
                {
                    Codigo = "400",
                    message = ex.Message,
                    Error = true
                };
                return mensajeResponse;
            }
            mensajeResponse = new MensajeResponse
            {
                Codigo = "404",
                Error = true
            };
            return mensajeResponse;
        }
        public async Task LogoutAsync(HttpContext httpContext)
        {
            await httpContext.SignOutAsync("EpicaWebEsquema");
            httpContext.Session.Clear();
            httpContext.Response.Clear();
        }
    }
}
