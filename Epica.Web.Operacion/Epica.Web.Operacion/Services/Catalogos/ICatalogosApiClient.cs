﻿namespace Epica.Web.Operacion.Services.Catalogos
{
    public interface ICatalogosApiClient
    {
        Task<List<DatosCatalogoResponse>> GetMediosPagoAsync();
        Task<List<DatosCatalogoResponse>> GetEmpresasAsync();
        Task<List<DatosCatalogoResponse>> GetPaisesAsync();
        Task<List<DatosCatalogoResponse>> GetOcupacionesAsync();
        Task<List<DatosCatalogoResponse>> GetNacionalidadesAsync();
        Task<List<DatosCatalogoResponse>> GetRolClienteAsync();
        Task<List<DatosCatalogoResponse>> GetTipoDocumentosAsync();
        Task<List<DatosCatalogoResponse>> GetRolesUsuarioAsync();
    }
}
