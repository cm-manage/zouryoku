using Model.Model;

public static class WorkingHourEntity
{
    public static WorkingHour CreateWorkingHours(
        long? id = 0,
        long? syainId = 0,
        DateOnly? hiduke = null,
        decimal? syukkinLatitude = 0,
        decimal? syukkinLongitude = 0,
        decimal? taikinLatitude = 0,
        decimal? taikinLongitude = 0,
        DateTime? syukkinTime = null,
        DateTime? taikinTime = null,
        bool? edited = false,
        bool? deleted = false,
        long? editSyainId = 0,
        long? ukagaiHeaderId = 0)
    {
        return new WorkingHour
        {
            Id = id ?? 1,
            SyainId = syainId ?? 1,
            Hiduke = hiduke ?? new DateOnly(2026, 1, 1),
            SyukkinLatitude = syukkinLatitude ?? 0,
            SyukkinLongitude = syukkinLongitude ?? 0,
            TaikinLatitude = taikinLatitude ?? 0,
            TaikinLongitude = taikinLongitude ?? 0,
            SyukkinTime = syukkinTime,
            TaikinTime = taikinTime,
            Edited = edited ?? false,
            Deleted = deleted ?? false,
            EditSyainId = editSyainId,
            UkagaiHeaderId = ukagaiHeaderId,
        };
    }
}
