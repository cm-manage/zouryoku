(function ($) {
    /**
     * カスタム済みのjqGridを提供します。
     * $(対象要素).customJqGridで呼び出し、通常のjqGrid同様のオプションが利用できます。
     * 追加の独自オプションとして、customOptionsが指定できます。
     * 詳細はjgGridのカスタムオプション（CustomJqGridOption）を参照
     */
    $.fn.customJqGrid = function (params) {
        const customOpts = params.customOptions ?? new CustomJqGridOption();

        // デフォルト設定
        const defaultParams = {
            pgbuttons: true,
            pginput: false,
            viewrecords: true,
            recordtext: '{2}件中 {0} - {1}件を表示',
            height: null,
            width: null,
            emptyrecords: "検索結果がありません", 
        };
        // 強制する設定
        const forceParams = {
            toppager: true,
            shrinkToFit: false,
            autowidth: true,
        }
        params = $.extend({}, defaultParams, forceParams, params);
        const grid = this.jqGrid(params);

        customJqGridSetting(grid, customOpts, params);

        return grid;
    }
})(jQuery);

/**
 * jgGridのカスタムオプション
 * customJqGrid生成時にjqGridの標準オプションと合わせて使用します。
 * 未指定の場合は、初期値が使用されます。
 * 使用例：
 * const customOptions = new CustomJqGridOption({
 *      autoHeight: false,
 * });
 * customOptions.headerBackgroundColor = '#2d85eb';
 * 
 * $(対象要素).customJqGrid({
 *      customOptions: customOptions,
 * });
 */
class CustomJqGridOption {
    /**
     * jgGridのカスタムオプションを初期化します。
     */
    constructor(options = {}) {
        /**
         * 親要素に合わせて高さを自動調整するかどうか（初期値：true）
         * 親要素の高さを適切に指定してください。
         */
        this.autoHeight = options.autoHeight || true;

        /**
         * ヘッダー背景色（初期値：#56AF45）
         */
        this.headerBackgroundColor = options.headerBackgroundColor || '#E1E1E1';
        /**
         * ヘッダーの境界線の色（初期値：#19a137）
         */
        this.headerBorderColor = options.headerBorderColor || '#c2c2c2';
        /**
         * ヘッダー文字色（初期値：#FFF）
         */
        this.headerFontColor = options.headerFontColor || '#000000';

        /**
         * 行の境界線の色（初期値：#E5E5E5）
         */
        this.rowBorderColor = options.rowBorderColor || '#E5E5E5';

        /**
         * 行のストライプカラー（初期値：#E4E4E4）
         */
        this.rowStripeColor = options.rowStripeColor || '#f6f6f6';

        /**
         * ページャーに表示するページボタンの最大数（初期値：5）
         * 3以上の値を指定してください。
         */
        this.maxPageButton = options.maxPageButton || 5;

        /**
         * ソート機能を無効にするかどうか（初期値：false）
         */
        this.sortDisable = options.sortDisable || false;
    }
}

function customJqGridSetting(grid, options, params) {
    const jqGrid = $(grid).closest('.ui-jqgrid');
    const header = $('.ui-jqgrid-hdiv', jqGrid);
    const pager = $('.ui-jqgrid-pager', jqGrid);
    const toppager = $('.ui-jqgrid-toppager', jqGrid);

    jqGrid.addClass('custom-jqgrid');

    $('.ui-jqgrid-labels', header).css({
        'background': options.headerBackgroundColor,
        'color': options.headerFontColor,
    });

    $('.ui-jqgrid-labels th:not([style*="display: none"]):not(:last-of-type)', header).css({
        'border-right': `2px solid ${options.headerBorderColor}`,
    });

    // グリッドの読み込みが完了したら実行する処理
    grid.on('jqGridAfterLoadComplete', function (e, data) {

        // 偶数行のみ背景色をグレーにするクラスを追加
        $('tbody > tr:even', grid).addClass('grid-even-row');

        $('tbody td:not([style*="display: none"]):not(:last-of-type)', grid).css({
            'border-right': `2px solid ${options.rowBorderColor}`,
        });

        $.each(params.colModel, (idx, val) => {
            // ヘッダーの文字配置調整
            if (val.header) {
                header.find('.ui-th-column').eq(idx).find('.ui-th-div').addClass(val.header);
            }
        });

        // ページャー
        const pagerControll = $('.ui-pager-control', pager);
        pagerControll.empty();

        let currentPage = grid.getGridParam('page');
        let lastPage = grid.getGridParam('lastpage');
        let rowNum = grid.getGridParam('rowNum');
        let rowList = grid.getGridParam('rowList');
        let startRecords = (currentPage - 1) * rowNum + 1;
        let lastRecords = startRecords + grid.getGridParam('reccount') - 1;

        if (data.userdata) {
            currentPage = data.userdata.find(x => x.name == 'page').value;
            lastPage = data.userdata.find(x => x.name == 'lastpage').value;
            rowNum = grid.getGridParam('rowNum');
            rowList = grid.getGridParam('rowList');
            startRecords = (currentPage - 1) * rowNum + 1;
            lastRecords = startRecords + grid.getGridParam('reccount') - 1;
        }

        // Param更新
        grid.jqGrid('setGridParam', { lastpage: lastPage });

        let isFirst = currentPage == 1;
        const firstButton = $(`<button type="button" class="custom-pager-button first" ${isFirst ? 'disabled' : ''}></button>`);
        const prevButton = $(`<button type="button" class="custom-pager-button prev" ${isFirst ? 'disabled' : ''}></button>`);

        firstButton.on('click', function () {
            customJqGridPaging(grid, { page: 1 }, 'first');
        });
        prevButton.on('click', function () {
            customJqGridPaging(grid, { page: currentPage - 1 }, 'prev');
        });

        pagerControll.append(firstButton);
        pagerControll.append(prevButton);

        let pageHalf = (options.maxPageButton - 1) / 2;
        let pageMin = Math.max(1, currentPage - pageHalf);
        let pageMax = Math.min(lastPage, currentPage + pageHalf);
        let pageDiff = options.maxPageButton - (pageMax - pageMin + 1);
        if (pageDiff != 0) { // ボタンが最大数より少ない場合、残りを追加してあげる
            pageMin = Math.max(1, pageMin - pageDiff);
            pageMax = Math.min(lastPage, pageMax + pageDiff);
        }
        let pageButtons = [...Array(pageMax - pageMin + 1)].map((_, i) => i + pageMin);

        pageButtons.forEach(page => {
            const button = $(`<button type="button" class="custom-pager-button page-btn" ${currentPage == page ? 'disabled' : ''} data-page="${page}">${page}</button>`);

            button.on('click', function() {
                // ページングボタン押下のイベントの為にuserDataを更新
                let userData = grid.jqGrid('getGridParam').userData;
                if (userData && Object.keys(userData).length !== 0) {
                    userData.find(x => x.name == 'page').value = page;
                    grid.jqGrid('setGridParam', { userData: userData });
                }

                customJqGridPaging(grid, { page: page }, page);
            });
            pagerControll.append(button);
        });

        let isLast = currentPage == lastPage;
        const nextButton = $(`<button type="button" class="custom-pager-button next" ${isLast ? 'disabled' : ''}></button>`);
        const lastButton = $(`<button type="button" class="custom-pager-button last" ${isLast ? 'disabled' : ''}></button>`);

        nextButton.on('click', function () {
            customJqGridPaging(grid, { page: currentPage + 1 }, 'next');
        });
        lastButton.on('click', function () {
            customJqGridPaging(grid, { page: lastPage }, 'last');
        });

        pagerControll.append(nextButton);
        pagerControll.append(lastButton);

        if (lastPage > 0) {
            // 行数変更セレクトボックス生成
            const rowSelect = $('<select class="custom-pager-select"></select>');
            $.each(rowList, function (_, row) {
                const option = $(`<option value="${row}" ${row == rowNum ? 'selected' : ''}>${row}</option>`);
                rowSelect.append(option);
            });
            rowSelect.on('change', function () {
                let rowNum = $(this).val();
                customJqGridPaging(grid, {
                    page: 1,
                    rowNum: rowNum
                }, 'first');
            });
            pagerControll.append(rowSelect);
        }

        // ヘッダー部生成
        const toppagerControl = $('.ui-pager-control', toppager);
        toppagerControl.empty();

        // ソート機能
        if (!options.sortDisable) {
            const sortname = grid.getGridParam('sortname');
            const sortorder = grid.getGridParam('sortorder');

            const sortItem = $('<div class="sort"><span>並び替え</span></div>')

            const sortSelect = $('<select class="custom-sort-select"></select>');
            $.each(grid.getGridParam('colModel'), (idx, val) => {
                if (!val.hidden && val.sortable != false) {
                    sortSelect.append(`<option value="${val.name}" ${sortname == val.name ? 'selected' : ''}>${val.label}</option>`);
                }
            });
            sortSelect.on('change', function () {
                customJqGridPaging(grid, { sortname: $(this).val(), sortorder: sortorder }, currentPage);
            });
            sortItem.append(sortSelect);

            const sortAscDescSelect = $('<select class="custom-ascdesc-select"></select>');
            sortAscDescSelect.append(`<option class="asc" value="asc" ${sortorder == 'asc' ? 'selected' : ''}>昇順 &#xf15d</option>`);
            sortAscDescSelect.append(`<option class="desc" value="desc" ${sortorder == 'desc' ? 'selected' : ''}>降順 &#xf881</option>`);
            sortAscDescSelect.on('change', function () {
                customJqGridPaging(grid, { sortname: sortname, sortorder: $(this).val() }, currentPage);
            });
            sortItem.append(sortAscDescSelect);

            toppagerControl.append(sortItem);
        }

        const records = data.userdata ?
            data.userdata.find(x => x.name == 'itemsCount').value
            : grid.getGridParam('records');
        toppagerControl.append(`<div class="page">${params.recordtext.replace('{0}', startRecords).replace('{1}', lastRecords).replace('{2}', records)}</div>`);

        // 親要素に合わせて高さを自動調整
        if (options.autoHeight) {
            const parent = jqGrid.parent();
            const otherHeight = $.map(parent.siblings(), (e) => $(e).outerHeight(true)).reduce((sum, val) => sum + val, 0);

            parent.css('height', `calc(100% - ${otherHeight}px)`);
            jqGrid.css('height', `100%`);

            const topPagerHeight = $('.ui-jqgrid-toppager', jqGrid).outerHeight(true);
            const bottomPagerHeight = $('.ui-jqgrid-pager', jqGrid).outerHeight(true);
            const headerHeight = $('.ui-jqgrid-hdiv', jqGrid).outerHeight(true)

            $('.ui-jqgrid-view', jqGrid).css('height', `calc(100% - ${bottomPagerHeight}px)`);
            $('.ui-jqgrid-bdiv', jqGrid).css('height', `calc(100% - ${topPagerHeight + headerHeight}px)`);
        }
    });
}

function customJqGridPaging(grid, gridParam, onPagingParam) {
    let onPaging = grid.jqGrid('getGridParam', 'onPaging');
    if (onPaging) {
        grid.jqGrid('setGridParam', gridParam).jqGrid('getGridParam', 'onPaging')(onPagingParam);
    }
    else {
        grid.jqGrid('setGridParam', gridParam).trigger('reloadGrid');
    }
}