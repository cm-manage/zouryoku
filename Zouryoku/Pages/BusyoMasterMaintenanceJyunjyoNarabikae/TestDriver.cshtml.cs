using Microsoft.Extensions.Options;
using Model.Data;
using Zouryoku.Attributes;
using Zouryoku.Pages.Shared;

namespace Zouryoku.Pages.BusyoMasterMaintenanceJyunjyoNarabikae;

/// <summary>
/// テスト検証用のドライバページモデルです。開発・テスト環境専用であり、本番環境では使用しません。
/// </summary>
[FunctionAuthorization]
public class TestDriverModel : BasePageModel<TestDriverModel>
{
    public override bool UseInputAssets => true;

    public TestDriverModel(ZouContext db, ILogger<TestDriverModel> logger, IOptions<AppConfig> optionsAccessor)
        : base(db, logger, optionsAccessor)
    {
#if !DEBUG
        throw new InvalidOperationException("TestDriverModelは開発・テスト環境専用のページモデルであり、本番環境では使用できません。");
#endif
    }
}
