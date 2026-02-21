function modalSetting(url, modalId, title, subtitle, closedFunc, width, height, zindex, headerColor) {
    modalId = defaultArgument(modalId, 'iziModal');
    title = defaultArgument(title, '');
    subtitle = defaultArgument(subtitle, '');
    // ifame内にmodalIdを引き渡すために追記
    if (url && url.indexOf('?') == -1) {
        url += "?modalId=" + modalId;
    } else {
        url += "&modalId=" + modalId;
    }
    closedFunc = defaultArgument(closedFunc, function () { });
    let destroyAndclosedFunc = function () {
        // destroyしないと同一画面で再使用時にキャッシュが残る場合がある
        // モーダルのアニメーションや他のイベント処理が完了する前に destroy を呼ぶと、内部で参照している DOM 要素が存在しない状態を考慮し、時間をずらす
        setTimeout(() => {
            $('#' + modalId).iziModal('destroy');
            parent.$('#' + modalId).iziModal('destroy');
        }, 100);

        if (closedFunc) {
            closedFunc();
        }
    };
    // モーダル初期設定
    let settingJson = {
        headerColor: 'var(--iframe-header-color)',
        icon: 'icon-info',
        iframe: true,
        fullscreen: true,
        overlayClose: true,
        title: title,
        subtitle: subtitle,
        transitionIn: 'comingIn',
        transitionOut: 'comingOut',
        iframeURL: url,
        closeButton: true,
        onClosed: destroyAndclosedFunc
    };
    if (!isUndefined(width)) settingJson.width = width;
    if (!isUndefined(height)) settingJson.iframeHeight = height;
    if (!isUndefined(zindex)) settingJson.zindex = zindex;
    if (!isUndefined(headerColor)) settingJson.headerColor = headerColor;
    return settingJson;
}

function openProcess(modalId, settingJson, isParent) {
    let ctrl = isParent ? parent.$('#' + modalId) : $('#' + modalId);
    if (!ctrl.length) {
        let base = isParent ? parent.$("body") : $("body");
        base.append('<div id="' + modalId + '"></div>');
    }
    // 再取得
    ctrl = isParent ? parent.$('#' + modalId) : $('#' + modalId);

    ctrl.iziModal(settingJson);
    // モーダル表示
    openIziModal(ctrl, settingJson.openFullscreen);
}

/**
 * モーダルを表示します
 * @param {number} id
 * @param {string} url
 * @param {string} modalId
 * @param {string} title
 * @param {string} subtitle
 * @param {number} width
 * @param {number} height
 */
function showModal(modalId, url, title, subtitle, closedFunc, width, height, headerColor) {
    // モーダル初期設定
    let settingJson = modalSetting(url, modalId, title, subtitle, closedFunc, width, height, null, headerColor);
    settingJson.zindex = getZindex();
    // モーダル表示
    openProcess(modalId, settingJson, false);
}

/**
 * モーダルをフルスクリーンで表示します
 * @param {string} modalId
 * @param {string} url
 * @param {string} title
 * @param {string} subtitle
 * @param {Function} closedFunc
 */
function showFullModal(modalId, url, title, subtitle, closedFunc, headerColor) {
    // モーダル初期設定
    var settingJson = modalSetting(url, modalId, title, subtitle, closedFunc, null, null, null, headerColor);
    settingJson.openFullscreen = true;
    settingJson.fullscreen = false;
    settingJson.zindex = getZindex();
    // モーダル表示
    openProcess(modalId, settingJson, false);
}

/**
 * 親画面側でモーダルを表示します(iframe用)
 * @param {string} modalId
 * @param {string} url
 * @param {string} title
 * @param {string} subtitle
 * @param {number} width
 * @param {number} height
 */
function showParentModal(modalId, url, title, subtitle, closedFunc, width, height, headerColor) {
    // モーダル初期設定
    var settingJson = modalSetting(url, modalId, title, subtitle, closedFunc, width, height, null, headerColor);
    settingJson.zindex = getZindex();
    // モーダル表示
    openProcess(modalId, settingJson, true);
}

/**
 * 親画面側でモーダルをフルスクリーンで表示します(iframe用)
 * @param {string} modalId
 * @param {string} url
 * @param {string} title
 * @param {string} subtitle
 */
function showParentFullModal(modalId, url, title, subtitle, closedFunc, headerColor) {
    // モーダル初期設定
    var settingJson = modalSetting(url, modalId, title, subtitle, closedFunc, null, null, null, headerColor);
    settingJson.openFullscreen = true;
    settingJson.fullscreen = false;
    settingJson.zindex = getZindex();
    // モーダル表示
    openProcess(modalId, settingJson, true);
}

/**
 * 編集画面用モーダルを表示します
 * @param {string} modalId
 * @param {URL} url
 * @param {number} id
 * @param {Function} closedFunc
 * @param {number} width
 * @param {number} height
 */
function showInputModal(modalId, url, id, title, closedFunc, width, height, headerColor) {
    var inputUrl = url;
    if (id) {
        inputUrl = toQueryUrl(inputUrl) + 'id=' + id;
    }
    showModal(modalId, inputUrl, title, '', closedFunc, width, height, headerColor);
}

/**
 * 親の編集画面用モーダルを表示します
 * @param {string} modalId
 * @param {URL} url
 * @param {number} id
 * @param {Function} closedFunc
 * @param {number} width
 * @param {number} height
 */
function showParentInputModal(modalId, url, id, title, closedFunc, width, height, headerColor) {
    var inputUrl = url;
    if (id) {
        inputUrl = toQueryUrl(inputUrl) + 'id=' + id;
    }
    showParentModal(modalId, inputUrl, title, '', closedFunc, width, height, headerColor);
}

function showPrevNextModal(modalId, url, title, id, idList, closedFunc, width, height, headerColor) {
    let modal = ""
        + '<div id="' + modalId + '">'
        + '     <ul id="' + idListKeyId + '" style="display:none;"></ul>'
        + '</div>';
    parent.$('#' + modalId).remove()
    parent.$("body").append(modal);

    $.each(idList, function (index, val) {
        parent.$('#' + modalId).find('#' + idListKeyId).append('<li value="' + val + '"></li>');
    });

    var inputUrl = url;
    if (id) {
        inputUrl = toQueryUrl(inputUrl) + 'id=' + id;
    }

    showCommonModal(modalId, inputUrl, title, closedFunc, false, width, height, headerColor);
}

function showSelectModal(modalId, url, key, title, closedFunc, width, height, headerColor) {
    if (!parent.$('#' + modalId).length) {
        let modal = ""
            + '<div id="' + modalId + '">'
            + '     <input type="hidden" id="' + cancelSelectedKeyId + '" value="true"/>'
            + '</div>';
        parent.$("body").append(modal);
    }
    parent.$('#' + modalId).find('#' + cancelSelectedKeyId).val(true);

    saveSelectKey(key, function () {

        //選択されたKeyとキャンセル状況をcallbackで返すため
        let func = function () {
            if (closedFunc) {
                getSelectKey(function (record) {
                    closedFunc(parseStrToBoolean(parent.$('#' + modalId).find('#' + cancelSelectedKeyId).val()), record);
                });
            }
        };
        let modalTitle = title ? title : '選択画面';
        showCommonModal(modalId, url, modalTitle, func, false, width, height, headerColor);
    });
}

function showMultiSelectModal(modalId, url, selectedKeys, selectableKeys, title, closedFunc, width, height, headerColor) {
    if (!parent.$('#' + modalId).length) {
        let modalHtml = ""
            + '<div id="' + modalId + '">'
            + '     <input type="hidden" id="' + cancelSelectedKeyId + '" value="true"/>'
            + '</div>';
        parent.$("body").append(modalHtml);
    }

    parent.$('#' + modalId).find('#' + cancelSelectedKeyId).val(true);
    saveMultiSelectKeysAndMultiSelectableKeys(selectedKeys, selectableKeys, function () {

        //選択されたKeyとキャンセル状況をcallbackで返すため
        let func = function () {

            if (closedFunc) {
                getMultiSelectKeys(function (records) {
                    closedFunc(parseStrToBoolean(parent.$('#' + modalId).find('#' + cancelSelectedKeyId).val()), records);
                });
            }
        };
        let modalTitle = title ? title : '選択画面';
        showCommonModal(modalId, url, modalTitle, func, false, width, height, headerColor);
    });
}

function showImageModal(modalId, url, closedFunc, width, height, headerColor) {
    if (!parent.$('#' + modalId).length) {
        parent.$("body").append('<div id="' + modalId + '"></div>');
    }
    showCommonModal(modalId, url, '画像画面', closedFunc, true, width, height, headerColor);
}

function showCommonModal(modalId, url, title, closedFunc, isFull, width, height, headerColor) {
    // モーダル初期設定
    var settingJson = modalSetting(url, modalId, title, '', closedFunc, width, height, null, headerColor);
    settingJson.openFullscreen = false;
    settingJson.fullscreen = true;
    if (isFull) {
        settingJson = modalSetting(url, modalId, title, '', closedFunc, null, null, null, headerColor);
        settingJson.openFullscreen = true;
        settingJson.fullscreen = false;
    }

    let zindex = getZindex();
    if ($('#' + modalId).length) {
        settingJson.zindex = zindex > 1000 ? zindex : 1000;
        $('#' + modalId).iziModal(settingJson);
        openIziModal($('#' + modalId), isFull);
    }
    else {
        settingJson.zindex = zindex > 1000 ? zindex : 2000;
        parent.$('#' + modalId).iziModal(settingJson);
        openIziModal(parent.$('#' + modalId), isFull);
    }
}

function getZindex() {
    let zindex = 0;
    $.each($('.iziModal'), function (index, data) {
        let zi = parseInt($(data).css('z-index'));
        if (zi > zindex) {
            zindex = zi;
        }
    });
    $.each(parent.$('.iziModal'), function (index, data) {
        let zi = parseInt($(data).css('z-index'));
        if (zi > zindex) {
            zindex = zi;
        }
    });
    return zindex + 1000;
}

function openIziModal(modal, isFull) {
    if (!isFull) {
        if (modal.hasClass("isFullscreen")) {
            modal.removeClass("isFullscreen");
        }
        if (modal.hasClass("isAttached")) {
            modal.removeClass("isAttached");
        }
    }
    modal.iziModal('open');
}

/**
 * モーダルを閉じます
 * 呼び出したmodal画面側が自分自身を閉じるためにこの関数を呼び出します
 * ex) parent.closeIziModal(); 
 */
function closeIziModal(modal) {
    let ctrl = $('[data-izimodal-close]')[0];
    if (modal) {
        ctrl = $('#' + modal).find('[data-izimodal-close]');
        if (!ctrl || !ctrl.length) {
            ctrl = parent.$('#' + modal).find('[data-izimodal-close]');
        }
    }
    if (ctrl) {
        ctrl.click();
    }
}

/**
 * 現在のモーダルを閉じます
 */
function closeCurrentIziModal() {
    let currentModalId = $("#parent-modal").val();

    if (!currentModalId) {
        let modal = getCurrentIziModal(parent);
        if (modal.length) {
            currentModalId = modal[0].id;
        }
    }

    parent.closeIziModal(currentModalId);
}


/**
 *  親要素から現在表示中の iziModal を取得する。
 *  @param {any} parent 親要素（window.top）
 */
function getCurrentIziModal(parent) {
    var modal = $(parent.$('[aria-hidden="false"]')[0]);
    parent.$('[aria-hidden="false"]').each(function (index, ctrl) {
        if ($(ctrl).css('z-index') > modal.css('z-index')) {
            modal = $(ctrl);
        }
    });
    return modal;
}
