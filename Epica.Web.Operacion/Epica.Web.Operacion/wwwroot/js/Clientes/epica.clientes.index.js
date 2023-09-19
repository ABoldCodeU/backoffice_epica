var table;
var datatable;
var filterAccount;

var KTDatatableRemoteAjax = function () {
    var table;
    var initDatatable = function () {
        datatable = $('#kt_datatable_user').DataTable({
            "order": [],
            pageLength: 15,
            language: {
                "decimal": "",
                "emptyTable": "No hay información disponible",
                "info": "Mostrando _END_ de _TOTAL_ entradas",
                "infoEmpty": "Mostrando 0 de 0 entradas",
                "infoFiltered": "(Filtrado de _MAX_ total entradas)",
                "infoPostFix": "",
                "thousands": ",",
                "lengthMenu": "Mostrar _MENU_ Entradas",
                "loadingRecords": "Cargando...",
                "processing": "Procesando...",
                "search": "Buscar:",
                "zeroRecords": "No se encontraron resultados para mostrar",
                "paginate": {
                    "first": "Primero",
                    "last": "Último",
                    "next": "&raquo;",
                    "previous": "&laquo;"
                }
            },
            responsive: true,
            pagingType: 'simple_numbers',
            searching: true,
            lengthMenu: [5, 10, 15, 25, 50, 100],
            processing: true,
            serverSide: true,
            filter: true,
            ordering: true,
            ajax: {
                url: siteLocation + 'Clientes/Consulta',
                type: 'POST',
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
            map: function (raw) {
                // sample data mapping
                var dataSet = raw;
                if (typeof raw.data !== 'undefined') {
                    dataSet = raw.data;
                }
                return dataSet;
            },
            columnDefs: [{
                "defaultContent": "-",
                "targets": "_all"
            }],
            columns: [
                //{ data: 'id', name: 'Id', title: 'Id Usuario' },
                {
                    data: 'nombreCompleto', name: 'NombreCompleto', title: 'Nombre',
                    render: function (data, type, row) {
                        var partes = data.split("|"); // Separar la parte entera y decimal
                        var Nombre = partes[0];
                        var ID = partes[1]
                        return "<a href='/Clientes/Detalle/DatosGenerales?id="+ID+"'>" + Nombre +"</a>";
                    }
                },
                { data: 'telefono', name: 'Telefono', title: 'Teléfono' },
                { data: 'email', name: 'Email', title: 'Correo Electrónico' },
                { data: 'curp', name: 'Curp', title: 'CURP' },
                { data: 'organizacion', name: 'Organizacion', title: 'Organización' },
                {
                    data: 'estatusweb', name: 'estatusweb', title: 'Estatus Web',
                    render: function (data, type, row) {
                        if (data == "1") {
                            return "<span class='badge badge-light-success' >Activo</span>";
                        } else if (data == "2") {
                            return "<span class='badge badge-light-danger' >Bloqueo Web</span>";
                        }
                    }
                },
                {
                    data: 'estatus', name: 'Estatus', title: 'Estatus Total',
                    render: function (data, type, row) {
                        if ((data == 5) || (data == 10))  {
                            return "<span class='badge badge-light-success' >Activo</span>";
                        } else if (data == -10) {
                            return "<span class='badge badge-light-danger' >Bloqueo Total</span>";
                        }
                    }
                },
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
                }
            ],
        });
        $('thead tr').addClass('text-start text-gray-400 fw-bold fs-7 text-uppercase gs-0');

    };

    var exportButtons = () => {
        const documentTitle = 'Clientes';
        var buttons = new $.fn.dataTable.Buttons(table, {
            buttons: [
                {
                    extend: 'excelHtml5',
                    title: documentTitle,
                    exportOptions: {
                        columns: [0, 1, 2, 3, 4, 5]
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

    var handleSearchDatatable = function () {
        var filterSearch = document.getElementById('search_input');
        filterSearch.addEventListener('keyup', function (e) {
            datatable.search(e.target.value).draw();
        });
    }

    // Reset Filter
    var handleResetForm = () => {
        // Select reset button
        const resetButton = document.querySelector('[data-kt-customer-table-filter="reset"]');

        // Reset datatable
        resetButton.addEventListener('click', function () {
            datatable.search('').draw();
        });
    }

    var recargar = function () {
        datatable.ajax.reload();
    }

    $(".btn-filtrar").click(function () {
        recargar();
    })

    $(".btn_limpiar_filtros").click(function () {
        $(".filtro-control").val('');
        $(".filtro-control-select").val(null).trigger('change');;
        recargar();
    });

    return {
        init: function () {
            //init();
            table = document.querySelector('#kt_datatable_user');

            if (!table) {
                return;
            }

            initDatatable();
            exportButtons();
            handleSearchDatatable();
            //handleFilterDatatable();
        },
        recargar: function () {
            recargar();
        }
    };
}();

jQuery(document).ready(function () {
    KTDatatableRemoteAjax.init();
});

function GestionarClienteWeb(AccountID, estatus) {
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

            // Realiza la validación del token y código de seguridad
            $.ajax({
                url: '/Autenticacion/ValidarTokenYCodigo',
                async: true,
                cache: false,
                type: 'POST',
                data: { token: tokenInput, codigo: codigoInput },
                success: function (validationResult) {
                    if (validationResult.mensaje === true) {
                        Swal.fire({
                            title: 'Bloqueo Web',
                            text: '¿Está seguro que desea bloquear o desbloquear la opción web para este cliente?',
                            icon: 'warning',
                            showCancelButton: true,
                            confirmButtonColor: '#3085d6',
                            cancelButtonColor: '#d33',
                            confirmButtonText: 'Aceptar'
                        }).then((result) => {
                            if (result.isConfirmed) {
                                $.ajax({
                                    url: siteLocation + 'Clientes/GestionarEstatusClienteWeb',
                                    async: true,
                                    cache: false,
                                    type: 'POST',
                                    data: { id: AccountID, Estatus: estatus },
                                    success: function (data) {
                                        datatable.ajax.reload();
                                        Swal.fire(
                                            'Cliente Actualizado',
                                            'Se ha actualizado el estatus con exito.',
                                            'success'
                                        )
                                    },
                                    error: function (xhr, status, error) {
                                        console.log(error);
                                    }
                                });
                            }
                        });
                    } else {
                        // Token o código de seguridad incorrectos, muestra un mensaje de error
                        Swal.fire(
                            'Error',
                            'Token o código de seguridad incorrectos. Inténtalo de nuevo.',
                            'error'
                        );
                    }
                },
                error: function () {
                }
            });
        }
    });
}

function GestionarClienteTotal(AccountID, estatus) {
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

            // Realiza la validación del token y código de seguridad
            $.ajax({
                url: '/Autenticacion/ValidarTokenYCodigo',
                async: true,
                cache: false,
                type: 'POST',
                data: { token: tokenInput, codigo: codigoInput },
                success: function (validationResult) {
                    if (validationResult.mensaje === true) {
    Swal.fire({
        title: 'Bloqueo Total',
        text: "¿Esta seguro que desea bloquear o desbloquear la opción total para este cliente?",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Aceptar'
    }).then((result) => {
        if (result.isConfirmed) {

            $.ajax({
                url: siteLocation + 'Clientes/GestionarEstatusClienteTotal',
                async: true,
                cache: false,
                type: 'POST',
                data: { id: AccountID, Estatus: estatus },
                success: function (data) {

                    datatable.ajax.reload();
                    Swal.fire(
                        'Cliente Actualizado',
                        'Se ha actualizado el estatus con exito.',
                        'success'
                    )
                },
                error: function (xhr, status, error) {
                    console.log(error);
                }
            });
        }
    });
                    } else {
                        // Token o código de seguridad incorrectos, muestra un mensaje de error
                        Swal.fire(
                            'Error',
                            'Token o código de seguridad incorrectos. Inténtalo de nuevo.',
                            'error'
                        );
                    }
                },
                error: function () {
                }
            });
        }
    });
}

//function Registro() {
//    Swal.fire({
//        title: '<span class="swal-title-custom"><b>Valida tu identidad</b></span>',
//        text: 'Por favor, ingrese su token y código de seguridad:',
//        html:
//            '<label for="swal-input1" class="swal-label"><b>Token:</b></label>' +
//            '<input id="swal-input1" class="swal2-input" style="font-size:14px;width:250px"  placeholder="Token">' +
//            '<div style="margin-top: 20px;"></div>' +
//            '<label for="swal-input2" class="swal-label"><b>Código de Seguridad:</b></label>' +
//            '<input id="swal-input2" class="swal2-input" style="font-size:14px;width:250px" type="password" placeholder="Código de Seguridad">',
//        showCancelButton: true,
//        confirmButtonColor: '#0493a8',
//        confirmButtonText: 'Aceptar',
//        cancelButtonText: 'Cancelar',
//        preConfirm: () => {
//            const swalInput1 = Swal.getPopup().querySelector('#swal-input1').value;
//            const swalInput2 = Swal.getPopup().querySelector('#swal-input2').value;
//            return [swalInput1, swalInput2];
//        }
//    }).then((result) => {
//        if (result.isConfirmed) {
//            const [tokenInput, codigoInput] = result.value;

//            // Realiza la validación del token y código de seguridad
//            $.ajax({
//                url: '/Autenticacion/ValidarTokenYCodigo',
//                async: true,
//                cache: false,
//                type: 'POST',
//                data: { token: tokenInput, codigo: codigoInput },
//                success: function (validationResult) {
//                    if (validationResult.mensaje === true) {
//                        // Redirige a la vista "clientes/registro"
//                        window.location.href = '/clientes/registro';
//                    } else {
//                        // Token o código de seguridad incorrectos, muestra un mensaje de error
//                        Swal.fire(
//                            'Error',
//                            'Token o código de seguridad incorrectos. Inténtalo de nuevo.',
//                            'error'
//                        );
//                    }
//                },
//                error: function () {
//                }
//            });
//        }
//    });
//}

function ResetContrasenaEmail(Email, ID) {
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

            // Realiza la validación del token y código de seguridad
            $.ajax({
                url: '/Autenticacion/ValidarTokenYCodigo',
                async: true,
                cache: false,
                type: 'POST',
                data: { token: tokenInput, codigo: codigoInput },
                success: function (validationResult) {
                    if (validationResult.mensaje === true) {
    Swal.fire({
        title: 'Enviar Reseteo de Contraseña por Correo Electrónico',
        text: "¿Esta seguro que desea enviar el reseteo de contraseña para este cliente?",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Aceptar'
    }).then((result) => {
        if (result.isConfirmed) {

            $.ajax({
                url: siteLocation + 'Clientes/EnvioContrasenaCorreo',
                async: true,
                cache: false,
                type: 'POST',
                data: { correo: Email, id : ID },
                success: function (data) {

                    datatable.ajax.reload();
                    Swal.fire(
                        'Envio Reseteo Contraseña',
                        'Se ha enviado la petición con exito.',
                        'success'
                    )
                },
                error: function (xhr, status, error) {
                    Swal.fire(
                        'Envio Reseteo Contraseña',
                        'Hubo un problema al realizar esta solicitud. Intentelo más tarde.',
                        'danger'
                    )
                }
            });
        }
    });
                    } else {
                        // Token o código de seguridad incorrectos, muestra un mensaje de error
                        Swal.fire(
                            'Error',
                            'Token o código de seguridad incorrectos. Inténtalo de nuevo.',
                            'error'
                        );
                    }
                },
                error: function () {
                }
            });
        }
    });
}

function ResetContrasenaTelefono(Telefono, ID) {
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

            // Realiza la validación del token y código de seguridad
            $.ajax({
                url: '/Autenticacion/ValidarTokenYCodigo',
                async: true,
                cache: false,
                type: 'POST',
                data: { token: tokenInput, codigo: codigoInput },
                success: function (validationResult) {
                    if (validationResult.mensaje === true) {
    Swal.fire({
        title: 'Enviar Reseteo de Contraseña por SMS',
        text: "¿Esta seguro que desea enviar el reseteo de contraseña para este cliente?",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Aceptar'
    }).then((result) => {
        if (result.isConfirmed) {

            $.ajax({
                url: siteLocation + 'Clientes/EnvioContrasenaTelefono',
                async: true,
                cache: false,
                type: 'POST',
                data: { telefono: Telefono, id: ID },
                success: function (data) {

                    datatable.ajax.reload();
                    Swal.fire(
                        'Envio Reseteo Contraseña',
                        'Se ha enviado la petición con exito.',
                        'success'
                    )
                },
                error: function (xhr, status, error) {
                    Swal.fire(
                        'Envio Reseteo Contraseña',
                        'Hubo un problema al realizar esta solicitud. Intentelo más tarde.',
                        'danger'
                    )
                }
            });
        }
    });
                    } else {
                        // Token o código de seguridad incorrectos, muestra un mensaje de error
                        Swal.fire(
                            'Error',
                            'Token o código de seguridad incorrectos. Inténtalo de nuevo.',
                            'error'
                        );
                    }
                },
                error: function () {
                }
            });
        }
    });
}