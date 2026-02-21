function parseStrToBoolean(str) {
    return (str.toLowerCase() == 'true') ? true : false;
}
function parseJson(data) {
    var returnJson = {};
    for (idx = 0; idx < data.length; idx++) {
        returnJson[data[idx].name] = data[idx].value;
    }
    return JSON.stringify(returnJson);
}
// 指定要素をJsonにparseするjQueryプラグイン
(function ($) {
    $.fn.parseJson = function () {
        return parseJson(this.serializeArray());
    };
})(jQuery);