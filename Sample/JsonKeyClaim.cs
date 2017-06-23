using Newtonsoft.Json.Linq;
using System.Security.Claims;

namespace Sample
{
    internal class JsonKeyClaim
    {
        private ClaimsIdentity identity;
        private string issuer;
        private JObject userData;

        public JsonKeyClaim(ClaimsIdentity identity, JObject userData, string issuer)
        {
            this.identity = identity;
            this.userData = userData;
            this.issuer = issuer;
        }

        internal void TryAddClaimByJsonKey(string propertyName,
            string claimType, string valueType = ClaimValueTypes.String)
        {
            var value = userData?.Value<string>(propertyName);
            if (!string.IsNullOrEmpty(value))
            {
                identity.AddClaim(new Claim(claimType, value, valueType, issuer));
            }
        }

        internal void TryAddClaimByJsonSubKey(string propertyName, string subProperty,
                string claimType, string valueType = ClaimValueTypes.String)
        {
            if (userData != null && userData.TryGetValue(propertyName, out var value))
            {
                var subObject = JObject.Parse(value.ToString());
                if (subObject != null && subObject.TryGetValue(subProperty, out value))
                {
                    if (!string.IsNullOrEmpty(value.ToString()))
                    {
                        identity.AddClaim(new Claim(claimType, value.ToString(), valueType, issuer));
                    }
                }
            }
        }
    }
}