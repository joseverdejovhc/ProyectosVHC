var array_ajax = []
var helper = {
    baseUrl: '/higia',
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

    MostrarOk: function (mensaje) {
        $("#alerta").removeClass("alert-danger");
        $("#alerta").addClass("alert-success");
        $("#alerta").html(mensaje);
        $("#alerta").show("slow").delay(2000).hide("slow");
    },

    MostrarError: function (mensaje) {
        $("#alerta").removeClass("alert-success");
        $("#alerta").addClass("alert-danger");
        $("#alerta").html(mensaje + `<button type="button" class="close ml-2" onclick="$('#alerta').hide();">&times;</button>`);
        $("#alerta").show("slow");//.delay(5000).hide("slow");
    },
    MostrarAlertaError: (icon,mensaje) => {
        Swal.fire({
            icon: icon,
            title: 'Info',
            text: mensaje,
            confirmButtonText: 'Ok'
        })
    },

    CerrarModal: function (ventana) {
        $(ventana).modal('hide');
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
    },
    locale: {
        firstDayOfWeek: 1,
        weekdays: {
            shorthand: ['Do', 'Lu', 'Ma', 'Mi', 'Ju', 'Vi', 'Sa'],
            longhand: ['Domingo', 'Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado'],
        },
        months: {
            shorthand: ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun', 'Jul', 'Ago', 'Sep', 'Оct', 'Nov', 'Dic'],
            longhand: ['Enero', 'Febreo', 'Мarzo', 'Abril', 'Mayo', 'Junio', 'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'],
        },
    },
    addLeadingZeros: (value) => {
        return ('00000000' + value.toString()).slice(- value.length);
    }


}



