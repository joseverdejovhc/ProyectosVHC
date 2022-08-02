var modulo_pedidos = {
    init: () => {
        modulo_pedidos.putEventsOnButtons();
        modulo_pedidos.loadAllData();
        localStorage.setItem("accion_pedido", "nuevo");
        localStorage.setItem("numero_pedido", "");
    },
    putEventsOnButtons: () => {

    },
    loadAllData: () => {
        helper.MostrarLoader();
        var url = helper.baseUrl + '/Home/getAllPedidos';

        helper.ajax(url, "GET").then(result => {
            if (result.success) {
                var pedidos = JSON.parse(result.pedidos);
                modulo_pedidos.loadDatatable(pedidos);

            } else {
            }

            helper.QuitarLoader();
        });

    },
    loadDatatable: (pedidos) => {

        if ($.fn.DataTable.isDataTable("#tabla_pedidos")) {
            $("#tabla_pedidos").DataTable().destroy();
        }

        $("#tabla_pedidos").DataTable({
            "data": pedidos,
            "dom": "Bfrtip",
            "columns": [
                { "data": "num_pedido" },
                { "data": "nom_proveedor" },
                { "data": "importe", render: $.fn.dataTable.render.number('.', ',', 2, ''), "className": "text-right" },
                { "data": "facturado", render: $.fn.dataTable.render.number('.', ',', 2, ''), "className": "text-right" },
                {
                    render: function (data, type, row) {
                        return `<a href="#" class="btn btn-light btn-sm" onclick="modulo_pedidos.editar_pedido(` + row.id + `);"><i class="fa fa-edit"></i></a>
                                <a href="#" class="btn btn-light btn-sm" onclick="modulo_pedidos.borrarPedido(` + row.id + `);"><i class="fa fa-trash"></i></a>`;
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
            "columnDefs": [
                { "width": "7%", "targets": 0 }
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

                total_facturado = api
                    .column(3)
                    .data()
                    .reduce(function (a, b) {
                        return intVal(a) + intVal(b);
                    }, 0);
                // Update footer

                $(api.column(2).footer()).html(helper.number_format_js(total_importe, 0, ',', '.'));
                $(api.column(3).footer()).html(helper.number_format_js(total_facturado, 0, ',', '.'));

            },
            buttons: [
                {
                    text: '<i class="fa fa-file-excel fa-lg" title="Excel" aria-hidden="true"></i> Excel',
                    // title: 'Grupo: ' + $('.ListGrupo option:selected').text() + '     Empresa: ' + $('.ListEmpresa option:selected').text(),
                    extend: 'excelHtml5',
                    exportOptions: {
                        columns: ':not(:eq(4))',
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
                        modulo_pedidos.loadAllData();
                    }
                }
            ],
            initComplete: function (settings, json) {
                $(".btn_refresh").removeClass("dt-button");
            },
        });
    },
    editar_pedido: (id) => {
        localStorage.setItem("accion_pedido", "editar")
        localStorage.setItem("numero_pedido", id);
        window.open(helper.baseUrl + '/Home/NuevoPedido');
    },
    borrarPedido: (id) => {
        if (confirm("Estás seguro de querer borrar este pedido?")) {
            var data = {
                operacion:'DELETE',
                id_pedido: id,
                fecha_pedido: $("#fecha_pedido").val(),
                cod_proveedor: $("#proveedor_codigo").val(),
                cod_acc_proveedor: $("#proveedor_codigo_acceso").val(),
                nom_proveedor: $("#proveedor_nombre").val()
            }
            var url = helper.baseUrl + '/Home/CRUDPedido';

            helper.ajax(url, "POST", data).then(result => {
                if (result.success == 1) {
                    helper.MostrarOk(result.mensaje);
                    modulo_pedidos.loadAllData();
                } else {
                    helper.MostrarError(result.mensaje);
                }
            });
        }
    },
    nuevopedido: () => {
        localStorage.setItem("accion_pedido", "nuevo");
    }


}