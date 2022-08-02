
MostrarOk = function (id_alerta, mensaje) {
    $(id_alerta).removeClass("alert-danger");
    $(id_alerta).addClass("alert-success");
    $(id_alerta).html(mensaje);
    $(id_alerta).show("slow").delay(2000).hide("slow");
}

MostrarError = function (id_alerta, mensaje) {
    $(id_alerta).removeClass("alert-success");
    $(id_alerta).addClass("alert-danger");
    $(id_alerta).html(mensaje + '<button type="button" class="btn-close ml-2" onclick="$(\'' + id_alerta + '\').hide();"></button>');
    $(id_alerta).show("slow");//.delay(5000).hide("slow");
}

CerrarModal = function () {
    $('.modal').modal('hide');
}

CerrarVentana = function (ventana) {
    $(ventana).modal('hide');
}

AbrirModal = function (ventana) {
    $(ventana).modal('show');
}

Mostrar = function (elemento) {
    $(elemento).removeClass("d-none");
}

Ocultar = function (elemento) {
    $(elemento).addClass("d-none");
}

//Date picker
$('.fecha').datepicker({
    autoclose: true,
    format: 'dd-mm-yyyy',
    //language: 'es',
    orientation: 'bottom',
    //calendarWeeks: true,
    todayHighlight: true
});

$('.select2').select2({
    theme: "bootstrap-5",
    width: $(this).data('width') ? $(this).data('width') : $(this).hasClass('w-100') ? '100%' : 'style',
    //placeholder: $(this).data('placeholder'),
    closeOnSelect: true
});

// Hacemos que desde el header de todos los modales de bootstrap se pueda arrastrar
$(".modal").draggable({
    handle: ".modal-header"
});

$('.num2decimales').inputmask('numeric', {
    radixPoint: ',',
    groupSeparator: '.',
    autoGroup: true,
    autoUnmask: false,
    rightAlign: false,
    unmaskAsNumber: true,
    allowMinus: true,
    digits: 2,
    removeMaskOnSubmit: false,
    placeholder: ''
});

var scripts = {
    init: function () {

        // Activamos los tooltip de bootstrap
        $('[data-bs-toggle="tooltip"]').tooltip();


        if (!$.fn.DataTable.isDataTable('.datatable_usuarios')) {

            $('.datatable_usuarios').DataTable({
                //dom: 'Blfrtip',
                deferRender: false,
                dom: "<'row'<'col-md-3 col-sm-3'l><'col-md-9 col-sm-9 text-left'f>>" +
                    "<'row'<'col-md-12 col-sm-12'tr>>" +
                    "<'row'<'col-md-5 col-sm-5'i><'col-sm-7 col-sm-7'p>>",
                "paging": true,
                stateSave: false,
                "initComplete": function (settings, json) {
                    $('.datatable_usuarios').show();
                },
                // Lenguaje
                "language": {
                    //"url": "../lib/datatables/lang/Spanish.json", // Vamos a verlo en inglés
                    "decimal": ",",
                    "thousands": "."
                },
                "order": [[1, "asc"]],
                columnDefs: [
                    { "targets": 2, "orderable": false }
                ]
            });

        }

        if (!$.fn.DataTable.isDataTable('.datatable_cargas')) {

            $.fn.dataTable.moment('DD-MM-YYYY'); // Ordena columnas con formato fecha (utiliza moment.js)

            $('.datatable_cargas').DataTable({
                //dom: 'Blfrtip',
                deferRender: true,
                dom: "<'row'<'col-md-3 col-sm-3'l><'col-md-9 col-sm-9 text-left'f>>" +
                    "<'row'<'col-md-12 col-sm-12'tr>>" +
                    "<'row'<'col-md-5 col-sm-5'i><'col-sm-7 col-sm-7'p>>",
                "paging": true,
                stateSave: false,
                // Lenguaje
                "language": {
                    "decimal": ",",
                    "thousands": "."
                },
                "order": [[3, "desc"]],
                /*columnDefs: [
                    { "targets": 4, "orderable": false }
                ]*/
            });

        }
     
        if (!$.fn.DataTable.isDataTable('.datatable_clientes')) {

            $('.datatable_clientes').DataTable({
                //dom: 'Blfrtip',
                deferRender: true,
                dom: "<'row'<'col-md-3 col-sm-3'l><'col-md-9 col-sm-9 text-left'f>>" +
                    "<'row'<'col-md-12 col-sm-12'tr>>",
                "paging": false,
                stateSave: false,
                // Lenguaje
                "language": {
                    "decimal": ",",
                    "thousands": "."
                },
                "order": [[1, "asc"]]
            });

        }

        if (!$.fn.DataTable.isDataTable('.datatable_referencias')) {

            $('.datatable_referencias').DataTable({
                //dom: 'Blfrtip',
                deferRender: true,
                dom: "<'row'<'col-md-3 col-sm-3'l><'col-md-9 col-sm-9 text-left'f>>" +
                    "<'row'<'col-md-12 col-sm-12'tr>>",
                "paging": false,
                stateSave: false,
                // Lenguaje
                "language": {
                    "decimal": ",",
                    "thousands": "."
                },
                "order": [[1, "asc"]]
            });

        }

        if (!$.fn.DataTable.isDataTable('.datatable_lotes')) {

            $('.datatable_lotes').DataTable({
                //dom: 'Blfrtip',
                deferRender: false,
                dom: "<'row'<'col-md-3 col-sm-3'l><'col-md-9 col-sm-9 text-left'f>>" +
                    "<'row'<'col-md-12 col-sm-12'tr>>",
                "paging": false,
                stateSave: false,
                // Lenguaje
                "language": {
                    "decimal": ",",
                    "thousands": "."
                },
                order: [[$('th.ordenado_por_defecto').index(), 'desc']]
            });

        }

        if (!$.fn.DataTable.isDataTable('.datatable_claims')) {

            $('.datatable_claims').DataTable({
                //dom: 'Blfrtip',
                deferRender: true,
                dom: "<'row'<'col-md-3 col-sm-3'l><'col-md-6 col-sm-7 text-left'B><'col-md-3 col-sm-3 text-right'f>>" +
                    "<'row'<'col-md-12 col-sm-12'tr>>" +
                    "<'row'<'col-md-5 col-sm-5'i><'col-sm-7 col-sm-7'p>>",
                "paging": true,
                stateSave: false,
                // Lenguaje
                "language": {
                    //"url": "../lib/datatables.net/lang/Spanish.json",
                    "decimal": ",",
                    "thousands": "."
                },
                order: [[$('th.ordenado_por_defecto').index(), 'desc']],
                /*columnDefs: [
                    { "targets": $('th.sin_orden').index(), "orderable": false }
                ],*/
                // Botones pdf y excel
                buttons: [
                    {
                        text: '<i class="fa fa-file-excel " title="Excel" aria-hidden="true"></i> Excel',
                        extend: 'excelHtml5',
                        exportOptions: {
                            columns: ':visible:not(.ocultar-d)',
                            format: {
                                body: function (data, row, column, node) {
                                    if ($.isNumeric(data.replace(/<.*?\>/, '').replace(/<\/.*?>/, '').replace(/[.]/g, '').replace(/[,]/g, '.'))) {
                                        return data.replace(/<.*?\>/, '').replace(/<\/.*?>/, '').replace(/[.]/g, '').replace(/[,]/g, '.');
                                    } else {
                                        // Elimina espacios en blanco y etiquetas html que añade en los Template Field del Gridview
                                        return data.replace(/&nbsp;/g, '').replace(/<.*?\>/, '').replace(/<\/.*?>/, '');
                                    }
                                }
                            }
                        }
                    },
                    {
                        text: '<i class="fa fa-print " title="Imprimir o Exportar a PDF" aria-hidden="true"></i> <i class="fa fa-file-pdf fa-lg" title="Imprimir o Exportar a PDF" aria-hidden="true"></i>',
                        extend: 'print',
                        exportOptions: {
                            columns: ':visible:not(.ocultar-d)'
                        }
                    },
                    {
                        text: '<i class="fa fa-eye " title="Columnas Visibles" aria-hidden="true"></i>',
                        extend: 'colvis',
                        columns: ':not(.ocultar-d)'
                    }
                ]
            });
        }

        if (!$.fn.DataTable.isDataTable('.datatable_AE')) {

            $('.datatable_AE').DataTable({
                //dom: 'Blfrtip',
                deferRender: true,
                dom: "<'row'<'col-md-3 col-sm-3'l><'col-md-6 col-sm-7 text-left'B><'col-md-3 col-sm-3 text-right'f>>" +
                    "<'row'<'col-md-12 col-sm-12'tr>>" +
                    "<'row'<'col-md-5 col-sm-5'i><'col-sm-7 col-sm-7'p>>",
                "paging": true,
                stateSave: false,
                // Lenguaje
                "language": {
                    //"url": "../lib/datatables.net/lang/Spanish.json",
                    "decimal": ",",
                    "thousands": "."
                },
                order: [ 0, 'desc'],
                /*columnDefs: [
                    { "targets": 6, "orderable": false }
                ],*/
                initComplete: function () {
                    var table = this.api();

                    // Permite ocultar columnas por su clase en lugar de ir por su posición en la tabla
                    table.columns('.ojo').visible(false);
                },
                // Botones pdf y excel
                buttons: [
                    {
                        text: '<i class="fa fa-file-excel " title="Excel" aria-hidden="true"></i> Excel',
                        extend: 'excelHtml5',
                        exportOptions: {
                            columns: ':visible:not(.ocultar-d)',
                            format: {
                                body: function (data, row, column, node) {
                                    if ($.isNumeric(data.replace(/<.*?\>/, '').replace(/<\/.*?>/, '').replace(/[.]/g, '').replace(/[,]/g, '.'))) {
                                        return data.replace(/<.*?\>/, '').replace(/<\/.*?>/, '').replace(/[.]/g, '').replace(/[,]/g, '.');
                                    } else {
                                        // Elimina espacios en blanco y etiquetas html que añade en los Template Field del Gridview
                                        return data.replace(/&nbsp;/g, '').replace(/<.*?\>/, '').replace(/<\/.*?>/, '');
                                    }
                                }
                            }
                        }
                    },
                    {
                        text: '<i class="fa fa-print " title="Imprimir o Exportar a PDF" aria-hidden="true"></i> <i class="fa fa-file-pdf fa-lg" title="Imprimir o Exportar a PDF" aria-hidden="true"></i>',
                        extend: 'print',
                        exportOptions: {
                            columns: ':visible:not(.ocultar-d)'
                        }
                    },
                    {
                        text: '<i class="fa fa-eye " title="Columnas Visibles" aria-hidden="true"></i>',
                        extend: 'colvis',
                        columns: ':not(.ocultar-d)'
                    }
                ]
            });
        }

        if (!$.fn.DataTable.isDataTable('.datatable_AI')) {

            $('.datatable_AI').DataTable({
                //dom: 'Blfrtip',
                deferRender: true,
                dom: "<'row'<'col-md-3 col-sm-3'l><'col-md-6 col-sm-7 text-left'B><'col-md-3 col-sm-3 text-right'f>>" +
                    "<'row'<'col-md-12 col-sm-12'tr>>" +
                    "<'row'<'col-md-5 col-sm-5'i><'col-sm-7 col-sm-7'p>>",
                "paging": true,
                stateSave: false,
                // Lenguaje
                "language": {
                    //"url": "../lib/datatables.net/lang/Spanish.json",
                    "decimal": ",",
                    "thousands": "."
                },
                order: [0, 'desc'],
                /*columnDefs: [
                    { "targets": 5, "orderable": false }
                ],*/
                initComplete: function () {
                    var table = this.api();

                    // Permite ocultar columnas por su clase en lugar de ir por su posición en la tabla
                    table.columns('.ojo').visible(false);
                },
                // Botones pdf y excel
                buttons: [
                    {
                        text: '<i class="fa fa-file-excel " title="Excel" aria-hidden="true"></i> Excel',
                        extend: 'excelHtml5',
                        exportOptions: {
                            columns: ':visible:not(.ocultar-d)',
                            format: {
                                body: function (data, row, column, node) {
                                    if ($.isNumeric(data.replace(/<.*?\>/, '').replace(/<\/.*?>/, '').replace(/[.]/g, '').replace(/[,]/g, '.'))) {
                                        return data.replace(/<.*?\>/, '').replace(/<\/.*?>/, '').replace(/[.]/g, '').replace(/[,]/g, '.');
                                    } else {
                                        // Elimina espacios en blanco y etiquetas html que añade en los Template Field del Gridview
                                        return data.replace(/&nbsp;/g, '').replace(/<.*?\>/, '').replace(/<\/.*?>/, '');
                                    }
                                }
                            }
                        }
                    },
                    {
                        text: '<i class="fa fa-print " title="Imprimir o Exportar a PDF" aria-hidden="true"></i> <i class="fa fa-file-pdf fa-lg" title="Imprimir o Exportar a PDF" aria-hidden="true"></i>',
                        extend: 'print',
                        exportOptions: {
                            columns: ':visible:not(.ocultar-d)'
                        }
                    },
                    {
                        text: '<i class="fa fa-eye " title="Columnas Visibles" aria-hidden="true"></i>',
                        extend: 'colvis',
                        columns: ':not(.ocultar-d)'
                    }
                ]
            });
        }

        if (!$.fn.DataTable.isDataTable('.datatable_AE_todas')) {

            $('.datatable_AE_todas').DataTable({
                //dom: 'Blfrtip',
                deferRender: true,
                dom: "<'row'<'col-md-3 col-sm-3'l><'col-md-6 col-sm-7 text-left'B><'col-md-3 col-sm-3 text-right'f>>" +
                    "<'row'<'col-md-12 col-sm-12'tr>>" +
                    "<'row'<'col-md-5 col-sm-5'i><'col-sm-7 col-sm-7'p>>",
                "paging": true,
                stateSave: false,
                // Lenguaje
                "language": {
                    //"url": "../lib/datatables.net/lang/Spanish.json",
                    "decimal": ",",
                    "thousands": "."
                },
                order: [0, 'desc'],
               /* columnDefs: [
                    { "targets": 6, "orderable": false } // Da fallo cuando se oculta esta columna
                ],*/
                initComplete: function () {
                    var table = this.api();

                    // Permite ocultar columnas por su clase en lugar de ir por su posición en la tabla
                    table.columns('.ojo').visible(false);
                },
                // Botones pdf y excel
                buttons: [
                    {
                        text: '<i class="fa fa-file-excel " title="Excel" aria-hidden="true"></i> Excel',
                        extend: 'excelHtml5',
                        exportOptions: {
                            columns: ':visible:not(.ocultar-d)',
                            format: {
                                body: function (data, row, column, node) {
                                    if ($.isNumeric(data.replace(/<.*?\>/, '').replace(/<\/.*?>/, '').replace(/[.]/g, '').replace(/[,]/g, '.'))) {
                                        return data.replace(/<.*?\>/, '').replace(/<\/.*?>/, '').replace(/[.]/g, '').replace(/[,]/g, '.');
                                    } else {
                                        // Elimina espacios en blanco y etiquetas html que añade en los Template Field del Gridview
                                        return data.replace(/&nbsp;/g, '').replace(/<.*?\>/, '').replace(/<\/.*?>/, '');
                                    }
                                }
                            }
                        }
                    },
                    {
                        text: '<i class="fa fa-print " title="Imprimir o Exportar a PDF" aria-hidden="true"></i> <i class="fa fa-file-pdf fa-lg" title="Imprimir o Exportar a PDF" aria-hidden="true"></i>',
                        extend: 'print',
                        exportOptions: {
                            columns: ':visible:not(.ocultar-d)'
                        }
                    },
                    {
                        text: '<i class="fa fa-eye " title="Columnas Visibles" aria-hidden="true"></i>',
                        extend: 'colvis',
                        columns: ':not(.ocultar-d)'
                    }
                ]
            });
        }

        if (!$.fn.DataTable.isDataTable('.datatable_AI_todas')) {

            $('.datatable_AI_todas').DataTable({
                //dom: 'Blfrtip',
                deferRender: true,
                dom: "<'row'<'col-md-3 col-sm-3'l><'col-md-6 col-sm-7 text-left'B><'col-md-3 col-sm-3 text-right'f>>" +
                    "<'row'<'col-md-12 col-sm-12'tr>>" +
                    "<'row'<'col-md-5 col-sm-5'i><'col-sm-7 col-sm-7'p>>",
                "paging": true,
                stateSave: false,
                // Lenguaje
                "language": {
                    //"url": "../lib/datatables.net/lang/Spanish.json",
                    "decimal": ",",
                    "thousands": "."
                },
                order: [0, 'desc'],
               /* columnDefs: [
                    { "targets": 6, "orderable": false } // Da fallo cuando se oculta esta columna 
                ],*/
                initComplete: function () {
                    var table = this.api();

                    // Permite ocultar columnas por su clase en lugar de ir por su posición en la tabla
                    table.columns('.ojo').visible(false);
                },
                // Botones pdf y excel
                buttons: [
                    {
                        text: '<i class="fa fa-file-excel " title="Excel" aria-hidden="true"></i> Excel',
                        extend: 'excelHtml5',
                        exportOptions: {
                            columns: ':visible:not(.ocultar-d)',
                            format: {
                                body: function (data, row, column, node) {
                                    if ($.isNumeric(data.replace(/<.*?\>/, '').replace(/<\/.*?>/, '').replace(/[.]/g, '').replace(/[,]/g, '.'))) {
                                        return data.replace(/<.*?\>/, '').replace(/<\/.*?>/, '').replace(/[.]/g, '').replace(/[,]/g, '.');
                                    } else {
                                        // Elimina espacios en blanco y etiquetas html que añade en los Template Field del Gridview
                                        return data.replace(/&nbsp;/g, '').replace(/<.*?\>/, '').replace(/<\/.*?>/, '');
                                    }
                                }
                            }
                        }
                    },
                    {
                        text: '<i class="fa fa-print " title="Imprimir o Exportar a PDF" aria-hidden="true"></i> <i class="fa fa-file-pdf fa-lg" title="Imprimir o Exportar a PDF" aria-hidden="true"></i>',
                        extend: 'print',
                        exportOptions: {
                            columns: ':visible:not(.ocultar-d)'
                        }
                    },
                    {
                        text: '<i class="fa fa-eye " title="Columnas Visibles" aria-hidden="true"></i>',
                        extend: 'colvis',
                        columns: ':not(.ocultar-d)'
                    }
                ]
            });
        }
    }
}



