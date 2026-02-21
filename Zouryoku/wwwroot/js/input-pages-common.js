// 入力画面共通JS
(function(){
  window.InputPages = window.InputPages || {};
  InputPages.closeModal = function(){
    try{ parent.closeIziModal(); }catch(e){ /* noop */ }
  };
})();
