﻿using Epica.Web.Operacion.Config;
using Epica.Web.Operacion.Models.Common;
using Epica.Web.Operacion.Models.Request;
using Epica.Web.Operacion.Services.UserResolver;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;

namespace Epica.Web.Operacion.Services.Transaccion
{
    public class TarjetasApiClient : ApiClientBase, ITarjetasApiClient
    {
        public TarjetasApiClient(
            HttpClient httpClient, 
            ILogger<CuentaApiClient> logger, 
            IOptions<UrlsConfig> config, 
            IHttpContextAccessor httpContext, 
            IConfiguration configuration, 
            IUserResolver userResolver) : base(httpClient, logger, config, httpContext, configuration, userResolver)
        {
        }

        public async Task<ResumenTarjetasResponse> GetBuscarTarjetasasync(string noTarjeta)
        {
            ResumenTarjetasResponse tarjetaResponse = new ResumenTarjetasResponse();
            try
            {
                var uri = Urls.Transaccion + UrlsConfig.TarjetasOperations.GetBuscarTarjeta(noTarjeta);
                var response = await ApiClient.GetAsync(uri);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    tarjetaResponse = JsonConvert.DeserializeObject<ResumenTarjetasResponse>(jsonResponse)!;
                }

            }
            catch (Exception ex)
            {
                return tarjetaResponse;
            }

            return tarjetaResponse;
        }
        public async Task<List<TarjetasResponse>> GetTarjetasClientesAsync(int idCliente)
        {

            List<TarjetasResponse>? listaClientes = new List<TarjetasResponse>();

            try
            {
                var uri = Urls.Transaccion + UrlsConfig.TarjetasOperations.GetTarjetasCliente(idCliente);
                var response = await ApiClient.GetAsync(uri);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        MissingMemberHandling = MissingMemberHandling.Ignore
                    };
                    listaClientes = JsonConvert.DeserializeObject<List<TarjetasResponse>>(jsonResponse,settings);
                }

            }
            catch (Exception ex)
            {
                return listaClientes!;
            }

            return listaClientes!;
        }

        public async Task<(List<TarjetasResponse>, int)> GetTarjetasAsync(int pageNumber, int recordsTotal, int columna, bool ascendente)
        {

            List<TarjetasResponse>? listaClientes = new List<TarjetasResponse>();
            int paginacion = 0;

            try
            {
                var uri = Urls.Transaccion + UrlsConfig.TarjetasOperations.GetTarjetas(pageNumber, recordsTotal);

                if (columna != 0) {
                    uri += string.Format("&columna={0}&ascendente={1}", Convert.ToString(columna), ascendente);
                }

                var response = await ApiClient.GetAsync(uri);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        MissingMemberHandling = MissingMemberHandling.Ignore
                    };
                    listaClientes = JsonConvert.DeserializeObject<List<TarjetasResponse>>(jsonResponse,settings);
                    paginacion = int.Parse(response.Headers.GetValues("x-totalRecord").FirstOrDefault()!.ToString());
                    return (listaClientes, paginacion)!;
                }

            }
            catch (Exception ex)
            {
                return (listaClientes, paginacion)!; ;
            }

            return (listaClientes, paginacion);
        }

        public async Task<(List<TarjetasResponse>, int)> GetTarjetasFilterAsync(int pageNumber, int recordsTotal, int columna, bool ascendente, List<RequestListFilters> filters)
        {

            FiltroDatosResponseTarjetas? responseDatos = new FiltroDatosResponseTarjetas();
            List<TarjetasResponse>? listaClientes = new List<TarjetasResponse>();
            int paginacion = 0;

            try
            {
                var uri = Urls.Transaccion + UrlsConfig.TarjetasOperations.GetTarjetasFilter(pageNumber, recordsTotal);

                var filtronombreTitular = filters.FirstOrDefault(x => x.Key == "nombreTitular");
                var filtrotarjeta = filters.FirstOrDefault(x => x.Key == "tarjeta");
                var filtrocuentaClabe = filters.FirstOrDefault(x => x.Key == "cuentaClabe");
                var filtronumeroProxy = filters.FirstOrDefault(x => x.Key == "numeroProxy");
                var filtroestatus = filters.FirstOrDefault(x => x.Key == "estatusCuenta");

                if (filtronombreTitular!.Value != null)
                {
                    uri += string.Format("&nombreTitular={0}", Convert.ToString(filtronombreTitular.Value));
                }

                if (filtrotarjeta!.Value != null)
                {
                    uri += string.Format("&tarjeta={0}", Convert.ToString(filtrotarjeta.Value));
                }

                if (filtrocuentaClabe!.Value != null)
                {
                    uri += string.Format("&cuentaClabe={0}", Convert.ToString(filtrocuentaClabe.Value));
                }

                if (filtronumeroProxy!.Value != null)
                {
                    uri += string.Format("&numeroProxy={0}", Convert.ToString(filtronumeroProxy.Value));
                }

                if (filtroestatus!.Value != null)
                {
                    uri += string.Format("&estatus={0}", Convert.ToString(filtroestatus.Value));
                }

                var response = await ApiClient.GetAsync(uri);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var settings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        MissingMemberHandling = MissingMemberHandling.Ignore
                    };

                    responseDatos = JsonConvert.DeserializeObject<FiltroDatosResponseTarjetas>(jsonResponse, settings);

                    listaClientes = responseDatos.listaResultado;
                    paginacion = Convert.ToInt32(responseDatos.cantidadEncontrada);

                    return (listaClientes, paginacion)!;
                }

            }
            catch (Exception ex)
            {
                return (listaClientes, paginacion)!; ;
            }

            return (listaClientes, paginacion);
        }

        public async Task<MensajeResponse> GetRegistroTarjetaAsync(RegistrarTarjetaRequest request)
        {
            MensajeResponse respuesta = new MensajeResponse();

            try
            {
                var uri = Urls.Transaccion + UrlsConfig.TarjetasOperations.GetTarjetasRegistra();
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await ApiClient.PostAsync(uri, content);
                if (response.IsSuccessStatusCode)
                {
                    response.EnsureSuccessStatusCode();
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    respuesta = JsonConvert.DeserializeObject<MensajeResponse>(jsonResponse)!;
                }

            }
            catch (Exception ex)
            {
                return respuesta;
            }

            return respuesta;
        }

        public async Task<MensajeResponse> GetBloqueoTarjeta(string numeroTarjeta, int status, string token)
        {
            MensajeResponse respuesta = new MensajeResponse();

            try
            {
                var uri = Urls.Transaccion + UrlsConfig.TarjetasOperations.GetBloquearDesbloquearTarjeta(numeroTarjeta, status, token);
                var json = JsonConvert.SerializeObject("");
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await ApiClient.PostAsync(uri, content);
                if (response.IsSuccessStatusCode)
                {
                    response.EnsureSuccessStatusCode();
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    respuesta = JsonConvert.DeserializeObject<MensajeResponse>(jsonResponse)!;
                }

            }
            catch (Exception ex)
            {
                respuesta.Error = true;
                return respuesta;
            }

            return respuesta;
        }
    }
}
