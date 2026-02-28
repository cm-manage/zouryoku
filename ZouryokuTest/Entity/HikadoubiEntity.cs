using Model.Model;

public static class HokadoubiEntity
{
    public static Hikadoubi CreateHikadoubi(
        long id,
        DateOnly ymd)
    {
        return new Hikadoubi
        {
            Id = id,
            Ymd = ymd
        };
    }
}
