// DatePickerの設定を行うjQueryプラグイン
(function ($) {
    $.fn.datePickerDefault = function (addSetting) {
        defaultArgument(addSetting, {});
        var setting = {
            lang: 'ja',
            weekStart: 0,
            time: false,
            format: 'YYYY/MM/DD',
            clearButton: true,
        };
        $.extend(setting, addSetting);
        return $(this).bootstrapMaterialDatePicker(setting);
    }
})(jQuery);
// DatePicker要素に選択可能範囲を設定するjQueryプラグイン
(function ($) {
    $.fn.limitMinDate = function (date) {
        return $(this).bootstrapMaterialDatePicker('setMinDate', date);
    };
    $.fn.limitMaxDate = function (date) {
        return $(this).bootstrapMaterialDatePicker('setMaxDate', date);
    };
    $.fn.limitDate = function (start, end) {
        return $(this).limitMinDate(start).limitMaxDate(end);
    };
})(jQuery);

// 0～59の配列を返します。
function minuteArray() {
    var arr = new Array(60);
    $.each(arr, function (idx) {
        arr[idx] = idx;
    });
    return arr;
}
// 0,5,10,15,...55を除いた0～59の配列を返します。
function minuteArrayWithout5() {
    return $.grep(minuteArray(), function (value) {
        return value % 5 != 0;
    });
}