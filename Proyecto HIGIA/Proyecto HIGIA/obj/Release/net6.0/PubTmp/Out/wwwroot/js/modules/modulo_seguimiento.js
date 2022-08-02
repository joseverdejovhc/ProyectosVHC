var modulo_seguimiento = {
    tabla_capitulos: null,
    tabla_subcapitulos:null,
    init: () => {
        modulo_seguimiento.putEventsOnButtons();
        modulo_seguimiento.loadSelectCapitulos();
        modulo_seguimiento.loadAllData();
    },
    putEventsOnButtons: () => {

        $("#sel_capitulo").on("change", function (e) {
            e.preventDefault();
            var capitulo = $("#sel_capitulo").val();
            modulo_seguimiento.loadAllData(capitulo);
        });

        $("#btn_desglose_pedidos").on("click", function (e) {
            e.preventDefault();
            $("#sel_capitulo_pedidos").val(null);

            modulo_seguimiento.AbrirModalDesglosePedidos();
        })

        $("#btn_desglose_facturas").on("click", function (e) {
            e.preventDefault();
            $("#sel_capitulo_facturas").val(null);

            modulo_seguimiento.AbrirModalDesgloseFacturas();
        })

        $("#sel_capitulo_pedidos").on("change", function (e) {
            e.preventDefault();
            var capitulo = $("#sel_capitulo_pedidos").val();
            modulo_seguimiento.AbrirModalDesglosePedidos(capitulo);
        })

        $("#sel_capitulo_facturas").on("change", function (e) {
            e.preventDefault();
            var capitulo = $("#sel_capitulo_facturas").val();
            modulo_seguimiento.AbrirModalDesgloseFacturas(capitulo);
        })

        $('a[data-toggle="tab"]').on('shown.bs.tab', function (e) {
            $($.fn.dataTable.tables(true)).DataTable().columns.adjust();
        })
    },
    loadAllData: (capitulo=null) => {

        var data = {
            capitulo:capitulo
        }

        var url = helper.baseUrl + '/Home/getSeguimiento';

        helper.ajax(url, "POST", data).then(result => {
            if (result.success) {
                var capitulos = JSON.parse(result.capitulos);
                var subcapitulos = JSON.parse(result.subcapitulos);
                modulo_seguimiento.loadDatatableCapitulos(capitulos);
                modulo_seguimiento.loadDatatableSubcapitulos(subcapitulos);
            }
        });

    },

    loadSelectCapitulos: () => {

        var data = {
            capitulo: null
        }

        var url = helper.baseUrl + '/Home/getAllCapitulos';

        helper.ajax(url, "POST", data).then(result => {
            if (result.success) {
                var capitulos = JSON.parse(result.capitulos);

                $("#sel_capitulo option").remove();
                $("#sel_capitulo_pedidos option").remove();
                $("#sel_capitulo_facturas option").remove();

                $('#sel_capitulo').append(new Option("", null));
                $('#sel_capitulo_pedidos').append(new Option("", null));
                $('#sel_capitulo_facturas').append(new Option("", null));

                capitulos.forEach((capitulo) => {
                    $('#sel_capitulo').append(new Option(capitulo.cod_capitulo + "-" + capitulo.nom_capitulo, capitulo.cod_capitulo));
                    $('#sel_capitulo_pedidos').append(new Option(capitulo.cod_capitulo + "-" + capitulo.nom_capitulo, capitulo.cod_capitulo));
                    $('#sel_capitulo_facturas').append(new Option(capitulo.cod_capitulo + "-" + capitulo.nom_capitulo, capitulo.cod_capitulo));
                });
            }
        });

       
    },
    loadDatatableCapitulos: (capitulos) => {
        

        if ($.fn.DataTable.isDataTable("#tabla_capitulos")) {
            $("#tabla_capitulos").DataTable().destroy();
        }

        modulo_seguimiento.tabla_capitulos=$("#tabla_capitulos").DataTable({
            "data": capitulos,
            "dom": "Bfrtip",
            "columns": [
                { "data": "cod_capitulo" },
                { "data": "nom_capitulo" },
                { "data": "presupuesto", render: $.fn.dataTable.render.number('.', ',', 2, ''), "className": "text-right" },
                { "data": "pedido", render: $.fn.dataTable.render.number('.', ',', 2, ''), "className": "text-right" },
                { "data": "importe_facturacion", render: $.fn.dataTable.render.number('.', ',', 2, ''), "className": "text-right" }
            ],

            "paging": true,
            stateSave: false,
            "language": {
                "url": "../lib/datatables.net/lang/Spanish.json",
                "decimal": ",",
                "thousands": "."
            },
            buttons: [
                {
                    text: '<i class="fa fa-file-excel fa-lg" title="Excel" aria-hidden="true"></i> Excel',
                    // title: 'Grupo: ' + $('.ListGrupo option:selected').text() + '     Empresa: ' + $('.ListEmpresa option:selected').text(),
                    extend: 'excelHtml5',
                    exportOptions: {

                        format: {
                            body: function (data, row, column, node) {
                                var data = String(data);

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
                    text: '<i class="fa fa-refresh"></i>',
                    className: 'btn btn-info btn-xs text-white btn_refresh',
                    action: function (e, dt, node, config) {
                        modulo_seguimiento.loadAllData(null);
                        $($.fn.dataTable.tables(true)).DataTable().columns.adjust();
                    }
                }
            ],
            "autoWidth": false,
            "columnDefs": [
                { "width": "5%", "targets": 0 },
                { "width": "12%", "targets": 2 },
                { "width": "12%", "targets": 3 },
                { "width": "12%", "targets": 4 }
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
                    .column(2)
                    .data()
                    .reduce(function (a, b) {
                        return intVal(a) + intVal(b);
                    }, 0);

                total_pedido = api
                    .column(3)
                    .data()
                    .reduce(function (a, b) {
                        return intVal(a) + intVal(b);
                    }, 0);

                total_facturado = api
                    .column(4)
                    .data()
                    .reduce(function (a, b) {
                        return intVal(a) + intVal(b);
                    }, 0);

                // Update footer

                $(api.column(2).footer()).html(helper.number_format_js(total_importe, 0, ',', '.'));
                $(api.column(3).footer()).html(helper.number_format_js(total_pedido, 0, ',', '.'));
                $(api.column(4).footer()).html(helper.number_format_js(total_facturado, 0, ',', '.'));

            },
            initComplete: function (settings, json) {
                $(".btn_refresh").removeClass("dt-button");
            }
        });
    },
    loadDatatableSubcapitulos: (subcapitulos) => {

        if ($.fn.DataTable.isDataTable("#tabla_subcapitulos")) {
            $("#tabla_subcapitulos").DataTable().destroy();
        }

        modulo_seguimiento.tabla_subcapitulos =$("#tabla_subcapitulos").DataTable({
            "data": subcapitulos,
            "dom": "Bfrtip",
            "columns": [
                { "data": "cod_capitulo" },
                { "data": "nom_capitulo" },
                { "data": "cod_subcapitulo" },
                { "data": "nom_subcapitulo" },
                { "data": "presupuesto", render: $.fn.dataTable.render.number('.', ',', 2, ''), "className": "table-cell text-right"  },
                { "data": "pedido", render: $.fn.dataTable.render.number('.', ',', 2, ''), "className": "table-cell text-right"  },
                { "data": "importe_facturacion", render: $.fn.dataTable.render.number('.', ',', 2, ''), "className": "table-cell text-right"  }
            ],
            "columnDefs": [
                { "width": "5%", "targets": 0 },
                { "width": "5%", "targets": 2 },
                { "width": "8%", "targets": 4 },
                { "width": "8%", "targets": 5 },
                { "width": "8%", "targets": 6 }
            ],
            "autoWidth": false,
            "paging": true,
            stateSave: false,
            "language": {
                "url": "../lib/datatables.net/lang/Spanish.json",
                "decimal": ",",
                "thousands": "."
            },
            buttons: [
                {
                    text: '<i class="fa fa-file-excel fa-lg" title="Excel" aria-hidden="true"></i> Excel',
                    // title: 'Grupo: ' + $('.ListGrupo option:selected').text() + '     Empresa: ' + $('.ListEmpresa option:selected').text(),
                    extend: 'excelHtml5',
                    exportOptions: {
                        format: {
                            body: function (data, row, column, node) {
                                var data = String(data);

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
                    text: '<i class="fa fa-refresh"></i>',
                    className: 'btn btn-info btn-xs text-white btn_refresh',
                    action: function (e, dt, node, config) {
                        modulo_seguimiento.loadAllData(null);
                        $($.fn.dataTable.tables(true)).DataTable().columns.adjust();
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
                total_importe = api
                    .column(4)
                    .data()
                    .reduce(function (a, b) {
                        return intVal(a) + intVal(b);
                    }, 0);

                total_pedido = api
                    .column(5)
                    .data()
                    .reduce(function (a, b) {
                        return intVal(a) + intVal(b);
                    }, 0);

                total_facturado = api
                    .column(6)
                    .data()
                    .reduce(function (a, b) {
                        return intVal(a) + intVal(b);
                    }, 0);

                // Update footer

                $(api.column(4).footer()).html(helper.number_format_js(total_importe, 0, ',', '.'));
                $(api.column(5).footer()).html(helper.number_format_js(total_pedido, 0, ',', '.'));
                $(api.column(6).footer()).html(helper.number_format_js(total_facturado, 0, ',', '.'));

            },
            initComplete: function (settings, json) {
                $(".btn_refresh").removeClass("dt-button");
            }
        });
    },
    AbrirModalDesglosePedidos: (capitulo) => {
        var data = {
            capitulo: capitulo
        }

        var url = helper.baseUrl + '/Home/getDesglosePedidos';

        helper.ajax(url, "POST", data).then(result => {
            if (result.success) {
                var desglose = JSON.parse(result.desglose);

                if ($.fn.DataTable.isDataTable("#tabla_desglose_pedidos")) {
                    $("#tabla_desglose_pedidos").DataTable().destroy();
                }

                $("#tabla_desglose_pedidos").DataTable({
                    "data": desglose,
                    "dom": "Bfrtip",
                    "columns": [
                        { "data": "num_pedido" },
                        { "data": "cod_proveedor" },
                        { "data": "nom_proveedor" },
                        { "data": "cod_capitulo" },
                        { "data": "nom_capitulo" },
                        { "data": "cod_subcapitulo" },
                        { "data": "nom_subcapitulo" },
                        { "data": "importe", render: $.fn.dataTable.render.number('.', ',', 2, ''), "className": "table-cell text-right" },
                        {
                            render: function (data, type, row) {
                                return `<a href="#" class="btn btn-light btn-sm" onclick="modulo_seguimiento.ver_pedido(` + row.id + `);"><i class="fa fa-eye"></i></a>`;
                            }
                        }
                    ],
                    "columnDefs": [
                        { "width": "5%", "targets": [1,3,5] }
                    ],
                    "paging": true,
                    stateSave: false,
                    "language": {
                        "url": "../lib/datatables.net/lang/Spanish.json",
                        "decimal": ",",
                        "thousands": "."
                    },
                    buttons: [
                        {
                            text: '<i class="fa fa-file-excel fa-lg" title="Excel" aria-hidden="true"></i> Excel',
                            // title: 'Grupo: ' + $('.ListGrupo option:selected').text() + '     Empresa: ' + $('.ListEmpresa option:selected').text(),
                            extend: 'excelHtml5',
                            exportOptions: {
                                columns: 'th:not(:last-child)',
                                format: {
                                    body: function (data, row, column, node) {
                                        var data = String(data);

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
                        total_importe = api
                            .column(7)
                            .data()
                            .reduce(function (a, b) {
                                return intVal(a) + intVal(b);
                            }, 0);
                                               

                        $(api.column(7).footer()).html(helper.number_format_js(total_importe, 0, ',', '.'));

                    }
                });

            }
        });

        helper.AbrirModal("#ventana_desglose_pedidos");
    },
    AbrirModalDesgloseFacturas: (capitulo) => {
        var data = {
            capitulo: capitulo
        }

        var url = helper.baseUrl + '/Home/getDesgloseFacturas';

        helper.ajax(url, "POST", data).then(result => {
            if (result.success) {
                var desglose = JSON.parse(result.desglose);

                if ($.fn.DataTable.isDataTable("#tabla_desglose_facturas")) {
                    $("#tabla_desglose_facturas").DataTable().destroy();
                }

                $("#tabla_desglose_facturas").DataTable({
                    "data": desglose,
                    "dom": "Bfrtip",
                    "columns": [
                        { "data": "num_factura" },
                        { "data": "num_pedido" },
                        { "data": "cod_proveedor" },
                        { "data": "nom_proveedor" },
                        { "data": "cod_capitulo" },
                        { "data": "nom_capitulo" },
                        { "data": "cod_subcapitulo" },
                        { "data": "nom_subcapitulo" },
                        { "data": "facturado", render: $.fn.dataTable.render.number('.', ',', 2, ''), "className": "table-cell text-right" },
                        {
                            render: function (data, type, row) {
                                return `<a href="#" class="btn btn-light btn-sm" onclick='modulo_seguimiento.ver_factura(` + JSON.stringify(row) + `);'><i class="fa fa-eye"></i></a>`;
                            }
                        }
                    ],
                    "columnDefs": [
                        { "width": "5%", "targets": [0,1,2,4,5] }
                    ],
                    "paging": true,
                    stateSave: false,
                    "language": {
                        "url": "../lib/datatables.net/lang/Spanish.json",
                        "decimal": ",",
                        "thousands": "."
                    },
                    buttons: [
                        {
                            text: '<i class="fa fa-file-excel fa-lg" title="Excel" aria-hidden="true"></i> Excel',
                            // title: 'Grupo: ' + $('.ListGrupo option:selected').text() + '     Empresa: ' + $('.ListEmpresa option:selected').text(),
                            extend: 'excelHtml5',
                            exportOptions: {
                                columns: 'th:not(:last-child)',
                                format: {
                                    body: function (data, row, column, node) {
                                        var data = String(data);

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
                        total_importe = api
                            .column(8)
                            .data()
                            .reduce(function (a, b) {
                                return intVal(a) + intVal(b);
                            }, 0);


                        $(api.column(8).footer()).html(helper.number_format_js(total_importe, 0, ',', '.'));

                    }
                });

            }
        });
        helper.AbrirModal("#ventana_desglose_facturas");

    },
    ver_pedido: (id) => {
        localStorage.setItem("accion_pedido", "editar")
        localStorage.setItem("numero_pedido", id);
        window.open(helper.baseUrl + '/Home/NuevoPedido');
    },
    ver_factura: (factura) => {
        $("#identi_factura").val(factura.id);
        $("#identi_pedido").val(factura.fk_pedido);
        $("#num_factura").val(factura.num_factura);
        $("#num_expediente").val(factura.num_expediente);
        $("#fech_factura").val(factura.fecha_factura);
        modulo_seguimiento.AbrirModalFactura(factura);
    },
    AbrirModalFactura: async (factura = null) => {
        var data_table = null;
        var url = helper.baseUrl + '/Home/getSubcapitulosFactura';
        var data = {
            factura: factura.id
        }

        await helper.ajax(url, "POST", data).then(result => {
            if (result.success) {
                data_table = JSON.parse(result.subcapitulos);
            }
        });
        $("#num_factura").prop("readonly", true);
        $("#num_expediente").prop("readonly", true);
        $("#fech_factura").prop("readonly", true);
        $("#modulo").val("seguimiento");

        //$("#btn_save_factura").prop("disabled", true);

        $("#action_factura").val("UPDATE");

        $("#fech_factura").flatpickr({
            "defaultDate": factura.fecha_factura,
            dateFormat: "d/m/Y",
            allowInput: true
        });


        if ($.fn.DataTable.isDataTable("#tabla_factura_subcapitulo")) {
            $("#tabla_factura_subcapitulo").DataTable().destroy();
        }

        modulo_factura.tabla_subcapitulos_facturacion=$("#tabla_factura_subcapitulo").DataTable({
            "data": data_table,
            "dom": "Bfrtip",
            "columns": [
                { "data": "cod_capitulo" },
                { "data": "nom_capitulo" },
                { "data": "cod_subcapitulo" },
                { "data": "nom_subcapitulo" },
                { "data": "facturado", render: $.fn.dataTable.render.number('.', ',', 2, ''), "className": "text-right" },
                {
                    render: function (data, type, row, meta) {
                        return `<a href="#" class="btn btn-light btn-sm" onclick='modulo_factura.AbrirModalImporteFacturado(` + row.id_subcapitulo + `,` + meta.row + `,` + row.facturado + `);'><i class="fa fa-plus"></i></a>`;
                    }
                }
            ],
            "columnDefs": [
                { "width": "5%", "targets": [0,2] }
            ],
            "paging": true,
            stateSave: false,
            "language": {
                "url": "../lib/datatables.net/lang/Spanish.json",
                "decimal": ",",
                "thousands": "."
            },
            buttons: [
                {
                    text: '<i class="fa fa-file-excel fa-lg" title="Excel" aria-hidden="true"></i> Excel',
                    // title: 'Grupo: ' + $('.ListGrupo option:selected').text() + '     Empresa: ' + $('.ListEmpresa option:selected').text(),
                    extend: 'excelHtml5',
                    exportOptions: {                        
                        format: {
                            body: function (data, row, column, node) {
                                var data = String(data);

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
                total_importe = api
                    .column(4)
                    .data()
                    .reduce(function (a, b) {
                        return intVal(a) + intVal(b);
                    }, 0);


                // Update footer

                $(api.column(4).footer()).html(helper.number_format_js(total_importe, 0, ',', '.'));
            }
        });


        helper.AbrirModal("#ventana_add_factura");

    }
}