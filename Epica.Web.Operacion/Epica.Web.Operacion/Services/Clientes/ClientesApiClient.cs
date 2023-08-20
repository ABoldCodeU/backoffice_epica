﻿using Epica.Web.Operacion.Config;
using Epica.Web.Operacion.Models.Entities;
using Epica.Web.Operacion.Models.Request;
using Epica.Web.Operacion.Services.UserResolver;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using static Epica.Web.Operacion.Controllers.CuentaController;

namespace Epica.Web.Operacion.Services.Transaccion
{
    public class ClientesApiClient : ApiClientBase, IClientesApiClient
    {
        public ClientesApiClient(
            HttpClient httpClient, 
            ILogger<CuentaApiClient> logger, 
            IOptions<UrlsConfig> config, 
            IHttpContextAccessor httpContext, 
            IConfiguration configuration, 
            IUserResolver userResolver) : base(httpClient, logger, config, httpContext, configuration, userResolver)
        {
        }

        public async Task<List<ClienteResponse>> GetClientesAsync(int pageNumber, int recordsTotal)
        {

            List<ClienteResponse>? listaClientes = new List<ClienteResponse>();

            try
            {
                var uri = Urls.Transaccion + UrlsConfig.ClientesOperations.GetClienteInfo(pageNumber, recordsTotal);
                var response = await ApiClient.GetAsync(uri);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    listaClientes = JsonConvert.DeserializeObject<List<ClienteResponse>>(jsonResponse);
                }

            } catch (Exception ex) {
                return listaClientes;
            }

            return listaClientes;
        }

        public async Task<int> GetTotalClientesAsync()
        {
            int result = 0;
            try
            {
                var uri = Urls.Transaccion + UrlsConfig.ClientesOperations.GetClientesTotal();
                var response = await ApiClient.GetAsync(uri);

                if (response.IsSuccessStatusCode)
                {
                    response.EnsureSuccessStatusCode();
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    result = int.Parse(jsonResponse);
                }

            }
            catch (Exception)
            {
                return result;
            }

            return result;
        }

        public async Task<DatosClienteResponse> GetClienteAsync(int id)
        {
            DatosClienteResponse? cliente = new DatosClienteResponse();
            try
            {
                var uri = Urls.Transaccion + UrlsConfig.ClientesOperations.GetCliente(id);
                var response = await ApiClient.GetAsync(uri);

                if (response.IsSuccessStatusCode)
                {
                    response.EnsureSuccessStatusCode();
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    cliente = JsonConvert.DeserializeObject<DatosClienteResponse>(jsonResponse);
                }

            }
            catch (Exception)
            {
                return cliente;
            }

            return cliente;
        }

        public async Task<List<DocumentosClienteResponse>> GetDocumentosClienteAsync(int id)
        {
            List<DocumentosClienteResponse>? listaDocumentosCliente = new List<DocumentosClienteResponse>();

            try
            {
                var uri = Urls.Transaccion + UrlsConfig.ClientesOperations.GetClienteDocumentos(id);
                var response = await ApiClient.GetAsync(uri);

                if (response.IsSuccessStatusCode)
                {
                    response.EnsureSuccessStatusCode();
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    listaDocumentosCliente = JsonConvert.DeserializeObject<List<DocumentosClienteResponse>>(jsonResponse);
                }

            }
            catch (Exception)
            {
                return listaDocumentosCliente;
            }

            return listaDocumentosCliente;
        }

        public async Task<BloqueoWebResponse> GetBloqueoWeb(BloqueoWebClienteRequest request)
        {
            BloqueoWebResponse respuesta = new BloqueoWebResponse();

            try
            {
                var uri = Urls.Transaccion + UrlsConfig.ClientesOperations.GetBloqueaWebCliente();
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await ApiClient.PostAsync(uri, content);
                if (response.IsSuccessStatusCode)
                {
                    response.EnsureSuccessStatusCode();
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    respuesta = JsonConvert.DeserializeObject<BloqueoWebResponse>(jsonResponse);
                }

            }
            catch (Exception ex)
            {
                respuesta.error = true;
                respuesta.detalle = ex.Message;
                return respuesta;
            }

            return respuesta;
        }

        public async Task<BloqueoTotalResponse> GetBloqueoTotal(BloqueoTotalClienteRequest request)
        {
            BloqueoTotalResponse respuesta = new BloqueoTotalResponse();

            try
            {
                var uri = Urls.Transaccion + UrlsConfig.ClientesOperations.GetBloqueaTotalCliente();
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await ApiClient.PostAsync(uri, content);
                if (response.IsSuccessStatusCode)
                {
                    response.EnsureSuccessStatusCode();
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    respuesta = JsonConvert.DeserializeObject<BloqueoTotalResponse>(jsonResponse);
                }

            }
            catch (Exception ex)
            {
                respuesta.error = true;
                respuesta.detalle = ex.Message;
                return respuesta;
            }

            return respuesta;
        }

        public async Task<RegistrarModificarClienteResponse> GetRegistroCliente(RegistroModificacionClienteRequest request)
        {
            RegistrarModificarClienteResponse respuesta = new RegistrarModificarClienteResponse();

            try
            {
                var uri = Urls.Transaccion + UrlsConfig.ClientesOperations.InsertarClienteNuevo();
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await ApiClient.PostAsync(uri, content);
                if (response.IsSuccessStatusCode)
                {
                    response.EnsureSuccessStatusCode();
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    respuesta = JsonConvert.DeserializeObject<RegistrarModificarClienteResponse>(jsonResponse);
                }

            }
            catch (Exception ex)
            {
                respuesta.error = true;
                respuesta.detalle = ex.Message;
                return respuesta;
            }

            return respuesta;
        }

        public async Task<ClienteDetailsResponse> GetDetallesCliente(int id)
        {
            ClienteDetailsResponse? cliente = new ClienteDetailsResponse();
            try
            {
                var uri = Urls.Transaccion + UrlsConfig.ClientesOperations.GetCliente(id);
                var response = await ApiClient.GetAsync(uri);

                if (response.IsSuccessStatusCode)
                {
                    response.EnsureSuccessStatusCode();
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    cliente = JsonConvert.DeserializeObject<ClienteDetailsResponse>(jsonResponse);
                }

            }
            catch (Exception)
            {
                return cliente;
            }

            return cliente;
        }
    }
}