using Model.Model;

public static class SyukkinKubunEntity
{
    public static SyukkinKubun CreateSyukkinKubun(
        long? id = 1,
        string? name = null,
        string? nameRyaku = null,
        bool? isSyukkin = false,
        bool? isVacation = false,
        bool? isHoliday = false,
        bool? isNeedKubun1 = false,
        bool? isNeedKubun2 = false)
    {
        return new SyukkinKubun
        {
            Id = id ?? 1,
            Name = name?.Trim() ?? "出勤",
            NameRyaku = nameRyaku?.Trim() ?? "出勤",
            IsSyukkin = isSyukkin ?? false,
            IsVacation = isVacation ?? false,
            IsHoliday = isHoliday ?? false,
            IsNeedKubun1 = isNeedKubun1 ?? false,
            IsNeedKubun2 = isNeedKubun2 ?? false
        };
    }
}
