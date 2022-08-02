var modulo_conf_referencias = {
    init: async (permiso) => {
        permisos.ocultar_permisos(permiso);

        modulo_conf_referencias.putEventsOnButtons();
        helper.MostrarLoader();
        await modulo_conf_referencias.getReferencias();
    },
    putEventsOnButtons: () => {
        $("#sel_agrupacion").on("change", function () {
            modulo_conf_referencias.getReferencias();
        });

        $("#btn_guardar_modal").on("click", function () {
            modulo_conf_referencias.saveReferencia();
        })

        $("#btn_nueva_referencia").on("click", async function () {
            helper.MostrarLoader();
            await modulo_conf_referencias.loadreferenciasnoimportadas();

        })

        $("#chk_sin_agrupacion").on("change", function () {
            var sin_agrupacion = $("#chk_sin_agrupacion").prop("checked");
            modulo_conf_referencias.getReferencias(sin_agrupacion);
        })
    },
    loadreferenciasnoimportadas: async () => {
        var url = helper.baseUrl +'/Home/GetReferenciasNoImportadas';

        helper.ajax(url, "GET").then(result => {

            if (result.success) {
                var referencias = JSON.parse(result.referencias);

                if ($.fn.DataTable.isDataTable('#tabla_add_referencias')) {
                    $("#tabla_add_referencias").DataTable().destroy();
                }

                $('#tabla_add_referencias').DataTable({
                    "data": referencias,
                    "columns": [
                        { "data": "CODIRE" },
                        { "data": "CODALT" },
                        { "data": "NOMREF" },
                        { "data": "TIPREF" },
                        {
                            render: function (data, type, row) {
                                return `<a href="#" class="btn_accesos btn btn-light btn-sm" onclick="modulo_conf_referencias.add_referencia(` + row.CODIRE + `,'` + row.CODALT + `','` + row.NOMREF + `'); "><i class="fa fa-plus"></i></a>`;
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

                helper.AbrirModal("#ventana_add_refs");

            }
        });

    },
    getReferencias: async (chk_agrupacion = null) => {
        var agrupacion = null;
        if (chk_agrupacion != null && chk_agrupacion == true) {
            agrupacion = -1;
        } else if ($("#sel_agrupacion").val() == '') {
            agrupacion = 0;
        } else {
            agrupacion = $("#sel_agrupacion").val();
        }

        var data = {
            agrupacion: agrupacion
        }

        var url = helper.baseUrl +'/Home/GetReferencias';
        helper.ajax(url, "POST", data).then(result => {
            if (result.success) {
                var referencias = JSON.parse(result.referencias);

                if ($.fn.DataTable.isDataTable('#tabla_referencias')) {
                    $("#tabla_referencias").DataTable().destroy();
                }

                $('#tabla_referencias').DataTable({
                    "data": referencias,
                    "columns": [
                        { "data": "CODALT" },
                        { "data": "NOMREF" },
                        { "data": "agrupacion" },
                        {
                            render: function (data, type, row) {
                                return `<a href="#" class="btn_accesos btn btn-light btn-sm" onclick="modulo_conf_referencias.show_modal(` + row.id + `,'` + row.CODIRE + `','` + row.CODALT + `','`+ row.NOMREF + `',` + row.FK_AGR +`);"><i class="fa fa-edit"></i></a>`;
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
    show_modal: (id, codire, codalt, nomref, fk_arg) => {
        $("#hidden_id_usuario").val(id);
        $("#cod_alt").val(codalt);
        $("#nomref").val(nomref);
        $("#codire").val(codire);
        $("#codire").prop("readonly", "true");
        $("#sele_agrupacion_creacion").val(fk_arg);
        helper.AbrirModal("#ventana_refs");
    },
    add_referencia: (codire, cod_alt, nomref) => {

        if (confirm("¿Estás seguro de querer añadir esta referencia?")) {

            
            var data = {
                CODIRE: codire,
                cod_alt: cod_alt,
                nomref: nomref
            }
            var url = helper.baseUrl +'/Home/addreferencia';

            helper.ajax(url, "POST", data).then(result => {
                if (result.success) {
                    helper.MostrarOk("#alerta", result.message);
                    helper.CerrarModal("#ventana_refs");
                    modulo_conf_referencias.getReferencias();
                } else {
                    helper.MostrarError("#alerta", result.error);

                }
            });

        }

    },
    saveReferencia: () => {
        var data = {
            id: $("#hidden_id_usuario").val(),
            codire: $("#codire").val(),
            nom_ref: $("#nomref").val(),
            cod_alt: $("#cod_alt").val(),
            agrupacion: $("#sele_agrupacion_creacion").val()
        }

        var url = helper.baseUrl +'/Home/saveReferencia';
        helper.ajax(url, "POST", data).then(result => {
            if (result.success) {
                helper.MostrarOk("#alerta", result.message);
                helper.CerrarModal("#ventana_refs");
                modulo_conf_referencias.getReferencias();
            } else {
                helper.MostrarError("#alerta", result.error);

            }
        });

    }
}