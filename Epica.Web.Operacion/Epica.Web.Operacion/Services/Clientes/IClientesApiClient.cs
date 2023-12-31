﻿using Epica.Web.Operacion.Models.Common;
using Epica.Web.Operacion.Models.Entities;
using Epica.Web.Operacion.Models.Request;
using static Epica.Web.Operacion.Controllers.CuentaController;

namespace Epica.Web.Operacion.Services.Transaccion
{
    public interface IClientesApiClient
    {
        Task<List<EmpresaResponse>> GetEmpresasClienteAsync(int id);
        Task<ClienteDetailsResponse> GetDetallesGeneralesClienteAsync(int id);
        Task<MensajeResponse> GetClienteExisteTelefonoAsync(string telefono);
        Task<MensajeResponse> GetClienteExisteCorreoAsync(string correo);
        Task<List<DatosClienteEntity>> GetDetallesClientesByNombresAsync(string nombres);
        Task<List<DatosCatalogoResponse>> GetClientesbyNombreAsync(string nombreCliente);
        Task<(List<ClienteResponse>, int)> GetClientesAsync(int pageNumber, int recordsTotal, int columna, bool ascendente);
        Task<(List<ClienteResponse>, int)> GetClientesFilterAsync(int pageNumber, int recordsTotal, int columna, bool ascendente, List<RequestListFilters> filters);
        Task<int> GetTotalClientesAsync();
        Task<ClienteDetailsResponse> GetClienteAsync(int id);
        Task<DocumentosClienteDetailsResponse> GetDocumentosClienteAsync(int id);
        Task<BloqueoWebResponse> GetBloqueoWeb(BloqueoWebClienteRequest request);
        Task<BloqueoTotalResponse> GetBloqueoTotal(BloqueoTotalClienteRequest request);
        Task<MensajeResponse> GetRegistroCliente(RegistroModificacionClienteRequest request);
        Task<ClienteDetailsResponse> GetDetallesCliente(int id);
        Task<MensajeResponse> GetModificarCliente(RegistroModificacionClienteRequest request);
        Task<MensajeResponse> GetRegistroAsignacionCuentaCliente(AsignarCuentaResponse request);
        Task<MensajeResponse> GetRegistroDesvincularCuentaCliente(DesvincularCuentaResponse request);
        //Task<RegistrarModificarClienteResponse> GetModificaCliente(RegistroModificacionClienteRequest request);
        Task<MensajeResponse> GetRestablecerContraseñaCorreo(string correo);
        Task<MensajeResponse> GetRestablecerContraseñaTelefono(string telefono);
        Task<DocumentoShowResponse> GetVisualizarDocumentosClienteAsync(string url);
        Task<MensajeResponse> GetInsertaDocumentoClienteAsync(DocumentoClienteRequest request);
    }
}
