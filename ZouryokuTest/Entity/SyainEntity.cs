using Model.Enums;
using Model.Model;
using static Model.Enums.BusinessTripRole;

public static class SyainEntity
{
    public static Syain CreateSyain(
        long? id = 1,
        string? code = null,
        string? name = null,
        string? kanaName = null,
        char? seibetsu = null,
        string? busyoCode = null,
        int? syokusyuCode = null,
        int? syokusyuBunruiCode = null,
        DateOnly? nyushaYmd = null,
        DateOnly? startYmd = null,
        DateOnly? endYmd = null,
        short? kyusyoku = 0,
        BusinessTripRole? syucyoSyokui = null,
        string? kingsSyozoku = null,
        short? kaisyaCode = 0,
        bool? isGenkaRendou = false,
        string? eMail = null,
        string? keitaiMail = null,
        EmployeeAuthority? kengen = null,
        short? jyunjyo = 0,
        bool? retired = false,
        long? gyoumuTypeId = 1,
        string? phoneNumber = null,
        long? syainBaseId = 1,
        long? busyoId = 1,
        long? kintaiZokuseiId = 1,
        long? userRoleId = 1)
    {
        var result = new Syain
        {
            Code = code?.Trim() ?? $"S{id:D4}",
            Name = name?.Trim() ?? $"社員{id}",
            KanaName = kanaName?.Trim() ?? $"シャイン{id}",
            Seibetsu = seibetsu ?? '1',
            BusyoCode = busyoCode?.Trim() ?? $"B{id:D4}",
            SyokusyuCode = syokusyuCode ?? 0,
            SyokusyuBunruiCode = syokusyuBunruiCode ?? 0,
            NyuusyaYmd = nyushaYmd ?? new DateOnly(2020, 1, 1),
            StartYmd = startYmd ?? DateOnly.MinValue,
            EndYmd = endYmd ?? DateOnly.MaxValue,
            Kyusyoku = kyusyoku ?? 0,
            SyucyoSyokui = syucyoSyokui ?? _2_6級,
            KingsSyozoku = kingsSyozoku?.Trim() ?? $"K{id:D4}",
            KaisyaCode = kaisyaCode ?? 0,
            IsGenkaRendou = isGenkaRendou ?? false,
            EMail = eMail?.Trim() ?? $"syain{id}@example.com",
            KeitaiMail = keitaiMail?.Trim() ?? $"keitai{id}@example.com",
            Kengen = kengen ?? 0,
            Jyunjyo = jyunjyo ?? 0,
            Retired = retired ?? false,
            GyoumuTypeId = gyoumuTypeId,
            PhoneNumber = phoneNumber,
            SyainBaseId = syainBaseId ?? 1,
            BusyoId = busyoId ?? 1,
            KintaiZokuseiId = kintaiZokuseiId ?? 1,
            UserRoleId = userRoleId ?? 1,
        };

        if (id.HasValue)
        {
            result.Id = id.Value;
        }

        return result;
    }
}
