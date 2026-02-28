using Model.Model;

public static class BusyoEntity
{
    public static Busyo CreateBusyo(
        long? id = 1,
        string? code = null,
        string? name = null,
        string? kanaName = null,
        string? oyaCode = null,
        DateOnly? startYmd = null,
        DateOnly? endYmd = null,
        short? jyunjyo = 1,
        string? kasyoCode = null,
        string? kaikeiCode = null,
        string? keiriCode = null,
        bool? isActive = true,
        string? ryakusyou = null,
        long? busyoBaseId = 1,
        long? oyaId = 0,
        long? shoninBusyoId = 0)
    {
        var result = new Busyo()
        {
            Code = code?.Trim() ?? $"B{id:D4}",
            Name = name?.Trim() ?? $"部署{id}",
            KanaName = kanaName?.Trim() ?? $"ブショ{id}",
            OyaCode = oyaCode?.Trim() ?? $"OB{id:D4}",
            StartYmd = startYmd ?? DateOnly.MinValue,
            EndYmd = endYmd ?? DateOnly.MaxValue,
            Jyunjyo = jyunjyo ?? 1,
            KasyoCode = kasyoCode?.Trim() ?? $"KAS{id:D4}",
            KaikeiCode = kaikeiCode?.Trim() ?? $"KK{id:D4}",
            KeiriCode = keiriCode?.Trim() ?? $"KR{id:D4}",
            IsActive = isActive ?? true,
            Ryakusyou = ryakusyou?.Trim() ?? $"R{id}",
            BusyoBaseId = busyoBaseId ?? 1,
            OyaId = oyaId ?? 0,
            ShoninBusyoId = shoninBusyoId ?? 0
        };

        if (id.HasValue)
        {
            result.Id = id.Value;
        }

        return result;
    }
}
