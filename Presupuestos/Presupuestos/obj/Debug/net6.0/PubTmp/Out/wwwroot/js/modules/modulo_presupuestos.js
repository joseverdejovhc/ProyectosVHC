var modulo_presupuestos = {
    init: () => {
        modulo_presupuestos.loadAllData();
        modulo_presupuestos.loadEventsButtons();
    },
    cdf_venta: 0,
    antes_impuesto: 0,
    loadEventsButtons: () => {
        $("#inp_ejer_presupuesto").on("change", async function () {
            var aprobado = await modulo_presupuestos.checkPresupuestoAprobado();
            if (aprobado) {
                $("#btn_aprobar").hide();
                $("#btn_abrir").show();
            } else {
                $("#btn_aprobar").show();
                $("#btn_abrir").hide();
            }
            modulo_presupuestos.getUltimaCarga();
            modulo_presupuestos.getDatosPresupuesto();
        })

        $("#btn_abrir").on("click", function () {
            var aprobado = 0;
            modulo_presupuestos.guardar_presupuesto(aprobado);
        })

        $("#btn_aprobar").on("click", function () {
            var aprobado = 1;
            modulo_presupuestos.guardar_presupuesto(aprobado);
        })
    },
    loadAllData: async () => {
        var url = helper.baseUrl + '/Home/getUltimoPresuprodAprobado';


        helper.ajax(url, "GET").then(async (result) => {
            if (result.success) {
                var ejercicio = result.ejercicio;
                $("#inp_ejer_presupuesto").val(ejercicio);

                var aprobado = await modulo_presupuestos.checkPresupuestoAprobado();
                if (aprobado) {
                    $("#btn_aprobar").hide();
                    $("#btn_abrir").show();
                } else {
                    $("#btn_aprobar").show();
                    $("#btn_abrir").hide();
                }
                modulo_presupuestos.getUltimaCarga();
                modulo_presupuestos.getDatosPresupuesto();
            }
        });
    },
    guardar_presupuesto: (aprobado) => {
        var data = {
            "ejercicio": $("#inp_ejer_presupuesto").val(),
            "margen_bruto": $("#td_margen_bruto").text(),
            "coste_estructura": $("#td_coste_estructura").text(),
            "coste_linea": $("#td_coste_linea").text(),
            "antes_impuesto": $("#td_antes_impuesto").text(),
            "coste_directo_fijo": $("#td_cdf").text(),
            "despues_impuesto": $("#td_res_despues_impuesto").text(),
            "ebitda": $("#td_EBITDA").text(),
            "aprobado": aprobado
        }
        var url = helper.baseUrl + '/Home/savePresupuesto';

        helper.ajax(url, "POST", data).then(result => {
            if (result.success) {
                console.log(result);
              //  helper.MostrarOk("#alerta", result.message);
                modulo_presupuestos.getUltimaCarga();

                if (aprobado == 1) {
                    $("#btn_aprobar").hide();
                    $("#btn_abrir").show();
                    helper.MostrarOk("#alerta", "Presupuesto aprobado correctamente!");
                } else {
                    $("#btn_aprobar").show();
                    $("#btn_abrir").hide();
                    helper.MostrarOk("#alerta", "Presupuesto abierto correctamente!");
                }

            } else {
            }

        });
    },
    getUltimaCarga: async () => {
        var url = helper.baseUrl + '/Home/getUltimaCargaPresupuesto';

        var data = {
            ejercicio: $("#inp_ejer_presupuesto").val()
        }

        helper.ajax(url, "POST", data).then(result => {
            if (result.success) {
                if (result.cargada_por != "" && result.cargada_por != null) {
                    $("#sp_ultima_carga").text("Aprobado por: " + result.cargada_por);
                } else {
                    $("#sp_ultima_carga").text("");
                }
            }
        });

    },
    checkPresupuestoAprobado: async () => {
        var url = helper.baseUrl + '/Home/comprobarPresupuestoAprobado';

        var data = {
            ejercicio: $("#inp_ejer_presupuesto").val()
        }

        var aprobado = false;

        await helper.ajax(url, "POST", data).then(result => {
            aprobado = result.aprobado;
        });

        return aprobado;
    },
    getDatosPresupuesto: () => {

        helper.MostrarLoader();
        var data = {
            "ejercicio": $("#inp_ejer_presupuesto").val()
        }
        var url = helper.baseUrl + '/Home/getDatosPresupuesto';

        helper.ajax(url, "POST", data).then(result => {
            if (result.success) {
                modulo_presupuestos.cdf_venta = result.cdf_cargo_almacen + result.cdf_cargo_produccion;
                modulo_presupuestos.antes_impuesto = result.antes_impuestos;
                $("#td_margen_bruto").text(helper.number_format_js(result.margen_bruto, 0, ',', '.'));
                $("#td_coste_estructura").text(helper.number_format_js(result.coste_estructura, 0, ',', '.'));
                $("#td_coste_linea").text(helper.number_format_js(result.coste_linea, 0, ',', '.'));
                $("#td_cdf").text(helper.number_format_js(result.coste_directo_fijo,0, ',', '.'));
                $("#td_res_antes_impuesto").text(helper.number_format_js(result.antes_impuestos, 0, ',', '.'));
                $("#td_antes_impuesto").text(helper.number_format_js(result.antes_impuestos, 0, ',', '.'))
                $("#td_res_despues_impuesto").text(helper.number_format_js(Math.round(result.despues_impuestos), 0, ',', '.'));
                $("#td_cdf_cargo_almacen").text(helper.number_format_js(result.cdf_cargo_almacen, 0, ',', '.'));
                $("#td_cdf_cargo_produccion").text(helper.number_format_js(result.cdf_cargo_produccion, 0, ',', '.'));
                $("#ft_total_cdf_venta").text(helper.number_format_js(modulo_presupuestos.cdf_venta, 0, ',', '.'));
                $("#td_EBITDA").text(helper.number_format_js(result.antes_impuestos + result.gastosCuentasContables.total, 0, ',', '.'));
                $("#tabla_presupuesto").show();
                $("#tabla_presupuesto").DataTable();
            } else {
            }

            helper.QuitarLoader();
        });
    },

    detallesMargenBruto: () => {
        var data = {
            "ejercicio": $("#inp_ejer_presupuesto").val()
        }
        var url = helper.baseUrl + '/Home/getMargenBrutoAgrupacion';

        helper.ajax(url, "POST", data).then(result => {
            if (result.success) {
                var agrupaciones = JSON.parse(result.agrupaciones);

                if ($.fn.DataTable.isDataTable('#tabla_margen_detalle')) {
                    $("#tabla_margen_detalle").DataTable().destroy();
                }

                $('#tabla_margen_detalle').DataTable({
                    "data": agrupaciones,
                    "columns": [
                        { "data": "agrupacion" },
                        { "data": "kilos_venta", render: $.fn.dataTable.render.number('.', ',', 0, '') },
                        { "data": "euros_venta", render: $.fn.dataTable.render.number('.', ',', 0, '') },
                        { "data": "cdv_total", render: $.fn.dataTable.render.number('.', ',', 0, '') },
                        { "data": "porcen_margen", render: $.fn.dataTable.render.number('.', ',', 2, '') },
                        { "data": "margen_bruto", render: $.fn.dataTable.render.number('.', ',', 0, '') }
                    ],
                    stateSave: false,
                    "language": {
                        "url": "../lib/datatables.net/lang/Spanish.json",
                        "decimal": ",",
                        "thousands": "."
                    },
                    "columnDefs": [
                        { "className": "dt-body-right", "targets": [1,2,3,4,5] }
                    ],
                    dom: 'Brtip',
                    buttons: [
                        {
                            text: '<i class="fa fa-file-excel fa-lg" title="Excel" aria-hidden="true"></i> Excel',
                            // title: 'Grupo: ' + $('.ListGrupo option:selected').text() + '     Empresa: ' + $('.ListEmpresa option:selected').text(),
                            extend: 'excelHtml5',
                            exportOptions: {
                                // columns: ':not(:eq(4))'
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
                        }
                    ],
                    "footerCallback": function (row, data, start, end, display) {
                        var api = this.api();

                        // Remove the formatting to get integer data for summation
                        var intVal = function (i) {
                            return typeof i === 'string' ?
                                i.replace(/[\$,]/g, '') * 1 :
                                typeof i === 'number' ?
                                    i : 0;
                        };

                        // Total over all pages
                        total_kilos = api
                            .column(1)
                            .data()
                            .reduce(function (a, b) {
                                return intVal(a) + intVal(b);
                            }, 0);

                        total_euros = api
                            .column(2)
                            .data()
                            .reduce(function (a, b) {
                                return intVal(a) + intVal(b);
                            }, 0);

                        total_cdv = api
                            .column(3)
                            .data()
                            .reduce(function (a, b) {
                                return intVal(a) + intVal(b);
                            }, 0);

                        total_porcen = api
                            .column(4)
                            .data()
                            .reduce(function (a, b) {
                                return intVal(a) + intVal(b);
                            }, 0);

                        total_margen = api
                            .column(5)
                            .data()
                            .reduce(function (a, b) {
                                return intVal(a) + intVal(b);
                            }, 0);


                        // Update footer
                        $(api.column(1).footer()).html(helper.number_format_js(total_kilos, 0, ',', '.'));
                        $(api.column(2).footer()).html(helper.number_format_js(total_euros, 0, ',', '.'));
                        $(api.column(3).footer()).html(helper.number_format_js(total_cdv, 0, ',', '.'));
                        $(api.column(4).footer()).html(helper.number_format_js(Math.round(total_porcen / agrupaciones.length), 2, ',', '.'));
                        $(api.column(5).footer()).html(helper.number_format_js(total_margen, 0, ',', '.'));

                    }
                });

                helper.AbrirModal("#margen_bruto_modal");

            } else {
            }

        });
    },
    detallesCosteEstructura: () => {
        var url = helper.baseUrl + '/Home/GetCentrosCostePresupuestos';

        var data = {
            ejercicio: $("#inp_ejer_presupuesto").val()
        }

        helper.ajax(url, "POST", data).then(result => {
            if (result.success) {
                $("#span_error").text('');

                var centros_coste = JSON.parse(result.centros_coste);

                if ($.fn.DataTable.isDataTable('#tabla_presupuesto')) {
                    $("#tabla_presupuesto").DataTable().destroy();
                }

                $('#tabla_presupuesto').DataTable({
                    "data": centros_coste,
                    order: [[1, 'asc']],
                    "columns": [
                        { "data": "nom_centro_coste" },
                        { "data": "clasificacion" },
                        { "data": "importe", render: $.fn.dataTable.render.number('.', ',', 0, '') }
                    ],
                    "columnDefs": [
                        {
                            "targets": [1],
                            "visible": false,
                            "searchable": false
                        },
                        { "className": "text-center", "targets": 2 }
                    ],
                    "paging": false,
                    stateSave: false,
                    rowGroup: true,
                    "language": {
                        "url": "../lib/datatables.net/lang/Spanish.json",
                        "decimal": ",",
                        "thousands": "."
                    },
                    rowGroup: {
                        startRender: function (rows, group) {

                            return $('<tr style="background-color: #585858ee; color:white;">').append('<td colspan="2">' + group + '</td>');

                        },
                        endRender: function (rows, group) {
                            var ageAvg = rows
                                .data()
                                .pluck("importe")
                                .reduce(function (a, b) {
                                    return a + b;
                                }, 0);

                            return $('<tr style="background-color: #c7c5c5ee;">').append('<td><b>Total</b></td><td style="text-align:center;">' + helper.number_format_js(ageAvg, 0, ',', '.') + '</td>');
                        },
                        /*  endRender: function (rows, group) {
  
                              var ageAvg = rows
                                  .data()
                                  .pluck("importe")
                                  .reduce(function (a, b) {
                                      return a + b;
                                  }, 0);
  
                              return $('<tr style="background-color: #c7c5c5ee;">').append('<td><b>Total</b></td><td>' + helper.number_format_js(ageAvg, 0, ',', '.') + '</td>');
  
                          },*/
                        dataSrc: function (data) {
                            return data.clasificacion;
                        }
                    },
                    dom: 'Brtip',
                    buttons: [
                        {
                            text: '<i class="fa fa-file-excel fa-lg" title="Excel" aria-hidden="true"></i> Excel',
                            // title: 'Grupo: ' + $('.ListGrupo option:selected').text() + '     Empresa: ' + $('.ListEmpresa option:selected').text(),
                            extend: 'excelHtml5',
                            exportOptions: {
                                // columns: ':not(:eq(4))'
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
                        }
                    ]
                });

                /*
                 ,
                    "drawCallback": function (settings) {
                        var api = this.api();
                        var rows = api.rows({ page: 'current' }).nodes();
                        var last = null;

                        api.column(2, { page: 'current' }).data().each(function (group, i) {
                            if (last !== group) {
                                $(rows).eq(i).before(
                                    '<tr class="group"><td colspan="5">' + group + '</td><td></td></tr>'
                                );

                                last = group;
                            }
                        });
                    }*/
                // Order by the grouping
                $('#tabla_presupuesto tbody').on('click', 'tr.group', function () {
                    var currentOrder = table.order()[0];
                    if (currentOrder[0] === 2 && currentOrder[1] === 'asc') {
                        table.order([2, 'desc']).draw();
                    }
                    else {
                        table.order([2, 'asc']).draw();
                    }
                });

                helper.AbrirModal("#centros_coste_modal");


            } else {
                $("#span_error").text(result.mensaje);

            }

        });
    },
    detallesCosteDirectoFijo: () => {
        var url = helper.baseUrl + '/Home/GetCDFPresupuestos';

        var data = {
            ejercicio: $("#inp_ejer_presupuesto").val()
        }

        helper.ajax(url, "POST", data).then(result => {
            if (result.success) {
                var cdf_almacen = result.cdf_almacen;
                var cdf_produccion = result.cdf_produccion;
                var departamentos = JSON.parse(result.centros_coste);

                $("#td_cdf_cargo_almacen").html(helper.number_format_js(cdf_almacen, 0, ',', '.'));
                $("#td_cdf_cargo_produccion").html(helper.number_format_js(cdf_produccion, 0, ',', '.'));
                $("#tabla_cdf_venta").DataTable();

                if ($.fn.DataTable.isDataTable('#tabla_cdf_departamentos')) {
                    $("#tabla_cdf_departamentos").DataTable().destroy();
                }

                $('#tabla_cdf_departamentos').DataTable({
                    "data": departamentos,

                    "columns": [
                        { "data": "nom_centro_coste" },
                        { "data": "importe", render: $.fn.dataTable.render.number('.', ',', 0, '') }
                    ],
                    stateSave: false,
                    "language": {
                        "url": "../lib/datatables.net/lang/Spanish.json",
                        "decimal": ",",
                        "thousands": "."
                    },
                    "columnDefs": [
                        { "className": "text-center", "targets": 1 }
                    ],
                    "footerCallback": function (row, data, start, end, display) {
                        var api = this.api();

                        // Remove the formatting to get integer data for summation
                        var intVal = function (i) {
                            return typeof i === 'string' ?
                                i.replace(/[\$,]/g, '') * 1 :
                                typeof i === 'number' ?
                                    i : 0;
                        };

                        // Total over all pages
                        total_importe = api
                            .column(1)
                            .data()
                            .reduce(function (a, b) {
                                return intVal(a) + intVal(b);
                            }, 0);

                        // Update footer
                        $(api.column(1).footer()).html(helper.number_format_js(total_importe, 0, ',', '.'));

                        $("#td_diferencia_cdf").html(helper.number_format_js(modulo_presupuestos.cdf_venta - total_importe, 0, ',', '.'))

                    },
                    dom: 'Brtip',
                    buttons: [
                        {
                            text: '<i class="fa fa-file-excel fa-lg" title="Excel" aria-hidden="true"></i> Excel',
                            // title: 'Grupo: ' + $('.ListGrupo option:selected').text() + '     Empresa: ' + $('.ListEmpresa option:selected').text(),
                            extend: 'excelHtml5',
                            exportOptions: {
                                // columns: ':not(:eq(4))'
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
                        }
                    ]
                });

                helper.AbrirModal("#cdf_modal");
            }

        });
    },
    detalles_ebitda: () => {
        var url = helper.baseUrl + '/Home/getImportesPorClasificacion';

        var data = {
            ejercicio: $("#inp_ejer_presupuesto").val()
        }

        helper.ajax(url, "POST", data).then(result => {
            if (result.success) {
                var gastos = JSON.parse(result.gastos);

                if ($.fn.DataTable.isDataTable('#tabla_ebitda')) {
                    $("#tabla_ebitda").DataTable().destroy();
                }

                $('#tabla_ebitda').DataTable({
                    "data": gastos,
                    "columns": [
                        { "data": "clasificacion" },
                        { "data": "importe", render: $.fn.dataTable.render.number('.', ',', 0, '') }
                    ],
                    stateSave: false,
                    "language": {
                        "url": "../lib/datatables.net/lang/Spanish.json",
                        "decimal": ",",
                        "thousands": "."
                    },
                    "columnDefs": [
                        { "className": "text-center", "targets": 1 }
                    ],
                    "footerCallback": function (row, data, start, end, display) {
                        var api = this.api();

                        // Remove the formatting to get integer data for summation
                        var intVal = function (i) {
                            return typeof i === 'string' ?
                                i.replace(/[\$,]/g, '') * 1 :
                                typeof i === 'number' ?
                                    i : 0;
                        };

                        // Total over all pages
                        total_importe = api
                            .column(1)
                            .data()
                            .reduce(function (a, b) {
                                return intVal(a) + intVal(b);
                            }, 0);

                        // Update footer
                        $(api.column(1).footer()).html(helper.number_format_js(modulo_presupuestos.antes_impuesto + total_importe, 0, ',', '.'));
                    },
                    dom: 'Brtip',
                    buttons: [
                        {
                            text: '<i class="fa fa-file-excel fa-lg" title="Excel" aria-hidden="true"></i> Excel',
                            // title: 'Grupo: ' + $('.ListGrupo option:selected').text() + '     Empresa: ' + $('.ListEmpresa option:selected').text(),
                            extend: 'excelHtml5',
                            exportOptions: {
                                // columns: ':not(:eq(4))'
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
                        }
                    ]
                });

                helper.AbrirModal("#ebitda_modal");
            }

        });
    }

}