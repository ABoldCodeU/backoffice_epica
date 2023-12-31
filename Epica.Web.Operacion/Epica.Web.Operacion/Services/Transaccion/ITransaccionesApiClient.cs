﻿using Epica.Web.Operacion.Models.Common;
using Epica.Web.Operacion.Models.Request;

namespace Epica.Web.Operacion.Services.Transaccion
{
    public interface ITransaccionesApiClient
    {
        Task<TransaccionDetailsResponse> GetTransaccionRastreoCobranzaAsync(string rastreoCobranza);
        Task<(List<CargaBachRequest>, int)> GetTransaccionesMasivaAsync(int pageNumber, int recordsTotal, int idUsuario, int columna, bool ascendente);
        Task<(List<CargaBachRequest>, int)> GetTransaccionesMasivaFilterAsync(int pageNumber, int recordsTotal, int idUsuario, int columna, bool ascendente, List<RequestListFilters> filters);
        Task<(List<TransaccionesResponse>, int)> GetTransaccionesAsync(int pageNumber, int recordsTotal, int columna, bool ascendente);
        Task<(List<TransaccionesResponse>, int)> GetTransaccionesFilterAsync(int pageNumber, int recordsTotal, int columna, bool ascendente, List<RequestListFilters> filters);
        Task<(List<TransaccionesResponse>, int)> GetTransaccionesFilterEspAsync(int pageNumber, int recordsTotal, int columna, bool ascendente, string valor);
        Task<TransaccionesResponse> GetTransaccionAsync(int idInterno);
        Task<int> GetTotalTransaccionesAsync();
        Task<TransaccionesDetailsgeneralResponse> GetTransaccionesCuentaAsync(int idCuenta);
        Task<MensajeResponse> GetRegistroTransaccion(RegistrarTransaccionRequest request);
        Task<MensajeResponse> GetModificarTransaccion(ModificarTransaccionRequest request);
        Task<TransaccionDetailsResponse> GetTransaccionDetalleAsync(int idCuenta);
        Task<TransaccionDetailsResponse> GetTransaccionDetalleByCobranzaAsync(string cobranzaRef);
        Task<TransaccionesDetailsgeneralResponse> GetTransaccionesClienteAsync(int idCliente);
        Task<MensajeResponse> GetInsertaTransaccionesBatchAsync(List<CargaBachRequest> request);
        Task<MensajeResponse> GetEliminarTransaccionBatchAsync(int idRegistro);
        Task<CargaBachRequest> GetTransaccionBatchAsync(int idRegistro);
        Task<string> GetModificaTransaccionBatchAsync(CargaBachRequest request);
        Task<MensajeResponse> GetProcesaTransaccion(int idUsuario);
        Task<VoucherResponse> GetVoucherTransaccionAsync(int cuentaAhorro, int transaccion, string token);
        Task<int> GetTotalTransaccionesBatchAsync(int idUsuario);
        Task<MensajeResponse> GetEliminarTransaccionesBatchAsync(int idUsuario);
        Task<int> GetValidaExistenciaTranBatch(int idUsuario, string ClaveRastreo);
    }
}
