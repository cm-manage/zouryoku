// 金額範囲バリデーションの定義
// C#のlong.MaxValue（9,223,372,036,854,775,807）を扱うために、JavaScriptのBigIntを使用しています。
$.validator.addMethod("currencyrange", function (value, element, params) {
    // 空欄は有効とする
    if (value === null || value.trim() === "") {
        return true;
    }

    // 数字とカンマのみ許可する正規表現
    var regex = /^(?:\d{1,3}(?:,\d{3})*|\d+)$/;
    if (!regex.test(value)) {
        return false;
    }

    // カンマを除去してからBigIntに変換
    var normalized = value.replace(/,/g, "");
    // C#のlong.Maxvalueを扱うために、BigIntで処理する
    var amount = BigInt(normalized);

    //if (isNaN(amount)) {
    //    return false;
    //}

    // 範囲チェック
    var min = BigInt(params.min);
    var max = BigInt(params.max);

    return min <= amount && amount <= max;
});

// 金額範囲バリデーションのアンカーを追加
// 使用例（Razorビュー）:
// <input asp-for="Amount" data-val="true" data-val-currencyrange="金額は{0}から{1}の間で入力してください。" data-val-currencyrange-min="1000" data-val-currencyrange-max="1000000" />
// 上記の例では、Amountフィールドに対して通貨範囲のバリデーションを設定しています。
// 最小値は1000、最大値は1000000で、エラーメッセージは「金額は{0}から{1}の間で入力してください。」となります。
// Razorビューでの属性の説明:
// data-val="true" : バリデーションを有効にする
// data-val-currencyrange : バリデーションエラーメッセージ
// data-val-currencyrange-min : 最小値
// data-val-currencyrange-max : 最大値
// これらの属性は、CurrencyRangeAttributeから自動的に生成されます。
$.validator.unobtrusive.adapters.add("currencyrange", ["min", "max"], function (options) {
    options.rules["currencyrange"] = {
        min: options.params.min,
        max: options.params.max
    };
    options.messages["currencyrange"] = options.message;
});
