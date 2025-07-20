using BlogMvc.Models.Enums;
using Microsoft.AspNetCore.Identity;

namespace BlogMvc.Helpers;

public static class AuthorizationHelper
{
    /// <summary>
    /// Checks if a user has any of the specified roles
    /// </summary>
    public static async Task<bool> IsInAnyRoleAsync(UserManager<IdentityUser> userManager, IdentityUser user, params string[] roles)
    {
        foreach (var role in roles)
        {
            if (await userManager.IsInRoleAsync(user, role))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Gets the highest role for a user (Admin > User > Guest)
    /// </summary>
    public static async Task<string> GetHighestRoleAsync(UserManager<IdentityUser> userManager, IdentityUser user)
    {
        if (await userManager.IsInRoleAsync(user, UserRoles.Admin))
        {
            return UserRoles.Admin;
        }

        if (await userManager.IsInRoleAsync(user, UserRoles.User))
        {
            return UserRoles.User;
        }

        return UserRoles.Guest;
    }

    /// <summary>
    /// Role-based permissions summary:
    /// - Guest: Can view and search posts only
    /// - User: Can create, edit, and delete their own posts + all Guest permissions
    /// - Admin: Can edit and delete any post + all User permissions
    /// </summary>
    public static class Permissions
    {
        public static bool CanViewPosts(string? role) => true; // Everyone can view

        public static bool CanSearchPosts(string? role) => true; // Everyone can search

        public static bool CanCreatePost(string? role) =>
            role == UserRoles.User || role == UserRoles.Admin;

        public static bool CanEditOwnPost(string? role) =>
            role == UserRoles.User || role == UserRoles.Admin;

        public static bool CanDeleteOwnPost(string? role) =>
            role == UserRoles.User || role == UserRoles.Admin;

        public static bool CanEditAnyPost(string? role) =>
            role == UserRoles.Admin;

        public static bool CanDeleteAnyPost(string? role) =>
            role == UserRoles.Admin;
    }
}