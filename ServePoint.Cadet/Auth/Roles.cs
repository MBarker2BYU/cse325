namespace ServePoint.Cadet.Auth;

public static class Roles
{
    public const string User = "User";
    public const string Organizer = "Organizer";
    public const string Instructor = "Instructor";
    public const string Admin = "Admin";

    public static readonly string[] All =
    {
        User,
        Organizer,
        Instructor,
        Admin
    };
}