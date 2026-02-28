using Microsoft.AspNetCore.Mvc.ModelBinding;
using Model.Data;
using Model.Enums;
using Model.Model;
using static Zouryoku.Utils.Const;

namespace Zouryoku.Pages.JissekiNyuryoku
{
    public class JissekiNyuryokuUnConfirmValidator(ZouContext db, Nippou nippou, ModelStateDictionary modelState)
    {

        private readonly JissekiNyuryokuQueryService _queryService = new(db);
        private readonly Nippou _nippou = nippou;
        private readonly ModelStateDictionary _modelState = modelState;

        /// <summary>
        /// 確定解除時チェック
        /// </summary>
        /// <returns></returns>
        public async Task ValidateOnUnconfirmAsync()
        {
            // 解除済み
            if (_nippou.TourokuKubun != DailyReportStatusClassification.確定保存)
            {
                _modelState.AddModelError(string.Empty, ErrorNippouAlreadyUnconfirmed);
                return;
            }

            // 連動済み
            if (_nippou.IsRendouZumi)
            {
                _modelState.AddModelError(string.Empty, ErrorNippouKeiriRendouzumi);
                return;
            }

            // 最終確定日から順に解除する必要あり
            var hasNipou = await _queryService.HasKakuteizumiNippouAferDate(_nippou.SyainId, _nippou.NippouYmd);
            if (hasNipou)
            {
                _modelState.AddModelError(string.Empty, ErrorCannotUnconfirmDueToLaterConfirmeData);
                return;
            }
        }
    }
}
