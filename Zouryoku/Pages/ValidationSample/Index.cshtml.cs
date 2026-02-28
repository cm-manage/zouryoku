using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Model.Data;
using System.ComponentModel.DataAnnotations;
using Zouryoku.Attributes;
using Zouryoku.Extensions;
using Zouryoku.Pages.Shared;

namespace Zouryoku.Pages.ValidationSample
{
    //[FunctionAuthorizationAttribute]
    public class IndexModel(ZouContext db, ILogger<IndexModel> logger, IOptions<AppConfig> options) : BasePageModel<IndexModel>(db, logger, options)
    {
        public override bool UseInputAssets { get; } = false;

        [BindProperty(SupportsGet = true)]
        public InputViewModel Input { get; set; } = new();

        public void OnGet()
        {
            Input.Code = "abc";
            Input.Name = "name";
        }

        public async Task<IActionResult> OnPostRegisterAsync()
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "POSTテストエラー１");
                ModelState.AddModelError(string.Empty, "POSTテストエラー２");
                var errorJson = ModelState.ErrorJson();
                if (errorJson is not null)
                {
                    return errorJson;
                }
            }
            return SuccessJson("成功");
        }

        public async Task<IActionResult> OnGetSearchAsync()
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "GETテストエラー１");
                ModelState.AddModelError(string.Empty, "GETテストエラー２");
                var errorJson = ModelState.ErrorJson();
                if (errorJson is not null)
                {
                    return errorJson;
                }
            }
            return SuccessJson(data: "成功");
        }

    }

    public class InputViewModel
    {
        [Display(Name = "コード")]
        [Required(ErrorMessage = "{0}は必須です。")]
        [MinLength(5, ErrorMessage = "{0}は5文字以上で入力してください")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "{0}は数字のみ入力可能です")]
        public string? Code { get; set; }

        [Display(Name = "名前")]
        [Required(ErrorMessage = "{0}は必須です。")]
        [MinLength(10, ErrorMessage = "{0}は10文字以上で入力してください")]
        public string? Name { get; set; }
    }



}
