const modulo_usuarios = {
    init: (permiso) => {
        permisos.ocultar_permisos(permiso);

        modulo_usuarios.load_users();
        modulo_usuarios.putEventsOnButtons();
        modulo_usuarios.load_secciones();
    },
    putEventsOnButtons: () => {
        $("#btn_guardar_usuario").on("click", function (e) {
            e.preventDefault();
            modulo_usuarios.save_user($(this).data('request-url'));
        });

        $(".btn_accesos").on("click", function (e) {
            e.preventDefault();
            modulo_usuarios.showmodal();
        });
    },
    load_secciones: async () => {
        $("#select_secciones").select2({
            dropdownParent: $('#ventana_accesos')
        });

        var url = helper.baseUrl+'/Home/getListaSecciones';
        helper.ajax(url, "GET").then(result => {
            if (result.success) {
                var secciones = JSON.parse(result.secciones);
                secciones.forEach((seccion) => {
                    var newOption = new Option(seccion.texto_modulo, seccion.id, false, false);
                    $('#select_secciones').append(newOption).trigger('change');
                });

            }
        });


    },
    load_users: async () => {
        var url = helper.baseUrl +'/Home/getListaUsuarios';
        helper.ajax(url, "GET").then(result => {
            if (result.success) {

                if ($.fn.DataTable.isDataTable('#tabla_usuarios')) {
                    $("#tabla_usuarios").DataTable().destroy();
                }

                var usuarios = JSON.parse(result.usuarios);

                $('#tabla_usuarios').DataTable({
                    "data": usuarios,
                    "columns": [
                        { "data": "login" },
                        { "data": "nombre" },
                        {
                            render: function (data, type, row) {
                                return `<a class="btn btn-light btn-sm nivel1" onclick="modulo_usuarios.deleteUser(` + row.id + `);"><i class="fa fa-trash text-dark"></i></a> |
                                        <a href="#" class="btn_accesos btn btn-light btn-sm" onclick="modulo_usuarios.show_modal(` + row.id + `, '` + row.nombre + `','` + row.login + `');">Accesos</a>`;
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


            }

        });
    },
    save_user: async (url) => {

        if ($("#login").val() != "" && $("#nombrelogin").val() != "") {
            $("#AjaxLoader").show();
            var data = {
                login: $("#login").val(),
                nombre: $("#nombre").val(),
                operacion: 'INSERT'
            };


            helper.ajax(url, "POST", data).then(result => {

                if (result.success) {
                    helper.MostrarOk("#alerta", result.message);
                    modulo_usuarios.load_users();
                } else {
                    helper.MostrarError("#alerta", result.error);
                }

            });

        } else {
            helper.MostrarError("#alerta", "Debes de rellenar todos los campos");
        }

    },
    deleteUser: (id) => {

        if (confirm("¿Estás seguro de querer eliminar este usuario?")) {

            var data = {
                id: id
            };

            var url = helper.baseUrl +"/home/EliminarUsuario";

            helper.ajax(url, "POST", data).then(result => {

                if (result.success) {
                    helper.MostrarOk("#alerta", result.message);
                    modulo_usuarios.load_users();
                } else {
                    helper.MostrarError("#alerta", result.error);
                }

            });

        }
    },
    show_modal: (id, nombre, login) => {
        $("#lbl_nombre_usuario").val(login);
        $("#hidden_id_usuario").val(id);
        modulo_usuarios.load_secciones_user(id);
        helper.AbrirModal("#ventana_accesos");
    },
    load_secciones_user: (id) => {

        var url = helper.baseUrl +"/home/getSeccionesUsuario";
        var data = {
            id: id
        }

        helper.ajax(url, "POST", data).then(result => {

            if (result.success) {
                var secciones = JSON.parse(result.secciones);
                secciones.forEach((seccion) => {
                    $("#" + seccion.alias).val(seccion.escritura);
                });

            } else {
            }

        });

    },
    save_secciones: () => {
        var id = $("#hidden_id_usuario").val();

        var url = helper.baseUrl +"/home/saveSeccionesUsuario";
        var data = {
            id: id,
            margen_bruto: $("#margen_bruto").val(),
            conf_centros_coste: $("#conf_centros_coste").val(),
            usuarios: $("#usuarios").val(),
            sie: $("#sie").val(),
            conf_cuenta_contable: $("#conf_cuenta_contable").val(),
            presupuesto: $("#presupuesto").val(),
            conf_referencia: $("#conf_referencia").val()
        }        
        
        helper.ajax(url, "POST", data).then(result => {

            if (result.success) {
                helper.MostrarOk("#alerta", "Se han aplicado los cambios correctamente");
                helper.CerrarModal("#ventana_accesos");
            } else {

            }

        });
    }



}
