using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace MyWebApi.Authentication
{
    public static class ConfigureAuthenticationServiceExtensions
    {
        private static RsaSecurityKey BuildRSAKey(string publicKeyJWT)
        {
            RSA rsa = RSA.Create();

            rsa.ImportSubjectPublicKeyInfo(

                source: Convert.FromBase64String(publicKeyJWT),
                bytesRead: out _
            );

            var IssuerSigningKey = new RsaSecurityKey(rsa);

            return IssuerSigningKey;
        }

        public static void ConfigureJWT(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IClaimsTransformation, ClaimsTransformer>();

            var AuthenticationBuilder = services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            });

            AuthenticationBuilder.AddJwtBearer(o =>
            {
                #region == JWT Token Validation ===

                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidIssuers = new[] { $"{configuration["Identity:Endpoint"]}/realms/{configuration["Identity:Realms"]}" },
                    IssuerSigningKey = BuildRSAKey(configuration["Identity:PublicKeyJWT"]),
                };
            });
        }
    }
}