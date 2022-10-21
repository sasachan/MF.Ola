﻿using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace MyWebApi.Authentication
{
    /// <summary>
    /// Used to get the role within the claims structure used by keycloak, then it adds the role(s) in the ClaimsItentity of ClaimsPrincipal.Identity
    /// </summary>
    public class ClaimsTransformer : IClaimsTransformation
    {
        private readonly IConfiguration _configuration;

        public ClaimsTransformer(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            ClaimsIdentity claimsIdentity = (ClaimsIdentity)principal.Identity;

            // flatten resource_access because Microsoft identity model doesn't support nested claims
            // by map it to Microsoft identity model, because automatic JWT bearer token mapping already processed here
            if (claimsIdentity.IsAuthenticated && claimsIdentity.HasClaim((claim) => claim.Type == "resource_access"))
            {
                var userRole = claimsIdentity.FindFirst((claim) => claim.Type == "resource_access");

                var content = Newtonsoft.Json.Linq.JObject.Parse(userRole.Value);

                foreach (var role in content[_configuration["Identity:ClientApp"]]["roles"])
                {
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role.ToString()));
                }
            }

            return Task.FromResult(principal);
        }
    }
}