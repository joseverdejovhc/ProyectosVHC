var modulo_comments = {
    init: () => {
        modulo_comments.putEventsOnButtons();
    },
    putEventsOnButtons: () => {
        $("#btn_new_comment").on("click", function (e) {
            e.preventDefault();
            AbrirModal("#modal_comments");
        });

        $("#btn_save_comment").on("click", function (e) {
            e.preventDefault();
            modulo_comments.generateAndSaveComment();
        })
    },
    generateAndSaveComment: () => {
        var data = modulo_comments.generateData();
        modulo_comments.saveComment(data);
    },
    generateData: () => {
        return {
            operacion: "INSERT",
            comment: $("#comment").val(),
            proposal: $("#number").val()            
        }
    },
    saveComment: (data) => {
        var url = helper.baseUrl + "/PDP/CRUDComments";
        helper.ajax(url, "POST", data).then(result => {
            if (result.success == 1) {
                helper.mostrarAlertaOk(result.mensaje);
                CerrarModal();

                setTimeout(function () {
                    modulo_new_proposal.renderView($("#number").val());
                }, 1000);
                
            } else {
                helper.mostrarAlertaError(result.mensaje);
            }
        });
    }



}