var modulo_factura = {
    init: () => {
        modulo_factura.validateForms();
        modulo_factura.putEventsOnButtons();
    },
    tabla_subcapitulos_facturacion: null,
    putEventsOnButtons: () => {
        $("#btn-save-subcapitulo_facturado").on("click", function (e) {
            e.preventDefault();
            $(this).prop('disabled', true);

            var valor = parseFloat($("#importe_facturado").val());
            var cell_row = $("#row_facturado").val();
            modulo_factura.tabla_subcapitulos_facturacion.cell(cell_row, 4).data(valor);
            modulo_factura.tabla_subcapitulos_facturacion.draw();
            $("#importe_facturado").val("")
            helper.CerrarModal("#ventana_facturado_subcapitulo");
            $(this).prop('disabled', false);

        });

        $("#btn_save_factura").on("click", async function (e) {
            e.preventDefault();
            $(this).prop('disabled', true);
            await modulo_factura.saveFactura();
            $(this).prop('disabled', false);

        })
    },
    saveFactura: async () => {

        if ($("#form_factura").valid()) {
            var subcapitulos = [];
            var rows = modulo_factura.tabla_subcapitulos_facturacion.rows().data();

            for (let i = 0; i < rows.length; i++) {
                subcapitulos.push(rows[i]);
            }

            var data = {
                operacion: $("#action_factura").val(),
                num_factura: $("#num_factura").val(),
                num_expediente: $("#num_expediente").val(),
                fecha_factura: $("#fech_factura").val(),
                pedido: $("#identi_pedido").val(),
                id_factura: $("#identi_factura").val(),
                subcapitulos: JSON.stringify(subcapitulos)
            }

            var url = helper.baseUrl + '/Home/CRUDFactura';

            helper.ajax(url, "POST", data).then(result => {
                
                helper.CerrarModal("#ventana_add_factura");
                if ($("#modulo").val() == "pedidos") {
                    modulo_nuevo_pedido.loadPedidoModificado();
                }
                if ($("#modulo").val() == "factura") {
                    helper.MostrarOk("Factura Guardada correctamente");
                    modulo_facturas.loadDatatable();
                }

                if ($("#modulo").val() == "seguimiento") {
                    modulo_seguimiento.loadAllData(null);
                    helper.CerrarModal("#ventana_desglose_facturas");
                    helper.MostrarOk("Factura Guardada correctamente");

                }
                if (result.success == 0) {
                    helper.MostrarAlertaError('info', result.mensaje);
                } else {
                    helper.MostrarOk(result.mensaje);
                }
            });
        }

    },
    AbrirModalImporteFacturado: (subcapitulo, row, importe) => {
        $("#id_subcapitulo_facturado").val(subcapitulo.id);
        importe = modulo_factura.tabla_subcapitulos_facturacion.cell(row, 4).data();
        $("#row_facturado").val(row);
        $("#importe_facturado").val(importe);

        helper.AbrirModal("#ventana_facturado_subcapitulo");

    },
    validateForms: () => {
       

        $("#form_factura").validate({
            rules: {
                num_factura: {
                    required: true
                },
                num_expediente: {
                    required: true,
                },
                fech_factura: {
                    required: true
                }
            },
            errorClass: "error fail-alert",
            messages: {
                num_factura: {
                    required: "El número de factura es obligatorio"
                },
                num_expediente: {
                    required: "El número de expediente es obligatorio"
                }, fech_factura: {
                    required: "La fecha de factura es obligatorio"
                },
            }
        });

    }
}