var modulo_conf_centros_coste = {
    init: (permiso) => {
       
        permisos.ocultar_permisos(permiso);
        modulo_conf_centros_coste.loadDatatable();
        modulo_conf_centros_coste.putEventsOnButtons();
    },
    putEventsOnButtons: () => {
        $("#btn_nuevo_centro").on("click", function () {
            modulo_conf_centros_coste.show_add_modal();
        });

        $("#btn_save_centro_coste").on("click", function () {
            modulo_conf_centros_coste.add_centro_coste();
        });

        $("#sel_filtro_clasificacion").on("change", function () {
            var clasificacion = $("#sel_filtro_clasificacion").val();
            modulo_conf_centros_coste.loadDatatable(clasificacion);

        });

    },
    loadDatatable: (clasificacion) => {
        var url = helper.baseUrl + '/Home/GetCentrosConfigurados';

        var data = {
            clasificacion: clasificacion
        }

        helper.ajax(url, "POST", data).then(result => {
            if (result.success) {
                var centros_coste = JSON.parse(result.centros_coste);

                if ($.fn.DataTable.isDataTable('#tabla_centros_coste')) {
                    $("#tabla_centros_coste").DataTable().destroy();
                }

                $('#tabla_centros_coste').DataTable({
                    "data": centros_coste,
                    "columns": [
                        { "data": "CODIGO" },
                        { "data": "NOMBRE" },
                        {
                            render: function (data, type, row) {
                                if (row.EBITDA == 1) {
                                    return 'Sí';
                                } else {
                                    return 'No';

                                }
                            }
                        },
                        { "data": "CLASIFICACION" },
                        {
                            render: function (data, type, row) {
                                return `<a href="#" class="btn_accesos btn btn-light btn-sm" onclick="modulo_conf_centros_coste.show_modal_add_centro_coste(` + row.ID + `,` + row.IDENTI + `,'` + row.CODIGO + `','` + row.NOMBRE + `','` + row.EBITDA + `','` + row.CLASIFICACION + `','UPDATE');"><i class="fa fa-edit"></i></a>`;
                            }
                        },
                        {
                            render: function (data, type, row) {
                                return `<a href="#" class="btn_accesos btn btn-light btn-sm" onclick="modulo_conf_centros_coste.removeCentroCoste(` + row.ID + `);"><i class="fa fa-trash"></i></a>`;
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
                helper.QuitarLoader();

            }
        });
    },
    removeCentroCoste: (id) => {

        if (confirm("¿Deseas borrar este centro de coste?")) {
            var url = helper.baseUrl + '/Home/CRUDCentroCoste';

            var data = {
                id: id,
                operacion:"DELETE"
            }

            helper.ajax(url, "POST", data).then(result => {
                if (result.success) {
                    helper.MostrarOk("#alerta", "Se ha borrado correctamente");
                    modulo_conf_centros_coste.loadDatatable();
                } else {
                    helper.MostrarError("#alerta", "El centro de coste no puede ser borrado, tiene datos cargados de SIE en algún ejercicio");

                }
            });
        }
    },
    loadDatatableNoConfigurados: async () => {
        var url = helper.baseUrl + '/Home/GetCentrosNoConfigurados';
        helper.MostrarLoader();

        helper.ajax(url, "GET").then(result => {
            if (result.success) {
                var centros_coste = JSON.parse(result.centros_coste);

                if ($.fn.DataTable.isDataTable('#tabla_add_centro_coste')) {
                    $("#tabla_add_centro_coste").DataTable().destroy();
                }

                $('#tabla_add_centro_coste').DataTable({
                    "data": centros_coste,
                    "columns": [
                        { "data": "CODIGO" },
                        { "data": "NOMBRE" },                      
                        {
                            render: function (data, type, row) {
                                return `<a href="#" class="btn_accesos btn btn-light btn-sm" onclick="modulo_conf_centros_coste.show_modal_add_centro_coste(` + null + ',' + row.IDENTI + `,'` + row.CODIGO + `','` + row.NOMBRE + `',` + null + `,` + null + `,'INSERT');"><i class="fa fa-edit"></i></a>`;
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
                helper.QuitarLoader();

            }
        });
    },
    show_add_modal: async () => {
        await modulo_conf_centros_coste.loadDatatableNoConfigurados();
        helper.AbrirModal("#ventana_add_centro_coste");
        
    },
    show_modal_add_centro_coste: (id,identi, codigo, nombre ,ebitda,clasificacion=null, operacion) => {
        $("#inp_identi").val(identi);
        $("#codigo").val(codigo);
        $("#nombre").val(nombre);
        $("#inp_operacion").val(operacion);
        $("#sele_ebitda").val(ebitda);
        $("#sele_agrupacion_creacion").val(clasificacion);

        $("#inp_id").val(id);
        helper.CerrarModal("#ventana_add_centro_coste");
        helper.AbrirModal("#ventana_conf_centro_coste");

    },
    add_centro_coste: () => {
        helper.MostrarLoader();

        if ($("#sele_agrupacion_creacion").val() != null && $("#sele_ebitda").val() != null) {
            var data = {
                id: $("#inp_id").val(),
                identi: $("#inp_identi").val(),
                codigo: $("#codigo").val(),
                nombre: $("#nombre").val(),
                clasificacion: $("#sele_agrupacion_creacion").val(),
                ebitda: $("#sele_ebitda").val(),
                operacion: $("#inp_operacion").val()
            }

            var url = helper.baseUrl + '/Home/CRUDCentroCoste';

            helper.ajax(url, "POST", data).then(result => {
                if (result.success) {
                    helper.MostrarOk("#alerta", result.message);
                    modulo_conf_centros_coste.loadDatatable();
                } else {
                    helper.MostrarError("#alerta", result.error);
                }
                helper.QuitarLoader();

            });

            helper.CerrarModal("#ventana_conf_centro_coste");
        } else {
            helper.MostrarError("#alerta", "Debes de rellenar todos los campos");
            helper.QuitarLoader();

        }       

    }

}