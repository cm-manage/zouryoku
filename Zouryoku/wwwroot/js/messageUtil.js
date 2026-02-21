/**
 * ajax処理が成功した場合の処理
 * @param {any} response
 * @param {Function} func
 */
function successResponse(response, successFunc) {

    console.log(response);

    clearValidation();
    if (response.isSuccess) {
        // 正常時
        successFunc();
    } else {
        // エラー表示時
        errorMessage(response.message);
    }
}
/**
 * ajaxにて処理を行った場合の処理
 * statusによって処理を分ける
 * @param {any} response
 */
function failResponse(response) {

    console.log(response);

    switch (response.status) {
        case 400:
            validation(response);
            break;
        case 404:
            notFoundError(response);
            break;
        case 409:
            conflictError(response);
            break;
        case 500:
            internalServerError(response);
            break;
        default:
            if (response.statusText == "timeout") {
                timeoutError();
            }
            else {
                console.log(response);
            }
            break;
    }
}
/**
 * validationエラー表示表示処理(400)
 * 
 * @param {any} response 登録処理時にWEB APIから返却されるエラー
 */
function validation(response) {
    var errorJson = defaultArgument(response.responseJSON.value, response.responseJSON.errors);
    clearValidation();
    $.each(errorJson, function (errorKey, errorValue) {
        var form = $('#' + errorKey.substring(0, 1).toUpperCase() + errorKey.substring(1));
        var parent = form.parent();
        // 入力フィールドのid は 'プロパティ名' を使用すること
        // dont-show-errorクラスを持つ時はエラーを表示しない
        if (!form.hasClass('dont-show-error')) {
            $.each(errorValue, function (_, value) {
                var message = '<div class="form-errors">' + value + '</div>';
                if (parent.hasClass('form-inline')) {
                    // 親要素にform-inlineクラスを指定している場合、要素の横にエラーが出てしまうため親要素の上にエラーを出す
                    parent.before(message);
                } else {
                    form.before(message);
                }
            });
        }
    });
}
/**
 * NotFound(404)時の処理
 * @param {any} response
 */
function notFoundError(response) {

    var data = response.responseJSON;
    var message = data.message;

    clearValidation();
    errorMessage(message);
}
/**
 * Conflict(409)時の処理
 * @param {any} response
 */
function conflictError(response) {

    var data = response.responseJSON;
    var message = data.message;

    clearValidation();
    errorMessage(message);
}
/**
 * InternalServerError(500)時の処理
 * @param {any} response
 */
function internalServerError(response) {
    var message = "";
    if (response.responseJSON) {
        var data = response.responseJSON.InnerException;

        switch (data.ClassName) {
            case "Npgsql.PostgresException":
                message = data.Detail;
                break;
            default:
                message = data.MessageText;
                break;
        }
    }
    else {
        message = response.responseText;
    }
    clearValidation();
    errorMessage(message);
}
/**
 * Timeout時の処理
 * @param {any} response
 */
function timeoutError() {
    clearValidation();
    errorMessage("タイムアウトしました。管理者に問い合わせてください。");
}
/**
 * エラーメッセージを表示する
 * @param {any} msg
 */
function errorMessage(msg, isAdd) {
    //var errorNotification =
    //    "<div class='alert alert-danger form-errors'>" + msg + "</div > "
    let list = msg.split(/\r\n|\n/);
    let msgHtm = "";
    list.forEach(m => {
        if (m.length) {
            msgHtm += "<li>" + m + "</li>";
        }
    });
    if (isAdd && $(".form-errors").length > 0) {
        $(".form-errors").children("ul").append(msgHtm);
        return;
    }
    let errorNotification =
        "<div class='alert alert-danger alert-errors'>" +
        '   <div style="margin: auto; "><i class="fas fa-exclamation-triangle fa-fw"></i></div > ' +
        "   <ul style='margin: unset; width: 100%; list-style: inside;  padding: unset; padding-left: 15px; max-height: 200px; overflow-y: auto;'>" + msgHtm + "</ul>" +
        "   <button type='button' class='close' data-dismiss='alert' aria-label='Close' style='background-color: transparent; cursor: pointer; align-items: start; display: flex;' onclick='messageClose(this)'><span aria-hidden='true'>×</span></button>" +
        "</div > ";
    $("#message-notification").html(errorNotification);
    $("#modal-message-notification").html(errorNotification);
}

/**
 * 情報メッセージを表示する
 * @@param {string} msg
 */
function infoMessage(msg) {
    const list = msg.split(/\r\n|\n/);
    let msgHtm = "";
    list.forEach(m => {
        if (m.length) {
            msgHtm += "<li>" + m + "</li>";
        }
    });
    const infoNotification =
        "<div class='alert alert-success alert-infos'>" +
        '   <div style="margin: auto; "><i class="fas fa-exclamation-triangle fa-fw"></i></div > ' +
        "   <ul style='margin: unset; width: 100%; list-style: inside;  padding: unset; padding-left: 15px; max-height: 200px; overflow-y: auto;'>" + msgHtm + "</ul>" +
        "   <button type='button' class='close' data-dismiss='alert' aria-label='Close' style='background-color: transparent; cursor: pointer; align-items: start; display: flex;' onclick='messageClose(this)'><span aria-hidden='true'>×</span></button>" +
        "</div > ";
    $("#message-notification").html(infoNotification);
    $("#modal-message-notification").html(infoNotification);
}

/**
 * 警告メッセージを表示する
 * @@param {string} msg
 */
function warningMessage(msg) {
    const list = msg.split(/\r\n|\n/);
    let msgHtm = "";
    list.forEach(m => {
        if (m.length) {
            msgHtm += "<li>" + m + "</li>";
        }
    });
    const warningNotification =
        "<div class='alert alert-warning alert-warnings'>" +
        '   <div style="margin: auto; "><i class="fas fa-exclamation-triangle fa-fw"></i></div > ' +
        "   <ul style='margin: unset; width: 100%; list-style: inside;  padding: unset; padding-left: 15px; max-height: 200px; overflow-y: auto;'>" + msgHtm + "</ul>" +
        "   <button type='button' class='close' data-dismiss='alert' aria-label='Close' style='background-color: transparent; cursor: pointer; align-items: start; display: flex;' onclick='messageClose(this)'><span aria-hidden='true'>×</span></button>" +
        "</div > ";
    $("#message-notification").html(warningNotification);
    $("#modal-message-notification").html(warningNotification);
}

function messageClose(obj) {
    $(obj).closest("div#message-notification.fix_area").empty();
    //$(obj).closest("div#modal-message-notification.fix_area").empty();
    // 暫定対応 上記のコメントアウトしているコードではモーダルのごと消えてしまうため下記のコードで対応
    $('#modal-message-notification').children().css('display', 'none');
}

function errorMessageClear() {
    $("#message-notification").empty();
    $("#modal-message-notification").empty();
}

// validationエラーをクリア
function clearValidation() {
    $('.form-errors span').remove();
}