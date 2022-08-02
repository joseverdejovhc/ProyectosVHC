
var modulo_margen_bruto = {
    total_cdf: 0,
    porcentaje: 0,
    cdf_total_repartir: 0,
    dt_margen_bruto: null,
    tabla_agrupacion: null,
    init: async () => {
        helper.MostrarLoader();
        modulo_margen_bruto.loadAllData();
        modulo_margen_bruto.putEventsOnButtons();
        await modulo_margen_bruto.loadSelectores();
        helper.QuitarLoader();
    },
    putEventsOnButtons: () => {

        $(".filtrar").on("change", async function (e) {
            e.preventDefault();
            var ejercicio = $("#inpt_ejercicio").val();
            var aprobado = await modulo_margen_bruto.checkPresupuestoAprobado(ejercicio);

            if (aprobado) {
                $("#btn_cargar_presupuesto").hide();
            } else {
                $("#btn_cargar_presupuesto").show();
            }

            modulo_margen_bruto.filtrar();
        });

        $("#btn_cargar_presupuesto").on("click", function (e) {
            e.preventDefault();
            modulo_margen_bruto.getPresupuestosAprobados();
            // helper.MostrarLoader();
            // modulo_margen_bruto.filtrar();
        });

        $("#btn_aprobar_cdf").on("click", function (e) {
            e.preventDefault();
            modulo_margen_bruto.comprobarPresupuestosYCargarCDF();
        })

        $("#btn_cdf_unitario").on("click", function (e) {
            e.preventDefault();
            helper.AbrirModal("#ventana_calculo_cdf");
        })

        $("#inpt_ejercicio_cdf").on("change",async function (e) {
            e.preventDefault();
            var ejercicio = $("#inpt_ejercicio_cdf").val();
            var aprobado = await modulo_margen_bruto.checkPresupuestoAprobado(ejercicio);

            if (aprobado) {
                $("#btn_aprobar_cdf").hide();
            } else {
                $("#btn_aprobar_cdf").show();
            }

            modulo_margen_bruto.loadUltimaCargaCDF();
            modulo_margen_bruto.loadDatatable_cdf_planta();
            modulo_margen_bruto.loadDatatable_cdf_departamentos();

        })

        $("#porcentaje_cdf").on("change", function (e) {
            e.preventDefault();
            modulo_margen_bruto.updateInputsModalCDF();
        })
    },
    loadAllData: async () => {


       
        var url = helper.baseUrl + '/Home/getUltimoPresuprodAprobado';       
        helper.ajax(url, "GET").then(async (result) => {
            if (result.success) {
                var ejercicio = result.ejercicio;
                $(".filtrar").val(ejercicio);

                var aprobado = await modulo_margen_bruto.checkPresupuestoAprobado(ejercicio);

                if (aprobado) {
                    $("#btn_cargar_presupuesto").hide();
                } else {
                    $("#btn_cargar_presupuesto").show();
                }

                modulo_margen_bruto.filtrar();
            }
        });
    },
    checkPresupuestoAprobado: async (ejercicio) => {
        var url = helper.baseUrl + '/Home/comprobarPresupuestoAprobado';

        var data = {
            ejercicio: ejercicio
        }

        var aprobado = false;

        await helper.ajax(url, "POST", data).then(result => {
            aprobado = result.aprobado;
        });

        return aprobado;
    },
    loadSelectores: async () => {
        var url = helper.baseUrl + '/Home/GetPlantasYLineas';
        helper.ajax(url, "GET").then(result => {
            if (result.success) {
                var plantas = JSON.parse(result.plantas);
                var lineas = JSON.parse(result.lineas);
                var sel_lineas = $("#sel_linea");
                var sel_plantas = $("#sel_planta");

                plantas.forEach((planta) => {
                    sel_plantas.append('<option value="' + planta.codigo + '">' + planta.codigo + ' - ' + planta.nombre + '</option>')
                })

                lineas.forEach((linea) => {
                    sel_lineas.append('<option value="' + linea.codigo + '">' + linea.codigo + ' - ' + linea.nombre + '</option>')
                })

            }

        });
    },
    loadDatatable_cdf_planta: () => {
        var url = helper.baseUrl + '/Home/GetKilosProduccionPlanta';
        $("#span_error").text('');
        var data = {
            ejercicio: $("#inpt_ejercicio_cdf").val()
        }

        helper.ajax(url, "POST", data).then(result => {
            if (result.success) {
                var plantas = JSON.parse(result.plantas);

                if ($.fn.DataTable.isDataTable('#tabla_cdf_agrupacion')) {
                    $("#tabla_cdf_agrupacion").DataTable().destroy();
                }

                modulo_margen_bruto.tabla_agrupacion = $('#tabla_cdf_agrupacion').DataTable({
                    "data": plantas,
                    "columns": [
                        { "data": "planta" },
                        { "data": "porcentaje_cdf" },
                        { "data": "gasto_repartir", render: $.fn.dataTable.render.number('.', ',', 2, '')}, 
                        { "data": "prevision_kilos", render: $.fn.dataTable.render.number('.', ',', 0, '')},
                        { "data": "cdf_kilos", render: $.fn.dataTable.render.number('.', ',', 4, '') },
                        {
                            render: function (data, type, row, meta) {
                                return `<a href="#" class="btn_accesos btn btn-light btn-sm" onclick='modulo_margen_bruto.modificar_cdf_planta(` + JSON.stringify(row) + `,`+meta.row+`);  '><i class="fa fa-edit"></i></a>`;
                            }
                        }
                    ],
                    stateSave: false,
                    "language": {
                        "url": "../lib/datatables.net/lang/Spanish.json",
                        "decimal": ",",
                        "thousands": "."
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
                                        if (data != "") {
                                            var data = String(data);
                                            if ($.isNumeric(data.replace(/[.]/g, '').replace(/[,]/g, '.'))) {
                                                return data.replace(/[.]/g, '').replace(/[,]/g, '.');
                                            } else {
                                                // Elimina espacios en blanco y etiquetas html que añade en los Template Field del Gridview
                                                return data.replace(/&nbsp;/g, '').replace(/<.*?\>/, '').replace(/<\/.*?>/, '');
                                            }
                                        } else {
                                            return "";
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
                            .column(3)
                            .data()
                            .reduce(function (a, b) {
                                return intVal(a) + intVal(b);
                            }, 0);

                        total_gasto_repartir = api
                            .column(2)
                            .data()
                            .reduce(function (a, b) {
                                return intVal(a) + intVal(b);
                            }, 0);


                        modulo_margen_bruto.total_cdf = api
                            .column(1)
                            .data()
                            .reduce(function (a, b) {
                                return intVal(a) + intVal(b);
                            }, 0);

                        $(api.column(1).footer()).html(
                            modulo_margen_bruto.total_cdf
                        );

                        modulo_margen_bruto.porcentaje = modulo_margen_bruto.total_cdf;

                        $(api.column(2).footer()).html(
                            helper.number_format_js(total_gasto_repartir, 0, ',', '.')
                        );

                        // Update footer
                        $(api.column(3).footer()).html(
                            helper.number_format_js(total_kilos, 0, ',', '.')

                        );
                    }
                });

               /* modulo_margen_bruto.tabla_agrupacion.MakeCellsEditable({
                    "onUpdate": modulo_margen_bruto.updateTable,
                    "inputCss": 'form-control form-control-sm col-6 float-left',
                    "columns": [1],
                    "allowNulls": {
                        "columns": [],
                        "errorClass": 'border-danger'
                    },
                    "inputTypes": [
                        {
                            "column": 1,
                            "type": "number",
                            "options": { "max": 100 }
                        }
                    ],
                    "confirmationButton": {
                        "confirmCss": 'btn btn-sm btn-success col-12 nivel1',
                        "cancelCss": 'btn btn-sm btn-warning col-12'
                    }
                });*/
            } else {
                $("#span_error").text(result.mensaje);
                $("#span_error").addClass("text-danger");
            }

        });
    },
    modificar_cdf_planta: (planta,row) => {
        console.log(row);
        $("#row_datatable").val(row);
        $("#planta_modal").val(planta.planta);
        $("#prevision_produccion").val(planta.prevision_kilos);
        $("#porcentaje_cdf").val(modulo_margen_bruto.tabla_agrupacion.cell(row, 1).data());
        $("#gasto_repartir").val(modulo_margen_bruto.tabla_agrupacion.cell(row, 2).data());
        $("#cdf_kilo").val(modulo_margen_bruto.tabla_agrupacion.cell(row, 4).data());

        helper.AbrirModal("#ventana_porcentaje_cdf");
    },

    updateInputsModalCDF:() =>  {
        var valor = parseInt($("#porcentaje_cdf").val());
        var cell_row = $("#row_datatable").val();
        modulo_margen_bruto.tabla_agrupacion.cell(cell_row, 1).data(0);

        modulo_margen_bruto.tabla_agrupacion.draw();

        if (modulo_margen_bruto.total_cdf < modulo_margen_bruto.porcentaje) {
            modulo_margen_bruto.porcentaje = modulo_margen_bruto.total_cdf;
        }
        
        if ((modulo_margen_bruto.porcentaje + valor) > 100) {            
            //valor = 100 - modulo_margen_bruto.porcentaje;
            $("#porcentaje_cdf").val(valor)

            modulo_margen_bruto.porcentaje = 100;
        } else {
            modulo_margen_bruto.porcentaje += valor;
        }

        modulo_margen_bruto.tabla_agrupacion.cell(cell_row, 1).data(valor);


        modulo_margen_bruto.calcularGastoRepartirPlanta(valor);
        modulo_margen_bruto.tabla_agrupacion.draw();


    },
    calcularGastoRepartirPlanta: (porcentaje) => {
        //modulo_margen_bruto.tabla_agrupacion = $("#tabla_cdf_agrupacion").DataTable();
        var cell_row = $("#row_datatable").val();
        var gasto_repartido = Math.round(modulo_margen_bruto.cdf_total_repartir * (porcentaje / 100));
        $("#gasto_repartir").val(gasto_repartido);
        modulo_margen_bruto.tabla_agrupacion.cell(cell_row, 2).data(gasto_repartido);
        /*Calculo el cdf por kg*/

        //Cojo el valor de los kilos de esta fila y hago el cálculo
        cell_column = 3;
        var kilos = $("#prevision_produccion").val();
        var cdf_kg = (gasto_repartido / kilos);

        //Hago el set de la celda con el valor
        $("#cdf_kilo").val(cdf_kg);
        modulo_margen_bruto.tabla_agrupacion.cell(cell_row, 4).data(cdf_kg);

    },
    updateTable: (updatedCell, updatedRow, oldValue) => {
        var id_usuario = updatedRow.find(".id").text();
        var valor = parseInt(updatedCell.data());
        // modulo_margen_bruto.calcularPorcentajeTotal();

        if (modulo_margen_bruto.total_cdf < modulo_margen_bruto.porcentaje) {
            modulo_margen_bruto.porcentaje = modulo_margen_bruto.total_cdf;
        }

        if ((modulo_margen_bruto.porcentaje + valor) > 100) {
            var cell_column = updatedCell[0][0]["column"];
            var cell_row = updatedCell[0][0]["row"];
            var cell = modulo_margen_bruto.tabla_agrupacion.cell(cell_row, cell_column);
            valor = 100 - modulo_margen_bruto.porcentaje;
            cell.data(valor)
            modulo_margen_bruto.porcentaje = 100;
            modulo_margen_bruto.total_cdf = 100;
        } else {
            modulo_margen_bruto.porcentaje += valor;
        }


        modulo_margen_bruto.calcularGastoRepartir(updatedCell, valor);
    },
    calcularGastoRepartir: (updatedCell, porcentaje) => {
        //modulo_margen_bruto.tabla_agrupacion = $("#tabla_cdf_agrupacion").DataTable();
        var gasto_repartido = Math.round(modulo_margen_bruto.cdf_total_repartir * (porcentaje / 100));
        var cell_column = 2;
        var cell_row = updatedCell[0][0]["row"];
        modulo_margen_bruto.tabla_agrupacion.cell(cell_row, cell_column).data(gasto_repartido);

        /*Calculo el cdf por kg*/

        //Cojo el valor de los kilos de esta fila y hago el cálculo
        cell_column = 3;
        var kilos = modulo_margen_bruto.tabla_agrupacion.cell(cell_row, cell_column).data();
        var cdf_kg = (gasto_repartido / kilos);

        //Hago el set de la celda con el valor
        cell_column = 4;
        modulo_margen_bruto.tabla_agrupacion.cell(cell_row, cell_column).data(cdf_kg);

    },
    comprobarPresupuestosYCargarCDF: () => {
        var url = helper.baseUrl + '/Home/getPresupuestoAprobados';

        var data = {
            ejercicio: $("#inpt_ejercicio_cdf").val()
        }

        helper.ajax(url, "POST", data).then(result => {
            if (result.success) {
                if (result.prepto_ventas == null) {
                    helper.MostrarError("#alerta", "El presupuesto de ventas no está cerrado");

                } else if (result.sie == null) {
                    helper.MostrarError("#alerta", "El sie no está cargado");

                } else {
                    modulo_margen_bruto.cargarCDF();

                }

            } else {
            }
        });

    },
    cargarCDF: () => {
        if (modulo_margen_bruto.total_cdf == 100) {
            var rows = modulo_margen_bruto.tabla_agrupacion.rows().data();

            var plantas = [];

            for (let i = 0; i < rows.length; i++) {
                rows[i].cdf_kilos = parseFloat(rows[i].cdf_kilos);
                plantas.push(rows[i]);
            }


            var data = {
                "plantas": JSON.stringify(plantas),
                "ejercicio": $("#inpt_ejercicio_cdf").val()
            }
            var url = helper.baseUrl + '/Home/cargarCDFUnitario';

            helper.ajax(url, "POST", data).then(result => {
                if (result.success) {
                    helper.CerrarModal("#ventana_calculo_cdf");
                    helper.MostrarOk("#alerta", "CDF Cargado Correctamente");
                    $("#lb_cdf_unitario").text("");
                    modulo_margen_bruto.loadUltimaCargaCDF();
                    console.log(result);
                } else {
                }

            });
        } else {
            helper.MostrarError("#alerta", "El porcentaje debe de sumar 100");
        }
        
    },
    loadDatatable_cdf_departamentos: () => {
        var url = helper.baseUrl + '/Home/GetSIECentrosCosteCDF';

        var data = {
            ejercicio: $("#inpt_ejercicio_cdf").val()
        }

        helper.ajax(url, "POST", data).then(result => {
            if (result.success) {

                var centros_coste = JSON.parse(result.centros_coste);

                if ($.fn.DataTable.isDataTable('#tabla_departamento')) {
                    $("#tabla_departamento").DataTable().destroy();
                }

                $('#tabla_departamento').DataTable({
                    "data": centros_coste,
                    "columns": [
                        { "data": "codigo" },
                        { "data": "nom_centro_coste" },
                        { "data": "importe", render: $.fn.dataTable.render.number('.', ',', 0, '') }
                    ],
                    stateSave: false,
                    "language": {
                        "url": "../lib/datatables.net/lang/Spanish.json",
                        "decimal": ",",
                        "thousands": "."
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
                        total = api
                            .column(2)
                            .data()
                            .reduce(function (a, b) {
                                return intVal(a) + intVal(b);
                            }, 0);

                        //$("#total_gasto").html(helper.number_format_js(total, 0, ',', '.'));
                        modulo_margen_bruto.cdf_total_repartir = total;

                        // Update footer
                        $(api.column(2).footer()).html(
                            'Total Coste Directo Fijo: ' + helper.number_format_js(total, 0, ',', '.') + '€'
                        );
                    }
                });


            } else {
                $("#span_error").text(result.mensaje);
            }

        });
    },
    save_margen_bruto: () => {
        helper.MostrarLoader();
        modulo_margen_bruto.margen_bruto.ejercicio = $("#inpt_ejercicio").val();

        var url = helper.baseUrl + '/Home/saveMargenBruto';

        helper.ajax(url, "POST", modulo_margen_bruto.margen_bruto).then(result => {
            if (result.success) {
                helper.MostrarOk("#alerta", result.message);

            } else {
                helper.MostrarError("#alerta", result.error);
            }
            helper.QuitarLoader();

        });

    },
    getPresupuestosAprobados: async () => {
        var url = helper.baseUrl + '/Home/getPresupuestoAprobados';

        var data = {
            ejercicio: $("#inpt_ejercicio").val()
        }
        helper.MostrarLoader();

        helper.ajax(url, "POST", data).then(result => {
            if (result.success) {
                if (result.prepto_ventas == null) {
                    helper.MostrarError("#alerta", "El presupuesto de ventas no está cerrado");

                } else {
                    modulo_margen_bruto.loadDatatable($("#inpt_ejercicio").val(), "", "", "", "", true);

                }
                /*  else if (result.prepto_produccion == null) {
                      helper.MostrarError("#alerta", "El presupuesto de producción no está cerrado");
                  } else {
                      modulo_margen_bruto.loadDatatable($("#inpt_ejercicio").val(), "", "", "", "");
                  }*/

            } else {
            }

            helper.QuitarLoader();

            $(".dataTables_filter").css("float", "left")


        });
    },
    filtrar: async () => {
        var ejercicio = $("#inpt_ejercicio").val();
        var planta = $("#sel_planta").val();
        var linea = $("#sel_linea").val();
        var tip_ref = $("#sel_referencia").val();
        var agrupacion = $("#sel_agrupacion").val();

        await modulo_margen_bruto.loadDatatable(ejercicio, planta, linea, tip_ref, agrupacion, false);
        $(".dataTables_filter").css("float", "left");
    },
    loadUltimaCarga: async (ejercicio) => {
        var url = helper.baseUrl + '/Home/getUltimaCarga';

        var data = {
            ejercicio: ejercicio
        }
        helper.ajax(url, "POST", data).then(result => {
            if (result.success) {
                $("#sp_ultima_carga").text("Última carga: " + result.cargadapor);
                $("#lb_cdf_unitario").text(result.cdf_unitario);
            } else {
                $("#sp_ultima_carga").text("");
                $("#lb_cdf_unitario").text("");
            }

        });
    },
    loadUltimaCargaCDF: async () => {
        var url = helper.baseUrl + '/Home/getUltimaCargaCDF';

        var data = {
            ejercicio: $("#inpt_ejercicio_cdf").val()
        }
        helper.ajax(url, "POST", data).then(result => {
            if (result.success && result.cargada_por != null) {
                $("#span_error").text("Última carga: " + result.cargada_por);
                $("#span_error").removeClass("text-danger");
            } else {
                $("#span_error").text("");
            }

        });
    },
    loadDatatable: async (ejercicio, planta, linea, tip_ref, agrupacion, cargar) => {
        helper.MostrarLoader();

        var data = {
            ejercicio: ejercicio,
            planta: planta,
            linea: linea,
            tip_ref: tip_ref,
            agrupacion: agrupacion,
            cargar: cargar
        }

        var url = helper.baseUrl + '/Home/getReferenciasMargenBruto';
        await helper.ajax(url, "POST", data).then(result => {
            if (result.success) {
                var referencias = JSON.parse(result.referencias);

                if ($.fn.DataTable.isDataTable('#tabla_margen_bruto')) {
                    $("#tabla_margen_bruto").DataTable().destroy();
                }


                if (result.mensaje == "No se ha realizado el cálculo del CDF Unitario") {
                    helper.MostrarError("#alerta", result.mensaje);
                }

                modulo_margen_bruto.dt_margen_bruto = $('#tabla_margen_bruto').DataTable({
                    "data": referencias,

                    "columns": [
                        { "data": "CODIRE" },
                        { "data": "CODALT" },
                        { "data": "NOMREF" },
                        { "data": "TIPPRD" },
                        { "data": "planta" },
                        { "data": "linea" },
                        {
                            render: function (data, type, row) {
                                if (row.agrupacion != null && row.agrupacion != '') {
                                    return row.agrupacion;
                                } else {
                                    helper.MostrarError("#alerta", "Hay referencias sin configurar");
                                    return row.agrupacion;
                                }
                            }
                        },

                        { "data": "UNISTK" },
                        { "data": "kilos_venta", render: $.fn.dataTable.render.number('.', ',', 0, '') },
                        { "data": "euros_venta", render: $.fn.dataTable.render.number('.', ',', 0, '') },
                        { "data": "pmv_kilos", render: $.fn.dataTable.render.number('.', ',', 4, '') },
                        { "data": "stock_previsto", render: $.fn.dataTable.render.number('.', ',', 0, '') },
                        { "data": "cdv_almacen", render: $.fn.dataTable.render.number('.', ',', 4, '') },
                        { "data": "cdf_almacen", render: $.fn.dataTable.render.number('.', ',', 4, '') },
                        { "data": "kilos_cargo_almacen", render: $.fn.dataTable.render.number('.', ',', 0, '') },
                        { "data": "cdv_cargo_almacen", render: $.fn.dataTable.render.number('.', ',', 0, '') },
                        { "data": "cdf_cargo_almacen", render: $.fn.dataTable.render.number('.', ',', 0, '') },
                        { "data": "kilos_cargo_produccion", render: $.fn.dataTable.render.number('.', ',', 0, '') },
                        { "data": "kilos_produccion", render: $.fn.dataTable.render.number('.', ',', 0, '') },
                        { "data": "cdv", render: $.fn.dataTable.render.number('.', ',', 4, '') },
                        { "data": "cdf", render: $.fn.dataTable.render.number('.', ',', 4, '') },
                        { "data": "cdv_cargo_produccion", render: $.fn.dataTable.render.number('.', ',', 0, '') },
                        { "data": "cdf_cargo_produccion", render: $.fn.dataTable.render.number('.', ',', 0, '') },
                        { "data": "cdv_total", render: $.fn.dataTable.render.number('.', ',', 0, '') },
                        { "data": "cdv_unitario", render: $.fn.dataTable.render.number('.', ',', 4, '') },
                        { "data": "cdf_total", render: $.fn.dataTable.render.number('.', ',', 0, '') },
                        { "data": "cdf_unitario", render: $.fn.dataTable.render.number('.', ',', 4, '') },
                        { "data": "margen_bruto", render: $.fn.dataTable.render.number('.', ',', 0, '') },
                        { "data": "porcen_margen", render: $.fn.dataTable.render.number('.', ',', 2, '') }
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
                    "paging": false,
                    "searching": true,
                    "language": {
                        "url": "../lib/datatables.net/lang/Spanish.json",
                        "decimal": ",",
                        "thousands": "."
                    },
                    "footerCallback": function (row, data, start, end, display) {
                        var api = this.api(), data;

                        // converting to interger to find total
                        var intVal = function (i) {
                            return typeof i === 'string' ?
                                i.replace(/[\$,]/g, '') * 1 :
                                typeof i === 'number' ?
                                    i : 0;
                        };

                        // computing column Total of the complete result 
                        var total_kilos = api
                            .column(8)
                            .data()
                            .reduce(function (a, b) {

                                return intVal(a) + intVal(b);
                            }, 0);
                        modulo_margen_bruto.margen_bruto.total_kilos = Math.round(total_kilos);

                        var total_euros_venta = api
                            .column(9)
                            .data()
                            .reduce(function (a, b) {
                                return intVal(a) + intVal(b);
                            }, 0);
                        modulo_margen_bruto.margen_bruto.euros_venta = Math.round(total_euros_venta);

                        var stock_fin = api
                            .column(11)
                            .data()
                            .reduce(function (a, b) {
                                return intVal(a) + intVal(b);
                            }, 0);
                        modulo_margen_bruto.margen_bruto.stock_fin_ejercicio = Math.round(stock_fin);

                        var kilos_cargo_almacen = api
                            .column(14)
                            .data()
                            .reduce(function (a, b) {
                                return intVal(a) + intVal(b);
                            }, 0);
                        modulo_margen_bruto.margen_bruto.kilos_cargo_almacen = Math.round(kilos_cargo_almacen);

                        var cdv_cargo_almacen = api
                            .column(15)
                            .data()
                            .reduce(function (a, b) {
                                return intVal(a) + intVal(b);
                            }, 0);
                        modulo_margen_bruto.margen_bruto.cdv_cargo_almacen = Math.round(cdv_cargo_almacen);

                        var cdf_cargo_almacen = api
                            .column(16)
                            .data()
                            .reduce(function (a, b) {
                                return intVal(a) + intVal(b);
                            }, 0);
                        modulo_margen_bruto.margen_bruto.cdf_cargo_almacen = Math.round(cdf_cargo_almacen);

                        var kilos_cargo_produccion = api
                            .column(17)
                            .data()
                            .reduce(function (a, b) {
                                return intVal(a) + intVal(b);
                            }, 0);
                        modulo_margen_bruto.margen_bruto.kilos_cargo_produccion = Math.round(kilos_cargo_produccion);

                        var kilos_produccion = api
                            .column(18)
                            .data()
                            .reduce(function (a, b) {
                                return intVal(a) + intVal(b);
                            }, 0);
                        modulo_margen_bruto.margen_bruto.kilos_produccion = Math.round(kilos_cargo_produccion);

                        var cdv_cargo_produccion = api
                            .column(21)
                            .data()
                            .reduce(function (a, b) {
                                return intVal(a) + intVal(b);
                            }, 0);
                        modulo_margen_bruto.margen_bruto.cdv_cargo_produccion = Math.round(cdv_cargo_produccion);

                        var cdf_cargo_produccion = api
                            .column(22)
                            .data()
                            .reduce(function (a, b) {
                                return intVal(a) + intVal(b);
                            }, 0);
                        modulo_margen_bruto.margen_bruto.cdf_cargo_produccion = Math.round(cdf_cargo_produccion);

                        var cdv_total = api
                            .column(23)
                            .data()
                            .reduce(function (a, b) {
                                return intVal(a) + intVal(b);
                            }, 0);
                        modulo_margen_bruto.margen_bruto.cdv_total = Math.round(cdv_total);

                        var cdf_total = api
                            .column(25)
                            .data()
                            .reduce(function (a, b) {
                                return intVal(a) + intVal(b);
                            }, 0);
                        modulo_margen_bruto.margen_bruto.cdf_total = Math.round(cdf_total);

                        var margen_bruto = api
                            .column(27)
                            .data()
                            .reduce(function (a, b) {
                                return intVal(a) + intVal(b);
                            }, 0);
                        modulo_margen_bruto.margen_bruto.margen_bruto = Math.round(margen_bruto);

                        var porcen_margen = api
                            .column(28)
                            .data()
                            .reduce(function (a, b) {
                                return ((intVal(a) + intVal(b)) / (referencias.length));
                            }, 0);
                        modulo_margen_bruto.margen_bruto.porcentaje_margen_bruto = Math.round(porcen_margen * 100);

                        // Update footer by showing the total with the reference of the column index 
                        $(api.column(0).footer()).html('Total');
                        $(api.column(8).footer()).html(helper.number_format_js(total_kilos, 0, ',', '.'));
                        $(api.column(9).footer()).html(helper.number_format_js(total_euros_venta, 0, ',', '.'));
                        $(api.column(11).footer()).html(helper.number_format_js(stock_fin, 0, ',', '.'));
                        $(api.column(14).footer()).html(helper.number_format_js(kilos_cargo_almacen, 0, ',', '.'));
                        $(api.column(15).footer()).html(helper.number_format_js(cdv_cargo_almacen, 0, ',', '.'));
                        $(api.column(16).footer()).html(helper.number_format_js(cdf_cargo_almacen, 0, ',', '.'));
                        $(api.column(17).footer()).html(helper.number_format_js(kilos_cargo_produccion, 0, ',', '.'));
                        $(api.column(18).footer()).html(helper.number_format_js(kilos_produccion, 0, ',', '.'));
                        $(api.column(21).footer()).html(helper.number_format_js(cdv_cargo_produccion, 0, ',', '.'));
                        $(api.column(22).footer()).html(helper.number_format_js(cdf_cargo_produccion, 0, ',', '.'));
                        $(api.column(23).footer()).html(helper.number_format_js(cdv_total, 0, ',', '.'));
                        $(api.column(25).footer()).html(helper.number_format_js(cdf_total, 0, ',', '.'));
                        $(api.column(27).footer()).html(helper.number_format_js(margen_bruto, 0, ',', '.'));
                        $(api.column(28).footer()).html((porcen_margen * 100) + "%");

                    }
                });
                modulo_margen_bruto.loadUltimaCarga($("#inpt_ejercicio").val());

                $('#myCustomSearchBox').keyup(function () {
                    modulo_margen_bruto.dt_margen_bruto.search($(this).val()).draw();   // this  is for customized searchbox with datatable search feature.
                })
                helper.QuitarLoader();

                $("footer").hide();

            }
        });



    },
    margen_bruto: {
        ejercicio: 0,
        kilos_venta: 0,
        euros_venta: 0,
        stock_fin_ejercicio: 0,
        kilos_cargo_almacen: 0,
        cdv_cargo_almacen: 0,
        cdf_cargo_almacen: 0,
        kilos_cargo_produccion: 0,
        kilos_produccion: 0,
        cdv_cargo_produccion: 0,
        cdf_cargo_produccion: 0,
        cdv_total: 0,
        cdf_total: 0,
        margen_bruto: 0,
        porcentaje_margen_bruto: 0
    }
}