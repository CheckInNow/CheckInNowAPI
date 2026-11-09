using System.Text;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using NSubstitute;

namespace Application.UnitTests.Features.Identity;

internal static class IdentityTestHelpers
{
    public static UserManager<ApplicationUser> CreateUserManager()
    {
        var store = Substitute.For<IUserStore<ApplicationUser>>();
        return Substitute.For<UserManager<ApplicationUser>>(
            store, null, null, null, null, null, null, null, null);
    }

    public static string Encode(string token) => WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
}
