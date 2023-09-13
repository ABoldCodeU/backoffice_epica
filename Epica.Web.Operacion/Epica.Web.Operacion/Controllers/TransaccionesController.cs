﻿using Epica.Web.Operacion.Helpers;
using Epica.Web.Operacion.Models;
using Epica.Web.Operacion.Models.Common;
using Epica.Web.Operacion.Models.Entities;
using Epica.Web.Operacion.Models.Request;
using Epica.Web.Operacion.Models.ViewModels;
using Epica.Web.Operacion.Services.Catalogos;
using Epica.Web.Operacion.Services.Log;
using Epica.Web.Operacion.Services.Transaccion;
using Epica.Web.Operacion.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using static Epica.Web.Operacion.Controllers.CuentaController;
using ExcelDataReader;
using System.IO;
using System.Data;
using System.Collections.Generic;

namespace Epica.Web.Operacion.Controllers
{
    [Authorize]
    public class TransaccionesController :  Controller
    {
        #region "Locales"
        private readonly UserContextService _userContextService;
        private readonly ITransaccionesApiClient _transaccionesApiClient;//Transacciones
        private readonly ICatalogosApiClient _catalogosApiClient;
        private readonly ILogsApiClient _logsApiClient;
        private readonly IWebHostEnvironment _webHostEnvironment;
        #endregion

        #region "Constructores"
        public TransaccionesController(ITransaccionesApiClient transaccionesApiClient,
            UserContextService userContextService,
            ICatalogosApiClient catalogosApiClient,
            ILogsApiClient logsApiClient,
            IWebHostEnvironment webHostEnvironment
            )
        {
            _userContextService = userContextService;
            _transaccionesApiClient = transaccionesApiClient;
            _catalogosApiClient = catalogosApiClient;
            _logsApiClient = logsApiClient;
            _webHostEnvironment = webHostEnvironment;
        }
        #endregion

        #region "Funciones"
        [Authorize]
        public IActionResult Index(string AccountID = "")
        {
            var loginResponse = _userContextService.GetLoginResponse();
            var validacion = loginResponse?.AccionesPorModulo.Any(modulo => modulo.ModuloAcceso == "Transacciones" && modulo.Ver == 0);
            if (validacion == true)
            {
                CargarDocumentoTransaccionesViewModel CargarInfo = new CargarDocumentoTransaccionesViewModel();
                ViewBag.Carga = CargarInfo;
                ViewBag.AccountID = AccountID;
                return View(loginResponse);
            }


            return NotFound();
        }

        [Authorize]
        public async Task<ActionResult> Registro()
        {
            var loginResponse = _userContextService.GetLoginResponse();
            var validacion = loginResponse?.AccionesPorModulo.Any(modulo => modulo.ModuloAcceso == "Transacciones" && modulo.Insertar == 0);
            if (validacion == true)
            {
                var listaMediosPago = await _catalogosApiClient.GetMediosPagoAsync();

                TransaccionesRegistroViewModel transaccionesRegistroViewModel = new TransaccionesRegistroViewModel
                {
                    TransacccionDetalles = new RegistrarTransaccionRequest(),
                    ListaMediosPago = listaMediosPago
                };

                ViewBag.Accion = "RegistrarTransaccion";
                ViewBag.TituloForm = "Registrar Transacción";

                return View(transaccionesRegistroViewModel);
            }


            return NotFound();
        }

        [Authorize]
        public async Task<IActionResult> Transacciones()
        {
            var loginResponse = _userContextService.GetLoginResponse();
            var validacion = loginResponse?.AccionesPorModulo.Any(modulo => modulo.ModuloAcceso == "Transacciones" && modulo.Ver == 0);
            if (validacion == true)
            {
                var recibir = await _transaccionesApiClient.GetTransaccionesAsync(1, 100);
                return Json(recibir);
            }
            return NotFound();

        }

        [Authorize]
        [HttpPost]
        public async Task<JsonResult> Consulta(List<RequestListFilters> filters, string idAccount = "")
        {
            var request = new RequestList();

            int totalRecord = 0;
            int filterRecord = 0;

            var draw = Request.Form["draw"].FirstOrDefault();
            int pageSize = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "0");
            int skip = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");

            request.Pagina = skip / pageSize + 1;
            request.Registros = pageSize;
            request.Busqueda = Request.Form["search[value]"].FirstOrDefault();
            request.ColumnaOrdenamiento = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            request.Ordenamiento = Request.Form["order[0][dir]"].FirstOrDefault();

            var gridData = new ResponseGrid<ResumenTransaccionResponseGrid>();
            List<TransaccionesResponse> ListPF = new List<TransaccionesResponse>();

            var filtronombreClaveRastreo = filters.FirstOrDefault(x => x.Key == "claveRastreo");

            if (filtronombreClaveRastreo.Value != null)
            {
                string transaccion = filtronombreClaveRastreo.Value;
                var listPF2 = await _transaccionesApiClient.GetTransaccionRastreoCobranzaAsync(transaccion);

                TransaccionesResponse transaccionResponse = new TransaccionesResponse()
                {
                    clabeCobranza = listPF2.value.ClabeCobranza,
                    claveRastreo = listPF2.value.ClaveRastreo,
                    concepto = listPF2.value.Concepto,
                    cuetaOrigenOrdenante = listPF2.value.cuetaOrigenOrdenante,
                    estatus = listPF2.value.Estatus ?? 0,
                    fechaActualizacion = listPF2.value.FechaActualizacion,
                    fechaAlta = listPF2.value.FechaAlta,
                    idCliente = listPF2.value.IdCliente,
                    idCuentaAhorro = listPF2.value.IdCuentaAhorro,
                    idMedioPago = listPF2.value.IdMedioPago ?? 0,
                    idTransaccion = listPF2.value.IdTrasaccion ?? 0,
                    monto = decimal.Parse(listPF2.value.Monto.ToString()),
                    nombreBeneficiario = listPF2.value.NombreBeneficiario,
                    nombreOrdenante = listPF2.value.nombreOrdenante,
                    fechaAutorizacion = listPF2.value.FechaAutorizacion,
                    fechaInstruccion = listPF2.value.FechaInstruccion
                };

                ListPF.Add(transaccionResponse);
            }
            else
            {
                ListPF = await _transaccionesApiClient.GetTransaccionesAsync(1, 100);
            }


            //Entorno local de pruebas
            //ListPF = GetList();

            var List = new List<ResumenTransaccionResponseGrid>();
            foreach (var row in ListPF)
            {
                row.claveRastreo = row.claveRastreo == null ? "N/A" : row.claveRastreo;
                List.Add(new ResumenTransaccionResponseGrid
                {
                    id = row.idTransaccion,
                    idCliente = row.idCliente,
                    claveRastreo = row.claveRastreo == null ? "N/A" : row.claveRastreo,
                    vinculo = row.claveRastreo + "|" + row.idCliente.ToString(),
                    nombreOrdenante = row.nombreOrdenante,
                    nombreBeneficiario = row.nombreBeneficiario,
                    monto = row.monto,
                    estatus = row.estatus,
                    concepto = row.concepto,
                    idMedioPago = row.idMedioPago,
                    idCuentaAhorro = row.idCuentaAhorro,
                    //fechaAlta = row.fechaAlta,
                    fechaInstruccion = row.fechaInstruccion,
                    fechaAutorizacion = row.fechaAutorizacion,
                    clabeCobranza = row.clabeCobranza,
                    cuetaOrigenOrdenante = row.cuetaOrigenOrdenante,
                    Acciones = await this.RenderViewToStringAsync("~/Views/Transacciones/_Acciones.cshtml", row)
                });
            }
            if (!string.IsNullOrEmpty(request.Busqueda))
            {
                List = List.Where(x =>
                (x.id.ToString().ToLower() ?? "").Contains(request.Busqueda.ToLower()) ||
                (x.claveRastreo?.ToLower() ?? "").Contains(request.Busqueda.ToLower()) ||
                (x.nombreOrdenante?.ToLower() ?? "").Contains(request.Busqueda.ToLower()) ||
                (x.nombreBeneficiario?.ToLower() ?? "").Contains(request.Busqueda.ToLower()) ||
                (x.monto.ToString().ToLower() ?? "").Contains(request.Busqueda.ToLower()) ||
                (x.estatus.ToString().ToLower() ?? "").Contains(request.Busqueda.ToLower()) ||
                (x.concepto?.ToLower() ?? "").Contains(request.Busqueda.ToLower()) ||
                (x.idMedioPago.ToString().ToLower() ?? "").Contains(request.Busqueda.ToLower()) ||
                (x.idCuentaAhorro.ToString().ToLower() ?? "").Contains(request.Busqueda.ToLower()) ||
                (x.fechaAlta.ToString().ToLower() ?? "").Contains(request.Busqueda.ToLower()) 
                ).ToList();
            }
            //Aplicacion de Filtros temporal, 
            var filtroCuentaOrdenante = filters.FirstOrDefault(x => x.Key == "cuentaOrdenante");
            var filtroNombreOrdenante = filters.FirstOrDefault(x => x.Key == "nombreOrdenante");
            var filtroNombreBeneficiario = filters.FirstOrDefault(x => x.Key == "nombreBeneficiario");
            var filtroConcepto = filters.FirstOrDefault(x => x.Key == "concepto");
            var filtroMonto = filters.FirstOrDefault(x => x.Key == "monto");
            var filtroFecha = filters.FirstOrDefault(x => x.Key == "fecha");
            var filtroEstatus = filters.FirstOrDefault(x => x.Key == "estatus");

            if (filtroCuentaOrdenante.Value != null)
            {
                List = List.Where(x => x.cuetaOrigenOrdenante.Contains(Convert.ToString(filtroCuentaOrdenante.Value))).ToList();
            }

            if (filtronombreClaveRastreo.Value != null)
            {
                List = List.Where(x => x.claveRastreo.Contains(Convert.ToString(filtronombreClaveRastreo.Value))).ToList();
            }

            if (filtroNombreOrdenante.Value != null)
            {
                List = List.Where(x => x.nombreOrdenante == Convert.ToString(filtroNombreOrdenante.Value)).ToList();
            }

            if (filtroNombreBeneficiario.Value != null)
            {
                List = List.Where(x => x.nombreBeneficiario == Convert.ToString(filtroNombreBeneficiario.Value)).ToList();
            }

            if (filtroConcepto.Value != null)
            {
                List = List.Where(x => x.concepto == Convert.ToString(filtroConcepto.Value)).ToList();
            }

            if (filtroMonto.Value != null)
            {
                List = List.Where(x => x.monto.ToString() == Convert.ToString(filtroMonto.Value)).ToList();
            }

            if (filtroFecha.Value != null)
            {
                List = List.Where(x => x.fechaAlta.ToString() == Convert.ToString(filtroFecha.Value)).ToList();
            }

            if (filtroEstatus.Value != null)
            {
                if (filtroEstatus.Value == "1") {

                    var estatusList = new[] { "1", "2" };
                    List = List.Where(x => estatusList.Contains(Convert.ToString(x.estatus))).ToList();

                } else {
                    List = List.Where(x => x.estatus == Convert.ToInt32(filtroEstatus.Value)).ToList();
                }
            }

            gridData.Data = List;
            gridData.RecordsTotal = List.Count;
            gridData.Data = gridData.Data.Skip(skip).Take(pageSize).ToList();
            filterRecord = string.IsNullOrEmpty(request.Busqueda) ? gridData.RecordsTotal ?? 0 : gridData.Data.Count;
            gridData.RecordsFiltered = filterRecord;
            gridData.Draw = draw;

            return Json(gridData);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> RegistrarTransaccion(TransaccionesRegistroViewModel model)
        {
            var loginResponse = _userContextService.GetLoginResponse();
            var validacion = loginResponse?.AccionesPorModulo.Any(modulo => modulo.ModuloAcceso == "Transacciones" && modulo.Insertar == 0);
            if (validacion == true)
            {
                try
                {
                    var response = await _transaccionesApiClient.GetRegistroTransaccion(model.TransacccionDetalles);

                    string detalle = response.Detalle;
                    int idRegistro = 0;
                    try
                    {
                        string pattern = @"\d+";
                        Match match = Regex.Match(detalle, pattern);
                        idRegistro = int.Parse(match.Value);
                    }
                    catch (FormatException)
                    {
                        idRegistro = 0;
                    }

                    LogRequest logRequest = new LogRequest
                    {
                        IdUser = loginResponse.IdUsuario.ToString(),
                        Modulo = "Transacciones",
                        Fecha = HoraHelper.GetHoraCiudadMexico(),
                        NombreEquipo = loginResponse.NombreDispositivo,
                        Accion = "Insertar",
                        Ip = loginResponse.DireccionIp,
                        Envio = JsonConvert.SerializeObject(model.TransacccionDetalles),
                        Respuesta = response.Error.ToString(),
                        Error = response.Error ? JsonConvert.SerializeObject(response.Detalle) : string.Empty,
                        IdRegistro = idRegistro
                    };
                    await _logsApiClient.InsertarLogAsync(logRequest);

                    if (response.Codigo == "200")
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        return RedirectToAction("Registro");
                    }
                }
                catch (Exception ex)
                {
                    return View();
                }
            }

            return NotFound();
        }

        [Authorize]
        public async Task<ActionResult> Modificar(int id)
        {
            var loginResponse = _userContextService.GetLoginResponse();
            var validacion = loginResponse?.AccionesPorModulo.Any(modulo => modulo.ModuloAcceso == "Transacciones" && modulo.Editar == 0);
            if (validacion == true)
            {
                try
                {
                    var transaccionResponse = await _transaccionesApiClient.GetTransaccionDetalleAsync(id);
                    ModificarTransaccionRequest transaccionDetalles = new ModificarTransaccionRequest()
                    {
                        ClaveRastreo = transaccionResponse.value.ClaveRastreo ?? "",
                        Concepto = transaccionResponse.value.Concepto ?? "",
                        NombreOrdenante = transaccionResponse.value.nombreOrdenante ?? "",
                        FechaOperacion = transaccionResponse.value.FechaActualizacion,
                        CuentaOrigenOrdenante = transaccionResponse.value.cuetaOrigenOrdenante ?? "",
                        IdTrasaccion = id,
                        NombreBeneficiario = transaccionResponse.value.NombreBeneficiario ?? "",
                        CuentaDestinoBeneficiario = transaccionResponse.value.CuentaDestinoBeneficiario ?? "",
                    };

                    var listaMediosPago = await _catalogosApiClient.GetMediosPagoAsync();

                    if (listaMediosPago == null)
                    {
                        return RedirectToAction("Error404", "Error");
                    }

                    TransaccionEditarViewModel transaccionesEditarViewModel = new TransaccionEditarViewModel
                    {
                        TransacccionDetalles = transaccionDetalles,
                        ListaMediosPago = listaMediosPago
                    };

                    ViewBag.Accion = "ModificarTransaccion";
                    ViewBag.TituloForm = "Modificar Transacción";
                    return View("~/Views/Transacciones/Editar.cshtml", transaccionesEditarViewModel);
                }
                catch (Exception)
                {
                    return RedirectToAction("Error", "Error");
                }
            }

            return NotFound();

        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ModificarTransaccion(TransaccionEditarViewModel model)
        {
            var loginResponse = _userContextService.GetLoginResponse();
            var validacion = loginResponse?.AccionesPorModulo.Any(modulo => modulo.ModuloAcceso == "Transacciones" && modulo.Editar == 0);
            if (validacion == true)
            {
                try
                {
                    var response = await _transaccionesApiClient.GetModificarTransaccion(model.TransacccionDetalles);

                    LogRequest logRequest = new LogRequest
                    {
                        IdUser = loginResponse.IdUsuario.ToString(),
                        Modulo = "Transacciones",
                        Fecha = HoraHelper.GetHoraCiudadMexico(),
                        NombreEquipo = loginResponse.NombreDispositivo,
                        Accion = "Editar",
                        Ip = loginResponse.DireccionIp,
                        Envio = JsonConvert.SerializeObject(model.TransacccionDetalles),
                        Respuesta = response.Error.ToString(),
                        Error = response.Error ? JsonConvert.SerializeObject(response.Detalle) : string.Empty,
                        IdRegistro = model.TransacccionDetalles.IdTrasaccion
                    };
                    await _logsApiClient.InsertarLogAsync(logRequest);

                    if (response.Codigo == "200")
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        return RedirectToAction("Registro");
                    }
                }
                catch (Exception ex)
                {
                    return View();
                }
            }

            return NotFound();
        }

        [Authorize]
        public async Task<IActionResult> DetalleTransaccion(string Id, string Estatus, string ClabeCobranza)
        {
            var result = new JsonResultDto();

            TransaccionDetailsResponse detalleTransaccion = new TransaccionDetailsResponse();
            try
            {
                if (Estatus == "0") {
                    detalleTransaccion = await _transaccionesApiClient.GetTransaccionDetalleByCobranzaAsync(ClabeCobranza);
                } else {
                    detalleTransaccion = await _transaccionesApiClient.GetTransaccionDetalleAsync(Convert.ToInt32(Id));
                }
                

                if (detalleTransaccion != null)
                {
                    result.Error = false;
                    result.Result = await this.RenderViewToStringAsync("~/Views/Transacciones/_Detalle.cshtml", detalleTransaccion.value);

                    var loginResponse = _userContextService.GetLoginResponse();
                    var validacion = loginResponse?.AccionesPorModulo.Any(modulo => modulo.ModuloAcceso == "Transacciones" && modulo.Editar == 0);
                    if (validacion == true) { 
                        result.permiso = true;
                    }
                }
                else
                {
                    result.Error = true;
                    result.ErrorDescription = "ERROR";
                    return Json(result);
                }
            }
            catch (Exception)
            {
                result.Error = true;
                result.ErrorDescription = "Error1";
            }
            return Json(result);
        }

        [HttpPost]
        public async Task<JsonResult> CargarDocumentoMasivoTransacciones(CargarDocumentoTransaccionesViewModel model)
        {

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            MensajeResponse response = new MensajeResponse();
            List<CargaBachRequest> ListaBach = new List<CargaBachRequest>();
            var loginResponse = _userContextService.GetLoginResponse();

            try
            {
                var filenameTemp = Path.GetFileNameWithoutExtension(Path.GetTempFileName());
                var extension = Path.GetExtension(model.documento.FileName);
                var pathTemp = Path.GetTempPath();
                var FilenameSave = filenameTemp + extension;
                DataSet resultExcel = new DataSet();
                DataTable tablaDatos = new DataTable();
                var filePath = pathTemp + "" + FilenameSave;

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.documento.CopyToAsync(stream);
                    stream.Close();
                }
         
                using (var streamExcel = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    IExcelDataReader reader = ExcelDataReader.ExcelReaderFactory.CreateOpenXmlReader(streamExcel);
                    resultExcel = reader.AsDataSet(new ExcelDataSetConfiguration());
                    reader.Close();
                    tablaDatos = resultExcel.Tables[0];
                }

                for (var rowCell = 0; rowCell <= tablaDatos.Rows.Count - 1; rowCell++)
                {
                    var rowData = tablaDatos.Rows[rowCell];

                    //Remover Encabezado
                    if (rowCell < 1) {
                        continue;
                    }

                    try
                    {
                        CargaBachRequest RegistrarTransaccion = new CargaBachRequest();

                        if (rowData.ItemArray[0].ToString().ToLower() == "in") {
                            RegistrarTransaccion.TipoOperacion = 1;
                            RegistrarTransaccion.Comision = 0;
                        } else if (rowData.ItemArray[0].ToString().ToLower() == "out") {
                            RegistrarTransaccion.TipoOperacion = 2;
                            RegistrarTransaccion.Comision = 0;
                        } else {
                            RegistrarTransaccion.TipoOperacion = 3;
                            RegistrarTransaccion.Comision = 1;
                        }

                        RegistrarTransaccion.FechaOperacion = Convert.ToDateTime(rowData.ItemArray[4]);
                        RegistrarTransaccion.CuentaBeneficiario = rowData.ItemArray[1].ToString();
                        RegistrarTransaccion.Monto = Convert.ToDouble(rowData.ItemArray[2]);
                        RegistrarTransaccion.ConceptoPago = rowData.ItemArray[3].ToString();
                        RegistrarTransaccion.MedioPago = Convert.ToInt32(rowData.ItemArray[6]);
                        RegistrarTransaccion.Ordenante = rowData.ItemArray[7].ToString();
                        RegistrarTransaccion.ClaveRastreo = rowData.ItemArray[5].ToString();
                        RegistrarTransaccion.IdUsuario = Convert.ToInt32(loginResponse.IdUsuario.ToString());

                        ListaBach.Add(RegistrarTransaccion);

                    } catch (Exception ex) {
                        continue;
                    }
                }

                response = await _transaccionesApiClient.GetInsertaTransaccionesBatchAsync(ListaBach);

            } catch (Exception ex) {
                response.Error = true;
            }

            return Json(response);
        }

        public IActionResult GetBlobDownloadFormat()
        {
            var net = new System.Net.WebClient();

            string webRootPath = _webHostEnvironment.WebRootPath;
            string path = Path.Combine(webRootPath, "Formatos");

            string FolderPath = Path.Combine(path, "Formato_Carga_Masiva_Transacciones.xlsx"); // <-- Error here
            var data = net.DownloadData(FolderPath);
            var content = new System.IO.MemoryStream(data);
            var contentType = "APPLICATION/octet-stream";
            var fileName = "Formato_Carga_Masiva_Transacciones.xlsx";
            return File(content, contentType, fileName);
        }


        #region Devoluciones

        [Authorize]
        public IActionResult Devoluciones()
        {
            var loginResponse = _userContextService.GetLoginResponse();
            var validacion = loginResponse?.AccionesPorModulo.Any(modulo => modulo.ModuloAcceso == "Transacciones" && modulo.Ver == 0);
            if (validacion == true)
            {
                return View(loginResponse);
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<JsonResult> ConsultaDevoluciones(List<RequestListFilters> filters)
        {
            var request = new RequestList();

            int totalRecord = 0;
            int filterRecord = 0;

            var draw = Request.Form["draw"].FirstOrDefault();
            int pageSize = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "0");
            int skip = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");

            request.Pagina = skip / pageSize + 1;
            request.Registros = pageSize;
            request.Busqueda = Request.Form["search[value]"].FirstOrDefault();
            request.ColumnaOrdenamiento = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            request.Ordenamiento = Request.Form["order[0][dir]"].FirstOrDefault();

            var gridData = new ResponseGrid<DevolucionesResponseGrid>();
            List<DevolucionesResponse> ListPF = new List<DevolucionesResponse>();

            ListPF = QAGetListDevoluciones();

            var List = new List<DevolucionesResponseGrid>();
            foreach (var row in ListPF)
            {
                List.Add(new DevolucionesResponseGrid
                {
                    idTransaccion = row.idTransaccion,
                    ClaveRastreo = row.ClaveRastreo,
                    CuentaOrigen = row.CuentaOrigen,
                    Monto = row.Monto,
                    Concepto = row.Concepto,
                    FechaAlta = row.FechaAlta,
                    CuentaDestino = row.CuentaDestino,
                    BancoTxt = row.BancoTxt,
                    Acciones = await this.RenderViewToStringAsync("~/Views/Transacciones/_AccionesDevoluciones.cshtml", row)
                });
            }
            if (!string.IsNullOrEmpty(request.Busqueda))
            {
                List = List.Where(x =>
                (x.idTransaccion.ToString().ToLower() ?? "").Contains(request.Busqueda.ToLower()) ||
                (x.ClaveRastreo?.ToLower() ?? "").Contains(request.Busqueda.ToLower()) ||
                (x.CuentaOrigen?.ToLower() ?? "").Contains(request.Busqueda.ToLower()) ||
                (x.Monto.ToString().ToLower() ?? "").Contains(request.Busqueda.ToLower()) ||
                (x.Concepto?.ToLower() ?? "").Contains(request.Busqueda.ToLower()) ||
                (x.FechaAlta?.ToString().ToLower() ?? "").Contains(request.Busqueda.ToLower()) ||
                (x.CuentaDestino?.ToString().ToLower() ?? "").Contains(request.Busqueda.ToLower()) ||
                (x.BancoTxt?.ToString().ToLower() ?? "").Contains(request.Busqueda.ToLower())
                ).ToList();
            }
            //Aplicacion de Filtros temporal, 
            //var filtroCuentaOrdenante = filters.FirstOrDefault(x => x.Key == "cuentaOrdenante");
            //var filtroNombreOrdenante = filters.FirstOrDefault(x => x.Key == "nombreOrdenante");
            //var filtroNombreBeneficiario = filters.FirstOrDefault(x => x.Key == "nombreBeneficiario");
            //var filtroConcepto = filters.FirstOrDefault(x => x.Key == "concepto");
            //var filtroMonto = filters.FirstOrDefault(x => x.Key == "monto");
            //var filtroFecha = filters.FirstOrDefault(x => x.Key == "fecha");
            //var filtroEstatus = filters.FirstOrDefault(x => x.Key == "estatus");

            //if (filtroCuentaOrdenante.Value != null)
            //{
            //    List = List.Where(x => x.cuetaOrigenOrdenante.Contains(Convert.ToString(filtroCuentaOrdenante.Value))).ToList();
            //}

            //if (filtronombreClaveRastreo.Value != null)
            //{
            //    List = List.Where(x => x.claveRastreo.Contains(Convert.ToString(filtronombreClaveRastreo.Value))).ToList();
            //}

            //if (filtroNombreOrdenante.Value != null)
            //{
            //    List = List.Where(x => x.nombreOrdenante == Convert.ToString(filtroNombreOrdenante.Value)).ToList();
            //}

            //if (filtroNombreBeneficiario.Value != null)
            //{
            //    List = List.Where(x => x.nombreBeneficiario == Convert.ToString(filtroNombreBeneficiario.Value)).ToList();
            //}

            //if (filtroConcepto.Value != null)
            //{
            //    List = List.Where(x => x.concepto == Convert.ToString(filtroConcepto.Value)).ToList();
            //}

            //if (filtroMonto.Value != null)
            //{
            //    List = List.Where(x => x.monto.ToString() == Convert.ToString(filtroMonto.Value)).ToList();
            //}

            //if (filtroFecha.Value != null)
            //{
            //    List = List.Where(x => x.fechaAlta.ToString() == Convert.ToString(filtroFecha.Value)).ToList();
            //}

            //if (filtroEstatus.Value != null)
            //{
            //    if (filtroEstatus.Value == "1")
            //    {

            //        var estatusList = new[] { "1", "2" };
            //        List = List.Where(x => estatusList.Contains(Convert.ToString(x.estatus))).ToList();

            //    }
            //    else
            //    {
            //        List = List.Where(x => x.estatus == Convert.ToInt32(filtroEstatus.Value)).ToList();
            //    }
            //}

            gridData.Data = List;
            gridData.RecordsTotal = List.Count;
            gridData.Data = gridData.Data.Skip(skip).Take(pageSize).ToList();
            filterRecord = string.IsNullOrEmpty(request.Busqueda) ? gridData.RecordsTotal ?? 0 : gridData.Data.Count;
            gridData.RecordsFiltered = filterRecord;
            gridData.Draw = draw;

            return Json(gridData);
        }

        #endregion


        #endregion

        #region "Modelos"
        public class RequestListFilters
        {
            [JsonProperty("key")]
            public string Key { get; set; }
            [JsonProperty("value")]
            public string Value { get; set; }
        }
        public class ResumenTransaccionResponse
        {
            public int id { get; set; }
            public int idCliente { get; set; }
            public string claveRastreo { get; set; }
            public string vinculo { get; set; }
            public string nombreOrdenante { get; set; }
            public string cuetaOrigenOrdenante { get; set; }
            public string clabeCobranza { get; set; }
            public string nombreBeneficiario { get; set; }
            public decimal monto { get; set; }
            public int estatus { get; set; }
            public string concepto { get; set; }
            public int idMedioPago { get; set; }
            public int idCuentaAhorro { get; set; }
            public string fechaAlta { get; set; }
            public string fechaActualizacion { get; set; }
            public string fechaInstruccion { get; set; }
            public string fechaAutorizacion { get; set; }

        }
        public class ResumenTransaccionResponseGrid : ResumenTransaccionResponse
        {
            public string Acciones { get; set; }
        }
        #endregion

        #region "Funciones Auxiliares"

        private string generarnombreBanco(int i)
        {
            Random rnd = new Random((int)DateTime.Now.Ticks);

            i = i - 1;

            String[] nombres = { "BBVA", "BANAMEX", "BANCOPPEL", "SANTANDER", "NU", "FARABELA", "PALACIO DE HIERRO", "JP MORGAN", "HSBC", "BANREGIO" };

            var genNombre = nombres[i];

            return genNombre;
        }

        public List<DevolucionesResponse> QAGetListDevoluciones()
        {


            var List = new List<DevolucionesResponse>();
            Random rnd = new Random();
            const string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            String[] characters2 = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J" };

            try
            {
                for (int i = 1; i <= 10; i++)
                {
                    var gen = generarnombreBanco(i);

                    var pf = new DevolucionesResponse();
                    pf.idTransaccion = i;
                    pf.ClaveRastreo = string.Format("AQPAY0000{0}{1}", DateTime.Now.ToString("yyyyMMdd"), i);
                    pf.CuentaOrigen = string.Format("46000047896324{0}", i);
                    pf.Monto = Convert.ToDecimal(rnd.Next(0001, 99999));
                    pf.Concepto = string.Format("Test De Transacciones {0}", i);
                    pf.FechaAlta = DateTime.Now.ToString();
                    pf.CuentaDestino = string.Format("112311234324{0}", i);
                    pf.CanDevolver = true;
                    pf.BancoTxt = gen;

                    List.Add(pf);
                }
            }
            catch (Exception e)
            {
                List = new List<DevolucionesResponse>();
            }

            return List;
        }
        #endregion
    }
}