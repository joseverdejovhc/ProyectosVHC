var modulo_conf_cuentas_contables = {
    init: async (permiso) => {
        permisos.ocultar_permisos(permiso);

        await modulo_conf_cuentas_contables.loadDatatables();
        modulo_conf_cuentas_contables.putEventsOnButtons();
    },
    putEventsOnButtons: () => {
        $(".btn_add_cuenta").on("click", async function (e) {
            e.preventDefault();
            var seccion = $(this).data('category');
            await modulo_conf_cuentas_contables.loadDatatableNoConfigurados();

            $("#inp_clasificacion").val(seccion);

        });
    },
    loadDatatables: async () => {
        var url = helper.baseUrl + '/Home/GetCuentasConfiguradas';


        helper.ajax(url, "GET").then(result => {
            if (result.success) {
                var cuentas_amortizaciones = JSON.parse(result["Amortizaciones y subvenciones"]);
                var gastos_financieros = JSON.parse(result["Gastos Financieros"]);
                var ingresos_financieros = JSON.parse(result["Ingresos Financieros"]);

                modulo_conf_cuentas_contables.loadDatatableCuentas("#tabla_amortizaciones_subvenciones", cuentas_amortizaciones);
                modulo_conf_cuentas_contables.loadDatatableCuentas("#tabla_gastos_financieros", gastos_financieros);
                modulo_conf_cuentas_contables.loadDatatableCuentas("#tabla_ingresos_financieros", ingresos_financieros);



            }
        });
    },
    loadDatatableCuentas: (id_jquery, cuentas_contables) => {
        if ($.fn.DataTable.isDataTable(id_jquery)) {
            $(id_jquery).DataTable().destroy();
        }

        $(id_jquery).DataTable({
            "data": cuentas_contables,
            "columns": [
                { "data": "NUMERO" },
                { "data": "NOMBRE" },
                {
                    render: function (data, type, row) {
                        return `<a href="#" class="btn_accesos btn btn-light btn-sm" onclick="modulo_conf_cuentas_contables.remove_cuenta_contable(` + row.ID + `,'` + row.NUMERO + `','` + row.NOMBRE + `','INSERT');"><i class="fa fa-trash" aria-hidden="true"></i></a>`;
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
    loadDatatableNoConfigurados: async () => {
        var url = helper.baseUrl + '/Home/GetCuentasNoConfiguradas';

        helper.MostrarLoader();
        helper.ajax(url, "GET").then(result => {
            if (result.success) {
                var cuentas_contables = JSON.parse(result.cuentas_contables);
                helper.AbrirModal("#ventana_add_cuenta");
                if ($.fn.DataTable.isDataTable('#tabla_add_cuenta')) {
                    $("#tabla_add_cuenta").DataTable().destroy();
                }

                $('#tabla_add_cuenta').DataTable({
                    "data": cuentas_contables,
                    "columns": [
                        { "data": "NUMERO" },
                        { "data": "NOMBRE" },
                        {
                            render: function (data, type, row) {
                                return `<a href="#" class="btn_accesos btn btn-light btn-sm" onclick="modulo_conf_cuentas_contables.add_cuenta_contable('` +  row.NUMERO + `','` + row.NOMBRE + `','INSERT');"><i class="fa fa-plus"></i></a>`;
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
            helper.QuitarLoader();
        });
    },
    add_cuenta_contable: (numero, nombre, operacion) => {
        helper.CerrarModal("#ventana_add_cuenta");
        modulo_conf_cuentas_contables.crud_cuenta_contable(null, numero, nombre, operacion);
    },
    remove_cuenta_contable: (id) => {
        if (confirm('¿Estás segur@ de querer borrar la configuración de esta cuenta contable?')) {
            modulo_conf_cuentas_contables.crud_cuenta_contable(id, null, null, "DELETE");
        }
    },
    crud_cuenta_contable: (id, numero, nombre, operacion) => {
        helper.MostrarLoader();

        var data = {
            id: id,
            numero: numero,
            nombre: nombre,
            clasificacion: $("#inp_clasificacion").val(),
            operacion: operacion
        }

        var url = helper.baseUrl + '/Home/CRUDCuentaContable';

        helper.ajax(url, "POST", data).then(result => {
            if (result.success) {
                helper.MostrarOk("#alerta", result.message);
                modulo_conf_cuentas_contables.loadDatatables();
            } else {
                helper.MostrarError("#alerta", result.error);
            }
            helper.QuitarLoader();

        });
    }

}