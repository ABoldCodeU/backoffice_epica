var table;
var datatable;
var filterAccount;

var KTDatatableRemoteAjax = function () {
    var table;
    var initDatatable = function () {
        datatable = $('#kt_datatable_card').DataTable({
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
            processing: false,
            serverSide: true,
            filter: true,
            ordering: false,
            ajax: {
                url: siteLocation + 'Tarjetas/Consulta',
                type: 'POST',
                error: function (jqXHR, textStatus, errorThrown) {
                    $(".dataTables_processing").hide();
                    toastr.error("No se pudo encontrar información disponible.");
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
                {
                    data: 'vinculo', name: 'Vinculo', title: 'Nombre Titular',
                    render: function (data, type, row) {
                        var partes = data.split("|"); // Separar la parte entera y decimal
                        var nombreCompleto = partes[0];
                        var idCliente = partes[1];

                        if (nombreCompleto == "-") {
                            return nombreCompleto;
                        } else {
                            return "<a href='/Clientes/Detalle/Tarjetas?id=" + idCliente + "'>" + nombreCompleto + "</a>";
                        }
                    }
                },
                {
                    data: 'tarjeta', name: 'tarjeta', title: 'Tarjeta',                  
                    render: function (data, type, row) {
                        return data.replace(/\d(?=\d{4})/g, "*");
                    }
                },
                { data: 'clabe', name: 'clabe', title: 'Cuenta Clabe' },
                { data: 'proxyNumber', name: 'proxyNumber', title: 'Número de Proxy' },
                {
                    data: 'estatus', name: 'Estatus', title: 'Estatus',
                    render: function (data, type, row) {
                        return data == 2 ?
                            "<span class='badge badge-light-success' >Activo</span>" : "<span class='badge badge-light-danger' >Desactivado</span>";
                    }
                },
                {
                    data: 'tipoProducto', name: 'TipoProducto', title: 'Tipo Producto'
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
        const documentTitle = 'Tarjetas';
        var buttons = new $.fn.dataTable.Buttons(table, {
            buttons: [
                {
                    extend: 'excelHtml5',
                    title: documentTitle,
                    exportOptions: {
                        columns: [0, 1, 2, 3]
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
        const filterSearch = document.querySelector('[data-kt-customer-table-filter="search"]');
        filterSearch.addEventListener('keyup', function (e) {
            if (e.key === 'Enter') {
                if (e.target.value.length >= 16 && e.target.value.length <= 16) {
                    datatable.search(e.target.value).draw();
                }
            } else if (filterSearch.value === '') {
                datatable.search(e.target.value).draw();
            }
        });
    }

    //var handleFilterDatatable = () => {
    //    // Select filter options
    //    filterAccount = document.querySelectorAll('[data-kt-customer-table-filter="payment_type"] [name="payment_type"]');
    //    const filterButton = document.querySelector('[data-kt-customer-table-filter="filter"]');

    //    // Filter datatable on submit
    //    filterButton.addEventListener('click', function () {
    //        // Get filter values
    //        //let paymentValue = '';

    //        //// Get payment value
    //        //filterAccount.forEach(r => {
    //        //    if (r.checked) {
    //        //        paymentValue = r.value;
    //        //    }

    //        //    // Reset payment value if "All" is selected
    //        //    if (paymentValue === 'all') {
    //        //        paymentValue = '';
    //        //    }
    //        //});

    //        // Filter datatable --- official docs reference: https://datatables.net/reference/api/search()
    //        datatable.search('4652364789632453').draw();
    //    });
    //}

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
    var ErrorControl = function () {
        dataTable.ext.errMode = 'throw';
    };

    $(".btn-filtrar").click(function () {
        $('#search_input').val('');
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
            table = document.querySelector('#kt_datatable_card');

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
        },
        ErrorControl: function () {
            ErrorControl();
        },
    };
}();

jQuery(document).ready(function () {
    KTDatatableRemoteAjax.init();
});

function validateNumbersInput(inputElement) {
    var inputValue = inputElement.value;
    var cleanValue = '';
    var prevChar = '';

    for (var i = 0; i < inputValue.length; i++) {
        var char = inputValue[i];

        if (!isNaN(char) && char !== ' ') {
            cleanValue += char;
        }
    }

    inputElement.value = cleanValue;
}

function validateLettersInput(inputElement) {
    var inputValue = inputElement.value;
    var cleanValue = '';
    var hasSpace = false;

    for (var i = 0; i < inputValue.length; i++) {
        var char = inputValue[i];

        if (/[a-zA-ZÀÁàáÒÓòóÈÉèéÌÍìíÙÚùúÑñ]/.test(char) && cleanValue.length < 40) {
            cleanValue += char;
            hasSpace = false;
        } else if (char === ' ' && !hasSpace) {
            cleanValue += char;
            hasSpace = true;
        }
    }

    if (cleanValue.length < 3) {
        cleanValue = cleanValue.slice(0, 3);
    } else if (cleanValue.length > 40) {
        cleanValue = cleanValue.slice(0, 40);
    }

    inputElement.value = cleanValue;
}


function GestionarTarjeta(numgen, estatus, id) {
    Swal.fire({
        title: '<span class="swal-title-custom"><b>Valida tu identidad</b></span>',
        text: 'Por favor, ingrese su token y código de seguridad:',
        html:
            '<label for="swal-input1" class="swal-label"><b>Token:</b></label>' +
            '<input id="swal-input1" class="swal2-input" style="font-size:14px;width:250px" type="password" placeholder="Token" oninput="validateInputToken(this)" maxlength="6">' +
            '<div style="margin-top: 20px;"></div>' +
            '<label for="swal-input2" class="swal-label"><b>Código de Seguridad:</b></label>' +
            '<input id="swal-input2" class="swal2-input" style="font-size:14px;width:250px" type="password" placeholder="Código de Seguridad" oninput="validateInputToken(this)" maxlength="6">',
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

            if (isValidInput(tokenInput) && isValidInput(codigoInput)) {
                $.ajax({
                    url: '/Autenticacion/ValidarTokenYCodigo',
                    async: true,
                    cache: false,
                    type: 'POST',
                    data: { token: tokenInput, codigo: codigoInput },
                    success: function (validationResult) {
                        if (validationResult.mensaje === true) {
                            Swal.fire({
                                title: (estatus.toLowerCase() === 'true' ? 'Bloqueo de Tarjetas' : 'Desbloqueo de Tarjetas'),
                                text: (estatus.toLowerCase() === 'true' ? "¿Estás seguro que deseas bloquear esta tarjeta?" : "¿Estás seguro que deseas desbloquear esta tarjeta?"),
                                icon: 'warning',
                                showCancelButton: true,
                                confirmButtonColor: '#3085d6',
                                cancelButtonColor: '#d33',
                                confirmButtonText: 'Aceptar'
                            }).then((result) => {
                                if (result.isConfirmed) {
                                    toastr.info('Procesando...', "");

                                    $.ajax({
                                        url: 'Tarjetas/GestionarEstatusTarjetas',
                                        async: true,
                                        cache: false,
                                        type: 'POST',
                                        data: { NumGen: numgen, estatus: estatus, id: id },
                                        success: function (data) {
                                            datatable.ajax.reload();
                                            Swal.fire(
                                                'Estatus Actualizado',
                                                'Se ha actualizado el estatus de la tarjeta con éxito.',
                                                'success'
                                            );
                                        },
                                        error: function (xhr, status, error) {
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
                    }
                });
            } else {
                Swal.fire(
                    'Error',
                    'Los campos Token y Código de Seguridad deben contener 6 números.',
                    'error'
                );
            }
        }
    });
}

function validateInputToken(inputElement) {
    inputElement.value = inputElement.value.replace(/[^0-9]/g, '');
}

function isValidInput(input) {
    return input.length === 6 && !isNaN(input);
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
//                        // Redirige a la vista "transacciones/registro"
//                        window.location.href = '/transacciones/registro'; 
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
