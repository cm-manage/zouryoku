document.addEventListener("DOMContentLoaded", function () {
    // --------------------------------------------------------------
    // 定数
    // --------------------------------------------------------------
    const accordion = $('#accordion-menu');
    const parentMenuBadge = $('#badge-parent-menu');
    const offcanvas = $('#offcanvas-menu');

    // --------------------------------------------------------------
    // 初回バッジ表示
    // --------------------------------------------------------------
    fetchBadgeCount();

    // --------------------------------------------------------------
    // Offcanvas 内のリンクをクリックしたときの処理
    // --------------------------------------------------------------
    $(document).on('click', '[data-role="hide-link"]', function (e) {
        e.preventDefault();
        const hrefLink = this.href;

        // Offcanvas を閉じる
        const instance = bootstrap.Offcanvas.getOrCreateInstance(offcanvas[0]);
        instance.hide();

        offcanvas.one('hidden.bs.offcanvas', function () {
            window.location.href = hrefLink;
        });
    });

    // --------------------------------------------------------------
    // Offcanvas 内のモーダルを開くリンクをクリックしたときの処理
    // --------------------------------------------------------------
    $(document).on('click', '[data-role="open-modal"]', function () {
        const modalId = $(this).data('modal-id');
        const url = $(this).data('url');
        const title = $(this).data('title');

        // Offcanvas を閉じる
        const instance = bootstrap.Offcanvas.getOrCreateInstance(offcanvas[0]);
        instance.hide();

        // 少し待ってからモーダルを表示
        offcanvas.one('hidden.bs.offcanvas', function () {
            ensureIziModalLoaded(function () {
                showFullModal(modalId, url, title, '');
            });
        });
    });

    // --------------------------------------------------------------
    // アコーディオンのパネルが開いたときの処理
    // --------------------------------------------------------------
    accordion.on('show.bs.collapse', function (e) {

        // 親メニューのボタンを取得
        const header = $(e.target)
            .closest('.accordion-item')
            .children('.accordion-header')
            .find('.accordion-button');

        // 親メニューボタン内のバッジを取得
        const badge = header.find('.badge');

        // 親メニューをアクティブ化
        header.addClass('app-active_header');

        // バッジを非表示
        $(badge).addClass('d-none');
    });

    // --------------------------------------------------------------
    // アコーディオンのパネルが閉じたときの処理
    // --------------------------------------------------------------
    accordion.on('hidden.bs.collapse', function (e) {

        // 親メニューのボタンを取得
        const header = $(e.target)
            .closest('.accordion-item')
            .children('.accordion-header')
            .find('.accordion-button');

        // 親メニューボタン内のバッジを取得
        // 注意：バッジに数値を表示する前提の処理
        // 　　　文字列は0件として扱い表示しない
        // 　　　"99+"などの文字列を表示したい場合は修正する必要あり
        const badge = header.find('.badge');
        const count = Number($(badge).text()) || 0;

        // 親メニューを非アクティブ化
        header.removeClass('app-active_header');
        
        // バッジ件数が１件以上の場合、バッジを表示
        if (count > 0) {
            $(badge).removeClass('d-none');
        }
    });

    // --------------------------------------------------------------
    // Offcanvas が閉じたときの処理
    // --------------------------------------------------------------
    $(document).on('hidden.bs.offcanvas', function (e) {
        if (e.target.id !== 'offcanvas-menu') return;

        // 開いているパネルをすべて閉じる
        accordion.find('.accordion-collapse.show').each(function () {
            bootstrap.Collapse.getOrCreateInstance(this).hide();
            $(this).prev().find('.accordion-button').addClass('collapsed');
        });
    });

    // --------------------------------------------------------------
    // メニューボタンをクリックしたときの処理
    // --------------------------------------------------------------
    $(document).on('click', '#menu-button', function () {
        // バッジを再取得する
        fetchBadgeCount();
    });

    /**
     * 申請確認のバッジの件数を取得して表示する
     */
    function fetchBadgeCount() {

        const url = "/api/SinseiKensuBadge/count";
        const id = "#badge-parent-menu, #badge-child-menu";

        $.ajax({
            url: url,
            method: "GET",
            global: false,
            success: function (count) {

                if (count > 0) {
                    $(id).text(count).removeClass("d-none");
                } else {
                    $(id).text(0).addClass("d-none");
                }
            },
            error: function () {
                console.error("バッジの件数の取得に失敗しました。");
            }
        });
    };

    /**
     * iziModalの読み込みを保証する処理
     * @param {any} callback - 読み込み保証後の処理
     */
    function ensureIziModalLoaded(callback) {
        if ($.fn.iziModal) {
            // すでにiziModalを読み込んでいる場合、処理実行
            if (callback) callback();
            return;
        }

        // iziModalを読み込む
        $.getScript("/assets/modules/iziModal/js/iziModal.min.js")
            .done(function () {
                // iziModalを読み込めたら処理を行う
                if (callback) callback();
            })
            .fail(function () {
                console.error("iziModalの読み込みに失敗しました。");
            });
    }
});
