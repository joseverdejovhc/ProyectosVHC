var modulo_nutricional_information = {
    init: () => {
        modulo_nutricional_information.loadDatatables();
        modulo_nutricional_information.loadEventsButtons();
    },
    loadEventsButtons: () => {
        $('input[type=radio][name=unit_minerals]').change(function () {
            if (this.value == 'fsmp') {
                $(".minerales").prop("disabled", true);
            } else {
                $(".minerales").prop("disabled", false);

            }
        });

        $('input[type=radio][name=unit_vitamins]').change(function () {
            if (this.value == 'fsmp') {
                $(".vitamins").prop("disabled", true);
            } else {
                $(".vitamins").prop("disabled", false);

            }
        });

    },
    tabla_macronutrients: null,
    tabla_minerals: null,
    tabla_vitamins:null,
    loadDatatables: () => {
        $.fn.dataTable.ext.order['dom-text-numeric'] = function (settings, col) {
            return this.api().column(col, { order: 'index' }).nodes().map(function (td, i) {
                return $('input', td).val() * 1;
            });
        }

        $.fn.dataTable.ext.order['dom-text'] = function (settings, col) {
            return this.api().column(col, { order: 'index' }).nodes().map(function (td, i) {
                return $('input', td).val();
            });
        };

        if ($.fn.DataTable.isDataTable("#tabla_macronutrients")) {
            $("#tabla_macronutrients").DataTable().destroy();
        }

        modulo_nutricional_information.tabla_macronutrients=$('#tabla_macronutrients').DataTable({
            "paging": false,

            "columns": [
                null,
                { "orderDataType": "dom-text" },
                { "orderDataType": "dom-text-numeric" }
            ], "columnDefs": [
                { "width": "12%", "targets": 1 },
                { "orderDataType": "dom-text-numeric", "targets": 2 }
            ],
            "ordering": false
        });

        if ($.fn.DataTable.isDataTable("#tabla_minerals")) {
            $("#tabla_minerals").DataTable().destroy();
        }

        modulo_nutricional_information.tabla_minerals = $('#tabla_minerals').DataTable({
            "paging": false,
            "columns": [
                null,
                { "orderDataType": "dom-text" },
                { "orderDataType": "dom-text-numeric" }
            ], "columnDefs": [
                { "width": "12%", "targets": 1 },
                { "orderDataType": "dom-text-numeric", "targets": 2 }
            ],
            "ordering": false
        });

        if ($.fn.DataTable.isDataTable("#tabla_vitamins")) {
            $("#tabla_vitamins").DataTable().destroy();
        }

        modulo_nutricional_information.tabla_vitamins = $('#tabla_vitamins').DataTable({
            "paging": false,
            "columns": [
                null,
                { "orderDataType": "dom-text" },
                { "orderDataType": "dom-text-numeric" }
            ], "columnDefs": [
                { "width": "12%", "targets": 1 },
                { "orderDataType": "dom-text-numeric", "targets": 2 }
            ],
            "ordering": false
        });
    }
}