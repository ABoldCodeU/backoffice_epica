﻿using System.Text.Json.Serialization;
using static Epica.Web.Operacion.Controllers.CuentaController;

namespace Epica.Web.Operacion.Models.Response;

public class CuentasResponse
{
    public long idCuenta { get; set; }
    public int idCliente { get; set; }
    public string noCuenta { get; set; }
    public string nombrePersona { get; set; }
    public int estatus { get; set; }
    public decimal saldo { get; set; }
    public string tipoPersona { get; set; }
    public string alias { get; set; }
    public string fechaAlta { get; set; }
    public string fechaActualizacion { get; set; }
    public string fechaAltaFormat { get; set; }
    public string fechaActualizacionformat { get; set; }
}

public class CuentasResponseGrid : CuentasResponse
{
    public string Acciones { get; set; }
}