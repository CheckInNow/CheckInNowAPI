using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Identity;

public sealed class EmailConfirmationTokenProviderOptions : DataProtectionTokenProviderOptions
{
    public const string ProviderName = "EmailConfirmation";
    public static readonly TimeSpan DefaultLifespan = TimeSpan.FromHours(24);

    public EmailConfirmationTokenProviderOptions()
    {
        Name = ProviderName;
        TokenLifespan = DefaultLifespan;
    }
}

public sealed class EmailConfirmationTokenProvider<TUser>(
    IDataProtectionProvider dataProtectionProvider,
    IOptions<EmailConfirmationTokenProviderOptions> options,
    ILogger<DataProtectorTokenProvider<TUser>> logger)
    : DataProtectorTokenProvider<TUser>(dataProtectionProvider, options, logger)
    where TUser : class;
