var array_ajax = []
var helper = {
    baseUrl: '/Nutrisens',
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
    },
    mostrarLoader: () => {
        $("#AjaxLoader").show();
    },
    quitarLoader: () => {
        $("#AjaxLoader").hide();
    },
    mostrarAlertaOk: (mensaje) => {
        Swal.fire({
            icon: 'success',
            text: mensaje
        })
    },
    mostrarAlertaError: (mensaje) => {
        Swal.fire({
            icon: 'error',
            text: mensaje
        })
    }


}



