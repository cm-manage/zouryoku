/**
 * SweetAlert2をSuccessで表示します
 * @param {string} title Alertのタイトルです
 * @param {string} msg Alertのメッセージです
 * @param {Function} func メッセージ表示後に実行するFunction
 */
function showSwal(title, msg, func, type) {
    func = defaultArgument(func, function () { });
    
    Swal.fire({
        title: title,
        html: msg,
        type: type,
    }).then(func);
}

function successSwal(title, msg, func) {
    showSwal(title, msg, func, 'success');
}

function errorSwal(title, msg, func) {
    showSwal(title, msg, func, 'error');
}

function warningSwal(title, msg, func) {
    showSwal(title, msg, func, 'warning');
}

/**
 * 登録成功用、SweetAlert2をSuccessで表示します
 * @param {string} msg
 * @param {Function} func
 */
function registerSuccessSwal(title, msg, func) {
    func = defaultArgument(func, function () { });
    let regTitle = '登録しました';
    if (isValid(title)) {
        regTitle = title;
    }
    successSwal(regTitle, msg, func)
}
/**
 * 削除成功用、SweetAlert2をSuccessで表示します
 * @param {string} msg
 * @param {Function} func
 */
function deleteSuccessSwal(msg, func) {
    func = defaultArgument(func, function () { });

    successSwal('削除しました', msg, func)
}
/**
 * SweetAlert2で確認ダイアログを表示します
 * @param {string} title
 * @param {string} msg
 * @param {string} confirmButtonText
 * @param {Function} okFunc
 * @param {string} width
 */
function confirmSwal(title, msg, confirmButtonText, okFunc, width) {
    confirmOkOrNgFuncSwal(title, msg, confirmButtonText, okFunc, null, width);
}

/**
 * SweetAlert2で確認ダイアログを表示します
 * @param {string} title
 * @param {string} msg
 * @param {string} confirmButtonText
 * @param {Function} okFunc
 * @param {Function} ngFunc
 * @param {string} width
 */
function confirmOkOrNgFuncSwal(title, msg, confirmButtonText, okFunc, ngFunc, width) {
    Swal.fire({
        title: title,
        html: msg,
        type: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: confirmButtonText,
        cancelButtonText: 'キャンセル',
        width: width
    }).then(function (result) {
        if (result.value && okFunc) {
            okFunc();
        } else if (ngFunc) {
            ngFunc();
        }
    });
};

function commonConfirm(msg, subMsg, okFunc, ngFunc) {
    let title = '登録します。よろしいですか？';
    if (isValid(msg)) {
        title = msg;
    }
    let func = function () {
        $('form').submit();
    };
    if (okFunc) {
        func = okFunc;
    }
    confirmOkOrNgFuncSwal(title, subMsg, 'OK', func, ngFunc);
}
/**
 * SweetAlert2で削除確認ダイアログを表示します
 * @param {string} title
 * @param {string} msg
 * @param {Function} okFunc
 */
function deleteConfirmSwal(title, msg, okFunc) {
    confirmSwal(title, msg, '削除', okFunc);
}

/**
 * SweetAlert2で削除確認ダイアログを表示し、削除後に削除成功情報をSuccessで表示します
 * @param {URL} url
 * @param {string} title
 * @param {string} msg
 * @param {Function} func
 */
function deleteConfirmSuccessSwal(url, title, msg, func) {
    deleteConfirmSwal(title, msg, function () {
        $.ajax({
            type: "DELETE",
            url: url
        }).done(function (result) {
            successResponse(result, function () {
                deleteSuccessSwal(result.message, func());
            });
        }).fail(function (response) {
            failResponse(response);
        });
    })
}

/**
 * SweetAlert2で送信確認ダイアログを表示します
 * @param {string} title
 * @param {string} msg
 * @param {Function} okFunc
 */
function mailsentConfirmSwal(title, msg, okFunc) {
    confirmSwal(title, msg, '送信', okFunc);
}

/**
 * SweetAlert2で送信確認ダイアログを表示し、送信後に送信成功情報をSuccessで表示します
 * @param {URL} url
 * @param {string} title
 * @param {string} msg
 * @param {Function} func
 */
function mailsentConfirmSuccessSwal(url, title, msg, func) {
    var formdata = $('form').serializeArray();
    mailsentConfirmSwal(title, msg, function () {
        $.post({
            url: url,
            data: formdata,
            processData: false,
            contentType: false,
        }).done(function (result) {
            successResponse(result, function () {
                mailsentSuccessSwal(result.message, func);
            });
        }).fail(function (response) {
            failResponse(response);
        });
    })
}

/**
* SweetAlert2でトースト表示
* @param {string} text
* @param {string} html
* @param {string} type
* @param {number} timer
* @param {string} position
*/
function toastSwal(text, html, type, timer, position) {
    type = type ?? 'success';
    timer = timer ?? 2000;
    position = position ?? 'top-end';

    Swal.fire({
        text: text,
        html: html,
        position: position,
        width: 'auto',
        type: type,
        toast: true,
        showConfirmButton: false,
        timer: timer,
        //didOpen: (toast) => {
        //    toast.addEventListener('click', () => Swal.close());
        //},
    });
}