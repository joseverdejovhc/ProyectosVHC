var array_ajax = []
var helper = {
    baseUrl: '/presupuestotest',
    ajax: async (url, type, data = null) => {
        return new Promise((resolve, reject) => {
            $.ajax({
                url: url, // Url
                data: data,
                type: "POST",
                dataType: "json",
                beforeSend: function (jqXHR, settings) {
                    array_ajax.push(jqXHR);
                },
                cache: false
            })
                .done(function (result) {
                    resolve(result)
                })
                .fail(function (error) {
                    resolve(error)
                })
        })
    },

    MostrarOk: function (id_alerta, mensaje) {
        $(id_alerta).removeClass("alert-danger");
        $(id_alerta).addClass("alert-success");
        $(id_alerta).html(mensaje);
        $(id_alerta).show("slow").delay(2000).hide("slow");
    },

    MostrarError: function (id_alerta, mensaje) {
        $(id_alerta).removeClass("alert-success");
        $(id_alerta).addClass("alert-danger");
        $(id_alerta).html(mensaje + '<button type="button" class="close ml-2" onclick="$(\'' + id_alerta + '\').hide();">&times;</button>');
        $(id_alerta).show("slow");//.delay(5000).hide("slow");
    },

    CerrarModal: function () {
        $('.modal').modal('hide');
    },

    AbrirModal: function (ventana) {
        $(ventana).modal('show');
    },

    MostrarLoader: function () {
        $("#AjaxLoader").show();
    },

    QuitarLoader: function () {
        $("#AjaxLoader").hide();
    },


    Mostrar: function (elemento) {
        $(elemento).removeClass("d-none");
    },

    Ocultar: function (elemento) {
        $(elemento).addClass("d-none");
    },
    number_format_js: (number, decimals, dec_point, thousands_point) => {

        if (number == null || !isFinite(number)) {
            throw new TypeError("number is not valid");
        }

        if (!decimals) {
            var len = number.toString().split('.').length;
            decimals = len > 1 ? len : 0;
        }

        if (!dec_point) {
            dec_point = '.';
        }

        if (!thousands_point) {
            thousands_point = ',';
        }

        number = parseFloat(number).toFixed(decimals);

        number = number.replace(".", dec_point);

        var splitNum = number.split(dec_point);
        splitNum[0] = splitNum[0].replace(/\B(?=(\d{3})+(?!\d))/g, thousands_point);
        number = splitNum.join(dec_point);

        return number;
    },

    stop_all_ajax: () => {
        $.each(array_ajax, function (idx, jqXHR) {
            jqXHR.abort();
            array_ajax.splice(idx);
        });
    }


}

var permisos = {
    ocultar_permisos: (nivel) => {
        for (var i = 1; i < nivel; i++) {
            $(".nivel" + i).hide();
        }
    }
}



