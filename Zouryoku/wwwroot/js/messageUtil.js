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
 * メッセージ本文をHTMLエスケープする
 * @param {string} str
 * @returns {string}
 */
function escapeHtml(str) {
    if (!str) return '';
    return str.replace(/[&<>"'`]/g, function (match) {
        const escape = {
            '&': '&amp;',
            '<': '&lt;',
            '>': '&gt;',
            '"': '&quot;',
            "'": '&#39;',
            '`': '&#96;'
        };
        return escape[match];
    });
}

/**
 * メッセージを表示する共通処理
 * @param {string} msg メッセージ本文
 * @param {string} type メッセージ種別: 'error' | 'info' | 'warning'
 * @param {boolean} [isAdd] エラー時のみ追加表示
 */
function showMessage(msg, type, isAdd) {
    const alertClass = {
        error: 'alert-danger alert-errors',
        info: 'alert-success alert-infos',
        warning: 'alert-warning alert-warnings'
    }[type] || 'alert-danger alert-errors';

    const alertIcon = {
        error: 'fa-exclamation-triangle',
        info: 'fa-info-circle',
        warning: 'fa-exclamation-circle'
    }[type] || 'fa-exclamation-triangle';

    const list = msg.split(/\r\n|\n/);
    let msgHtm = "";
    list.forEach(m => {
        if (m.length) {
            msgHtm += "<li>" + escapeHtml(m) + "</li>";
        }
    });

    if (type === 'error' && isAdd && $(".form-errors").length > 0) {
        $(".form-errors").children("ul").append(msgHtm);
        return;
    }

    const notification =
        `<div class='alert ${alertClass}'>` +
        `   <div style="margin: auto; "><i class="fas ${alertIcon} fa-fw"></i></div > ` +
        "   <ul style='margin: unset; width: 100%; list-style: inside;  padding: unset; padding-left: 15px; max-height: 200px; overflow-y: auto;'>" + msgHtm + "</ul>" +
        "   <button type='button' class='btn-close' data-bs-dismiss='alert' aria-label='Close' style='background-color: transparent; cursor: pointer; align-items: start; display: flex;' onclick='messageClose(this)'><span aria-hidden='true'>×</span></button>" +
        "</div > ";
    $("#message-notification").html(notification);
    $("#modal-message-notification").html(notification);
}

/**
 * エラーメッセージを表示する
 * @param {any} msg
 * @param {boolean} [isAdd]
 */
function errorMessage(msg, isAdd) {
    showMessage(msg, 'error', isAdd);
}

/**
 * 情報メッセージを表示する
 * @param {string} msg
 */
function infoMessage(msg) {
    showMessage(msg, 'info');
}

/**
 * 警告メッセージを表示する
 * @param {string} msg
 */
function warningMessage(msg) {
    showMessage(msg, 'warning');
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