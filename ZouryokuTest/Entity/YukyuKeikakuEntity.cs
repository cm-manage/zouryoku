using Model.Enums;
using Model.Model;

public static class YukyuKeikakuEntity
{
    public static YukyuKeikaku CreateYukyuKeikaku(
        long? id = 0,
        LeavePlanStatus? status = LeavePlanStatus.未申請,
        long? yukyuNendoId = 0,
        long? syainBaseId = 0,
        ICollection<YukyuKeikakuMeisai>? yukyuKeikakuMeisais = null
    )
    {
        var entity = new YukyuKeikaku()
        {
            Id = id ?? 0,
            Status = status ?? LeavePlanStatus.未申請,
            YukyuNendoId = yukyuNendoId ?? 0,
            SyainBaseId = syainBaseId ?? 0
        };

        if (yukyuKeikakuMeisais != null)
        {
            entity.YukyuKeikakuMeisais = yukyuKeikakuMeisais;
        }
        return entity;
    }
}
