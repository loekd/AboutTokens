using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;
using Duende.IdentityServer.Validation;
using System.Security.Claims;
using Duende.IdentityServer.Extensions;
using System.Diagnostics;

namespace IdentityServer.Customizations;


/// <summary>
/// Default consent service
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="DefaultConsentService" /> class.
/// </remarks>
/// <param name="clock">The clock.</param>
/// <param name="userConsentStore">The user consent store.</param>
/// <param name="logger">The logger.</param>
/// <exception cref="System.ArgumentNullException">store</exception>
public class DefaultConsentService(IClock clock, IUserConsentStore userConsentStore, ILogger<DefaultConsentService> logger) : Duende.IdentityServer.Services.DefaultConsentService(clock, userConsentStore, logger)
{
    public override async Task<bool> RequiresConsentAsync(ClaimsPrincipal subject, Client client, IEnumerable<ParsedScopeValue> parsedScopes)
    {
        if (client == null) throw new ArgumentNullException(nameof(client));
        if (subject == null) throw new ArgumentNullException(nameof(subject));

        //return Task.FromResult(false);

        //if (!client.RequireConsent)
        //{
        //    Logger.LogDebug("Client is configured to not require consent, no consent is required");
        //    return false;
        //}

        if (parsedScopes == null || !parsedScopes.Any())
        {
            Logger.LogDebug("No scopes being requested, no consent is required");
            return false;
        }

        if (!client.AllowRememberConsent)
        {
            Logger.LogDebug("Client is configured to not allow remembering consent, consent is required");
            return true;
        }

        if (parsedScopes.Any(x => x.ParsedName != x.RawValue))
        {
            Logger.LogDebug("Scopes contains parameterized values, consent is required");
            return true;
        }

        var scopes = parsedScopes.Select(x => x.RawValue).ToArray();

        // we always require consent for offline access if
        // the client has not disabled RequireConsent 
        //if (scopes.Contains(IdentityServerConstants.StandardScopes.OfflineAccess))
        //{
        //    Logger.LogDebug("Scopes contains offline_access, consent is required");
        //    return true;
        //}

        var consent = await UserConsentStore.GetUserConsentAsync(subject.GetSubjectId(), client.ClientId);

        if (consent == null)
        {
            Logger.LogDebug("Found no prior consent from consent store, consent is required");
            return true;
        }

        if (consent.Expiration.HasExpired(Clock.UtcNow.UtcDateTime))
        {
            Logger.LogDebug("Consent found in consent store is expired, consent is required");
            await UserConsentStore.RemoveUserConsentAsync(consent.SubjectId, consent.ClientId);
            return true;
        }

        if (consent.Scopes != null)
        {
            var intersect = scopes.Intersect(consent.Scopes);
            var different = scopes.Count() != intersect.Count();

            if (different)
            {
                Logger.LogDebug("Consent found in consent store is different than current request, consent is required");
            }
            else
            {
                Logger.LogDebug("Consent found in consent store is same as current request, consent is not required");
            }

            return different;
        }

        Logger.LogDebug("Consent found in consent store has no scopes, consent is required");

        return true;
    }

    public override async Task UpdateConsentAsync(ClaimsPrincipal subject, Client client, IEnumerable<ParsedScopeValue> parsedScopes)
    {
        if (client == null) throw new ArgumentNullException(nameof(client));
        if (subject == null) throw new ArgumentNullException(nameof(subject));

        if (client.AllowRememberConsent)
        {
            var subjectId = subject.GetSubjectId();
            var clientId = client.ClientId;

            var scopes = parsedScopes?.Select(x => x.RawValue).ToArray();
            if (scopes != null && scopes.Any())
            {
                Logger.LogDebug("Client allows remembering consent, and consent given. Updating consent store for subject: {subject}", subject.GetSubjectId());

                var consent = new Consent
                {
                    CreationTime = Clock.UtcNow.UtcDateTime,
                    SubjectId = subjectId,
                    ClientId = clientId,
                    Scopes = scopes
                };

                if (client.ConsentLifetime.HasValue)
                {
                    consent.Expiration = consent.CreationTime.AddSeconds(client.ConsentLifetime.Value);
                }

                await UserConsentStore.StoreUserConsentAsync(consent);
            }
            else
            {
                Logger.LogDebug("Client allows remembering consent, and no scopes provided. Removing consent from consent store for subject: {subject}", subject.GetSubjectId());

                await UserConsentStore.RemoveUserConsentAsync(subjectId, clientId);
            }
        }
    }
}

internal static class DateTimeExtensions
{
    [DebuggerStepThrough]
    public static bool HasExceeded(this DateTime creationTime, int seconds, DateTime now)
    {
        return (now > creationTime.AddSeconds(seconds));
    }

    [DebuggerStepThrough]
    public static int GetLifetimeInSeconds(this DateTime creationTime, DateTime now)
    {
        return ((int)(now - creationTime).TotalSeconds);
    }

    [DebuggerStepThrough]
    public static bool HasExpired(this DateTime? expirationTime, DateTime now)
    {
        if (expirationTime.HasValue &&
            expirationTime.Value.HasExpired(now))
        {
            return true;
        }

        return false;
    }

    [DebuggerStepThrough]
    public static bool HasExpired(this DateTime expirationTime, DateTime now)
    {
        if (now > expirationTime)
        {
            return true;
        }

        return false;
    }
}

