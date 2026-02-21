using Microsoft.Extensions.Options;
using Model.Data;
using Zouryoku.Attributes;
using Zouryoku.Pages.Shared;

namespace Zouryoku.Pages.DakokuJikanSyusei
{
    /// <summary>
    /// テスト検証用のドライバページモデルです。開発・テスト環境専用であり、本番環境では使用しません。
    /// </summary>
    [FunctionAuthorization]
    public class TestDriverModel(ZouContext context, ILogger<TestDriverModel> logger, IOptions<AppConfig> options)
        : BasePageModel<TestDriverModel>(context, logger, options)
    {
        public override bool UseInputAssets => true;
    }
}
