var modulo_index = {
    init: () => {
        modulo_index.loadMyProposals();
    },
    putEventsOnButtons: () => {

    },
    loadMyProposals: () => {
        var url = helper.baseUrl + '/PDP/getAllProposals';

        helper.ajax(url, "GET").then(result => {
            var my_results = JSON.parse(result.my_proposal);
            modulo_index.loadDatatableMyResults(my_results);

            if (result.appoved_1 != null) {
                var appoved_1 = JSON.parse(result.appoved_1);
                modulo_index.loadDatatableApproved1(appoved_1);
            }

            if (result.all_proposals != null) {
                var all_proposals = JSON.parse(result.all_proposals);
                modulo_index.loadDatatableAllProposals(all_proposals);

            }


        });
    },
    loadDatatableMyResults: (my_results) => {
        if ($.fn.DataTable.isDataTable("#tabla_my_proposals")) {
            $("#tabla_my_proposals").DataTable().destroy();
        }

        $("#tabla_my_proposals").DataTable({
            "data": my_results,
            dom: '<"top"<"left-col"B><"center-col"l><"right-col"f>>rtip',
            "columns": [
                { "data": "code" },
                { "data": "creator" },
                { "data": "fech_creacion" },
                { "data": "project_name" },
                { "data": "state" },
                { "data": "company" },

                {
                    render: function (data, type, row) {
                        return `<a href="` + helper.baseUrl + `/PDP/NewProposal?number=` + row.code + `"  target="_blank" class="btn btn-light btn-sm"><i class="fa fa-eye"></i></a>`;
                    }
                }/*,

                {
                    render: function (data, type, row) {
                        if (row.state == "Rejected") {
                            return `<a href="#" class="btn btn-light btn-sm" onclick="modulo_index.deleteProposal('` + row.code + `');"><i class="fa fa-trash"></i></a>`;
                        } else {
                            return ``;
                        }
                    }
                }*/
            ],
            buttons: [
                {
                    text: 'New Proposal',
                    className: 'btn btn-warning mr-2',
                    action: function (e, dt, node, config) {
                        modulo_index.nuevoproposal();
                    }
                },
                {
                    text: '<i class="fa fa-refresh"></i>',
                    className: 'btn btn-info btn-xs text-white btn_refresh margin',
                    action: function (e, dt, node, config) {
                        modulo_index.loadMyProposals();
                    }
                }
            ],
            "paging": true
        });
    },
    loadDatatableApproved1: (appoved_1) => {
        if ($.fn.DataTable.isDataTable("#tabla_my_appoval_proposals")) {
            $("#tabla_my_appoval_proposals").DataTable().destroy();
        }

        $("#tabla_my_appoval_proposals").DataTable({
            "data": appoved_1,
            dom: '<"top"<"left-col"B><"center-col"l><"right-col"f>>rtip',
            "columns": [
                { "data": "code" },
                { "data": "creator" },
                { "data": "fech_creacion" },
                { "data": "project_name" },
                { "data": "state" },
                { "data": "company" },

                {
                    render: function (data, type, row) {
                        return `<a href="` + helper.baseUrl + `/PDP/NewProposal?number=` + row.code + `"  target="_blank" class="btn btn-light btn-sm"><i class="fa fa-eye"></i></a>`;
                    }
                }
            ],
            buttons: [
                {
                    text: '<i class="fa fa-refresh"></i>',
                    className: 'btn btn-info btn-xs text-white btn_refresh',
                    action: function (e, dt, node, config) {
                        modulo_index.loadMyProposals();
                    }
                }
            ],
            "paging": true,
            stateSave: false
        });
    },
    loadDatatableAllProposals: (all_proposals) => {
        if ($.fn.DataTable.isDataTable("#tabla_all_proposals")) {
            $("#tabla_all_proposals").DataTable().destroy();
        }

        $("#tabla_all_proposals").DataTable({
            "data": all_proposals,
            dom: '<"top"<"left-col"B><"center-col"l><"right-col"f>>rtip',
            "columns": [
                { "data": "code" },
                { "data": "creator" },
                { "data": "fech_creacion" },
                { "data": "project_name" },
                { "data": "state" },
                { "data": "company" },

                {
                    render: function (data, type, row) {
                        return `<a href="` + helper.baseUrl + `/PDP/NewProposal?number=` + row.code + `"  target="_blank" class="btn btn-light btn-sm"><i class="fa fa-eye"></i></a>`;
                    }
                }
            ],
            buttons: [
                {
                    text: '<i class="fa fa-refresh"></i>',
                    className: 'btn btn-info btn-xs text-white btn_refresh',
                    action: function (e, dt, node, config) {
                        modulo_index.loadMyProposals();
                    }
                }
            ],
            "paging": true,
            stateSave: false
        });
    },
    deleteProposal: (proposal) => {
        if (confirm("Are you sure to delete this proposal?")) {
            var data = {
                operacion: "DELETE",
                number: proposal
            }

            var url = helper.baseUrl + '/PDP/CRUDProposal';

            helper.ajax(url, "POST", data).then(result => {
                if (result.success == 1) {
                    MostrarOk("#alerta", result.mensaje);
                    modulo_index.init();
                } else {
                    MostrarError("#alerta", result.mensaje);
                }
            });
        }


    },
    loadProposal: (proposal) => {

        var data = {
            number: proposal
        }

        var url = helper.baseUrl + '/PDP/NewProposal';

        helper.ajax(url, "POST", data).then(result => {

        });

        /*   localStorage.setItem("accion", "editar");
           localStorage.setItem("number", proposal);
           window.open(helper.baseUrl + '/PDP/NewProposal');*/
    },
    nuevoproposal: () => {
        window.open(helper.baseUrl + "/PDP/NewProposal");
    }

}