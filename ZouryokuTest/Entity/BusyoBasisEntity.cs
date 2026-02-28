using Model.Model;

public static class BusyoBasisEntity
{
    public static BusyoBasis CreateBusyoBasis(
        long? id = 1,
        string? name = null,
        long? bumoncyoId = 0)
    {
        return new BusyoBasis
        {
            Id = id ?? 1,
            Name = name?.Trim() ?? $"部署{id}",
            BumoncyoId = bumoncyoId
        };
    }
}
