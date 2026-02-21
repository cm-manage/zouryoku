/**
 * 部署選択画面をモーダルで起動する
 * 使用例: openBusyoSelectModal(true, [1, 2, 3]);
 * @param {boolean} isMultiSelect - 複数選択を許可するかどうか
 * @param {Array<number|string>|null} preSelectedIds - 事前選択する部署ID配列（null 可）
 */
function openBusyoSelectModal(isMultiSelect, preSelectedIds) {

    // モーダルの設定用定数
    const MODAL_WIDTH = 900;
    const MODAL_HEIGHT = 700;
    const MODAL_ID = 'busyo-select-modal';
    const MODAL_TITLE = '部署選択';
    const HEADER_COLOR = 'var(--iframe-header-color)';

    // クエリパラメータの生成
    const params = new URLSearchParams();
    params.append('multiFlag', isMultiSelect);

    // preSelectedIds が配列の場合のみクエリに追加
    if (Array.isArray(preSelectedIds)) {
        preSelectedIds.forEach(id => params.append('PreSelectedIds', id));
    }

    // 部署選択画面のURLを生成
    const url = `${location.origin}/BusyoSentaku/Index?${params.toString()}`;

    // モーダル表示
    showInputModal(
        MODAL_ID,
        url,
        null,
        MODAL_TITLE,
        null,
        MODAL_WIDTH,
        MODAL_HEIGHT,
        HEADER_COLOR
    );
}

/**
 * 社員選択画面をモーダルで起動する
 * 使用例: openSyainSelectModal(true, [1_2_3]);
 * @param {boolean} isMultipleSelect - 複数選択フラグ
 * @param {Array<number | string> | null}  preSelectedIds - 事前選択する社員BaseID配列（null可）
 */
function openSyainSelectModal(isMultipleSelect, preSelectedIds = null) {

    // モーダル設定用定数
    const MODAL_ID = 'syain-sentaku-modal';
    const MODAL_TITLE = '社員選択';
    const MODAL_WIDTH = 1000;
    const MODAL_HEIGHT = 700;
    const HEADER_COLOR = 'var(--iframe-header-color)';

    // クエリパラメータの生成
    const params = new URLSearchParams();
    params.append('isMultipleSelection', isMultipleSelect);

    // idsパラメータを追加（社員BaseIDを'_'で連結）
    if (isMultipleSelect && Array.isArray(preSelectedIds) && preSelectedIds.length > 0) {
        params.append('ids', preSelectedIds.join('_'));
    }

    // 社員選択画面のURLを生成
    const url = `${location.origin}/SyainSentaku/Index?${params.toString()}`;

    // モーダル表示
    showInputModal(
        MODAL_ID,
        url,
        null,
        MODAL_TITLE,
        null,
        MODAL_WIDTH,
        MODAL_HEIGHT,
        HEADER_COLOR
    );
}
