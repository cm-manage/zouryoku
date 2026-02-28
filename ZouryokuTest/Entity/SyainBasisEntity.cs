using Model.Model;

public static class SyainBasisEntity
{
    public static SyainBasis CreateSyainBasis(
        long? id = 1,
        string? name = null,
        string? code = null)
    {
        var result = new SyainBasis
        {
            Name = name?.Trim() ?? $"社員{id}",
            Code = code?.Trim() ?? $"S{id:D4}"
        };

        if (id.HasValue)
        {
            result.Id = id.Value;
        }

        return result;
    }
}
