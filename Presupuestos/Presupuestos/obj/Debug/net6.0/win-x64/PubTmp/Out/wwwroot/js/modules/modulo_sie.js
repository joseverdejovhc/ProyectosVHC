var modulo_sie = {
    permiso:0,
    init: (permiso) => {
        modulo_sie.permiso = permiso;
        permisos.ocultar_permisos(permiso);

        modulo_sie.loadAllData();

        modulo_sie.putEventsOnButtons();
    },
    putEventsOnButtons: () => {
        $(".filtrar").on("change", async function (e) {
            e.preventDefault();
            helper.MostrarLoader();

            var aprobado = await modulo_sie.checkPresupuestoAprobado();

            if (!aprobado && modulo_sie.permiso==1) {
                $("#btn_cargar_sie").show();
                
            } else {
                $("#btn_cargar_sie").hide();
            }
            await modulo_sie.loadDatatableCentrosCoste();
            await modulo_sie.loadDatatableCuentasContables();
            modulo_sie.getUltimaCarga();

            helper.QuitarLoader();

        });

        $("#btn_cargar_sie").on("click", async function (e) {
            e.preventDefault();
            helper.MostrarLoader();
            await modulo_sie.loadDatatableCentrosCoste("cargar");
            modulo_sie.getUltimaCarga();
            helper.QuitarLoader();

            modulo_sie.loadDatatableCuentasContables("cargar");

        });

    },
    loadAllData: async () => {
        var url = helper.baseUrl + '/Home/getUltimoPresuprodAprobado';


        helper.ajax(url, "GET").then(async (result) => {
            if (result.success) {
                var ejercicio = result.ejercicio;
                $(".filtrar").val(ejercicio);

                var aprobado = await modulo_sie.checkPresupuestoAprobado();


                if (!aprobado && modulo_sie.permiso == 1) {
                    $("#btn_cargar_sie").show();

                } else {
                    $("#btn_cargar_sie").hide();
                }

                modulo_sie.getUltimaCarga();
                modulo_sie.loadDatatableCentrosCoste();
                modulo_sie.loadDatatableCuentasContables();
            }
        });
    },
    checkPresupuestoAprobado: async () => {
        var url = helper.baseUrl + '/Home/comprobarPresupuestoAprobado';

        var data = {
            ejercicio: $("#inpt_ejercicio").val()
        }

        var aprobado = false;

        await helper.ajax(url, "POST", data).then(result => {
            aprobado = result.aprobado;

            if (aprobado) {

            }
        });

        return aprobado;
    },
    getUltimaCarga: async () => {
        var url = helper.baseUrl + '/Home/getSIEUltimaCarga';

        var data = {
            ejercicio: $("#inpt_ejercicio").val()
        }


        await helper.ajax(url, "POST", data).then(result => {

            if (result.success) {
                var cargadapor = result.cargadapor;
                if (cargadapor != "") {
                    $("#sp_info_sie").text("Última carga: " + cargadapor);

                } else {
                    $("#sp_info_sie").text("");
                }
            } else {
                $("#sp_info_sie").text("");
            }
        });

    },
    loadDatatableCentrosCoste: async (operacion = '') => {
        var url = helper.baseUrl + '/Home/GetSIECentrosCoste';

        var data = {
            ejercicio: $("#inpt_ejercicio").val(),
            clasificacion: $("#sel_clasificacion").val(),
            operacion: operacion
        }

        await helper.ajax(url, "POST", data).then(result => {
            if (result.success) {
                var centros_coste = JSON.parse(result.centros_coste);

                if ($.fn.DataTable.isDataTable('#tabla_centro_coste')) {
                    $("#tabla_centro_coste").DataTable().destroy();
                }

                if (operacion == 'cargar') {
                    helper.MostrarOk("#alerta", "SIE Cargado Correctamente");
                }

                $('#tabla_centro_coste').DataTable({
                    "data": centros_coste,
                    "columns": [
                        { "data": "codigo" },
                        { "data": "nom_centro_coste" },
                        { "data": "clasificacion" },
                        { "data": "importe", render: $.fn.dataTable.render.number('.', ',', 0, '') }
                    ],
                    "paging": false,
                    stateSave: false,
                    "language": {
                        "url": "../lib/datatables.net/lang/Spanish.json",
                        "decimal": ",",
                        "thousands": "."
                    },
                    columnDefs: [{ //Centro el contenido de las n últimas columnas
                        className:"dt-body-right", "targets": 3
                    }],
                    dom: 'Bfrtip',
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
                        total_gasto = api
                            .column(3)
                            .data()
                            .reduce(function (a, b) {
                                return intVal(a) + intVal(b);
                            }, 0);


                        $(api.column(3).footer()).html(
                            helper.number_format_js(total_gasto, 0, ',', '.')
                        );

                    }
                });

            } else {
                helper.MostrarError("#alerta", result.mensaje);
            }

        });
    },
    loadDatatableCuentasContables: async (operacion) => {
        var url = helper.baseUrl + '/Home/GetSIECuentasContables';

        var data = {
            ejercicio: $("#inpt_ejercicio").val(),
            clasificacion: $("#sel_clasificacion").val(),
            operacion: operacion
        }

        await helper.ajax(url, "POST", data).then(result => {
            if (result.success) {
                var centros_coste = JSON.parse(result.centros_coste);

                if ($.fn.DataTable.isDataTable('#tabla_cuenta_contable')) {
                    $("#tabla_cuenta_contable").DataTable().destroy();
                }

                $('#tabla_cuenta_contable').DataTable({
                    "data": centros_coste,
                    "columns": [
                        { "data": "codigo" },
                        { "data": "nom_centro_coste" },
                        { "data": "cuenta" },
                        { "data": "nombre_cuenta" },
                        { "data": "importe", render: $.fn.dataTable.render.number('.', ',', 0, '') }
                    ],
                    "paging": true,
                    searching: true,
                    stateSave: false,
                    dom: 'Bfrtip',
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
                    
                    columnDefs: [{ //Centro el contenido de las n últimas columnas
                        className:"dt-body-right", "targets": 4
                    }],
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


                        total_presupuesto = api
                            .column(4)
                            .data()
                            .reduce(function (a, b) {
                                return intVal(a) + intVal(b);
                            }, 0);

                        total_presupuesto = helper.number_format_js(Math.round(total_presupuesto), 0, ',', '.');
                        // Update footer
                        $(api.column(4).footer()).html(
                            total_presupuesto
                        );
                    }
                });


            }
        });
    }
}