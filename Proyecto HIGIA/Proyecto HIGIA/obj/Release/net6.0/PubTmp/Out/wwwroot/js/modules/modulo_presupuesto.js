var modulo_presupuesto = {
    init: () => {
        modulo_presupuesto.getAllCapitulos(null);
        modulo_presupuesto.putEventsOnButtons();
    },
    putEventsOnButtons: () => {

        $('#sel_capitulo').on("change", function (e) {
            e.preventDefault();
            var capitulo = $("#sel_capitulo").val();
            modulo_presupuesto.getAllCapitulos(capitulo);
        })

        $("#btn-save-subcapitulo").on("click", function (e) {
            e.preventDefault();
            modulo_presupuesto.saveImporte();
        })

        $('a[data-toggle="tab"]').on('shown.bs.tab', function (e) {
            e.preventDefault();
            $($.fn.dataTable.tables(true)).DataTable().columns.adjust();
        })
      

    },
    getAllCapitulos: (capitulo) => {
        var url = helper.baseUrl + '/Home/getAllCapitulos';
        var data = {
            capitulo:capitulo
        }
        helper.ajax(url, "POST", data).then(result => {
            if (result.success) {
                var capitulos = JSON.parse(result.capitulos);
                var subcapitulos = JSON.parse(result.subcapitulos);
                modulo_presupuesto.loadDatatables(capitulos, subcapitulos);
                
                if (capitulo == null) {
                    $("#sel_capitulo option").remove();
                    $('#sel_capitulo').append(new Option("", null));
                    capitulos.forEach((capitulo) => {
                        $('#sel_capitulo').append(new Option(capitulo.cod_capitulo + "-" + capitulo.nom_capitulo, capitulo.cod_capitulo));
                    });
                }
               
                $($.fn.dataTable.tables(true)).DataTable().columns.adjust();

            } else {

            }
        });
    },
    loadDatatables: (capitulos,subcapitulos) => {

        modulo_presupuesto.loadDatatableCapitulos(capitulos);
        modulo_presupuesto.loadDatatableSubcapitulos(subcapitulos);
    },

    loadDatatableCapitulos: (capitulos) => {

        if ($.fn.DataTable.isDataTable("#tabla_capitulos")) {
            $("#tabla_capitulos").DataTable().destroy();
        }

        $("#tabla_capitulos").DataTable({
            "data": capitulos,
            "dom": "Bfrtip",
            "columns": [
                { "data": "cod_capitulo" },
                { "data": "nom_capitulo" },
                { "data": "importe", render: $.fn.dataTable.render.number('.', ',', 2, ''), "className": "table-cell text-right" }
            ],
            "autoWidth": false,
            columnDefs: [
                { "width": "20px", "targets": 0 },
                { "width": "80px", "targets": 2 }
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

                // Update footer
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
                },
                {
                    text: '<i class="fa fa-refresh"></i>',
                    className: 'btn btn-info btn-xs text-white btn_refresh',
                    action: function (e, dt, node, config) {
                        modulo_presupuesto.getAllCapitulos(null);
                    }
                }
            ]
        });


    },
    loadDatatableSubcapitulos: (subcapitulos) => {

        if ($.fn.DataTable.isDataTable("#tabla_subcapitulos")) {
            $("#tabla_subcapitulos").DataTable().destroy();
        }

        $("#tabla_subcapitulos").DataTable({
            "data": subcapitulos,
            "dom": "Bfrtip",
            "order": [[0, "asc"]],
            "columns": [
                { "data": "cod_capitulo" },
                { "data": "nom_capitulo" },
                { "data": "cod_subcapitulo" },
                { "data": "nom_subcapitulo" },
                { "data": "importe", render: $.fn.dataTable.render.number('.', ',', 2, ''), "className": "text-right" },
                {
                    render: function (data, type, row) {
                        return `<a href="#" class="btn btn-light btn-sm" onclick='modulo_presupuesto.show_modal(` + JSON.stringify(row) + `);'><i class="fa fa-edit"></i></a>`;
                    }
                }
            ],
            columnDefs: [
                { "targets": [0, 2, 4, 5], "width": "20px" }
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
                        modulo_presupuesto.getAllCapitulos(null);

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

            },
            initComplete: function (settings, json) {
                $(".btn_refresh").removeClass("dt-button");
            }
        });
  

    },
    saveImporte: () => {
        helper.MostrarLoader();
        helper.CerrarModal();
        var url = helper.baseUrl + '/Home/SaveImporte';
        var data = {
            subcapitulo: $("#id_subcapitulo").val(),
            importe: $("#importe").val().toString() //No lo pilla como decimal
        }

        helper.ajax(url, "POST", data).then(result => {
            if (result.success == 1) {
                helper.MostrarOk(result.mensaje);
                modulo_presupuesto.getAllCapitulos(null);
                helper.CerrarModal("#ventana_importe_subcapitulo");
            } else {
                helper.MostrarError(result.mensaje);
            }

            helper.QuitarLoader();
        });
    },
    show_modal: (subcapitulo) => {
        $("#id_subcapitulo").val(subcapitulo.id);
        $("#importe").val(subcapitulo.importe);
        helper.AbrirModal("#ventana_importe_subcapitulo");
    }

}