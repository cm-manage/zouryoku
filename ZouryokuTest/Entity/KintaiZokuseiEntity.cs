using Model.Enums;
using Model.Model;

public static class KintaiZokuseiEntity
{
    public static KintaiZokusei CreateKintaiZokusei(
        long? id = 1,
        string? name = null,
        decimal? seigenTime = 0.00m,
        bool? isMinashi = false,
        decimal? maxLimitTime = null,
        bool? isOvertimeLimit3m = false,
        EmployeeWorkType? code = null)
    {
        var result = new KintaiZokusei
        {
            Name = name?.Trim() ?? "標準",
            SeigenTime = seigenTime ?? 45.00m,
            IsMinashi = isMinashi ?? false,
            MaxLimitTime = maxLimitTime ?? 0m,
            IsOvertimeLimit3m = isOvertimeLimit3m ?? false,
            Code = code ?? EmployeeWorkType.月45時間
        };

        if (id.HasValue)
        {
            result.Id = id.Value;
        }

        return result;
    }
}
