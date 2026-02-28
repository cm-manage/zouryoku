// Validatorによるバリデーションの共通設定および関数群
// Bootstrap 4/5のスタイルに合わせた設定を行う
// jQuery Validation Plugin と jQuery Unobtrusive Validation を使用することを前提とする
// jQuery と jQuery Validation Plugin、jQuery Unobtrusive Validation の読み込みが必要
// _ValidationScriptsPartial.cshtml よりあとで読み込むこと

/*
* jQuery Validation Default Settings
* Bootstrapのスタイルに合わせたエラーのスタイルクラス適用
* エラーメッセージの配置をdata-valmsg-for属性のspan要素に変更
*/
$.validator.setDefaults({
    highlight: function (element) {
        updateGroupValidation(element, true);
    },
    unhighlight: function (element) {
        updateGroupValidation(element, false);
    },
    errorClass: "invalid-feedback",
    errorPlacement: function (error, element) {
        var name = element.attr("name");
        $("span[data-valmsg-for='" + name + "']").html(error);
    }
});

/**
 * グループバリデーションのハイライト/アンハイライト処理
 * data-valid-group属性、data-valid-group-target属性を使用して
 * グループバリデーションのスタイルを制御する
 * data-valid-group属性はグループのキーを指定し、
 * data-valid-group-target属性はターゲットのキーを指定する
 * data-valid-group属性を持つ要素がバリデーションエラーの場合、
 * 同じグループキーを持つ要素すべてにis-invalidクラスを付与する
 * @param {any} element - highlight/unhighlightを行う要素
 * @param {any} isInvalid - 自身のバリデーション結果
 * @returns - グループバリデーションの結果
 */
function updateGroupValidation(element, isInvalid) {
    const $element = $(element);
    const targetKey = element.dataset.validGroup ?? element.dataset.validGroupTarget;
    const validator = $element.closest('form').data('validator');

    // 通常のバリデーションスタイル更新
    $element.toggleClass("is-invalid", isInvalid);

    // グループバリデーション
    if (!targetKey) return;

    // バリデーション対象の要素
    const targets = $(`[data-valid-group-target='${targetKey}']`).not($element);
    // バリデーション結果を反映する要素
    const groups = $(`[data-valid-group-target='${targetKey}'],[data-valid-group='${targetKey}']`);

    // data-valid-group-target属性を持つ要素のバリデーション結果をグループ全体の結果とする
    const hasInvalid = targets.toArray().some(el => !validator.element(el));

    if (element.dataset.validGroupTarget) {
        // data-valid-group-target属性を持つ要素の場合、自身のバリデーションも確認する
        // 無限ループを避けるため、自身のバリデーション結果は引数で受け取る
        groups.toggleClass('is-invalid', hasInvalid || isInvalid);
        return;
    }

    // data-valid-group属性を持つ要素の場合、ターゲット要素のバリデーション結果のみ確認する
    groups.toggleClass('is-invalid', hasInvalid);
}

/**
* validatorの初期化
* バリデーションを行う画面は必ず、当関数を呼ぶ必要がある。
* @param {string} formSelector - フォームのセレクタ
*/
function initValidation(formSelector) {
    $.validator.unobtrusive.parse(formSelector);
}

/**
* エラー表示をクリアする共通処理
* @param {string} formSelector - フォームのセレクタ
*/
function clearErrors(formSelector) {

    var form = $(formSelector);

    // 入力欄から is-invalid を外す
    // input要素以外にも対応する場合は、ここを拡張する必要がある
    form.find("input, textarea, select").removeClass("is-invalid");

    // エラーメッセージをクリア
    form.find("span[data-valmsg-for]").text("");

}

/**
 * フォームフィールドごとのバリデーションメッセージを適用する共通関数
 * 使用例:
 * applyValidateMessages($("#myForm"), {
 *  "FieldName1": ["フィールド1のエラーメッセージ1", "フィールド1のエラーメッセージ2"],
 *  "FieldName2": ["フィールド2のエラーメッセージ"]
 * });
 * @param {any} $form フォームの jQuery オブジェクト
 * @param {any} errors サーバーからのバリデーションエラーオブジェクト
 * @return {void}
 */
function applyValidateMessages($form, errors) {
    var validator = $form.data("validator");
    if (!validator) {
        $.validator.unobtrusive.parse($form);
        validator = $form.data("validator");
    }
    // key が空文字列のものは、画面上部のエラーメッセージエリアに表示させるため除外
    const fieldErrors = {};
    for (const key in errors) {
        // 空文字列のキーはスキップ
        if (key === "") {
            continue;
        }
        // 改行でメッセージをjoin
        fieldErrors[key] = errors[key].join("<br>");
    }

    // フィールドエラーをUIに反映
    if (validator) {
        validator.showErrors(fieldErrors);
    }
}

/**
 * 画面上部へのメッセージ適用
 * キーが空文字列のエラーメッセージを画面上部に表示する
 * 例えば、
 * ModelState.AddModelError("", "全体のエラーメッセージ");
 * のようにサーバー側で設定されたメッセージを表示する
 * @param {any} messages - サーバーからのメッセージの配列
 * @returns {void}
 */
function applyValidationSummaryMessage(errors) {
    // 画面上部に表示するエラーのみ取り出し、あれば表示
    const messages = errors[""] ?? [];
    if (Array.isArray(messages) && messages.length !== 0) {
        const message = messages.join("\r\n");
        errorMessage(message);
    }
}