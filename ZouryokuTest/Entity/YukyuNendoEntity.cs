using Model.Model;

public static class YukyuNendoEntity
{
    public static YukyuNendo CreateYukyuNendo(
        long? id = 1,
        short? nendo = 2025,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        bool? isThisYear = false,
        bool? updated = false
    )
    {
        return new YukyuNendo()
        {
            Id = id ?? 1,
            Nendo = nendo ?? 2025,
            StartDate = startDate ?? new DateOnly(2024, 1, 1),
            EndDate = endDate ?? new DateOnly(2024, 12, 31),
            IsThisYear = isThisYear ?? false,
            Updated = updated ?? false
        };
    }
}
