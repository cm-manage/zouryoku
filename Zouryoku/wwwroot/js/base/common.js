//レスポンスの定数 Model.Enums.ResponseStatusで定義している値と合わせること
const responseStatusSuccess = 1;
const responseStatusWarning = 2;
const responseStatusError   = 3;

$(function () {
    // 数値入力対応
    //integer-digitで整数部の桁数指定（デフォルト9桁）
    //decimal-digitで小数部の桁数指定（デフォルト0桁）
    setNumeric($(".numeric"));
    // 数値入力対応（正の数）
    //integer-digitで整数部の桁数指定（デフォルト9桁）
    //decimal-digitで小数部の桁数指定（デフォルト0桁）
    setNumeric($(".numeric-positive"));
    // カンマ付与
    setComma($(".comma"));

    // ファンシーマルチプルセレクト
    setMultipleSelect($('.multiple-select'));
    // 表示のみマルチプルセレクト
    setOnlyDisplayMultipleSelect($('.only-display-multiple-select'));
    // 表示なしマルチプルセレクト
    setNotDisplayMultipleSelect($('.not-display-multiple-select'));
    // チェックボックスをトグルボタンに変更
    setCheckToggleBtn($(".check-toggle-btn"));
});

/**
 * 数値入力対応
 * @param {any} ctrls
 * attr integer-digit : 整数部の桁数（デフォルト9桁）
 * attr decimal-digit : 小数部の桁数（デフォルト0桁）
 */
function setNumeric(ctrls) {
    const numericKeys = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.', '-'];
    const canInputVals = ['', '.', '-'];
    let isOK = (value, canNegative, integerDigit, decimalDigit) => {
        // 数値変換できないとき
        if (!$.isNumeric(value) && !canInputVals.includes(value)) {
            return false;
        }
        // 0連続入力
        if (RegExp(/^[0]+$/).test(value) && value.length > 1) {
            return false;
        }

        let num = Number(value);

        // 負の数
        if (!canNegative && (value == '-' || Math.sign(num) == -1)) {
            return false;
        }
        // 整数部桁数
        let currentIntegerDigit = Math.floor(Math.log10(Math.abs(value))) + 1;
        if (integerDigit < currentIntegerDigit) {
            return false;
        }
        // 小数部桁数
        let currentDecimalDigit = value.includes('.') ? value.split('.')[1].length : 0;
        if (decimalDigit == 0 && value.includes('.')) {
            return false;
        }
        if (decimalDigit < currentDecimalDigit) {
            return false;
        }

        return true;
    };

    ctrls.each((_, control) => {
        let ctrl = $(control);
        let canNegative = ctrl.hasClass("numeric-positive") ? false : true;
        let integerDigit = parseInt(ctrl.attr("integer-digit") ? ctrl.attr("integer-digit") : 9);
        let decimalDigit = parseInt(ctrl.attr("decimal-digit") ? ctrl.attr("decimal-digit") : 0);
        let beforeVal = null;
        let beforeSelectionStart = null;
        let beforeSelectionEnd = null;

        // type="number" のスピンボタンを消す
        ctrl.attr('type', 'text');

        // 直前の値
        ctrl.on('keydown', e => {
            // 選択範囲
            let selectionStart = ctrl.get(0).selectionStart;
            let selectionEnd = ctrl.get(0).selectionEnd;

            // 前回入力情報
            beforeVal = ctrl.val();
            beforeSelectionStart = selectionStart;
            beforeSelectionEnd = selectionEnd;

            // 入力後の値（暫定）
            let afterVal = numericKeys.includes(e.key) ? ctrl.val().slice(0, selectionStart) + e.key + ctrl.val().slice(selectionEnd)
                : e.key == 'Backspace' ? ctrl.val().slice(0, selectionStart - 1) + ctrl.val().slice(selectionEnd)
                    : e.key == 'Delete' ? ctrl.val().slice(0, selectionStart) + ctrl.val().slice(selectionEnd + 1)
                        : ctrl.val();

            return isOK(afterVal, canNegative, integerDigit, decimalDigit);
        }).on('input', () => {
            if (!isOK(ctrl.val(), canNegative, integerDigit, decimalDigit)) {
                ctrl.val(beforeVal);
                ctrl.get(0).selectionStart = beforeSelectionStart;
                ctrl.get(0).selectionEnd = beforeSelectionEnd;
            }
        }).on('focusout', () => {
            if (!$.isNumeric(ctrl.val())) {
                ctrl.val('');
            }
            else {
                ctrl.val(Number(ctrl.val()));
            }
        });
    });
}

function setComma(ctrls) {
    ctrls.each((_, control) => {
        let ctrl = $(control);
        //初期値もカンマ区切り
        ctrl.val(commaBuff(ctrl.val()));

        ctrl.on('focusin', () => {
            if (ctrl.val()) {
                ctrl.val(commaDebuff(ctrl.val()));
            }
        }).on('focusout', () => {
            ctrl.val(commaBuff(ctrl.val()));
        });
    });
}

/**
 * カンマを付与
 * @param {any} val
 */
function commaBuff(val) {
    let result = val;
    if ($.isNumeric(result) && result) {
        return Number(result).toLocaleString('ja');
    }
    return result;
}

/**
 * カンマを除去
 * @param {any} val
 */
function commaDebuff(val) {
    return val.replace(/,/g, '');
}

function getDatePickerParam(param, ctrl, att) {
    let res = param;
    if (ctrl.attr(att)) {
        res = ctrl.attr(att);
    }
    return res ? new Date(res) : new Date();
}

function getAspNetId(str) {
    return str.replace(/\./g, "_");
}

function getAspNetName(str) {
    return str.replace(/_/g, ".");
}

function isOverMaxlength(ctrl, val, length) {
    let value = val ? val : ctrl.val();
    let maxlength = length ? length : ctrl.attr('maxlength');
    if (value && value.length >= maxlength) {
        return true;
    }
    return false;
}

function subStrMaxlength(ctrl, val, length) {
    let value = val ? val : ctrl.val();
    let maxlength = length ? length : ctrl.attr('maxlength');
    if (isOverMaxlength(ctrl, value, maxlength)) {
        ctrl.val(value.substr(0, maxlength));
    }
}

function isOverLength(ctrl, val, maxlen, digitlen) {
    let value = val ? val : ctrl.val();
    let maxlength = maxlen ? maxlen : ctrl.attr('maxlength');
    let digitlength = digitlen ? digitlen : ctrl.attr('digitlength');
    if (value) {
        let dIndex = value.indexOf(".");
        let befVal = dIndex != -1 ? value.substr(0, dIndex) : value;
        let aftVal = dIndex != -1 ? value.substr(dIndex + 1) : null;
        if (digitlength && digitlength > 0) {
            maxlength = maxlength - digitlength;
            if (dIndex != -1) {
                maxlength++;
            }

            console.log("befVal ", befVal);
            console.log("aftVal ", aftVal);
            console.log("dIndex ", dIndex);
            console.log("maxlength ", maxlength);
            console.log("digitlength ", digitlength);
            if (aftVal && aftVal.length > digitlength) {
                return true;
            }
        }
        if (befVal.length > maxlength) {
            return true;
        }
    }
    return false;
}

function subStrLength(ctrl, val, maxlen, digitlen) {
    let value = val ? val : ctrl.val();
    let maxlength = maxlen ? maxlen : ctrl.attr('maxlength');
    let digitlength = digitlen ? digitlen : ctrl.attr('digitlength');
    if (isOverLength(ctrl, value, maxlength, digitlength)) {
        let dIndex = value.indexOf(".");
        let befVal = dIndex != -1 ? value.substr(0, dIndex) : value;
        let aftVal = dIndex != -1 ? value.substr(dIndex + 1) : null;
        //console.log("befVal ", befVal);
        //console.log("aftVal ", aftVal);
        //console.log("dIndex ", dIndex);
        //console.log("digitlength ", digitlength);
        if (digitlength && digitlength > 0) {
            maxlength = maxlength - digitlength;
            if (dIndex != -1) {
                maxlength++;
            }
            //console.log("maxlength ", maxlength);
            //console.log("digitlength ", digitlength);
            if (aftVal && aftVal.length >= digitlength) {
                aftVal = aftVal.substr(0, digitlength);
            }
        }
        if (befVal.length >= maxlength) {
            befVal = befVal.substr(0, maxlength);
        }

        let resultVal = befVal;
        if (aftVal && aftVal.length > 0) {
            resultVal = befVal + "." + aftVal;
        }

        ctrl.val(resultVal);
    }
}

/**
 * 引数が有効かどうかチェックします。
 * undefined、null、空文字のいずれからならfalseが返ります。
 * @param {any} data
 */
function isValid(data) {
    return !(isUndefined(data) || isNullOrEmpty(data));
}

/**
 * 引数がundefinedかどうかチェックします。
 * @param {any} data
 */
function isUndefined(data) {
    return (typeof data === 'undefined') ? true : false;
}

/**
 * 引数がnullまたは空文字どうかチェックします。
 * @param {any} data
 */
function isNullOrEmpty(data) {
    return (!data || data.toString().toLowerCase() == 'null') ? true : false;
}

/**
 * 引数が空文字かどうかチェックします。
 * @param {any} data
 */
function isNull(data) {
    return (typeof data === null) ? true : false;
}

/**
 * undefinedの場合、デフォルト値を設定します
 * @param {any} arg
 * @param {any} defaultVal
 */
function defaultArgument(arg, defaultVal) {
    if (isUndefined(arg)) arg = defaultVal;
    return arg;
}

/**
 * iframe用のURLを返します。
 * @param {URL} url /iframeのurl(クエリなし)
 * @param {number} id /iframeに表示するデータのid
 */
function idParamUrl(url, id) {
    return toQueryUrl(url) + 'id=' + id;
};

/**
 * URLに適切なクエリ記号を付与して返します。
 * @param {URL} url
 */
function toQueryUrl(url) {
    if (url && url.indexOf('?') == -1) {
        return url + "?";
    } else {
        return url + "&";
    }
}

/**
 * 指定idの前後にあるidを返します
 * @param {number} id /iframeに表示するデータのid
 * @param {Array} idList /gridデータのidのリスト
 */
function prevNextId(id, idList) {
    var selectedIdx = 0;
    $.each(idList, function (index, obj) {
        if (obj == id) {
            selectedIdx = index;
        };
    });
    // ひとつ前のデータのidを取得
    var prevId = 0;
    if (selectedIdx >= 1) {
        prevId = idList[selectedIdx - 1];
    };
    // ひとつ後ろのデータのidを取得
    var nextId = 0;
    if ((Object.keys(idList).length - 1) > selectedIdx) {
        nextId = idList[selectedIdx + 1];
    };

    var result = new Object();
    result.prevId = prevId;
    result.nextId = nextId;

    return result;
}

// 日付をyyyy/MM/ddにフォーマット
function formatDate(date) {
    if (date != null) {
        var d = new Date(date),
            month = '' + (d.getMonth() + 1),
            day = '' + d.getDate(),
            year = d.getFullYear();

        if (month.length < 2) month = '0' + month;
        if (day.length < 2) day = '0' + day;

        return [year, month, day].join('/');
    } else {
        return null;
    }
}

// input[type=number]の上下矢印とスクロールによる数値の増減を禁止
$('input[type=number]')
    .on('wheel.disableScroll', function (e) {
        e.preventDefault();
    }).keydown(function (e) {
        return (e.keyCode == 40 || e.keyCode == 38) ? false : true;
    });

//CheckBoxの値をレンダリングするには、（自動生成される）hiddenのコントロールの値を変更する必要がある
$('input[type=checkbox]').on('click', function (e) {
    if ($(this).is(":checked")) {
        $('input[type=hidden][name="' + $(this).attr("name") + '"]').val(true);
    } else {
        $('input[type=hidden][name="' + $(this).attr("name") + '"]').val(false);
    }
});

function toJson(key, val) {
    //console.log("val.length", val.length);
    //if (val.length >= 2) {
    //    let result = "";
    //    $.each(val, function (index, data) {
    //        result +=
    //    });
    //    return { name: key, value: val };
    //}
    return { name: key, value: val };
}

function toUrl(val) {
    return '?handler=' + val;
}

/**
 * get送信する非同期Ajax
 * @param {URL} url /url
 * @param {any} data /送信するJSONデータ
 * @param {any} callback /successの際のコールバックメソッド
 * @param {any} isShowDialog / 通信成功時ダイアログ表示するか
 * @param {boolean} global / グローバルイベントを有効にするか（ajaxStart の $.blockUI を表示するか）
 */
function sendGetAjax(url, data, callback, isShowDialog, global = true) {
    $.get({
        url: url,
        data: data,
        global: global
    }).always(function () {
        // 再度エラーチェックしたいからクリアしない
        //clearValidation();
    }).done(function (result) {
        if (result) {
            if (result.status == responseStatusSuccess) {
                if (isShowDialog) {
                    registerSuccessSwal(null, result.message);
                }
                if (callback) {
                    callback(result.data);
                }
            }
            else if (result.status == responseStatusWarning) {
                commonConfirm(result.message, null, function () {
                    if (callback) {
                        callback(result.data);
                    }
                });
            }
            else if (result.status == responseStatusError) {
                errorMessage(result.message);
            }
        }
    }).fail(function (response, textStatus, errorThrown) {
        failResponse(response);
        //console.log("textStatus     : " + textStatus);    // タイムアウト、パースエラー
        //console.log("errorThrown    : " + errorThrown.message); // 例外情報
    });
}

/**
 * post送信する非同期Ajax
 * @param {URL} url /url
 * @param {any} data /送信するJSONデータ
 * @param {any} callback /successの際のコールバックメソッド
 * @param {any} isShowDialog / 通信成功時ダイアログ表示するか
 * @param {any} timeout / タイムアウト時間(ms)
 * @param {any} global / グローバルイベントを有効にするか（ajaxStart の $.blockUI を表示するか）
 */
function sendAjax(url, data, callback, isShowDialog, timeout, global) {
    let time = timeout ?? (1000 * 60 * 1);
    let glb = global ?? true;
    commonSendAjax(url, data, true, callback, isShowDialog, time, glb);
}

/**
 * post送信する同期Ajax
 * @param {URL} url /url
 * @param {any} data /送信するJSONデータ
 * @param {any} callback /successの際のコールバックメソッド
 * @param {any} isShowDialog / 通信成功時ダイアログ表示するか
 * @param {any} timeout / タイムアウト時間(ms)
 * @param {any} global / グローバルイベントを有効にするか（ajaxStart の $.blockUI を表示するか）
 */
function syncSendAjax(url, data, callback, isShowDialog, timeout, global) {
    let time = timeout ?? (1000 * 60 * 1);
    let glb = global ?? true;
    commonSendAjax(url, data, false, callback, isShowDialog, time, glb);
}

function commonSendAjax(url, data, isAsync, callback, isShowDialog, timeout, global) {
    $.post({
        url: url,
        data: data,
        traditional: true,
        async: isAsync,
        timeout: timeout,
        global: global
    })
        .done(function (result) {
            if (result) {
                // コントロールに色付ける
                if (result.errorControls) {
                    changeCtrlBorder(result.errorControls, true);
                }
                if (result.warningControls) {
                    changeCtrlBorder(result.warningControls, false);
                }

                if (result.status == responseStatusSuccess) {
                    if (isShowDialog) {
                        registerSuccessSwal(null, result.message);
                    }
                    if (callback) {
                        callback(result.data);
                    }
                }
                else if (result.status == responseStatusWarning) {
                    commonConfirm(result.message, null, function () {
                        if (callback) {
                            callback(result.data);
                        }
                    });
                }
                else if (result.status == responseStatusError) {
                    errorMessage(result.message);
                }
            }
        }).fail(function (response, textStatus, errorThrown) {
            failResponse(response);
            //console.log("textStatus     : " + textStatus);    // タイムアウト、パースエラー
            //console.log("errorThrown    : " + errorThrown.message); // 例外情報
        });
}

function changeCtrlBorder(ctrls, isError) {
    let borderClassName = isError ? "error-border" : "warning-border";
    $.each(ctrls, function (index, ctrl) {
        let tmpCtrl = $(ctrl);
        //FancySelectのみ特別な処理が必要
        if (tmpCtrl.hasClass("multiple-select")) {
            if (tmpCtrl.siblings(".selectMultiple").length) {
                tmpCtrl = tmpCtrl.siblings(".selectMultiple").eq(0);
            }
        }

        if (!tmpCtrl.hasClass(borderClassName)) {
            tmpCtrl.addClass(borderClassName);
        }
    });
}

function errorClear() {
    errorMessageClear();
    $.each($(".error-border"), function (index, ctrl) {
        $(ctrl).removeClass("error-border");
    });
    $.each($(".warning-border"), function (index, ctrl) {
        $(ctrl).removeClass("warning-border");
    });
}

/**
 * 登録処理のajax通信を行います。
 * @param {String} url post先
 * @param {Array} data 送信データ
 * @param {Function} callback コールバック
 */
function sendRegisterAjax(url, data, callback = function () { }, errorCallback = function () { }) {
    $.post({
        url: url,
        data: data,
    }).done(function (result) {
        if (result) {
            $('.input-validation-error').removeClass('input-validation-error');
            $('.form-errors').empty();

            if (!$.isEmptyObject(result.errors)) {
                $.each(result.errors, function (key, messages) {
                    $(`[name="${key}"]`).addClass('input-validation-error');
                    let validationField = $(`[data-valmsg-for="${key}"]`);

                    $.each(messages, function (_, message) {
                        validationField.append($(`<span>${message}</span>`));
                    });
                });

                if (errorCallback) {
                    errorCallback();
                }
            }
            else {
                if (callback) {
                    callback(result);
                }
            }
        }
    });
}

function fileSendRegisterAjax(url, data, callback = function () { }, errorCallback = function () { }, isShowDialog) {
    $.ajax({
        url: url,
        type: 'POST',
        dataType: 'json',
        data: data,
        processData: false,
        contentType: false
    }).done(function (result) {
        if (result) {
            $('.input-validation-error').removeClass('input-validation-error');
            $('.form-errors').empty();

            if (!$.isEmptyObject(result.errors)) {
                $.each(result.errors, function (key, messages) {
                    $(`[name="${key}"]`).addClass('input-validation-error');
                    let validationField = $(`[data-valmsg-for="${key}"]`);

                    $.each(messages, function (_, message) {
                        validationField.append($(`<span>${message}</span>`));
                    });
                });

                if (errorCallback) {
                    errorCallback();
                }
                return; // 以降の処理は行わない
            }
            else {
                // ステータス判定
                if (result.status == responseStatusSuccess) {
                    if (isShowDialog) {
                        registerSuccessSwal(null, result.message);
                    }
                    if (callback) {
                        callback();
                    }
                }
                else if (result.status == responseStatusWarning) {
                    commonConfirm(result.message, null, function () {
                        if (callback) {
                            callback();
                        }
                    });
                }
                else if (result.status == responseStatusError) {
                    errorMessage(result.message);
                    if (errorCallback) {
                        errorCallback();
                    }
                }
            }
        }
    });
}

function getFileSendJson() {
    var data = new FormData();
    data.append('__RequestVerificationToken', $("input[name='__RequestVerificationToken']").val());
    return data;
}

function fileSendAjax(url, data, callback, isShowDialog, isErrorExecuteCallback) {
    $.ajax({
        url: url,
        type: 'POST',
        dataType: 'json',
        data: data,
        processData: false,
        contentType: false
    }).always(function () {
        // 再度エラーチェックしたいからクリアしない
        //clearValidation();
    }).done(function (result) {
        if (result) {
            if (result.status == responseStatusSuccess) {
                if (isShowDialog) {
                    registerSuccessSwal(null, result.message);
                }
                if (callback) {
                    callback(result.data);
                }
            }
            else if (result.status == responseStatusWarning) {
                commonConfirm(result.message, null, function () {
                    if (callback) {
                        callback(result.data);
                    }
                });
            }
            else if (result.status == responseStatusError) {
                errorMessage(result.message);
                // CSV取込時のエラー出力
                if (isErrorExecuteCallback && callback) {
                    callback(result.data);
                }
            }
        }
    }).fail(function (response) {
        failResponse(response);
    });
}

function fileDownloadAjax(type, url, data, callback) {
    $.ajax({
        type: type,
        url: url,
        data: data,
        xhrFields: {
            responseType: "blob",
        },
    })
        .done(function (blob, status, xhr) {
            // ファイルがない時はダウンロード処理をせずに終わる
            if (!blob || !(blob instanceof Blob)) {
                if (callback) {
                    callback(false);
                }
                return;
            }

            // filename*がある場合、エンコードされたファイル名を取得
            let contentDisposition = xhr.getResponseHeader("Content-Disposition");
            let filename = "";
            if (contentDisposition && contentDisposition.indexOf("filename*") !== -1) {
                // filename* の部分を抽出して、エンコードされたファイル名を取り出す
                var matches = contentDisposition.match(/filename\*=UTF-8''([^;]+)/);
                if (matches && matches[1]) {
                    // エンコードされたファイル名をデコード
                    filename = decodeURIComponent(matches[1].replace(/\+/g, ' '));
                }
            } else if (contentDisposition && contentDisposition.indexOf("filename=") !== -1) {
                // filenameがそのままある場合（通常のエンコーディング）
                filename = contentDisposition.split('filename=')[1].replace(/"/g, "");
            }

            let url = window.URL.createObjectURL(blob);
            let link = $('<a></a>')
                .attr('href', url)
                .attr('download', filename)
                .appendTo('body');

            link.get(0).click();
            link.remove();

            window.URL.revokeObjectURL(url);

            if (callback) {
                callback(true);
            }
        })
        .fail(function () {
            // エラー処理
            errorMessage("ダウンロードに失敗しました。再度実行してください。");
        });
}

//モーダルを画面の中心に持ってくる
function CenteringModalSyncer(modalId) {
    var w = $(window).width() - 300;
    var h = $(window).height() - 50;
    var cw = $(modalId).outerWidth();
    var ch = $(modalId).outerHeight();
    $(modalId).css({ "left": ((w - cw) / 2) + "px", "top": ((h - ch) / 2) + "px", "position": "absolute" });
}

$('.helf-upper-change').on('change', function () {

    var kanaMap = {
        "ガ": "ｶﾞ", "ギ": "ｷﾞ", "グ": "ｸﾞ", "ゲ": "ｹﾞ", "ゴ": "ｺﾞ",
        "ザ": "ｻﾞ", "ジ": "ｼﾞ", "ズ": "ｽﾞ", "ゼ": "ｾﾞ", "ゾ": "ｿﾞ",
        "ダ": "ﾀﾞ", "ヂ": "ﾁﾞ", "ヅ": "ﾂﾞ", "デ": "ﾃﾞ", "ド": "ﾄﾞ",
        "バ": "ﾊﾞ", "ビ": "ﾋﾞ", "ブ": "ﾌﾞ", "ベ": "ﾍﾞ", "ボ": "ﾎﾞ",
        "パ": "ﾊﾟ", "ピ": "ﾋﾟ", "プ": "ﾌﾟ", "ペ": "ﾍﾟ", "ポ": "ﾎﾟ",
        "ヴ": "ｳﾞ", "ヷ": "ﾜﾞ", "ヺ": "ｦﾞ",
        "ア": "ｱ", "イ": "ｲ", "ウ": "ｳ", "エ": "ｴ", "オ": "ｵ",
        "カ": "ｶ", "キ": "ｷ", "ク": "ｸ", "ケ": "ｹ", "コ": "ｺ",
        "サ": "ｻ", "シ": "ｼ", "ス": "ｽ", "セ": "ｾ", "ソ": "ｿ",
        "タ": "ﾀ", "チ": "ﾁ", "ツ": "ﾂ", "テ": "ﾃ", "ト": "ﾄ",
        "ナ": "ﾅ", "ニ": "ﾆ", "ヌ": "ﾇ", "ネ": "ﾈ", "ノ": "ﾉ",
        "ハ": "ﾊ", "ヒ": "ﾋ", "フ": "ﾌ", "ヘ": "ﾍ", "ホ": "ﾎ",
        "マ": "ﾏ", "ミ": "ﾐ", "ム": "ﾑ", "メ": "ﾒ", "モ": "ﾓ",
        "ヤ": "ﾔ", "ユ": "ﾕ", "ヨ": "ﾖ",
        "ラ": "ﾗ", "リ": "ﾘ", "ル": "ﾙ", "レ": "ﾚ", "ロ": "ﾛ",
        "ワ": "ﾜ", "ヲ": "ｦ", "ン": "ﾝ",
        "ァ": "ｧ", "ィ": "ｨ", "ゥ": "ｩ", "ェ": "ｪ", "ォ": "ｫ",
        "ッ": "ｯ", "ャ": "ｬ", "ュ": "ｭ", "ョ": "ｮ",
        "。": "｡", "、": "､", "ー": "ｰ", "「": "｢", "」": "｣", "・": "･"
    };
    var text = $(this).val();
    var hen = text.replace(/[Ａ-Ｚａ-ｚ０-９！＂＃＄％＆＇（）＊＋，－．／：；＜＝＞？＠［＼］＾＿｀｛｜｝]/g, function (s) {
        return String.fromCharCode(s.charCodeAt(0) - 0xFEE0);
    })
        .replace(/[‐－―]/g, '-') // ハイフンなど
        .replace(/[～〜]/g, '~')   // チルダ
        .replace(/　/g, ' ')
        .toUpperCase();

    var reg = new RegExp('(' + Object.keys(kanaMap).join('|') + ')', 'g');
    hen = hen
        .replace(reg, function (match) {
            return kanaMap[match];
        })
        .replace(/゛/g, 'ﾞ')
        .replace(/゜/g, 'ﾟ');
    $(this).val(hen);
});

$('.helf-alphanumeric').on('keydown', function (e) {
    let k = e.keyCode;
    let str = String.fromCharCode(k);
    if (!(str.match(/[0-9a-zA-Z]/) || e.shiftKey || (37 <= k && k <= 40) || (96 <= k && k <= 105) || k === 8 || k == 9 || k === 46)) {
        return false;
    }
});
$('.helf-alphanumeric').on('keyup', function (e) {
    if (e.keyCode === 9 || e.keyCode === 16) return;
    this.value = this.value.replace(/[^0-9a-zA-Z]+/i, '');
});

$('.helf-alphanumeric').on('blur', function () {
    this.value = this.value.replace(/[^0-9a-zA-Z]+/i, '');
});

//パスワード　半角数字、英文字、符号のみ入力可能 
//システムメンテナンスの事業所詳細と、本部遷移後の事業所画面のパスワードに使っている

// 許可する記号の一覧
let safeSymbols = `!"#$%&'()*+,-./:;<=>?@[\\]^_\`{|}~`;

// 許可されていない文字を除外するための正規表現
let allowedPattern = new RegExp(`[^0-9a-zA-Z${safeSymbols.replace(/[-[\]/{}()*+?.\\^$|]/g, '\\$&')}]`, 'g');

// 特定のキー（例：Escキー）を無効化
$('.user-password').on('keydown', function (e) {
    let blockedKeys = [27]; // Escキー
    if (blockedKeys.includes(e.keyCode)) {
        return false;
    }
});

// キー入力後に許可されていない文字を削除
$('.user-password').on('keyup', function (e) {
    if (e.keyCode === 9 || e.keyCode === 16) return; // TabキーとShiftキーは無視
    this.value = this.value.replace(allowedPattern, '');
});

// フォーカスが外れたときに許可されていない文字を削除
$('.user-password').on('blur', function () {
    this.value = this.value.replace(allowedPattern, '');
});


// 半角のアルファベットのみ
$('.helf-alphabet').on('keydown', function (e) {
    let k = e.keyCode;
    let str = String.fromCharCode(k);
    // 半角アルファベットのみ許可
    if (!(str.match(/[a-zA-Z]/) || e.shiftKey || (37 <= k && k <= 40) || k === 8 || k == 9 || k === 46)) {
        return false;
    }
});

$('.helf-alphabet').on('keyup', function (e) {
    if (e.keyCode === 9 || e.keyCode === 16) return;
    // 半角アルファベットのみ許可
    this.value = this.value.replace(/[^a-zA-Z]+/g, '');
});

$('.helf-alphabet').on('blur', function () {
    // 半角アルファベットのみ許可
    this.value = this.value.replace(/[^a-zA-Z]+/g, '');
});

// 数字のみ
$('.helf-numeric').on('keydown', function (e) {
    let k = e.keyCode;
    let str = String.fromCharCode(k);
    //let length = $(this).val().length;
    //let maxLength = $(this).attr("maxlength");
    //if (length >= maxLength) {
    //    return false;
    //}
    if (!(str.match(/[0-9]/) || e.shiftKey || (37 <= k && k <= 40) || (96 <= k && k <= 105) || k === 8 || k == 9 || k === 46)) {
        return false;
    }
});

// 半角数値のみ入力可能とする
$('.helf-numeric').on('keyup', function (e) {
    if (e.keyCode === 9 || e.keyCode === 16) return;
    this.value = this.value.replace(/[^0-9]*/g, '');
});
$('.helf-numeric').on('blur', function () {
    this.value = this.value.replace(/[^0-9]*/g, '');
});

// 全角・半角カタカナのみ
$('.helf-katakana').on('blur', function () {
    this.value = this.value.replace(/[^ァ-ンー　ｧ-ﾝﾞﾟ ]+/i, '');
});

// 全角ひらがなのみ
$('.helf-hiragana').on('blur', function () {
    this.value = this.value.replace(/[^ぁ-ん]+/i, '');
});

/** ひらがなをカタカナに変換 */
function convertStr(str) {
    return str.replace(/[ぁ-ん]/g, function (s) {
        return String.fromCharCode(s.charCodeAt(0) + 0x60);
    });
}

//funcytreeのNodeExpanded（開いているか）保持用
function getNodeExpanded(list, nodes) {
    $.each(nodes, function (index, node) {
        list[node.key] = node.expanded;
        list = Object.assign(list, getNodeExpanded(list, node.children));
    });
    return list;
}

function setNodeExpanded(list, tree) {
    $.each(list, function (key, val) {
        let node = tree.getNodeByKey(key);
        if (node) {
            node.setExpanded(val);
        }
    });
}

function treeReloadKeepExpanded(treeKey, reloadUrl) {
    var tree = $.ui.fancytree.getTree('#' + treeKey);
    //選択されている情報を保持
    let selectedKey;
    if (tree.activeNode) {
        selectedKey = tree.activeNode.key;
    }
    //開閉状態を保持
    let nodeList = getNodeExpanded({}, tree.rootNode.children);
    //再検索
    tree.reload({
        url: reloadUrl,
    });
    //再検索は非同期処理なので、時間をずらして実行
    setTimeout(function () {
        if (selectedKey) {
            let node = tree.getNodeByKey(selectedKey);
            if (node) {
                node.setActive();
            }
        }
        setNodeExpanded(nodeList, tree);
    }, 300);
}

jQuery.fn.slideLeftHide = function (speed, width, callback) {
    let tmpW = "hide";
    if (width) {
        tmpW = width;
    }
    this.animate({
        width: tmpW
    }, speed, callback);
}

jQuery.fn.slideLeftShow = function (speed, width, callback) {
    let tmpW = "show";
    if (width) {
        tmpW = width;
    }
    this.animate({
        width: tmpW
    }, speed, callback);
}

$('.tree-slide-btn').on('click', function () {
    let me = $(this);
    let target = $("#" + me.attr("target"));
    let callback = me.attr('callback');
    if (target.length) {
        if (target.is(':hidden')) {
            me.text('close').removeClass('close-style');
            target.slideLeftShow(400, null, function () {
                if (callback) {
                    eval(callback);
                }
            });
        } else {
            target.slideLeftHide(400, null, function () {
                me.text('open').addClass('close-style');
                if (callback) {
                    eval(callback);
                }
            });
        }
    }

    return false;
});

//共通関数
function getCurrentDate(str) {
    let dateTime = new Date(str);
    return new Date(dateTime.getFullYear() + "/" + (dateTime.getMonth() + 1) + "/" + dateTime.getDate());
}

function getCurrentDateStr() {
    let dateTime = new Date();
    return dateTime.getFullYear() + getZeroFill(dateTime.getMonth() + 1, 2) + getZeroFill(dateTime.getDate(), 2) + getZeroFill(dateTime.getHours(), 2) + getZeroFill(dateTime.getMinutes(), 2) + getZeroFill(dateTime.getSeconds(), 2);
}

function getZeroFill(target, keta) {
    return ('0' + target).slice(-1 * keta);
}

function getDateStr(date, isHyfun) {
    let separator = isHyfun ? "-" : "/";
    return date.getFullYear() + separator + getZeroFill(date.getMonth() + 1, 2) + separator + getZeroFill(date.getDate(), 2);
}

function getDateTimeStr(dateTime, isHyfun) {
    let separator = isHyfun ? "-" : "/";
    let timePrefix = isHyfun ? "T" : " ";
    return dateTime.getFullYear() + separator + getZeroFill(dateTime.getMonth() + 1, 2) + separator + getZeroFill(dateTime.getDate(), 2) + timePrefix + getZeroFill(dateTime.getHours(), 2) + ":" + getZeroFill(dateTime.getMinutes(), 2);
}

function getDateTimeStrIncludeDayOfWeek(dateTime) {
    return dateTime.getFullYear() + "/" + getZeroFill(dateTime.getMonth() + 1, 2) + "/" + getZeroFill(dateTime.getDate(), 2) + "（" + getDayOfWeekStr(dateTime) + "）" + " " + getZeroFill(dateTime.getHours(), 2) + ":" + getZeroFill(dateTime.getMinutes(), 2);
}

function getDayOfWeekStr(dateTime) {
    let dayOfWeek = dateTime.getDay()
    return ["日", "月", "火", "水", "木", "金", "土"][dayOfWeek];	// 曜日(日本語表記)
}

// Fullcalendarのスクロール表示
function visibleCalendarScroll() {
    let rows = $(".fc-timeline-header-row");
    if (rows.length) {
        let targets = rows.eq(0).parents(".fc-scroller");
        if (targets.length) {
            targets.eq(0).css("overflow-y", "scroll");
        }
    }
}

function gridLoadDataTemplete(url, filter) {
    return gridLoadData(url, searchCondJson(filter))
        .then(function (p) {
            return {
                data: p.data,
                itemsCount: p.itemsCount,
                idList: p.idList,
            };
        });
}

/**
 * 通常の select multiple を Fancy multiple select に変換
 * @param {any} ctrls / jQueryオブジェクト
 * @param {any} slideTime / アニメーション時間
 */
function setMultipleSelect(ctrls, slideTime = 200) {
    ctrls.each((_, ctrl) => {
        // オリジナルマルチプルセレクト
        let select = $(ctrl);
        let allItemVal = select.attr("all-item");
        let isSelectingAllItem = false;
        let selectedCount = 0;
        let prevVals = select.val(); // 前回値記憶用

        // プレースホルダー
        let placeholder = $('<span>' + select.data('placeholder') + '</span>').addClass('placeholder').css('width', 'inherit');
        // 矢印アイコン
        let arrow = $('<div class="arrow" />');
        // 選択済みエリア
        let active = $('<div class="active" />').append(placeholder).append(arrow);
        // 選択候補エリア
        let list = $('<ul />').css('z-index', '1000');

        let allItemText = "";
        let allItemIndex = 0;
        // 初期選択設定
        select.find('option').each((i, opt) => {
            let option = $(opt);
            let isCurrentAllItem = option.val() == allItemVal;

            if (isCurrentAllItem) {
                allItemIndex = i;
                allItemText = option.text();
            }

            if (option.is(':selected')) {
                let htm = '<a index="' + i + '"';
                if (isCurrentAllItem) {
                    htm += 'isAllItem="true"';
                } else {
                    htm += 'isAllItem="false"';
                }
                htm += '/>';
                let aTag = $(htm).html('<em>' + option.text() + '</em><i></i>');
                aTagSelectEvent(aTag);
                active.append(aTag);
                active.find('.placeholder').hide();
            } else {
                let htm = '<li index="' + i + '"';
                if (isCurrentAllItem) {
                    htm += 'id="all-item-li" isAllItem="true"';
                } else {
                    htm += 'isAllItem="false"';
                }
                htm += '/>';
                let liTag = $(htm).html(option.text());
                liTagSelectEvent(liTag);
                list.append(liTag);
            }
        });

        let borderClass = select.hasClass("border") ? "border" : "";
        // オリジナルマルチプルセレクトを非表示にして、カスタムマルチプルセレクトを追加
        let selectMultiple = $('<div />').addClass('selectMultiple').addClass(borderClass).append(active).append(list);
        select.hide().after(selectMultiple);

        // 展開アクション設定
        arrow.on('click', () => selectMultiple.toggleClass('open'));
        selectMultiple.on('click', (e) => {
            // 全選択項目が選択された場合
            if (isSelectingAllItem) {
                selectMultiple.removeClass('open');
                return;
            }
            if (!$(e.target).hasClass('arrow') && !selectMultiple.hasClass('open')) {
                selectMultiple.addClass('open');
            }
        });
        selectMultiple.attr("tabindex", -1) // タブインデックス設定しないと focusout イベント設定できない
            .on('focusout', () => {
                selectMultiple.removeClass('open');
                // 前回値と差分があるかチェック
                let curentVals = select.val().sort();
                if (curentVals.toString() != prevVals.toString()) {
                    select.change();
                    prevVals = curentVals;
                }
            });

        // イベント用
        let isAnimation = false;

        // 選択イベント設定
        function liTagSelectEvent(liTag) {
            liTag.on('click', () => {
                // アニメーション中は選択させない
                if (isAnimation) {
                    return;
                }
                selectedCount++;
                // 全選択項目が選択された場合
                let isAllItem = parseStrToBoolean(liTag.attr("isAllItem"));
                if (isAllItem) {
                    isSelectingAllItem = true;
                }
                isAnimation = true;
                selectMultiple.find('.placeholder').hide();

                // オリジナルのマルチプルセレクトで選択操作
                select.find('option').each((_, opt) => {
                    if (liTag.text() == $(opt).text()) {
                        $(opt).prop('selected', true);
                        return false;
                    }
                });

                // 削除対象アニメーション
                liTag.addClass('remove');
                liTag.prev().addClass('beforeRemove');
                liTag.next().addClass('afterRemove');
                liTag.slideUp(slideTime, () => {
                    liTag.prev().removeClass('beforeRemove');
                    liTag.next().removeClass('afterRemove');
                    liTag.remove();
                    //全選択以外を選択された場合、全選択を選択不可にする
                    if (!isAllItem) {
                        $("#all-item-li").remove();
                    }
                });

                // 追加対象アニメーション（ちょっと遅れてから実行）
                let index = liTag.attr("index");
                let htm = '<a index="' + index + '"';
                if (isAllItem) {
                    htm += 'isAllItem="true"';
                } else {
                    htm += 'isAllItem="false"';
                }
                htm += '/>';
                let aTag = $(htm).hide().html('<em>' + liTag.text() + '</em><i></i>').appendTo(selectMultiple.children('.active'));
                //aTagSelectEvent(aTag);
                setTimeout(() => {
                    aTag.animate({ width: 'toggle', opacity: 'toggle' }, slideTime, () => {
                        isAnimation = false;
                    });
                    // ソートとEvent設定
                    sortTag(false);
                }, slideTime / 2);
            });
        };

        // 選択解除イベント設定
        function aTagSelectEvent(aTag) {
            aTag.on('click', () => {
                // アニメーション中は選択させない
                if (isAnimation) {
                    return;
                }
                selectedCount--;

                let isAllItem = parseStrToBoolean(aTag.attr("isAllItem"));
                // 全選択項目が選択された場合
                if (isAllItem) {
                    isSelectingAllItem = false;
                }
                isAnimation = true;

                // オリジナルのマルチプルセレクトで選択解除操作
                select.find('option:contains(' + aTag.children('em').text() + ')').prop('selected', false);
                if (!select.find('option:selected').length) {
                    selectMultiple.find('.placeholder').show();
                }

                // 削除対象アニメーション
                aTag.animate({ width: 0, opacity: 0 }, slideTime, () => {
                    aTag.remove();
                });
                let index = aTag.attr("index");
                // 追加対象アニメーション（ちょっと遅れてから実行）
                let htm = '<li index="' + index + '"';
                if (isAllItem) {
                    htm += 'id="all-item-li" isAllItem="true"';
                } else {
                    htm += 'isAllItem="false"';
                }
                htm += '/>';
                let liTag = $(htm).text(aTag.children('em').text()).hide().appendTo(selectMultiple.find('ul'));
                //選択されている項目がない場合は、全選択項目を選択可能にする
                if (selectedCount == 0 && !isAllItem && !$("#all-item-li").length && allItemVal) {
                    let allItem = $('<li index="' + allItemIndex + '" id="all-item-li" isAllItem="true"/>').text(allItemText).appendTo(selectMultiple.find('ul'));
                }
                setTimeout(() => {
                    liTag.slideDown(slideTime, () => {
                        isAnimation = false;
                    });
                    // ソートとEvent設定
                    sortTag(true);
                }, slideTime / 2);
            });
        };

        function sortTag(isUl) {
            if (isUl) {
                let ul = selectMultiple.find('ul');
                let children = ul.children('li');
                let $elements = children.sort(function (a, b) {
                    let aa = parseInt($(a).attr('index'));
                    let bb = parseInt($(b).attr('index'));
                    if (aa > bb) {
                        return 1;
                    } else if (aa < bb) {
                        return -1;
                    }
                    return 0;
                });

                //リスト（ulの中のli）を全て削除
                children.remove();

                //並び替えた順にliを追加する
                $elements.each(function () {
                    liTagSelectEvent($(this));
                    ul.append($(this));
                });
            }
            else {
                let ul = selectMultiple.children('.active');
                let children = ul.children('a');
                let $elements = children.sort(function (a, b) {
                    let aa = parseInt($(a).attr('index'));
                    let bb = parseInt($(b).attr('index'));
                    if (aa > bb) {
                        return 1;
                    } else if (aa < bb) {
                        return -1;
                    }
                    return 0;
                });

                //リスト（activeの中のa）を全て削除
                children.remove();

                //並び替えた順にliを追加する
                $elements.each(function () {
                    aTagSelectEvent($(this));
                    ul.append($(this));
                });
            }
        }
    });
}

/**
 * 通常の select multiple を Fancy multiple select に再変換
 * @param {any} ctrls / jQueryオブジェクト
 * @param {any} slideTime / アニメーション時間
 */
function resetMultipleSelect(ctrls, selects, slideTime = 200) {
    if (selects) {
        ctrls.val(selects);
    }
    ctrls.each((_, obj) => $(obj).parent().find('.selectMultiple ').remove());
    setMultipleSelect(ctrls, slideTime);
}

/**
 * 通常の select multiple を 表示のみにする
 * @param {any} ctrls / jQueryオブジェクト
 */
function setOnlyDisplayMultipleSelect(ctrls) {
    ctrls.each((_, ctrl) => {
        // オリジナルマルチプルセレクト
        let select = $(ctrl);

        // ボーダー
        let border = $('<div>').addClass('border').css('flex', '1').css('min-width', '300px').css('min-height', '40px');

        // 初期選択設定
        select.find('option').each((_, opt) => {
            let option = $(opt);
            if (option.is(':selected')) {
                let selected = $('<div>').css('font-size', '16px').css('margin', '10px').text(option.text());
                border.append(selected);
            }
        });

        select.hide().after(border);
    });
}

/**
 * 通常の select multiple を 表示させないようにする
 * @param {any} ctrls / jQueryオブジェクト
 */
function setNotDisplayMultipleSelect(ctrls) {
    ctrls.each((_, ctrl) => {
        // オリジナルマルチプルセレクト
        let select = $(ctrl);

        select.hide();
    });
}

/**
 * 通常の select multiple を F表示のみに再変換
 * @param {any} ctrls / jQueryオブジェクト
 */
function resetOnlyDisplayMultipleSelect(ctrls, selects) {
    if (selects) {
        ctrls.val(selects);
    }
    ctrls.each((_, obj) => $(obj).parent().find('div.border').remove());
    setOnlyDisplayMultipleSelect(ctrls);
}

/**
 * 通常の select multiple を 非表示に再変換
 * @param {any} ctrls / jQueryオブジェクト
 */
function resetNotDisplayMultipleSelect(ctrls, selects) {
    if (selects) {
        ctrls.val(selects);
    }
    ctrls.each((_, obj) => $(obj).parent().find('div.border').remove());
    setNotDisplayMultipleSelect(ctrls);
}

/**
 * チェックボックスをトグルボタンに変更
 * @param {any} ctrls
 * attr yes-text : true部のテキスト（はい）
 * attr no-text : false部のテキスト（いいえ）
 */
function setCheckToggleBtn(ctrls, isOnlyDisplay) {
    ctrls.each((_, control) => {
        let ctrl = $(control);
        let toggleBtn = $(''
            + ' <div class="input-area">'
            + `     <div class="separate-selecter ${isOnlyDisplay ? 'is-only-display' : ''}">`
            + `         <div class="yes-selecter">${ctrl.attr('yes-text') ?? 'はい'}</div>`
            + `      <div class="no-selecter">${ctrl.attr('no-text') ?? 'いいえ'}</div>`
            + `     </div>`
            + ` </div>`);
        if (!isOnlyDisplay) {
            toggleBtn.find('.separate-selecter > div').on('click', e => {
                $(e.currentTarget).addClass('active').siblings().removeClass('active');
                let isCheck = $(e.currentTarget).hasClass('yes-selecter');
                let ctrlVal = ctrl.val();

                if (isCheck != parseStrToBoolean(ctrlVal)) {
                    ctrl.val(isCheck).prop('checked', isCheck).trigger('change');
                }
            });
        }
        toggleBtn.find(`.separate-selecter > ${ctrl.prop('checked') ? '.yes-selecter' : '.no-selecter'}`).addClass('active');
        //初期値対策
        ctrl.val(ctrl.prop('checked'));

        ctrl.siblings('.input-area').remove(); // 初期化
        ctrl.hide().after(toggleBtn);
    });
}

/**
 * 年月日 air-datepicker をセット
 * @param {string} id / 要素のId属性
 * @param {string} today / 当日日付
 * @param {string} minDate / 最小日付
 * @param {string} maxDate / 最大日付
 * @param {{formattedDate: string, date: object}} selectFunc  / 選択時の関数
 */
function setAirDatePicker(id, today, minDate, maxDate, selectFunc) {
    setInitAirDatePicker(id, 'yyyy/mm/dd', today, minDate, maxDate, selectFunc);
}

/**
 * 年月 air-datepicker をセット
 * @param {string} id / 要素のId属性
 * @param {string} minDate / 最小日付
 * @param {string} maxDate / 最大日付
 * @param {{formattedDate: string, date: object}} selectFunc  / 選択時の関数
 */
function setAirYearMonthPicker(id, minDate, maxDate, selectFunc) {
    setInitAirDatePicker(id, 'yyyy/mm', null, minDate, maxDate, selectFunc);
}

/**
 * 年 air-datepicker をセット
 * @param {string} id / 要素のId属性
 * @param {string} minDate / 最小日付
 * @param {string} maxDate / 最大日付
 * @param {{formattedDate: string, date: object}} selectFunc  / 選択時の関数
 */
function setAirYearPicker(id, minDate, maxDate, selectFunc) {
    setInitAirDatePicker(id, 'yyyy', null, minDate, maxDate, selectFunc);
}

/**
 * 日時 air-datepicker をセット
 * @param {string} id / 要素のId属性
 * @param {string} today / 当日日付
 * @param {string} minDate / 最小日付
 * @param {string} maxDate / 最大日付
 * @param {{formattedDate: string, date: object}} selectFunc  / 選択時の関数
 */
function setAirDateTimePicker(id, today, minDate, maxDate, selectFunc) {
    setInitAirDatePicker(id, 'yyyy/mm/dd', today, minDate, maxDate, selectFunc);
}

/**
 * air-datepicker をセット
 * @param {string} id / 要素のId属性
 * @param {string} format / フォーマット（表示・値共に影響あり）
 * @param {string} today / 当日日付
 * @param {string} minDate / 最小日付
 * @param {string} maxDate / 最大日付
 * @param {{formattedDate: string, date: object}} selectFunc  / 選択時の関数
 */
function setInitAirDatePicker(id, format, today, minDate, maxDate, selectFunc) {
    $(id).datepicker({
        language: 'ja',
        dateFormat: format,
        autoClose: true,
        clearButton: $(id).data('is-clear-button') || false,
        todayButton: today,
        minDate: minDate,
        maxDate: maxDate,
        toggleSelected: false,
        navTitles: {
            days: 'YYYY年MM月',
            months: 'YYYY年',
            years: 'yyyy1 - yyyy2'
        },
        onSelect: (formattedDate, date) => {
            if (selectFunc) {
                selectFunc(formattedDate, date);
            }
        }
    });
}

/**
 * air-datepicker をセット
 * @param {string} id / 要素のId属性
 * @param {boolean} closedFunc  / Close時の関数
 */
function setAirTimePicker(id, closedFunc) {
    let hideCount = 0;

    $(id).datepicker({
        timeFormat: 'hh:ii',
        autoClose: true,
        timepicker: true,
        onlyTimepicker: true,
        clearButton: true,
        onHide: function (dp, animationCompleted) {
            //onHideは非表示アニメーションの開始時と終了時の2回呼ばれるため、開始時のみ呼び出し
            if (animationCompleted && hideCount === 0) {
                hideCount++;
                closedFunc(dp, animationCompleted);
            }
            hideCount = 0;
        },
    });
}

/**
 * air-datepicker をセット
 * @param {object} ctrls / jQueryオブジェクト
 * @param {int} startHour / 開始時刻の時
 * @param {int} startMinute / 開始時刻の分
 */
function setTimePicker(ctrls, startHour, startMinute) {
    $.each(ctrls, function (index, control) {
        let ctrl = $(control);
        let timeParts = ctrl.val().split(":");
        startHour = startHour ?? timeParts[0] ?? 0;
        startMinute = startMinute ?? timeParts[1] ?? 0;
        let min = parseInt(ctrl.data("min")) || 0;
        let max = parseInt(ctrl.data("max")) || 24;
        let step = parseInt(ctrl.data("step")) || 1;
        var startTime = new Date();
        startTime.setHours(startHour);
        startTime.setMinutes(startMinute);

        ctrl.datepicker({
            timeFormat: 'hh:ii',
            autoClose: true,
            minHours: min,
            maxHours: max,
            minutesStep: step,
            timepicker: true,
            onlyTimepicker: true,
            startDate: startTime
        });
    });
}

/**
 * air-datepicker の日付範囲をセット
 * @param {string} id / 要素のId属性
 * @param {string} start / 最小日付
 * @param {string} end / 最大日付
 */
function setAirPickerRange(id, start = null, end = null) {
    let startDate = new Date();
    if (start) {
        startDate = new Date(start.replaceAll("/", "-"));
        $(id).data('datepicker').minDate.setYear(startDate.getFullYear());
        $(id).data('datepicker').minDate.setMonth(startDate.getMonth());
        $(id).data('datepicker').minDate.setDate(startDate.getDate());
    }

    let endDate = new Date();
    if (end) {
        endDate = new Date(end.replaceAll("/", "-"));
        $(id).data('datepicker').maxDate.setYear(endDate.getFullYear());
        $(id).data('datepicker').maxDate.setMonth(endDate.getMonth());
        $(id).data('datepicker').maxDate.setDate(endDate.getDate());
    }

    if ($(id).val()) {
        let currentVal = $(id).val().replaceAll("/", "-"); //Firefox対応のため"/"を"-"に変換
        let currentDate = new Date(currentVal);
        $(id).data('datepicker').currentDate = currentDate;
        //設定の更新
        $(id).data('datepicker').update();

        //更新すると値がクリアされるため、日付を設定しなおし
        if (!($(id).data('datepicker').minDate <= currentDate && currentDate <= $(id).data('datepicker').maxDate)) {
            currentVal = '';
        }
        $(id).val(currentVal.replaceAll("-", "/")); //Firefox対応。レンジを設定後datepickerのテキストエリアを"/"表記にするため再度変換
    }
}

/**
 * form-check-input の値をセットし、表示更新
 * @param {boolean} isTurnOn / オンにするか
 * @param {Array} ids / 設定するid属性の配列（無しで全ての form-check-input を設定）
 */
function setFormCheckInput(isTurnOn, ids = []) {
    let objs = $('.form-check-input');
    if (ids.length) {
        objs = objs.filter((_, obj) => ids.includes($(obj).attr('id')));
    }
    objs.each((_, obj) => {
        let ctrl = $(obj);
        ctrl.prop("checked", isTurnOn);
        $('input[name="' + ctrl.attr('name') + '"]').val(isTurnOn);
    })
}

/**
 * form-check-inputのチェック状態をリセットします。
 */
function resetCheckInput() {
    let objs = $('.form-check-input');
    $.each(objs, function (_, obj) {
        $(obj).prop("checked", false);
        $(obj).val(true);
        $('input[name="' + $(obj).attr('name') + '"][type="hidden"]').val(false);
    });
}

function getDecimalFormat(decimalPlaces, defaultValue) {
    let dp = 1;
    if (decimalPlaces) {
        dp = decimalPlaces;
    }
    let dv = "";
    if (defaultValue) {
        dv = defaultValue;
    }
    return { decimalSeparator: ".", decimalPlaces: dp, thousandsSeparator: ",", defaultValue: dv };
}

function getDateFormat(srcformat, newformat, defaultValue) {
    let sf = 'ISO8601Long';
    if (srcformat) {
        sf = srcformat;
    }
    let nf = 'Y/m/d H:i';
    if (newformat) {
        nf = newformat;
    }
    let dv = null;
    if (defaultValue) {
        dv = defaultValue;
    }
    return {
        srcformat: sf,
        newformat: nf,
        defaultValue: dv
    };
}

/*jqGrid*/
/**
 * jqGridのページャーのデザイン操作面の設定
 * @param {any} gridId：gridのID（#なし）
 * @param {any} searchFunc：検索処理（json要素を第一引数にとれるように実装すること）
 * @param {any} maxPagers：複数ページになった際に何個ボタンが表示されるかを設定（省略可）
 * @param {any} hasAllData：全ページ分のデータを保有しているか
 */
function settingPager(gridId, searchFunc, maxPagers, hasAllData) {
    let MAX_PAGERS = 5;
    if (maxPagers) {
        MAX_PAGERS = maxPagers;
    }
    let i;
    let pageRefresh = function (e) {
        let nextPage = $(e.target).text();
        if (hasAllData) {
            $("#" + gridId).trigger("reloadGrid", [{ page: nextPage }]);
        }
        else {
            let jsonData = getSearchParams(gridId, nextPage);
            searchFunc(jsonData);
        }
        e.preventDefault();
    };

    let params = $("#" + gridId).jqGrid("getGridParam");
    $($("#" + gridId)[0].p.pager + '_center td.cus-grid-page').remove();
    let pagerPrevTD = $('<td>', { class: "cus-grid-page" });
    let prevPagesIncluded = 0;
    let pagerNextTD = $('<td>', { class: "cus-grid-page" });
    let nextPagesIncluded = 0;
    let totalStyle = $("#" + gridId)[0].p.pginput === false;
    let page = $("#" + gridId)[0].p.page;
    let lastpage = $("#" + gridId)[0].p.lastpage;

    if (params.userData && params.userData.length) {
        page = parseInt(params.userData.find(x => x.name == "page").value);
        lastpage = parseInt(params.userData.find(x => x.name == "lastpage").value);
        if (lastpage > 0) {
            if (page > 1) {
                $("#first_grid-page").removeClass('ui-state-disabled');
                $("#prev_grid-page").removeClass('ui-state-disabled');
            }
            if (page != lastpage) {
                $("#next_grid-page").removeClass('ui-state-disabled');
                $("#last_grid-page").removeClass('ui-state-disabled');
            }
        }
    }
    let startIndex = page - parseInt(MAX_PAGERS / 2);
    if (startIndex > lastpage - MAX_PAGERS + 1) {
        startIndex = lastpage - MAX_PAGERS + 1;
    }

    for (i = startIndex; i <= lastpage && (totalStyle ? (prevPagesIncluded + nextPagesIncluded < MAX_PAGERS) : (nextPagesIncluded < MAX_PAGERS)); i++) {
        if (i <= 0) { continue; }
        if (i === page) {
            let currentHtm = $('<label>');
            currentHtm.text(String(i));

            if (prevPagesIncluded > 0) { pagerPrevTD.append('<span>&nbsp;</span>'); }
            pagerPrevTD.append(currentHtm);
            prevPagesIncluded++;
            continue;
        }

        let link = $('<a>', { href: '#', click: pageRefresh });
        link.text(String(i));
        if (i < page || totalStyle) {
            if (prevPagesIncluded > 0) { pagerPrevTD.append('<span>&nbsp;</span>'); }
            pagerPrevTD.append(link);
            prevPagesIncluded++;
        } else {
            if (nextPagesIncluded > 0 || (totalStyle && prevPagesIncluded > 0)) { pagerNextTD.append('<span>&nbsp;</span>'); }
            pagerNextTD.append(link);
            nextPagesIncluded++;
        }
    }

    if (prevPagesIncluded > 0) {
        $($("#" + gridId)[0].p.pager + '_center td[id^="prev"]').after(pagerPrevTD);
    }
    if (nextPagesIncluded > 0) {
        $($("#" + gridId)[0].p.pager + '_center td[id^="next"]').before(pagerNextTD);
    }
}

/**
 * jqGridのページャーボタンをfontawesomeに変更
 * @param {any} gridId：gridのID（#なし）
 */
function chagePagerIcon(gridId) {
    let pager = $("#" + gridId).closest(".ui-jqgrid").find(".ui-pg-table");
    pager.find(".ui-pg-button>span.ui-icon-seek-first")
        .removeClass("ui-icon ui-icon-seek-first")
        .addClass("fa-solid fa-angles-left fa-fw");
    pager.find(".ui-pg-button>span.ui-icon-seek-prev")
        .removeClass("ui-icon ui-icon-seek-prev")
        .addClass("fa-solid fa-angle-left fa-fw");
    pager.find(".ui-pg-button>span.ui-icon-seek-next")
        .removeClass("ui-icon ui-icon-seek-next")
        .addClass("fa-solid fa-angle-right fa-fw");
    pager.find(".ui-pg-button>span.ui-icon-seek-end")
        .removeClass("ui-icon ui-icon-seek-end")
        .addClass("fa-solid fa-angles-right fa-fw");
}

/**
 * jqGridのリサイズ処理
 * @param {any} gridId：gridのID（#なし）
 * @param {any} parentId：gridの親要素のID（＃なし、Resizeに使用するため必須要素）
 */
function gridResize(gridId, parentId) {
    $('#' + gridId).setGridWidth(0);
    $('#' + gridId).setGridWidth($('#' + parentId).width());
}

/**
 * jqGridのloadCompleteの共通処理
 * @param {any} gridId：gridのID（#なし）
 * @param {any} parentId：gridの親要素のID（＃なし、Resizeに使用するため必須要素）
 * @param {any} pagerId：papgerを表示する要素のID
 * @param {any} data：gridのレコード
 * @param {any} searchFunc：検索処理（json要素を第一引数にとれるように実装すること）
 * @param {any} maxPagers：複数ページになった際に何個ボタンが表示されるかを設定（省略可）
 */
function commonLoadComplete(gridId, parentId, pagerId, data, searchFunc, maxPagers) {
    commonLoadCompleteBase(gridId, parentId, pagerId, data, maxPagers, searchFunc, false)
}

/**
 * jqGridのloadCompleteの共通処理
 * @param {any} gridId：gridのID（#なし）
 * @param {any} parentId：gridの親要素のID（＃なし、Resizeに使用するため必須要素）
 * @param {any} pagerId：papgerを表示する要素のID
 * @param {any} data：gridのレコード
 * @param {any} searchFunc：検索処理（json要素を第一引数にとれるように実装すること）
 * @param {any} maxPagers：複数ページになった際に何個ボタンが表示されるかを設定（省略可）
 * @param {any} hasAllData：全ページ分のデータを保有しているか
 */
function commonLoadCompleteAllData(gridId, parentId, pagerId, data, maxPagers) {
    commonLoadCompleteBase(gridId, parentId, pagerId, data, maxPagers, null, true)
}

/**
 * jqGridのloadCompleteの共通処理
 * @param {any} gridId：gridのID（#なし）
 * @param {any} parentId：gridの親要素のID（＃なし、Resizeに使用するため必須要素）
 * @param {any} pagerId：papgerを表示する要素のID
 * @param {any} data：gridのレコード
 * @param {any} searchFunc：検索処理（json要素を第一引数にとれるように実装すること）
 * @param {any} maxPagers：複数ページになった際に何個ボタンが表示されるかを設定（省略可）
 * @param {any} hasAllData：全ページ分のデータを保有しているか
 */
function commonLoadCompleteBase(gridId, parentId, pagerId, data, maxPagers, searchFunc, hasAllData) {
    //ページャーのデザイン変更
    settingPager(gridId, searchFunc, maxPagers, hasAllData);
    //ページャーのアイコンにfont-awesome適応
    chagePagerIcon(gridId);
    //画面に合わせて幅を自動調整
    $(window).bind('resize', function () {
        gridResize(gridId, parentId);
    }).trigger('resize');

    if (pagerId) {
        $("#" + pagerId).show();
        if (!data || data.total <= 0) {
            $("#" + pagerId).hide();
        }

        let params = $("#" + gridId).jqGrid("getGridParam");
        if (params.userData && params.userData.length) {
            let itemsCount = parseInt(params.userData.find(x => x.name == "itemsCount").value);
            $(".ui-paging-info").text("合計 : " + itemsCount + "件");
        }
    }
}

/**
 * jqGridの検索に必要なjsonデータを取得
 * @param {any} gridId：gridのID（#なし）
 * @param {any} page：次に取得するpageIndex（省略可、デフォルト現在ページ）
 */
function getSearchParams(gridId, page) {
    let params = $("#" + gridId).jqGrid("getGridParam");
    let jsonData = [];
    let searchPage = 1;
    if (params.userData && params.userData.length) {
        searchPage = params.userData.find(x => x.name == "page").value;
    }
    if (page) {
        searchPage = page;
    }
    jsonData.push(toJson('pageIndex', searchPage));
    jsonData.push(toJson('pageSize', params.rowNum));
    jsonData.push(toJson('sortField', params.sortname ? params.sortname : "_"));
    jsonData.push(toJson('sortOrder', params.sortorder));
    return jsonData;
}

/**
 * jqGridの検索の共通処理
 * @param {any} gridId：gridのID（#なし）
 * @param {any} items：検索結果（GridJsonの型）
 * @param {any} params：検索時のgetSearchParamsの値
 */
function commonSearch(gridId, items, params) {
    let currentPage = 1;
    if (params) {
        currentPage = params.find(x => x.name == "pageIndex").value;
    }
    if (!currentPage) {
        currentPage = 1;
    }
    let userData = [];
    if (params) {
        let pageSize = params.find(x => x.name == "pageSize").value;
        let totalPage = Math.ceil(items.itemsCount / pageSize);
        userData.push(toJson('page', currentPage));
        userData.push(toJson('lastpage', totalPage));
        userData.push(toJson('itemsCount', items.itemsCount));
    }
    else {
        let params = $("#" + gridId).jqGrid("getGridParam");
        userData = params.userData;
    }

    $("#" + gridId).jqGrid("clearGridData")
        .jqGrid("setGridParam", {
            data: items.data,
            userData: userData,
        })
        .trigger("reloadGrid");
}

/**
 * jqGridの検索に必要なjsonデータを取得（userData必須）
 * @param {any} gridId：gridのID（#なし）
 * @param {any} pgButton：検索時のgetSearchParamsの値
 */
function getSearchParamsOnPaging(gridId, pgButton) {
    let params = $("#" + gridId).jqGrid("getGridParam");
    let searchPage = params.page;
    let totalPage = params.lastpage;
    if (params.userData && params.userData.length) {
        searchPage = parseInt(params.userData.find(x => x.name == "page").value);
        totalPage = parseInt(params.userData.find(x => x.name == "lastpage").value);
    }

    if (pgButton == "next") {
        searchPage++;
    } else if (pgButton == "prev") {
        searchPage--;
    } else if (pgButton == "first") {
        searchPage = 1;
    } else if (pgButton == "last") {
        searchPage = totalPage;
    }

    return getSearchParams(gridId, searchPage);
}

/**
 * jqGrid にデータ表示させる
 * @param {any} gridId 
 * @param {any} data
 * @param {any} isKeepPage
 */
function setJqGridData(gridId, data, isKeepPage = false) {
    var grid = $("#" + gridId);
    var page = 1;
    if (isKeepPage) {
        var rowNum = grid.getGridParam("rowNum");
        var validPage = Math.ceil(data / rowNum);
        var currentpage = grid.getGridParam("page");
        page = validPage < currentpage ? 1 : currentpage;
    }
    grid.jqGrid("clearGridData")
        .jqGrid("setGridParam", { data: data, page: page })
        .trigger("reloadGrid");
}

/**
 * 選択データをsort保存用jqGrid に表示させる
 * @param {any} gridId 
 * @param {any} data
 */
function setSortJqGridData(gridId, data) {
    var grid = $("#" + gridId);
    grid.jqGrid("clearGridData");
    for (let i = 0; i < data.length; i++) {
        grid.jqGrid("addRowData", data[i].storeId, data[i]);
    }
}

/**
 * // TODO コメント変更、jsonではない
 * formのデータをjqGridの検索条件として渡すためのjsonに加工します
 * @param {any} form
 */
function getFormDataForJqGrid(form) {
    const data = {};
    const arrayFields = {};

    form.serializeArray().forEach(function (item) {
        const name = item.name;
        const value = item.value || '';

        if (!arrayFields[name]) {
            arrayFields[name] = [];
        }
        if (!arrayFields[name].includes(value)) {
            arrayFields[name].push(value);
        }
    });

    // チェックボックスの処理（未チェックは無視）
    form.find('input[type="checkbox"]').each(function () {
        const name = $(this).attr('name');
        const value = $(this).val();

        if ($(this).is(':checked')) {
            if (!arrayFields[name]) {
                arrayFields[name] = [];
            }
            if (!arrayFields[name].includes(value)) {
                arrayFields[name].push(value);
            }
        }
    });

    // 配列形式で data に格納（key[0], key[1], ...）
    Object.keys(arrayFields).forEach(function (key) {
        const values = arrayFields[key];
        if (values.length === 1) {
            data[key] = values[0];
        } else {
            values.forEach(function (val, idx) {
                data[`${key}[${idx}]`] = val;
            });
        }
    });
    return data;
}

/**
 * jqGridの検索を行う
 * @param {any} gridId
 * @param {any} form
 * @param {any} page
 */
function searchGrid(gridId, form, page) {
    resetGridParamData(gridId);

    if (page) page = $(`#${gridId}`).getGridParam('page');
    $(`#${gridId}`).jqGrid('setGridParam', {
        datatype: "json",
        postData: getFormDataForJqGrid($(form)),
        page: page
    }).trigger("reloadGrid");
}

/**
 * jqGridのpostDataをリセットします
 */
function resetGridParamData(gridId) {
    const grid = $(`#${gridId}`);
    const postData = grid.jqGrid('getGridParam', 'postData');

    $.each(postData, function (key) {
        if (postData.hasOwnProperty(key) && key !== '_search') {
            delete postData[key];
        }
    });
}

/**
* jquery-ui sortable の並び替え時のカラム幅をキープ
*/
function sortableSizeFixed(target) {
    // ヘッダーから列幅取得
    let widthes = $.makeArray(target.find('.jqgfirstrow td').map((_, x) => $(x).width()));

    target.find('tr.jqgrow').each(function () {
        let height = Math.max(...$.makeArray($(this).find('td').map((_, x) => parseInt($(x).height()))));
        $(this).find('td').each(function (index) {
            $(this).width(widthes[index]).height(height);
        });
    });
}

/**
 * jquery-ui sortable の並び替え時のカラム幅をリセット
 */
function sortableSizeReset(target) {
    target.find('tr.jqgrow').each(function () {
        $(this).find('td').each(function () {
            $(this).width('').height('');
        });
    });
}

/**
 * 添付ファイル選択ユニット追加
 * @param {any} addObj / 追加場所のjQueryオブジェクト
 * @param {any} accept / 許可する拡張子
 * @param {any} id / ファイルId
 * @param {any} name / ファイル名
 * @param {any} size / ファイルサイズ
 * @param {any} href / ダウンロードリンク
 * @param {any} aspFileIdName / バインド用ファイルIdの名前属性
 * @param {any} aspFileNameName / バインド用ファイル名の名前属性
 * @param {any} aspFileDataName / バインド用ファイルデータの名前属性
 */
function addFileSelectUnit(addObj, accept, id, name, size, href, aspFileIdName, aspFileNameName, aspFileDataName, aspFileSizeName) {
    if (!accept) { accept = '.*'; }
    if (!id) { id = ''; }
    if (!name) { name = ''; }
    if (!size) { size = ''; }
    if (!aspFileIdName) { aspFileIdName = ''; }
    if (!aspFileNameName) { aspFileNameName = ''; }
    if (!aspFileDataName) { aspFileDataName = ''; }
    if (!aspFileSizeName) { aspFileSizeName = ''; }

    let limit = addObj.data('limit');
    // アップロードできるファイルサイズの上限
    let maxByte = addObj.data('max-byte') ? Number(addObj.data('max-byte')) * 1024 * 1024 : 50 * 1024 * 1024;
    let maxByteSum = addObj.data('max-byte-sum') ? Number(addObj.data('max-byte-sum')) * 1024 * 1024 : 50 * 1024 * 1024;

    // ファイル選択本体
    let main = name
        ? `<a class="form-control border file-download" href=${href}>${name}</a>`
        : `<input type="file"  class="form-control border file-data no-data" accept="${accept}" name="${aspFileDataName}" id="${aspFileDataName.replace(/\./g, '_')}" >`

    // ファイル選択ユニット
    let fileSelectUnit = $(``
        + ` <div class="file-select-unit" >`
        + main
        + `     <input type="hidden" class="file-id" name="${aspFileIdName}" id="${aspFileIdName.replace(/\./g, '_')}" value="${id}" />`
        + `     <input type="hidden" class="file-name" name="${aspFileNameName}" id="${aspFileNameName.replace(/\./g, '_')}" value="${name}" />`
        + `     <input type="hidden" class="file-size" name="${aspFileSizeName}" id="${aspFileSizeName.replace(/\./g, '_')}" value="${size}" />`
        + `     <button class="file-delete-btn btn btn-red" type="button" style="display: ${name ? 'block' : 'none'}" >`
        + `         <i class="fa fa-trash icon-position"></i>削除`
        + `     </button>`
        + `</div>`);

    // 変更検知
    fileSelectUnit.find('.file-data').on('change', function () {
        let fileName = $(this).get(0).files[0]?.name;
        let fileSize = $(this).get(0).files[0]?.size;
        fileSelectUnit.find('.file-name').val(fileName);
        fileSelectUnit.find('.file-size').val(fileSize);

        //ファイルサイズの確認
        if ((fileSize > maxByte)) {
            fileSelectUnit.remove();
            setFileSelectUnitCount(addObj);
            canAddFileSelectUnit(addObj, () => {
                addFileSelectUnit(addObj, accept, null, null, null, null, aspFileIdName, aspFileNameName, aspFileDataName, aspFileSizeName);
            });

            errorSwal(`登録できるファイルの最大サイズは${addObj.data('max-byte') ?? 50}MBまでとなります`, '', function () { });
            return;
        }
        let fileSizeSum = 0;
        addObj.find('.file-size').each((_, obj) => {
            fileSizeSum = fileSizeSum + Number($(obj).val());
        })
        if ((fileSizeSum > maxByteSum)) {
            fileSelectUnit.remove();
            setFileSelectUnitCount(addObj);
            canAddFileSelectUnit(addObj, () => {
                addFileSelectUnit(addObj, accept, null, null, null, null, aspFileIdName, aspFileNameName, aspFileDataName, aspFileSizeName);
            });

            errorSwal(`登録できるファイルの最大合計サイズは${addObj.data('max-byte-sum') ?? 50}MBまでとなります`, '', function () { });
            return;
        }

        if (fileName) {
            $(this).removeClass('no-data');
            fileSelectUnit.find('.file-delete-btn').show();
        }
        else {
            $(this).addClass('no-data');
            if (!fileSelectUnit.next().length) {
                fileSelectUnit.find('.file-delete-btn').hide();
            }
        }
        setFileSelectUnitCount(addObj);
        canAddFileSelectUnit(addObj, () => {
            addFileSelectUnit(addObj, accept);
        });
    });

    // 削除ボタン押下
    fileSelectUnit.find('.file-delete-btn').on('click', function () {
        fileSelectUnit.remove();
        setFileSelectUnitCount(addObj);
        canAddFileSelectUnit(addObj, () => {
            addFileSelectUnit(addObj, accept, null, null, null, null, aspFileIdName, aspFileNameName, aspFileDataName, aspFileSizeName);
        });
    });

    // 追加
    addObj.append(fileSelectUnit);

    // ファイル数表示
    if (limit && limit != 1 && addObj.find('.file-count').length == 0) {
        addObj.prepend(`<div class='file-count'></div>`);
    }
    setFileSelectUnitCount(addObj);
}

/**
* ファイル選択ユニットが追加できるか判定
* @param {any} addObj / 追加場所のjQueryオブジェクト
* @param {any} safeFunc / 追加可能の時のアクション
* @param {any} limitFunc / 添付上限数に至った時のアクション
*/
function canAddFileSelectUnit(addObj, safeFunc, limitFunc) {
    let limit = addObj.data('limit') ?? 999;
    let isFiles = $.makeArray(addObj.find('.file-name').map((_, obj) => $(obj).val().length > 0));
    let isFull = isFiles.length == 0 || isFiles.every(x => x); // 全部セット済みでなくても追加していいようにする
    let isLimit = limit <= isFiles.length;
    if (isLimit && limitFunc) {
        limitFunc();
    }
    if (isFull && !isLimit && safeFunc) {
        safeFunc();
    }
}

/**
 * ファイル数を取得
 * @param {any} addObj / 追加場所のjQueryオブジェクト
 */
function setFileSelectUnitCount(addObj) {
    let limit = addObj.data('limit');
    if (!limit) {
        return;
    }
    let isFiles = $.makeArray(addObj.find('.file-name').map((_, obj) => $(obj).val().length > 0));
    addObj.find('.file-count').text(`${isFiles.filter(x => x).length}/${limit}`);
}

/**
 * 先頭のみ小文字にする
 * （例）ABCD => aBCD
 */
function toTopLower(str) {
    return (typeof str !== 'string' || !str)
        ? str
        : str.charAt(0).toLowerCase() + str.slice(1);
}

/**
 * 先頭のみ大文字にする
 * （例）abcd => Abcd
 */
function toTopUpper(str) {
    return (typeof str !== 'string' || !str)
        ? str
        : str.charAt(0).toUpperCase() + str.slice(1);
}

function getRequestTokenJson() {
    let tokenName = '__RequestVerificationToken';
    return [toJson(tokenName, $('[name="' + tokenName + '"]').val())];
}

/**
 * アイコン付きセレクトボックスの生成
 */
function createIconSelect(selector = '') {
    $(`select.icon-select${selector}`).each(function () {
        const select = $(this);
        select.wrap('<div class="icon-select-wrapper"></div>');

        const wrapper = select.parent('.icon-select-wrapper');

        var selectedVal = select.val();
        var selected = $('<div class="form-control icon-select-selected"></div>');
        if (select.hasClass('mini-label-text')) {
            selected.addClass('mini-label-text');
        }
        wrapper.append(selected);

        var items = $('<div class="icon-select-items icon-select-hide"></div>');
        select.find('option').each(function () {
            var option = $(this);
            var item = $(`<div class="icon-select-item" data-val=${option.val()}></div>`).html((option.is('[data-icon]') ? `<i>${option.data('icon')}</i> ` : '') + option.text());
            if (option.val() == selectedVal) {
                item.addClass('active');
            }
            item.on('click', function () {
                select.val(option.val()).change();
                if (select.hasClass('input-validation-error') || select.hasClass('valid')) {
                    select.show().valid();
                    select.hide();
                }
                selected.html($(this).html());
                if (select.hasClass('input-validation-error')) {
                    selected.addClass('input-validation-error');
                }
                else {
                    selected.removeClass('input-validation-error');
                }
                $('.icon-select-item', items).removeClass('active');
                item.addClass('active');
                items.addClass('icon-select-hide');
            });
            items.append(item);
        });
        wrapper.append(items);
        selected.html(wrapper.find(`.icon-select-item[data-val="${selectedVal}"]`).first().html())

        selected.on('click', function (e) {
            e.stopPropagation();
            iconSelectCloseAll(selected);
            items.toggleClass('icon-select-hide');
            items.animate({
                scrollTop: wrapper.find('.icon-select-item.active').index() * 36
            }, 1);
        });
    });
}

function iconSelectCloseAll(obj) {
    $('.icon-select-items').not(obj).addClass('icon-select-hide');
    $('.icon-select-selected').not(obj).removeClass('active');
};

$(function () {
    createIconSelect();

    $(document).on('click', function () {
        iconSelectCloseAll(null);
    });
});

/**
 * 汎用オートコンプリート初期化関数（debounce 対応）
 * data-min-length: 最小入力文字数（省略時は1）
 * 使用例: initAutocomplete("#Input_TokuisakiNmKj");
 * タグ例: <input asp-for="Input.TokuisakiNmKj"
 *              data-min-length="2"
 *              class="form-control" 
 *              autocomplete="off" />
 * @@param {string} $input - テキストボックスのjQueryオブジェクト
 * @@param {string} url - ハンドラーへのURL
 * @@param {Function} callback - 選択した項目に適用するコールバック関数
 */
function initAutocomplete($input, url, callback = null) {

    const minLength = $input.data("min-length") || 1;

    const settings = {
        minLength: minLength,
        delay: 200,             // jQuery UI の delay（内部の遅延）
        debounceWait: 300,      // 入力揺れを抑えるための debounce 時間
        source: url,            // function or URL
        allowFreeInput: false   // trueなら未選択でもOK
    };

    // -----------------------------
    // debounce 実装
    // -----------------------------
    function debounce(fn, wait) {
        let timer = null;
        return function (...args) {
            clearTimeout(timer);
            timer = setTimeout(() => fn.apply(this, args), wait);
        };
    }

    // -----------------------------
    // source をラップして debounce する
    // -----------------------------
    const debouncedSource = debounce(function (request, response) {
        $.ajax({
            url: settings.source,
            data: { term: request.term },
            global: false, // BlockUIを非表示
            success: response,
            error: () => response([])
        });
    }, settings.debounceWait);

    // -----------------------------
    // autocomplete 初期化
    // -----------------------------
    $input.autocomplete({
        minLength: settings.minLength,
        delay: settings.delay,
        source: debouncedSource,

        // フォーカスのみでは変更を加えない
        focus: function (event, ui) {
            event.preventDefault();
        },

        // 候補選択時
        select: function (event, ui) {
            event.preventDefault();
            $input.val(ui.item.label);
            // コールバックが指定されていればそれを実行する
            if (callback != null) callback(ui.item);
        }

    });

    // -----------------------------
    // フォーム送信時、候補 閉じる
    // -----------------------------
    $input.closest("form").on("submit", function (e) {
        $input.autocomplete("close");
    });
}

