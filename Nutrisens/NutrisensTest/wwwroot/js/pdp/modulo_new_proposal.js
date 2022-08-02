var modulo_new_proposal = {
    init: async (numero) => {
        helper.mostrarLoader();
        modulo_new_proposal.validateForms();
        modulo_new_proposal.putEventsOnButtons();
        await modulo_new_proposal.loadSelectors();
        modulo_new_proposal.getAccion(numero);
        helper.quitarLoader();
    },
    putEventsOnButtons: () => {
        $("#btn_send_approval").on("click", function (e) {
            e.preventDefault();
            modulo_new_proposal.saveProposal(true);
        });

        $("#btn_delete_proposal").on("click", function (e) {
            e.preventDefault();
            modulo_new_proposal.deleteProposal();
        });

        $("#btn_save_proposal").on("click", function (e) {
            e.preventDefault();
            modulo_new_proposal.saveProposal();
        });

        $("#btn_approve").on("click", function (e) {
            e.preventDefault();
            modulo_new_proposal.approveProposal();
        });

        $("#btn_reject").on("click", function (e) {
            e.preventDefault();
            AbrirModal("#modal_comment_reject");
        });

        $("#btn_reject_proposal").on("click", function (e) {
            e.preventDefault();
            modulo_new_proposal.rejectProposal();
            CerrarModal("#modal_comment_reject");
        });

        $(".form-control").on("keydown", function (e) {
            if (e.keyCode === 13) {
                e.preventDefault();
            }
        });


        $("#btn_new_document").on("click", function (e) {
            e.preventDefault();
            if (e.keyCode != 13 || e.which != 13) {
                modulo_new_proposal.subirArchivos();
            }
           
        })

        $('form').submit(function (e) {
            e.preventDefault();
        });

        /*$('.decimales').on('input', function () {
            this.value = this.value.replace(/[^0-9,]/g, '');
        });*/
    },
    loadSelectors: async () => {
        await modulo_new_proposal.getEmpresas();
        await modulo_new_proposal.getApprovers();
    },
    saveProposal: (send = null) => {
        var valid = true;
        var data = modulo_new_proposal.getDataNewProposal();

        if (send != null) {

            if (send && $("#form-proposal").valid()) {
                var $tabs = $('#myTab li a');
                $tabs.each(function (key, ele) {
                    var form_valid = true;
                    form_valid = modulo_new_proposal.validateTab(ele);
                    if (form_valid == false) {
                        valid = form_valid;
                    }
                });

                if (valid) {
                    data.send = "true";
                } else {
                    data.send = "false";
                }

            } else {
                valid = false;
            }
        } else {
            valid = true;
        }

        if (valid) {
            var datatables = modulo_new_proposal.getDataDatatables();

            data.datatables = JSON.stringify(datatables);
            data.information = JSON.stringify(modulo_new_proposal.getGeneralInformation());
            data.operacion = "INSERT";
            data.unit_macronutrientes = $('input[name="unit"]:checked').val();
            data.unit_minerals = $('input[name="unit_minerals"]:checked').val();
            data.unit_vitamins = $('input[name="unit_vitamins"]:checked').val();

            var url = helper.baseUrl + "/PDP/CRUDProposal";
            helper.ajax(url, "POST", data).then(result => {
                if (result.success == 1) {
                    //MostrarOk("#alerta",result.mensaje );

                    helper.mostrarAlertaOk(result.mensaje);

                    setTimeout(function () {
                        if (send) {
                            window.open(helper.baseUrl + "/PDP/NewProposal?number=" + data.number + "", "_self");
                        } else {
                            modulo_new_proposal.renderView(data.number);
                        }
                    }, 1000);



                } else {
                    //MostrarError("#alerta", result.mensaje);
                    helper.mostrarAlertaError(result.mensaje);

                }
            });

            if ($("#comments").val() != "") {

                var data = {
                    operacion: "INSERT",
                    comment: $("#comments").val(),
                    proposal: $("#number").val()
                }
                var url = helper.baseUrl + "/PDP/CRUDComments";
                helper.ajax(url, "POST", data).then(result => { });
            }

        } else {
            helper.mostrarAlertaError("Missing mandatory data to fill in");
        }



    },
    getDataDatatables: () => {

        var datatables = {};
        var macronutrientes = [];
        var minerals = [];
        var vitamins = [];

        modulo_nutricional_information.tabla_macronutrients.rows().every(function (rowIdx) {

            var columns = $(this.node()).find("td");

            var macronutriente = {};
            macronutriente.macronutriente = columns[0].innerText;

            macronutriente.unidad = columns[1].children[0].value;
            macronutriente.valor = columns[2].children[0].value.replaceAll('.', ','); 
            macronutrientes.push(macronutriente);

        });

        datatables.macronutrientes = macronutrientes;


        modulo_nutricional_information.tabla_minerals.rows().every(function (rowIdx) {

            var columns = $(this.node()).find("td");

            var mineral = {};
            mineral.mineral = columns[0].innerText;

            if ($('input[name="unit_minerals"]:checked').val() == "other") {
                mineral.unidad = columns[1].children[0].value;
                mineral.valor = columns[2].children[0].value.replaceAll('.', ',');
            } else {
                mineral.unidad = "";
                mineral.valor = "";
            }

           
            minerals.push(mineral);

        });

        datatables.minerales = minerals;


        modulo_nutricional_information.tabla_vitamins.rows().every(function (rowIdx) {

            var columns = $(this.node()).find("td");

            var vitamina = {};
            vitamina.vitamina = columns[0].innerText;
            if ($('input[name="unit_vitamins"]:checked').val() == "other") {
                vitamina.unidad = columns[1].children[0].value;
                vitamina.valor = columns[2].children[0].value.replaceAll('.', ',');
            } else {
                vitamina.unidad = "";
                vitamina.valor = "";
            }
            vitamins.push(vitamina);

        });
        datatables.vitaminas = vitamins;

        return datatables;

    },
    getGeneralInformation: () => {
        return {
            planification: $("#planification").val(),
            released_date: $("#released_date").val(),
            product_description: $("#product_description").val(),
            patent_proposal: $("#patent_proposal").val(),
            regulations: $("#regulations").val(),
            flavourings: $("#flavourings").val(),
            nutritional_info: $("#nutritional_info").val(),
            specific_needed: $("#specific_needed").val(),
            physical_requirements: $("#physical_requirements").val(),
            target_cost: $("#target_cost").val(),
            instructions_use: $("#instructions_use").val(),
            releasing_params: $("#releasing_params").val(),
            primary_packaging: $("#primary_packaging").val(),
            secondary_packaging: $("#secondary_packaging").val(),
            volume_per_year: $("#volume_per_year").val(),
            sell_by_year: $("#sell_by_year").val(),
            happpcc_system: $("#happpcc_system").val()
        }
    },
    getNewNumber: () => {
        var url = helper.baseUrl + "/PDP/NuevoNumero";
        helper.ajax(url, "GET").then(result => {
            $("#number").val(result.numero);
        });
    },
    getApprovers: async () => {
        var url = helper.baseUrl + "/PDP/getListaApprovers";
        await helper.ajax(url, "GET").then(result => {
            var approvers = JSON.parse(result.approvers);

            $("#approver_1 option").remove();
            $('#approver_1').append(new Option("Select Approver", ""));
            $('#approver_2').append(new Option("Select Approver", ""));
            $('#approver_3').append(new Option("Select Approver", ""));
            approvers.forEach((approver) => {
                $('#approver_1').append(new Option(approver.NombreCompleto, approver.usuario));
                $('#approver_2').append(new Option(approver.NombreCompleto, approver.usuario));
                $('#approver_3').append(new Option(approver.NombreCompleto, approver.usuario));

            });

        });
    },
    getEmpresas: async () => {
        var url = helper.baseUrl + "/PDP/getListaEmpresas";
        await helper.ajax(url, "GET").then(result => {
            var empresas = JSON.parse(result.empresas);

            $("#company option").remove();
            $('#company').append(new Option("Please select", null));
            empresas.forEach((company) => {
                $('#company').append(new Option(company.NombreEmpresa, company.Id));
            });

        });
    },
    getDataNewProposal: () => {
        return {
            company: $("#company").val(),
            number: $("#number").val(),
            business_unit: $("#business_unit").val(),
            user_approver_1: $("#approver_1").val(),
            user_approver_2: $("#approver_2").val(),
            user_approver_3: $("#approver_3").val(),
            project_name: $("#project_name").val()
        }
    },
    approveProposal: () => {
        if (confirm("Are you sure to approve this proposal?")) {

            var data = {
                number: $("#number").val(),
                estado_actual: $("#state").val()
            }

            var url = helper.baseUrl + "/PDP/cambiarEstado";
            helper.ajax(url, "POST", data).then(result => {
                if (result.success) {
                    // modulo_new_proposal.renderView(data.number);
                    helper.mostrarAlertaOk(result.mensaje);

                    //MostrarOk("#alerta", result.mensaje);

                    setTimeout(function () {
                        location.reload()
                    }, 1000);

                } else {
                    // MostrarError("#alerta", result.mensaje);
                    helper.MostrarAlertaError(result.mensaje);
                }

            });

        }
    },
    rejectProposal: () => {

        var data = {
            number: $("#number").val(),
            estado_actual: $("#state").val(),
            comment: $("#comment_reject").val(),
            rejected: "rejected"
        }

        var url = helper.baseUrl + "/PDP/cambiarEstado";
        helper.ajax(url, "POST", data).then(result => {
            if (result.success) {
                helper.mostrarAlertaOk(result.mensaje);

                setTimeout(function () {
                    location.reload()
                }, 1000);
            } else {
                helper.mostrarAlertaError(result.mensaje);
            }
        });


        if ($("#comment_reject").val() != "") {
            var data = {
                operacion: "INSERT",
                comment: $("#comment_reject").val(),
                proposal: $("#number").val(),
                is_comment_reject: 1
            }

            modulo_comments.saveComment(data);
        }



    },
    deleteProposal: () => {
        if (confirm("Are you sure to delete this proposal?")) {
            var data = {
                operacion: "DELETE",
                number: $("#number").val()
            }

            var url = helper.baseUrl + '/PDP/CRUDProposal';

            helper.ajax(url, "POST", data).then(result => {
                if (result.success == 1) {
                    helper.mostrarAlertaOk(result.mensaje);
                    setTimeout(function () {
                        window.open(helper.baseUrl + "/PDP/", "_self");
                    }, 1000);

                } else {
                    helper.mostrarAlertaError(result.mensaje);
                }
            });
        }


    },
    renderView: async (numero) => {
        var data = {
            proposal: numero
        }
        var url = helper.baseUrl + "/PDP/getProposal";

        await helper.ajax(url, "POST", data).then(result => {

            var proposal = JSON.parse(result.proposal)[0];
            var proposal_info = JSON.parse(result.proposal_info)[0];
            var macronutrientes = JSON.parse(result.macronutrientes);
            var minerals = JSON.parse(result.minerals);
            var vitamins = JSON.parse(result.vitamins);
            var comments = JSON.parse(result.comments);
            var documents = JSON.parse(result.documents);
            var cycles = JSON.parse(result.cycle);
            var unit_macronutrientes = result.unit_macronutrientes;
            var unit_minerales = result.unit_minerales;
            var unit_vitaminas = result.unit_vitaminas;

            modulo_new_proposal.loadProposalData(proposal);
            modulo_new_proposal.loadProposalInfo(proposal_info);
            modulo_new_proposal.loadMacronutrientes(macronutrientes);
            modulo_new_proposal.loadMinerals(minerals);
            modulo_new_proposal.loadVitamins(vitamins);
            modulo_new_proposal.loadComments(comments);
            modulo_new_proposal.loadDocuments(documents);
            modulo_new_proposal.loadCycles(cycles);

            if (proposal.state == "Approved") {
                $('#form-proposal input').prop('readonly', 'readonly');
                $('#form-proposal select').prop("disabled", true);
                $('#form_information input').prop('readonly', 'readonly');
                $('#form_information textarea').prop('readonly', 'readonly');
                //$("#released_date").prop('readonly', true)
                $("#released_date").flatpickr({
                    clickOpens: false,
                    "defaultDate": proposal_info.dr_released_date
                });

                $("#comment").prop('readonly', false);
            }
            $("input[name='unit'][value=" + unit_macronutrientes + "]").prop('checked', true);
            $("input[name='unit_minerals'][value=" + unit_minerales + "]").prop('checked', true);
            $("input[name='unit_vitamins'][value=" + unit_vitaminas + "]").prop('checked', true);

            if (unit_minerales == "fsmp") {
                $(".minerales").prop("disabled", true);

            }

            if (unit_vitaminas == "fsmp") {
                $(".vitamins").prop("disabled", true);

            }

        });


    },
    loadCycles: (cycles) => {
        if ($.fn.DataTable.isDataTable("#tabla_cycle")) {
            $("#tabla_cycle").DataTable().destroy();
        }

        $("#tabla_cycle").DataTable({
            "data": cycles,
            "dom": "frtip",
            "ordering": false,
            paging: false,
            "columns": [
                { "data": "fecha_action" },
                { "data": "NombreCompleto" },
                { "data": "action" }
            ],
            stateSave: false,
            "language": {
                "url": "../lib/datatables.net/lang/Spanish.json",
                "decimal": ",",
                "thousands": "."
            }
        });

    },
    loadProposalData: (proposal) => {
        $("#number").val(proposal.number);
        $("#company").val(proposal.company);
        $("#business_unit").val(proposal.business_unit);
        $("#creator").val(proposal.name_creator);
        $("#state").val(proposal.state);
        $("#approval_date").val(proposal.dt_approval_date);
        $("#approver_1").val(proposal.user_approver_1);
        $("#approver_2").val(proposal.user_approver_2);
        $("#approver_3").val(proposal.user_approver_3);
        $("#project_name").val(proposal.project_name);



    },
    loadProposalInfo: (info) => {
        $("#planification").val(info.planificacion);
        $("#released_date").flatpickr({
            "defaultDate": info.dr_released_date,
            minDate: "today",
            dateFormat: "d/m/Y",
            allowInput: true
        });
        /* $("#released_date").val(info.dr_released_date);*/
        $("#product_description").val(info.descripcion);
        $("#patent_proposal").val(info.patent);
        $("#regulations").val(info.regulations);
        $("#flavourings").val(info.flavourings);
        $("#nutritional_info").val(info.nutricional_info);
        $("#specific_needed").val(info.ingredientes_especificos);
        $("#physical_requirements").val(info.requerimientos_quimicos);
        $("#target_cost").val(info.coste_objetivo);
        $("#instructions_use").val(info.instrucciones);
        $("#releasing_params").val(info.parametros_salida);
        $("#primary_packaging").val(info.paquete_primario);
        $("#secondary_packaging").val(info.paquete_secundario);
        $("#volume_per_year").val(info.volumen_anyo);
        $("#sell_by_year").val(info.fecha_caducidad);
        $("#happpcc_system").val(info.happcc_system);
    },
    loadDocuments: (documents) => {
        if ($.fn.DataTable.isDataTable("#tabla_documents")) {
            $("#tabla_documents").DataTable().destroy();
        }

        $("#tabla_documents").DataTable({
            "data": documents,
            "dom": "frtip",
            "columns": [
                { "data": "dt_upload" },
                { "data": "NombreCompleto" },
                { "data": "name_document" },
                {
                    render: function (data, type, row) {

                        if (row.state == "Approved") {
                            return `<a href="#" class="btn btn-light btn-sm" onclick='modulo_new_proposal.download_file("` + row.name_document + `");'><i class="fa fa-download"></i></a>`;
                        } else {
                            return `<a href="#" class="btn btn-light btn-sm" onclick='modulo_new_proposal.download_file("` + row.name_document + `");'><i class="fa fa-download"></i></a>
                                <a href="#" class="btn btn-light btn-sm" onclick='modulo_new_proposal.delete_file("` + row.name_document + `");'><i class="fa fa-trash"></i></a>`;
                        }
                        
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
    subirArchivos: () => {
        var files = $("#inp_files").get(0).files;
        var fileData = new FormData();

        if (files.length > 0) {
            for (var i = 0; i < files.length; i++) {
                fileData.append("files", files[i]);
            }

            fileData.append("numero", $("#number").val());

            $.ajax({
                type: "POST",
                url: helper.baseUrl + "/PDP/FileUpload",
                dataType: "json",
                contentType: false, // Not to set any content header
                processData: false, // Not to process data
                data: fileData,
                success: function (result, status, xhr) {
                    if (result.success == 1) {
                        helper.mostrarAlertaOk("The file was upload successfully");
                        setTimeout(function () {
                            modulo_new_proposal.renderView($("#number").val());
                        }, 1000);

                    } else {
                        helper.mostrarAlertaError(result.mensaje);

                    }
                },
                error: function (xhr, status, error) {
                }
            });
        } else {
            helper.mostrarAlertaError("You must select a file")
        }



    },
    download_file: (name) => {

        var data = {
            name: name,
            numero: $("#number").val()
        }
        var url = helper.baseUrl + "/PDP/downloadFile";

        $.ajax({
            type: 'POST',
            url: url,
            xhrFields: {
                responseType: 'blob'
            },
            data: data,
            success: function (json) {
                var link = document.createElement('a');
                link.href = window.URL.createObjectURL(json);
                link.download = name;
                link.click();
                helper.QuitarLoader();

            },
            error: function () {
                console.log("Error");
            }
        });


    },
    delete_file: (name) => {
        if (confirm("Are you sure of remove this file??")) {
            var data = {
                name: name,
                numero: $("#number").val()
            }
            var url = helper.baseUrl + "/PDP/deleteFile";

            helper.ajax(url, "POST", data).then(result => {
                if (result.success) {
                    helper.mostrarAlertaOk(result.mensaje);

                    setTimeout(function () {
                        modulo_new_proposal.renderView($("#number").val());
                    }, 1000);
                } else {
                    helper.mostrarAlertaError(result.mensaje);
                }
            });
        }


    },
    loadMacronutrientes: (macronutrientes) => {



        if ($.fn.DataTable.isDataTable("#tabla_macronutrients")) {
            $("#tabla_macronutrients").DataTable().destroy();
        }

        modulo_nutricional_information.tabla_macronutrients = $('#tabla_macronutrients').DataTable({
            data: macronutrientes,
            "paging": false,
            "searching": false,
            "columns": [
                { "data": "macronutriente" },
                {
                    render: function (data, type, row, meta) {
                        return `<input type="text" value="` + row.unidad + `" class="form-control"/>`;
                    }
                },
                {
                    render: function (data, type, row, meta) {
                        return `<input type="number" step="any" value="` + row.valor + `" name="` + row.macronutriente + `" min="0" class="form-control"/>`;
                    }
                }
            ], "columnDefs": [
                { "width": "12%", "targets": 1 },
                { "orderDataType": "dom-text-numeric", "targets": 2 }
            ],
            "ordering": false
        });

    },
    loadMinerals: (minerals) => {


        if ($.fn.DataTable.isDataTable("#tabla_minerals")) {
            $("#tabla_minerals").DataTable().destroy();
        }

        modulo_nutricional_information.tabla_minerals = $('#tabla_minerals').DataTable({
            data: minerals,
            "paging": false,
            "searching": false,
            "columns": [
                { "data": "mineral" },
                {
                    render: function (data, type, row, meta) {
                        return `<input type="text" value="` + row.unidad + `" class="form-control minerales" name="` + row.mineral + `unidad"/>`;
                    }
                },
                {
                    render: function (data, type, row, meta) {
                        return `<input type="number" step="any" value="` + row.valor + `"  name="` + row.mineral + `" min="0" class="form-control minerales"/>`;
                    }
                }
            ], "columnDefs": [
                { "width": "12%", "targets": 1 },
                { "orderDataType": "dom-text-numeric", "targets": 2 }
            ],
            "ordering": false
        });

    },
    loadVitamins: (vitamins) => {

        if ($.fn.DataTable.isDataTable("#tabla_vitamins")) {
            $("#tabla_vitamins").DataTable().destroy();
        }

        modulo_nutricional_information.tabla_vitamins = $('#tabla_vitamins').DataTable({
            data: vitamins,
            "paging": false,
            "searching": false,
            "columns": [
                { "data": "vitamina" },
                {
                    render: function (data, type, row, meta) {
                        return `<input type="text" value="` + row.unidad + `" class="form-control vitamins" name="` + row.vitamina + `unidad"/>`;
                    }
                },
                {
                    render: function (data, type, row, meta) {
                        return `<input type="number" step="any" value="` + row.valor + `"  name="` + row.vitamina + `" min="0" class="form-control vitamins"/>`;
                    }
                }
            ], "columnDefs": [
                { "width": "12%", "targets": 1 },
                { "orderDataType": "dom-text-numeric", "targets": 2 }
            ],
            "ordering": false
        });

    },
    loadComments: (comments) => {
        if ($.fn.DataTable.isDataTable("#tabla_comment")) {
            $("#tabla_comment").DataTable().destroy();
        }

        $('#tabla_comment').DataTable({
            data: comments,
            "paging": false,
            "columns": [
                { "data": "dt_comment" },
                { "data": "NombreCompleto" },
                { "data": "text_comment" },
                {
                    render: function (data, type, row, meta) {
                        if (row.user_comment == user && row.state != "Approved") {
                            return `<a href="#" class="btn btn-light btn-sm" onclick='modulo_new_proposal.delete_comment("` + row.identificador + `");'><i class="fa fa-trash"></i></a>`;

                        } else {
                            return '';
                        }
                    }
                }
            ],
            "ordering": false
        });

    },
    delete_comment: (comment) => {
        if (confirm("Are you sure to delete this comment?")) {
            var data = {
                identificador: comment,
                operacion: "DELETE"
            }
            var url = helper.baseUrl + "/PDP/CRUDComments";

            helper.ajax(url, "POST", data).then(result => {
                if (result.success) {
                    helper.mostrarAlertaOk(result.mensaje);
                    setTimeout(function () {
                        modulo_new_proposal.renderView($("#number").val());
                    }, 1000);
                } else {
                    helper.MostrarAlertaError(result.mensaje);
                }
            });
        }

    },
    getAccion: (numero) => {
        if (numero != null && numero != "") {
            modulo_new_proposal.renderView(numero);
        } else {
            modulo_new_proposal.getNewNumber();
        }
    },
    validateTab: (element) => {
        //TODO:CAMBIAR
        var _element = $(element);
        var validatePane = _element.attr('data-bs-target');
        if (validatePane != "#cycle-tab") {
            var isValid = $(validatePane + ' :input').valid();
            var _li = _element.parent();

            console.log(validatePane + " - " + isValid);

            if (isValid) {
                _li.removeClass('alert-danger');
                _li.addClass('alert-success');
                return true;
            } else {
                _li.removeClass('alert-success');
                _li.addClass('alert-danger');
                return false;
            }
        }

    },
    validateForms: () => {

        $("#released_date").flatpickr({
            locale: helper.locale,
            minDate: "today",
            dateFormat: "d/m/Y",
            allowInput: true
        });



        $("#form-proposal").validate({
            rules: {
                company: {
                    required: true
                },
                business_unit: {
                    required: true
                },
                approver_1: {
                    required: true
                },
                approver_2: {
                    required: true
                },
                approver_3: {
                    required: true
                },
                project_name: {
                    required: true
                }
            },
            errorClass: "error fail-alert",
            messages: {
                business_unit: {
                    required: "Business unit is required",
                    min: "Business unit is required"
                },
                approver_1: {
                    required: "Approver 1 is required"
                }, approver_2: {
                    required: "Approver 2 is required"
                }, approver_3: {
                    required: "Approver 3 is required"
                }, project_name: {
                    required: "Project name is required"
                }, company: {
                    required: "Company is required"
                }
            }
        });




        $("#form_information").validate({
            rules: {
                target_cost: {
                    required: true
                },
                primary_packaging: {
                    required: true
                },
                secondary_packaging: {
                    required: true
                },
                volume_per_year: {
                    required: true
                },
                planification: {
                    required: true
                },
                released_date: {
                    required: true
                },
                unit: {
                    required: true
                },
                unit_minerals: {
                    required: true
                },
                unit_vitamins: {
                    required: true
                },
                
            },
            ignore: [],
            errorClass: "error fail-alert",
            messages: {

                target_cost: {
                    required: "Target cost of ingredients is required"
                },
                primary_packaging: {
                    required: "Primary packaging is required"
                }, secondary_packaging: {
                    required: "Secondary packaging is required"
                }, volume_per_year: {
                    required: "Volume per year is required"
                }, released_date: {
                    required: "Released date is required"
                }, planification: {
                    required: "Planification is required"
                }
            }
        });

        $.validator.messages.required = 'required';
    },
}