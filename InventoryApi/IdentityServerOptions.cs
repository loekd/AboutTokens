using System.ComponentModel.DataAnnotations;

namespace InventoryApi
{
    /// <summary>
    /// Can be bound from configuration to set up Identity Server for authentication and authorization to this API.
    /// </summary>
    public class IdentityServerOptions
    {
        public const string ConfigurationSectionName = "IdentityServer";

        /// <summary>
        /// Location of Identity Server
        /// </summary>
        [StringLength(128, MinimumLength = 20)]
        public string Authority { get; set; } = "https://localhost:7242";

        /// <summary>
        /// Identifier of this API resource.
        /// The required value of the audience (aud) claim in the token.
        /// Prevents token forwarding to other services.
        /// </summary>
        [StringLength(128, MinimumLength = 2)]
        public string Audience { get; set; } = "InventoryApi";

        /// <summary>
        /// The scopes allowed to access this API, separated by semicolons.
        /// </summary>
        [StringLength(128, MinimumLength = 2)]
        public string RequiredReadWriteScopes { get; set; } = "Inventory.All;Inventory.ReadWrite";

        /// <summary>
        /// Allowed token issuer FQDN, concatenated by ';'
        /// </summary>
        public string Issuers { get; set; } = "https://localhost:7242";

        public override string ToString()
        {
            return $"Auth: {Authority} - Aud: {Audience} - Scp: {RequiredReadWriteScopes} - Iss:{Issuers}";
        }
    }
}