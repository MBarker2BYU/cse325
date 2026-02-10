// ReSharper disable InconsistentNaming
namespace ServePoint.Cadet.Security;

public static class AppRoles
{
    public const string User = "User";
    public const string Organizer = "Organizer";
    public const string Instructor = "Instructor";
    public const string Admin = "Admin";

    public static readonly string[] All = [User, Organizer, Instructor, Admin];
}