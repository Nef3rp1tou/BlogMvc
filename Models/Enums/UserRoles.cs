namespace BlogMvc.Models.Enums;
public static class UserRoles
{
    public const string Guest = "Guest";
    public const string User = "User";
    public const string Admin = "Admin";

    public static readonly string[] AllRoles = { Guest, User, Admin };
}