var modulo_nuevo_pedido = {
    pedido: null,
    subcapitulos: null,
    tabla_subcapitulos_facturacion: null,
    init: () => {
        modulo_nuevo_pedido.loadFlatpickrs();
        modulo_nuevo_pedido.putEventsOnButtons();
        modulo_nuevo_pedido.getPedidoModificado();
        modulo_nuevo_pedido.validateForms();
    },
    putEventsOnButtons: () => {
        $("#btn_nuevo_subcapitulo").on("click", function (e) {
            e.preventDefault();
            modulo_nuevo_pedido.AbrirModalSubCapitulos();
        })

        $("#sel_capitulo").on("click", function (e) {
            e.preventDefault();
            var capitulo = $("#sel_capitulo").val();
            modulo_nuevo_pedido.loadNuevosSubcapitulos(capitulo);
        })

        $("#btn-save-subcapitulo").on("click", function (e) {
            e.preventDefault();
            $(this).prop('disabled', true);
            var subcapitulo = $("#id_subcapitulo").val();
            var importe = $("#importe").val();
            modulo_nuevo_pedido.crud_subcapitulo("UPDATE", subcapitulo, importe);
            helper.CerrarModal("#ventana_importe_subcapitulo");
            $(this).prop('disabled', false);

        })

        $("#proveedor_codigo").on("click", function (e) {
            e.preventDefault();
            modulo_nuevo_pedido.AbrirModalProveedores();
        })

        $("#btn_nueva_factura").on("click", function (e) {
            e.preventDefault();
            $("#num_factura").val("");
            $("#num_expediente").val("");
            $("#fech_factura").val("");
            modulo_nuevo_pedido.AbrirModalFactura("nuevo");
        })

        $("#btn_guardar_pedido").on("click", async function (e) {
            e.preventDefault();
            $(this).prop('disabled', true);
            await modulo_nuevo_pedido.guardarPedido();
            $(this).prop('disabled', false);

        });

        $("#btn_subir_archivo").on("click", function (e) {
            e.preventDefault();
            modulo_nuevo_pedido.subirArchivos();
        })

    },
    loadFlatpickrs: () => {

        $("#fecha_pedido").flatpickr({
            locale: helper.locale,
            maxDate: "today",
            dateFormat: "d/m/Y",
            allowInput: true
        });


        $("#fech_factura").flatpickr({
            locale: helper.locale,
            maxDate: "today",
            dateFormat: "d/m/Y",
            allowInput: true
        });

    },
    getPedidoModificado: () => {
        var accion_pedido = localStorage.getItem("accion_pedido");
        var pedido = localStorage.getItem("numero_pedido");
        if (accion_pedido == "editar" && pedido != "") {
            $("#id_pedido").val(pedido);
            modulo_nuevo_pedido.pedido = pedido;
        } else {
            modulo_nuevo_pedido.pedido = $("#id_pedido").val();
        }
        modulo_nuevo_pedido.loadPedidoModificado();

    },
    loadPedidoModificado: () => {
        var data = {
            pedido: modulo_nuevo_pedido.pedido
        }
        var url = helper.baseUrl + '/Home/getPedido';
        helper.ajax(url, "POST", data).then(result => {
            if (result.success) {
                var pedido = JSON.parse(result.pedido)[0];
                var subcapitulos = JSON.parse(result.subcapitulos);
                modulo_nuevo_pedido.subcapitulos = subcapitulos;
                var facturas = JSON.parse(result.facturas);
                var archivos = JSON.parse(result.archivos);

                $("#num_pedido").val(pedido.num_pedido);
                if (pedido.fecha_pedido != null && pedido.fecha_pedido != "") {
                    $("#fecha_pedido").flatpickr({
                        "defaultDate": pedido.fecha_pedido,
                        dateFormat: "d/m/Y",
                        allowInput: true
                    });
                }

                if (pedido.cod_proveedor != null && pedido.cod_proveedor != "") {
                    $("#proveedor_codigo").val(pedido.cod_proveedor);
                    $("#proveedor_codigo_acceso").val(pedido.cod_acc_proveedor);
                    $("#proveedor_nombre").val(pedido.nom_proveedor);
                }

                modulo_nuevo_pedido.loadDatatableSubCapitulos(subcapitulos);
                modulo_nuevo_pedido.loadDatatableFacturas(facturas);
                modulo_nuevo_pedido.loadDatatableArchivos(archivos);

            }
        });
    },
    loadDatatableSubCapitulos: (subcapitulos) => {
        if ($.fn.DataTable.isDataTable("#tabla_subcapitulos")) {
            $("#tabla_subcapitulos").DataTable().destroy();
        }

        $("#tabla_subcapitulos").DataTable({
            "data": subcapitulos,
            "dom": "Bfrtip",
            "columns": [
                { "data": "cod_capitulo" },
                { "data": "nom_capitulo" },
                { "data": "cod_subcapitulo" },
                { "data": "nom_subcapitulo" },
                { "data": "importe", render: $.fn.dataTable.render.number('.', ',', 2, ''), "className": "table-cell text-right" },
                { "data": "importe_facturacion", render: $.fn.dataTable.render.number('.', ',', 2, ''), "className": "table-cell-facturado text-right" },
                { "data": "pendiente", render: $.fn.dataTable.render.number('.', ',', 2, ''), "className": "table-cell text-right" },
                {
                    render: function (data, type, row) {
                        return `<a href="#" class="btn btn-light btn-sm" onclick="modulo_nuevo_pedido.editar_subcapitulo(` + row.id_subcapitulo + `,` + row.importe + `);"><i class="fa fa-edit"></i></a>
                            <a href="#" class="btn btn-light btn-sm" onclick ="modulo_nuevo_pedido.crud_subcapitulo('DELETE',` + row.id_subcapitulo + `);"><i class="fa fa-trash"></i></a>`;
                    }
                }
            ],
            "paging": true,
            stateSave: false,
            "language": {
                "url": "../lib/datatables.net/lang/Spanish.json",
                "decimal": ",",
                "thousands": "."
            }, "columnDefs": [
                { "width": "5%", "targets": [0, 2] },
                { "width": "8%", "targets": [4, 5, 6] }
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

                total_facturado = api
                    .column(5)
                    .data()
                    .reduce(function (a, b) {
                        return intVal(a) + intVal(b);
                    }, 0);

                total_pendiente = api
                    .column(6)
                    .data()
                    .reduce(function (a, b) {
                        return intVal(a) + intVal(b);
                    }, 0);
                // Update footer

                $(api.column(4).footer()).html(helper.number_format_js(total_importe, 0, ',', '.'));
                $(api.column(5).footer()).html(helper.number_format_js(total_facturado, 0, ',', '.'));
                $(api.column(6).footer()).html(helper.number_format_js(total_pendiente, 0, ',', '.'));

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
                        modulo_nuevo_pedido.loadPedidoModificado();
                    }
                }
            ],
            initComplete: function (settings, json) {
                $(".btn_refresh").removeClass("dt-button");
            },
        });
    },
    loadDatatableFacturas: (facturas) => {
        if ($.fn.DataTable.isDataTable("#tabla_facturas")) {
            $("#tabla_facturas").DataTable().destroy();
        }

        $("#tabla_facturas").DataTable({
            "data": facturas,
            "dom": "Bfrtip",
            "columns": [
                { "data": "num_factura" },
                { "data": "fecha_factura" },
                { "data": "importe", render: $.fn.dataTable.render.number('.', ',', 2, ''), "className": "text-right" },
                {
                    render: function (data, type, row) {
                        return `<a href="#" class="btn btn-light btn-sm" onclick='modulo_nuevo_pedido.editar_factura(` + JSON.stringify(row) + `);'><i class="fa fa-edit"></i></a>
                                <a href="#" class="btn btn-light btn-sm" onclick='modulo_nuevo_pedido.borrarfactura(` + JSON.stringify(row) + `);'><i class="fa fa-trash"></i></a>`;
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
                    .column(2)
                    .data()
                    .reduce(function (a, b) {
                        return intVal(a) + intVal(b);
                    }, 0);


                $(api.column(2).footer()).html(helper.number_format_js(total_importe, 0, ',', '.'));

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
                }/*,
                {
                    text: '<i class="fa fa-refresh"></i>',
                    className: 'btn btn-info btn-xs text-white btn_refresh',
                    action: function (e, dt, node, config) {
                        modulo_nuevo_pedido.loadPedidoModificado();
                    }
                }*/
            ],
            initComplete: function (settings, json) {
                $(".btn_refresh").removeClass("dt-button");
            }
        });
    },
    loadDatatableArchivos: (archivos) => {
        if ($.fn.DataTable.isDataTable("#tabla_archivos")) {
            $("#tabla_archivos").DataTable().destroy();
        }

        $("#tabla_archivos").DataTable({
            "data": archivos,
            "dom": "frtip",
            "columns": [
                { "data": "name" },
                {
                    render: function (data, type, row) {
                        return `<a href="#" class="btn btn-light btn-sm" onclick='modulo_nuevo_pedido.download_file("` + row.name + `");'><i class="fa fa-download"></i></a>
                                <a href="#" class="btn btn-light btn-sm" onclick='modulo_nuevo_pedido.delete_file("` + row.name + `");'><i class="fa fa-trash"></i></a>`;
                    }
                }
            ],
            "paging": true,
            stateSave: false,
            "language": {
                "url": "../lib/datatables.net/lang/Spanish.json",
                "decimal": ",",
                "thousands": "."
            }
        });
    },
    AbrirModalSubCapitulos: () => {
        var url = helper.baseUrl + '/Home/getAllCapitulos';

        helper.ajax(url, "GET").then(result => {
            if (result.success) {
                var capitulos = JSON.parse(result.capitulos);
                $("#sel_capitulo option").remove();
                $('#sel_capitulo').append(new Option("", null));
                capitulos.forEach((capitulo) => {
                    $('#sel_capitulo').append(new Option(capitulo.cod_capitulo + "-" + capitulo.nom_capitulo, capitulo.cod_capitulo));
                });
                modulo_nuevo_pedido.loadNuevosSubcapitulos(null);
            }
        });
    },
    AbrirModalProveedores: async () => {
        helper.MostrarLoader();
        var url = helper.baseUrl + '/Home/getProveedores';

        if ($.fn.DataTable.isDataTable("#tabla_add_proveedor")) {
            $("#tabla_add_proveedor").DataTable().clear().draw();
            $("#tabla_add_proveedor").DataTable().destroy();
        }

        await helper.ajax(url, "GET").then(result => {
            if (result.success) {
                var proveedores = JSON.parse(result.proveedores);

                if ($.fn.DataTable.isDataTable("#tabla_add_proveedor")) {
                    $("#tabla_add_proveedor").DataTable().destroy();
                }

                $("#tabla_add_proveedor").DataTable({
                    "data": proveedores,
                    "dom": "Bfrtip",
                    "columns": [
                        { "data": "CODIGO" },
                        { "data": "NOMBRE" },
                        { "data": "CODIGO_ACCESO" },
                        {
                            render: function (data, type, row) {
                                return `<a href="#" class="btn btn-light btn-sm" onclick='modulo_nuevo_pedido.add_proveedor(` + JSON.stringify(row) + `);'><i class="fa fa-plus"></i></a>`;
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
                    ]
                });
            }
        });
        helper.QuitarLoader();
        helper.AbrirModal("#ventana_add_proveedor");
    },
    AbrirModalFactura: async (accion, factura = null) => {
        var data_table = null;
        $("#identi_pedido").val(modulo_nuevo_pedido.pedido);
        $("#modulo").val("pedidos");
        if (accion == "nuevo") {
            data_table = modulo_nuevo_pedido.subcapitulos;
            if ($.fn.DataTable.isDataTable("#tabla_factura_subcapitulo")) {
                $("#tabla_factura_subcapitulo").DataTable().destroy();
            }
            $("#action_factura").val("INSERT");
        } else if (accion == "editar") {
            var url = helper.baseUrl + '/Home/getSubcapitulosFactura';

            var data = {
                factura: factura.id
            }

            await helper.ajax(url, "POST", data).then(result => {
                if (result.success) {
                    data_table = JSON.parse(result.subcapitulos);
                }
            });
            $("#action_factura").val("UPDATE");

            $("#fech_factura").flatpickr({
                "defaultDate": factura.fecha_factura,
                dateFormat: "d/m/Y",
                allowInput: true
            });
        }

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
                { "data": "facturado", render: $.fn.dataTable.render.number('.', ',', 2, ''), "className": "table-cell text-right" },
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

    },
    
    loadNuevosSubcapitulos: (capitulo) => {

        var url = helper.baseUrl + '/Home/getNuevosSubcapitulos';

        var data = {
            capitulo: capitulo,
            pedido: modulo_nuevo_pedido.pedido
        }

        helper.ajax(url, "POST", data).then(result => {
            if (result.success) {
                var subcapitulos = JSON.parse(result.subcapitulos);

                if ($.fn.DataTable.isDataTable("#tabla_add_subcapitulo")) {
                    $("#tabla_add_subcapitulo").DataTable().destroy();
                }

                $("#tabla_add_subcapitulo").DataTable({
                    "data": subcapitulos,
                    "dom": "Bfrtip",
                    "columns": [
                        { "data": "cod_capitulo" },
                        { "data": "nom_capitulo" },
                        { "data": "cod_subcapitulo" },
                        { "data": "nom_subcapitulo" },
                        {
                            render: function (data, type, row) {
                                return `<a href="#" class="btn btn-light btn-sm" onclick="modulo_nuevo_pedido.crud_subcapitulo('INSERT',` + row.id + `);"><i class="fa fa-plus"></i></a>`;
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
                    ]
                });

                helper.AbrirModal("#ventana_add_subcapitulo");

            } else {
                helper.MostrarAlertaError('info', result.mensaje);
            }

        });

    },
    guardarPedido: async () => {

        if ($("#form-pedido").valid()) {
            var data = {
                operacion: "UPDATE",
                id_pedido: modulo_nuevo_pedido.pedido,
                fecha_pedido: $("#fecha_pedido").val(),
                cod_proveedor: $("#proveedor_codigo").val(),
                cod_acc_proveedor: helper.addLeadingZeros($("#proveedor_codigo_acceso").val()),
                nom_proveedor: $("#proveedor_nombre").val()
            }
            var url = helper.baseUrl + '/Home/CRUDPedido';

            helper.ajax(url, "POST", data).then(result => {
                if (result.success == 1) {
                    helper.MostrarOk(result.mensaje);

                } else {
                    helper.MostrarError(result.mensaje);
                }
            });
        } else {
            helper.MostrarError("Faltan Datos para guardar el pedido");
        }


    },
    
    download_file: (name) => {
        helper.MostrarLoader();

        var data = {
            name: name,
            pedido: modulo_nuevo_pedido.pedido
        }
        var url = helper.baseUrl + "/Home/downloadFile";

        $.ajax({
            type: 'POST',
            url: url,
            xhrFields: {
                responseType: 'blob'
            },
            data: data,
            success: function (json) {
                var link = document.createElement('a');
                link.href = window.URL.createObjectURL(json);
                link.download = name;
                link.click();
                helper.QuitarLoader();

            },
            error: function () {
                console.log("Error");
            }
        });


    },
    delete_file: (name) => {
        if (confirm("Estás segur@ de querer borrar este archivo adjunto??")) {
            var data = {
                name: name,
                pedido: modulo_nuevo_pedido.pedido
            }
            var url = helper.baseUrl + "/Home/deleteFile";

            helper.ajax(url, "POST", data).then(result => {
                if (result.success) {
                    helper.MostrarOk(result.mensaje);
                    modulo_nuevo_pedido.loadPedidoModificado();
                } else {
                    helper.MostrarError(result.mensaje);
                }
            });
        }


    },
    editar_factura: (factura) => {
        $("#identi_factura").val(factura.id);
        $("#num_factura").val(factura.num_factura);
        $("#num_expediente").val(factura.num_expediente);
        $("#fech_factura").val(factura.fecha_factura);
        modulo_nuevo_pedido.AbrirModalFactura("editar", factura);
        // helper.AbrirModal("#ventana_add_factura");

    },
    borrarfactura: (factura) => {
        if (confirm("¿Estás segur@ de querer borrar esta factura?")) {
            var data = {
                operacion: 'DELETE',
                id_factura: factura.id
            }

            var url = helper.baseUrl + '/Home/CRUDFactura';

            helper.ajax(url, "POST", data).then(result => {
                if (result.success == 1) {
                    helper.MostrarOk(result.mensaje);
                    modulo_nuevo_pedido.loadPedidoModificado();

                } else {
                    helper.MostrarError(result.mensaje);
                }
            });
        }
    },
    add_proveedor: (proveedor) => {
        console.log(proveedor);
        $("#proveedor_codigo").val(proveedor.CODIGO);
        $("#proveedor_codigo_acceso").val(proveedor.CODIGO_ACCESO);
        $("#proveedor_nombre").val(proveedor.NOMBRE);
        helper.CerrarModal("#ventana_add_proveedor");
    },
    editar_subcapitulo: (subcapitulo, importe) => {
        $("#id_subcapitulo").val(subcapitulo);
        $("#importe").val(importe);
        helper.AbrirModal("#ventana_importe_subcapitulo");
    },
    crud_subcapitulo: async (operacion, subcapitulo, importe = null) => {

        if (importe != null) {
            importe = importe.toString(); //Lo paso a string para que pueda leer los decimales en c#
        }

        var url = helper.baseUrl + '/Home/CRUDSubcapituloPedido';
        var data = {
            subcapitulo: subcapitulo,
            pedido: modulo_nuevo_pedido.pedido,
            operacion: operacion,
            importe: importe
        }

        await helper.ajax(url, "POST", data).then(result => {
            if (result.success == 1) {
                modulo_nuevo_pedido.loadPedidoModificado();
                helper.CerrarModal("#ventana_importe_subcapitulo");

                if (operacion == "INSERT") {
                    modulo_nuevo_pedido.loadNuevosSubcapitulos($("#sel_capitulo").val());
                } else {
                    helper.MostrarOk(result.mensaje);
                }

            } else {
                helper.MostrarError(result.mensaje);
            }
        });
    },
    subirArchivos: () => {
        var files = $("#inp_files").get(0).files;
        var fileData = new FormData();

        for (var i = 0; i < files.length; i++) {
            fileData.append("files", files[i]);
        }

        fileData.append("pedido", modulo_nuevo_pedido.pedido);

        $.ajax({
            type: "POST",
            url: helper.baseUrl + "/Home/FileUpload",
            dataType: "json",
            contentType: false, // Not to set any content header
            processData: false, // Not to process data
            data: fileData,
            success: function (result, status, xhr) {
                if (result.success) {
                    helper.MostrarOk("Se adjuntó el archivo correctamente");
                    modulo_nuevo_pedido.loadPedidoModificado();
                }
            },
            error: function (xhr, status, error) {
            }
        });

    },
    validateForms: () => {
        $("#form-pedido").validate({
            rules: {
                num_pedido: {
                    required: true
                },
                fecha_pedido: {
                    required: true,
                },
                proveedor_codigo: {
                    required: true
                }
            },
            errorClass: "error fail-alert",
            messages: {
                num_pedido: {
                    required: "El número de pedido es obligatorio"
                },
                fecha_pedido: {
                    required: "La fecha del pedido es obligatoria"
                }, proveedor_codigo: {
                    required: "El proveedor es obligatorio"
                },
            }
        });


    }
}