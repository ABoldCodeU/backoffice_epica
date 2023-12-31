﻿"use strict";

//const KTBlockUI = require("../components/blockui");

var datatable_myAccounts;

//Agregar la tabla de transacciones
var KTDatatableTransacciones = (function () {
    var table;

    var init = function () {
        datatable_myAccounts = $("#kt_datatable_movements").DataTable({
            // Configuraciones de la tabla...
            "order": [],
            language: {
                "emptyTable": "No hay información",
                "info": "Mostrando _END_ de _TOTAL_ entradas",
                "infoEmpty": "Mostrando 0 de 0 entradas",
                "infoFiltered": "(Filtrado de _MAX_ total entradas)",
                "infoPostFix": "",
                "thousands": ",",
                "lengthMenu": "Mostrar _MENU_ Entradas",
                "loadingRecords": "Cargando...",
                "processing": "Procesando...",
                "search": "Buscar:",
                "zeroRecords": "No se encontraron resultados para mostrar"
            },
            responsive: true,
            pagingType: 'simple_numbers',
            searching: true,
            lengthMenu: [10, 15, 25, 50, 100],
            processing: false,
            serverSide: true,
            filter: true,
            ordering: false,
            // Configuración de la fuente de datos AJAX
            ajax: {
                url: "ConsultaDevoluciones",
                type: "POST",
                error: function (jqXHR, textStatus, errorThrown) {
                    $(".dataTables_processing").hide();
                    toastr.error("Se agoto el tiempo de espera para consultar estos datos. Inténtelo más tarde.");
                },
                data: function (d) {
                    var filtros = [];
                    $(".filtro-control").each(function () {
                        if ($(this).data('filtrar') == undefined) return;
                        filtros.push({
                            key: $(this).data('filtrar'),
                            value: $(this).val()
                        });
                    });
                    d.filters = filtros;
                },
            },
            // Configuración de las columnas
            columnDefs: [{
                "defaultContent": "-",
                "targets": "_all"
            }],
            columns: [
                //{
                //    data: "claveRastreo", name: "ClaveRastreo", title: "",
                //    render: function (data, type, row) {
                //        return "<input type='checkbox' class='reenviarCb' data-claveRastreo='" + data +"'>"
                //    }
                //},
                { data: "cuentaOrigen", name: "CuentaOrigen", title: "Cuenta Origen" },
                {
                    data: "monto", name: "Monto", title: "Monto",
                    render: function (data, type, row) {
                        return accounting.formatMoney(data);
                    }
                },
                { data: "concepto", name: "Concepto", title: "Concepto" },
                { data: "fechaAlta", name: "FechaAlta", title: "Fecha Operación" },
                { data: "claveRastreo", name: "claveRastreo", title: "Clave de Rastreo" },
                { data: "cuentaDestino", name: "CuentaDestino", title: "Cuenta Destino" },
                { data: "bancoTxt", name: "BancoTxt", title: "Banco Destino" },
                {
                    title: '',
                    orderable: false,
                    data: null,
                    defaultContent: '',
                    render: function (data, type, row) {
                        if (type === 'display') {
                            var htmlString = row.acciones;
                            return htmlString
                        }
                    }
                },
            ],
        });
        $('thead tr').addClass('text-start text-gray-400 fw-bold fs-7 text-uppercase gs-0');
    };

    var handleSearchDatatable = function () {
        const filterSearch = document.getElementById('search_input');
        filterSearch.addEventListener('keyup', function (e) {
            if (e.key === 'Enter') {
                if (filterSearch.value.length >= 6 && filterSearch.value.length <= 40) {
                    datatable_myAccounts.search(filterSearch.value).draw();
                }
            } else if (filterSearch.value === '') {
                datatable_myAccounts.search(filterSearch.value).draw();
            }
        });
    }

    var exportButtons = () => {
        const documentTitle = 'Transacciones - Reintentador';
        var buttons = new $.fn.dataTable.Buttons(table, {
            buttons: [
                {
                    extend: 'excelHtml5',
                    title: documentTitle,
                    exportOptions: {
                        columns: [0, 1, 2, 3, 4, 5, 6, 7]
                    }
                }
            ]
        }).container().appendTo($('#kt_datatable_buttons'));

        const exportButtons = document.querySelectorAll('#kt_datatable_export_menu [data-kt-export]');
        exportButtons.forEach(exportButton => {
            exportButton.addEventListener('click', e => {
                e.preventDefault();

                const exportValue = e.target.getAttribute('data-kt-export');
                const target = document.querySelector('.dt-buttons .buttons-' + exportValue);
                target.click();
            });
        });
    }

    // Función para descarga del archivo
    //var Download = function (idTransaccion) {
    //    var url = "Home/Archivo?idTransaccion=" + idTransaccion;
    //    window.location.href = url;
    //};

    $(".btn-filtrar").click(function () {
        reload();
    })

    $(".btn_limpiar_filtros").click(function () {
        $(".filtro-control").val('');
        $(".filtro-control-select").val(null).trigger('change');;
        reload();
    });

    // Función de recarga
    var reload = function () {
        datatable_myAccounts.ajax.reload();
    };

    var ErrorControl = function () {
        dataTable.ext.errMode = 'throw';
    };

    return {
        init: function () {
            table = document.querySelector("#kt_datatable_movements");

            //$(document.body).on('click', '#download-btn', function () {
            //    var idTransaccion = $(this).data('idtransaccion');
            //    Download(idTransaccion);
            //});

            if (!table) {
                return;
            }
            init();
            handleSearchDatatable();
            exportButtons();

        },
        reload: function () {
            reload();
        },
        ErrorControl: function () {
            ErrorControl();
        }
    };
})();
$(document).ready(function () {
    KTDatatableTransacciones.init();
});

$(document).on('click', '.btn_reenviar_transacciones', function () {
    var selected = [];
    $('.reenviarCb:checkbox:checked').each(function () {
        var claveRastreo = $(this).attr('data-claveRastreo');
        selected.push(claveRastreo);
    });
    if (selected.length <= 0) {
        Swal.fire({
            icon: 'warning',
            title: 'Selecciona al menos una transferencia para reenviar',
            showConfirmButton: true,
        });
        return false;
    }

    Swal.fire({
        title: 'Reenviar Transacciones',
        text: "¿Estás seguro que deseas reenviar las " + selected.length + " transferencias seleccionadas?",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Aceptar'
    }).then((result) => {
        if (result.isConfirmed) {

            //toastr.info('Reenviando Transacciones...', "");
            $.ajax({
                url: 'ReenviarTransacciones',
                async: true,
                cache: false,
                type: 'POST',
                data: { clavesRastreo: selected },
                success: function (data) {

                    if (data.error == true) {
                        Swal.fire(
                            'Reenviar Transacción',
                            'Hubo un problema al reenviar esta transacción, inténtelo más tarde o verifique su existencia.',
                            'error'
                        )
                    } else {
                        datatable_transaccion.ajax.reload();
                        Swal.fire(
                            'Reenviar Transacción',
                            'Se ha reenviado la transacción con éxito.',
                            'success'
                        )
                    }
                },
                error: function (xhr, status, error) {
                }
            });
        }
    })

});

$(document).on('click', '.btn_devolver_transacciones', function () {
    var selected = [];
    $('.reenviarCb:checkbox:checked').each(function () {
        var claveRastreo = $(this).attr('data-claveRastreo');
        selected.push(claveRastreo);
    });
    if (selected.length <= 0) {
        Swal.fire({
            icon: 'warning',
            title: 'Selecciona al menos una transferencia para devolver',
            showConfirmButton: true,
        });
        return false;
    }

    Swal.fire({
        title: 'Devolver Transacciones',
        text: "¿Estás seguro que deseas aplicar la devolución para las " + selected.length + " transferencias seleccionadas?",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Aceptar'
    }).then((result) => {
        if (result.isConfirmed) {

            toastr.info('Aplicando Devolución a transacciones...', "");

            $.ajax({
                url: 'DevolverTransacciones',
                async: true,
                cache: false,
                type: 'POST',
                data: { clavesRastreo: selected },
                success: function (data) {

                    if (data.error == true) {
                        Swal.fire(
                            'Devolver Transacción',
                            'Hubo un problema al aplicar la devolución de esta transacción, inténtelo más tarde o verifique su existencia.',
                            'error'
                        )
                    } else {
                        datatable_transaccion.ajax.reload();
                        Swal.fire(
                            'Devolver Transacción',
                            'Se ha realizado la devolución de la transacción con éxito.',
                            'success'
                        )
                    }
                },
                error: function (xhr, status, error) {
                }
            });
        }
    })

});

function DevolverTransaccion(claveRastreo) {
    Swal.fire({
        title: '<span class="swal-title-custom"><b>Valida tu identidad</b></span>',
        text: 'Por favor, ingrese su token y código de seguridad:',
        html:
            '<label for="swal-input1" class="swal-label"><b>Token:</b></label>' +
            '<input id="swal-input1" class="swal2-input" style="font-size:14px;width:250px"  placeholder="Token">' +
            '<div style="margin-top: 20px;"></div>' +
            '<label for="swal-input2" class="swal-label"><b>Código de Seguridad:</b></label>' +
            '<input id="swal-input2" class="swal2-input" style="font-size:14px;width:250px" type="password" placeholder="Código de Seguridad">',
        showCancelButton: true,
        confirmButtonColor: '#0493a8',
        confirmButtonText: 'Aceptar',
        cancelButtonText: 'Cancelar',
        preConfirm: () => {
            const swalInput1 = Swal.getPopup().querySelector('#swal-input1').value;
            const swalInput2 = Swal.getPopup().querySelector('#swal-input2').value;
            return [swalInput1, swalInput2];
        }
    }).then((result) => {
        if (result.isConfirmed) {
            KTApp.showPageLoading();
            const [tokenInput, codigoInput] = result.value;

            // Realiza la validación del token y código de seguridad
            $.ajax({
                url: '/Autenticacion/ValidarTokenYCodigo',
                async: true,
                cache: false,
                type: 'POST',
                data: { token: tokenInput, codigo: codigoInput },
                success: function (validationResult) {
                    KTApp.hidePageLoading();
                    if (validationResult.mensaje === true) {
    Swal.fire({
        title: 'Devolver Transacciones',
        text: "¿Estás seguro que deseas aplicar la devolución para esta transacción?",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Aceptar'
    }).then((result) => {
        if (result.isConfirmed) {
            KTApp.showPageLoading();
            //toastr.info('Aplicando Devolución a transacción...', "");

            $.ajax({
                url: 'DevolverTransaccion',
                async: true,
                cache: false,
                type: 'POST',
                data: { clavesRastreo: claveRastreo },
                success: function (data) {
                    KTApp.hidePageLoading();
                    if (data.error == true) {
                        Swal.fire(
                            'Devolver Transacción',
                            'Hubo un problema al aplicar la devolución de esta transacción, inténtelo más tarde o verifique su existencia.',
                            'error'
                        )
                    } else {
                        KTDatatableTransacciones.reload();
                        Swal.fire(
                            'Devolver Transacción',
                            'Se ha realizado la devolución de la transacción con éxito.',
                            'success'
                        )
                    }
                },
                error: function (xhr, status, error) {
                    KTApp.hidePageLoading();
                }
            });
        }
    });
                    } else {
                        Swal.fire(
                            'Error',
                            'Token o código de seguridad incorrectos. Inténtalo de nuevo.',
                            'error'
                        );
                    }
                },
                error: function () {
                    KTApp.hidePageLoading();
                }
            });
        }
    });
}

function ReenviarTransaccion(claveRastreo) {
    Swal.fire({
        title: '<span class="swal-title-custom"><b>Valida tu identidad</b></span>',
        text: 'Por favor, ingrese su token y código de seguridad:',
        html:
            '<label for="swal-input1" class="swal-label"><b>Token:</b></label>' +
            '<input id="swal-input1" class="swal2-input" style="font-size:14px;width:250px"  placeholder="Token">' +
            '<div style="margin-top: 20px;"></div>' +
            '<label for="swal-input2" class="swal-label"><b>Código de Seguridad:</b></label>' +
            '<input id="swal-input2" class="swal2-input" style="font-size:14px;width:250px" type="password" placeholder="Código de Seguridad">',
        showCancelButton: true,
        confirmButtonColor: '#0493a8',
        confirmButtonText: 'Aceptar',
        cancelButtonText: 'Cancelar',
        preConfirm: () => {
            const swalInput1 = Swal.getPopup().querySelector('#swal-input1').value;
            const swalInput2 = Swal.getPopup().querySelector('#swal-input2').value;
            return [swalInput1, swalInput2];
        }
    }).then((result) => {
        if (result.isConfirmed) {
            const [tokenInput, codigoInput] = result.value;
            KTApp.showPageLoading();
            // Realiza la validación del token y código de seguridad
            $.ajax({
                url: '/Autenticacion/ValidarTokenYCodigo',
                async: true,
                cache: false,
                type: 'POST',
                data: { token: tokenInput, codigo: codigoInput },
                success: function (validationResult) {
                    KTApp.hidePageLoading();
                    if (validationResult.mensaje === true) {
                        Swal.fire({
                            title: 'Reenviar Transacción',
                            text: "¿Estás seguro que deseas reenviar esta transacción?",
                            icon: 'warning',
                            showCancelButton: true,
                            confirmButtonColor: '#3085d6',
                            cancelButtonColor: '#d33',
                            confirmButtonText: 'Aceptar'
                        }).then((result) => {
                            if (result.isConfirmed) {
                                //toastr.info('Aplicando Devolución a transacción...', "");
                                KTApp.showPageLoading();
                                $.ajax({
                                    url: 'ReenviarTransaccion',
                                    async: true,
                                    cache: false,
                                    type: 'POST',
                                    data: { clavesRastreo: claveRastreo },
                                    success: function (data) {
                                        KTApp.hidePageLoading();
                                        if (data.error == true) {
                                            
                                            Swal.fire(
                                                'Reenviar Transacción',
                                                'Hubo un problema para reenviar esta transacción, inténtelo más tarde o verifique su existencia.',
                                                'error'
                                            )
                                        } else {
                                            KTDatatableTransacciones.reload();
                                            Swal.fire(
                                                'Reenviar Transacción',
                                                'Se ha realizado el reenvío de la transacción con éxito.',
                                                'success'
                                            )
                                        }
                                    },
                                    error: function (xhr, status, error) {
                                        KTApp.hidePageLoading();
                                    }
                                });
                            }
                        });
                    } else {
                        Swal.fire(
                            'Error',
                            'Token o código de seguridad incorrectos. Inténtalo de nuevo.',
                            'error'
                        );
                    }
                },
                error: function () {
                    KTApp.hidePageLoading();
                }
            });
        }
    });
}

$(document).on('click', '#ValidarTransaccionAdd', function () {
    const inputClaveRastreo = document.getElementById('add_clave_rastreo');
    const clave = inputClaveRastreo.value;

    if ((clave == null) || (clave == "")) {
        toastr.warning("El valor ingresado, no es válido");
        return false;
    }

    toastr.info("Localizando transacción.").preventDuplicates;
    toastr.clear();

    $.ajax({
        url: "AgregarTransaccionReintentador",
        type: "POST",
        data: { ClaveRastreo: clave },
        success: function (result) {

            $("#add_clave_rastreo").empty();
            toastr.info(result.message).preventDuplicates;

        },
        error: function (error) {
            toastr.warning("No se pudo localizar al usuario.").preventDuplicates;
        }
    });
});

$('#kt_datatable_movements').on('processing.dt', function (e, settings, processing) {
    if (processing) {
        KTApp.showPageLoading();
    } else {
        KTApp.hidePageLoading();
    }
})
