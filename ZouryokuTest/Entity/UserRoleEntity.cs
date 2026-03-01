using Model.Enums;
using Model.Model;

public static class UserRoleEntity
{
    public static UserRole CreateUserRole(
        long? id = 1,
        short? code = 0,
        string? name = null,
        short? jyunjo = 0,
        EmployeeAuthority? kengen = EmployeeAuthority.None)
    {
        var result = new UserRole()
        {
            Code = code ?? 0,
            Name = name ?? "株式会社サンプル",
            Jyunjo = jyunjo ?? 0,
            Kengen = kengen ?? EmployeeAuthority.None
        };

        if (id.HasValue)
        {
            result.Id = id.Value;
        }

        return result;
    }
}
