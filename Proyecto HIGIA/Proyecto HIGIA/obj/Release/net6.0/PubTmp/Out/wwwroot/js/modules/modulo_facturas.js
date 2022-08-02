var modulo_facturas = {
    init: () => {
        modulo_facturas.loadDatatable();
    },
    loadDatatable: () => {
        var url = helper.baseUrl + '/Home/getFacturas';
        helper.ajax(url, "GET").then(result => {
            if (result.success) {
                var facturas = JSON.parse(result.facturas);

                if ($.fn.DataTable.isDataTable("#tabla_facturas")) {
                    $("#tabla_facturas").DataTable().destroy();
                }

                $("#tabla_facturas").DataTable({
                    "data": facturas,
                    "dom": "Bfrtip",
                    "columns": [
                        { "data": "num_factura" },
                        { "data": "num_expediente" },
                        {
                            "data": "fecha_factura"
                        },
                        { "data": "num_pedido" },
                        { "data": "nom_proveedor" },
                        { "data": "importe", render: $.fn.dataTable.render.number('.', ',', 2, ''), "className": "text-right"},
                        {
                            render: function (data, type, row) {
                                return `<a href="#" class="btn btn-light btn-sm" onclick='modulo_facturas.ver_factura(` + JSON.stringify(row) + `);'><i class="fa fa-eye"></i></a>`;
                            }
                        }
                    ],
                    "paging": true,
                    stateSave: false,
                    "language": {
                        "url": "../lib/datatables.net/lang/Spanish.json",
                        "decimal": ",",
                        "thousands": "."
                    },
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
                            .column(5)
                            .data()
                            .reduce(function (a, b) {
                                return intVal(a) + intVal(b);
                            }, 0);


                        $(api.column(5).footer()).html(helper.number_format_js(total_importe, 0, ',', '.'));

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
                        },
                        {
                            text: '<i class="fa fa-refresh"></i>',
                            className: 'btn btn-info btn-xs text-white btn_refresh',
                            action: function (e, dt, node, config) {
                                modulo_facturas.loadDatatable();
                            }
                        }
                    ],
                    initComplete: function (settings, json) {
                        $(".btn_refresh").removeClass("dt-button");
                    }
                });
            }
        });

    },
    ver_factura: (factura) => {
        $("#identi_factura").val(factura.id);
        $("#identi_pedido").val(factura.fk_pedido);
        $("#num_factura").val(factura.num_factura);
        $("#num_expediente").val(factura.num_expediente);
        modulo_facturas.AbrirModalFactura("editar", factura);
    },
    AbrirModalFactura: async (accion, factura = null) => {
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
        $("#modulo").val("factura");
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

        modulo_factura.tabla_subcapitulos_facturacion = $("#tabla_factura_subcapitulo").DataTable({
            "data": data_table,
            "dom": "Bfrtip",
            "columns": [
                { "data": "cod_capitulo" },
                { "data": "nom_capitulo" },
                { "data": "cod_subcapitulo" },
                { "data": "nom_subcapitulo" },
                { "data": "facturado", render: $.fn.dataTable.render.number('.', ',', 2, ''), "className": "text-right"  },
                {
                    render: function (data, type, row, meta) {
                        return `<a href="#" class="btn btn-light btn-sm" onclick='modulo_factura.AbrirModalImporteFacturado(` + row.id_subcapitulo + `,` + meta.row + `,` + row.facturado + `);'><i class="fa fa-plus"></i></a>`;
                    }
                }
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