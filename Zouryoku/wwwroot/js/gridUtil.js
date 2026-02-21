// 外部jsファイル読み込み
//$.getScript(location.origin + rootUrl + 'js/parseUtil.js');
//$(function () {
//    jsGrid.locale('ja');
//});

/** 複数ボタンを配置するエリア */
function multipleBtnArea() {
    return '<div style="display: flex; flex-flow: row; gap: 5px;"></div>';
}

/** 編集ボタン */
function editIconBtn() {
    return "<button type='button' class='btn btn-edit edit-data icon-btn' style='color:#fff;'><i class='fa fa-pen'></i></button>";
};
/** 編集ボタン */
function editBtn() {
    return "<button type='button' class='btn btn-edit edit-data' style='color:#fff;'><i class='fa fa-pen'></i>編集</button>";
};
/** 削除ボタン */
function deleteBtn() {
    return "<button type='button' class='btn waves-effect waves-light icon-trash delete-data btn-grey' id='deleteBtn'>削除</button>";
}
/** パスワードボタン */
function passwordBtn() {
    return "<button type='button' class='btn btn-other waves-effect waves-light icon-lock edit-password'></button>";
}
/** パスワード設定/変更ボタン */
function passwordResetBtn() {
    return "<button type='button' class='btn btn-white setting-password'>パスワード設定/変更</button>";
}
/** 選択ボタン */
function selectBtn() {
    return "<button type='button' class='btn btn-white select-data'>選択</button>";
}
/** 詳細ボタン */
function detailBtn() {
    return "<button type='button' class='btn btn-white detail-data'>詳細</button>";
}
/** 並替ボタン */
function sortBtn() {
    return "<button type='button' class='btn btn-white sort-data'>並替</button>";
}
/** 遷移ボタン */
function transitionBtn(title, display) {
    if (display) {
        return "<button type='button' class='btn btn-white transition-data'>" + title + "遷移</button>";
    } else {
        return "";
    }
}
/** 一括登録ボタン */
function importBtn(display) {
    if (display) {
        return "<button type='button' class='btn btn-white import-data'>一括登録</button>";
    } else {
        return "";
    }
}

function checkBox(id) {
    return $("<input>").attr("type", "checkbox")
        .attr("checked", function () {
            if (value === false) {
                item.uncheck = true;
                item.uncheck = item.indeterminate = false;
                $(this).prop("checked", false);
            } else if (value === true) {
                $(this).prop("checked", true);
                item.checked = true;
                item.uncheck = item.indeterminate = false;
            } else {
                item.indeterminate = true;
                item.checked = item.uncheck = false;
                $(this).prop("indeterminate", true);
            }
        })
        .on("click", function () {
            if (item.uncheck === true && item.checked === false && item.indeterminate === false) {
                item.indeterminate = true;
                item.uncheck = item.checked = false;
                $(this).prop("indeterminate", true);
            } else if (item.uncheck === false && item.checked === false && item.indeterminate === true) {
                item.checked = true;
                item.uncheck = item.indeterminate = false;
                $(this).prop("checked", true);
            } else {
                item.uncheck = true;
                item.indeterminate = item.checked = false;
                $(this).prop("checked", false);
            }
        });
}
function csvOutputButton(url) {
    return '<a href="' + url + '"  class="btn btn-blue-color waves-effect waves-light csv-output">CSV出力</a>';
}

// 検索欄上で操作時にgridを1ページ目に戻して検索
function searchRegister(gridId, startCallback) {
    $('.item-control').on({
        // 検索欄上でEnterキー押下
        'keypress': function (e) {
            if (e.which == 13) {
                if (startCallback) {
                    startCallback();
                }
                searchInput(gridId);
            }
        },
        'change': function (e) {
            //// セレクトボックス変更
            //if ($(e.target).is('select')) {
            //    searchInput(gridId);
            //}
            //// チェックボックス変更
            //if ($(e.target).is('input[type="checkbox"]')) {
            //    searchInput(gridId);
            //}
            //// datepicker変更
            //if ($(e.target).attr('data-dtp')) {
            //    searchInput(gridId);
            //}
        },
    });
}

function searchInput(gridId, pageIndex = 1) {
    if (Object.keys($('#' + gridId).jsGrid("option", "data")).length) {
        $('#' + gridId).jsGrid('openPage', pageIndex);
    } else {
        $('#' + gridId).jsGrid('loadData');
    }
};

// jsGridデータ検索処理
// url:WEB APIのURL
// filter: 検索条件のJSON
function gridLoadData(url, filter, isAsync = true) {
    // 入力された検索条件を設定
    var deferred = $.Deferred();
    $.post({
        url: url,
        data: filter,
        dataType: "json",
        async: isAsync,
    }).done(function (result) {
        deferred.resolve(result);
    });
    return deferred.promise();
};

// 検索条件をセット
function searchCondJson(filter) {
    var param = $.extend(filter, JSON.parse($('form :not(.not-search-cond)').parseJson()));
    return param;
}

/**
 * URLにparamを付与して返します。
 * @param {string} url
 * @param {JSON} params
 */
function paramUrl(url, params) {
    var param = [];
    $.each(params, function (key, value) {
        param.push(key + '=' + value);
    });

    return url + '?' + param.join('&');
}

/**
 * URLにidとrowVersionのparamを付与して返します。
 * @param {string} url
 * @param {any} data
 */
function paramUrlVersion(url, data) {
    var params = {
        id: data.id,
        rowVersion: data.rowVersion
    };
    return paramUrl(url, params);
}