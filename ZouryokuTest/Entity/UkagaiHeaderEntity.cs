using Model.Enums;
using Model.Model;

public static class UkagaiHeaderEntity
{
    public static UkagaiHeader CreateUkagaiHeader(
        long? id = 1,
        long? syainId = 1,
        DateOnly? shinseiYmd = null,
        long? shoninSyainId = 1,
        DateOnly? shoninYmd = null,
        long? lastShoninSyainId = 1,
        ApprovalStatus? status = null,
        DateOnly? lastShoninYmd = null,
        DateOnly? workYmd = null,
        TimeOnly? kaishiJikoku = null,
        TimeOnly? syuryoJikoku = null,
        string? biko = null,
        bool? invalid = false)
    {
        return new UkagaiHeader
        {
            Id = id ?? 1,
            SyainId = syainId ?? 1,
            ShinseiYmd = shinseiYmd ?? new DateOnly(2026, 1, 1),
            ShoninSyainId = shoninSyainId,
            ShoninYmd = shoninYmd,
            LastShoninSyainId = lastShoninSyainId,
            Status = status ?? ApprovalStatus.承認待,
            LastShoninYmd = lastShoninYmd,
            WorkYmd = workYmd ?? new DateOnly(2026, 1, 1),
            KaishiJikoku = kaishiJikoku,
            SyuryoJikoku = syuryoJikoku,
            Biko = biko,
            Invalid = invalid ?? false,
        };
    }
}
