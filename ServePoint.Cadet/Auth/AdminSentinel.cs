namespace ServePoint.Cadet.Auth;

public class AdminSentinel
{
    public static string GetProtectedAdminEmail(IConfiguration config)
        => config["DefaultAdmin:Email"]
           ?? throw new InvalidOperationException("Missing config: DefaultAdmin:Email");

    public static bool IsProtectedAdmin(string? userEmail, IConfiguration config)
        => !string.IsNullOrWhiteSpace(userEmail)
           && string.Equals(userEmail, GetProtectedAdminEmail(config), StringComparison.OrdinalIgnoreCase);
}