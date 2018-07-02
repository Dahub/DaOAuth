using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Linq;
using System.Globalization;

namespace DaOAuthCore.Service
{
    public class JwtService : ServiceBase, IJwtService
    {
        public string GenerateToken(int minutesLifeTime, string tokenName, string clientId, string scope, Guid? userPublicId)
        {
            return GenerateToken(minutesLifeTime, tokenName, String.Empty, clientId, scope, userPublicId);
        }

        public string GenerateIdToken(int minutesLifeTime, string nonce, string clientId, Guid? userPublicId)
        {
            IList<Claim> claims = new List<Claim>();
            claims.Add(new Claim("iss", Configuration.Issuer));
            claims.Add(new Claim("sub", userPublicId.HasValue ? userPublicId.Value.ToString() : String.Empty));
            claims.Add(new Claim("aud", clientId));
            claims.Add(new Claim("exp", DateTimeOffset.Now.AddMinutes(minutesLifeTime).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture)));
            claims.Add(new Claim("iat", DateTimeOffset.Now.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture)));

            if(!String.IsNullOrEmpty(nonce))
                claims.Add(new Claim("nonce", nonce));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.SecurityKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                signingCredentials: creds,
                expires: DateTime.Now.AddMinutes(minutesLifeTime));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateToken(int minutesLifeTime, string tokenName, string userName, string clientId, string scope, Guid? userPublicId)
        {
            IList<Claim> claims = new List<Claim>();
            claims.Add(new Claim("client_id", clientId));
            claims.Add(new Claim("token_name", tokenName));
            claims.Add(new Claim("issued", DateTimeOffset.Now.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture)));
            claims.Add(new Claim("user_public_id", userPublicId.HasValue ? userPublicId.Value.ToString() : String.Empty));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, !String.IsNullOrEmpty(userName) ? userName : String.Empty));
            claims.Add(new Claim("scope", !String.IsNullOrEmpty(scope) ? scope : String.Empty));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.SecurityKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: Configuration.Issuer,
                audience: "DaOAuth",
                claims: claims,
                signingCredentials: creds,
                expires: DateTime.Now.AddMinutes(minutesLifeTime));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public bool CheckIfTokenIsValid(string token, string tokenName, out long expire, out ClaimsPrincipal user)
        {
            expire = 0;
            user = null;

            if (String.IsNullOrEmpty(token))
                return false;

            var handler = new JwtSecurityTokenHandler();
            TokenValidationParameters validationParameters = new TokenValidationParameters
            {
                ValidIssuer = Configuration.Issuer,
                ValidAudience = "DaOAuth",
                IssuerSigningKeys = new List<SecurityKey>() { new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.SecurityKey)) }
            };
            
            SecurityToken validatedToken;
            try
            {
                user = handler.ValidateToken(token, validationParameters, out validatedToken);
            }
            catch
            {
                return false;
            }

            if (!long.TryParse(GetValueFromClaim(user.Claims, "exp"), out expire))
                return false;

            if (expire < DateTimeOffset.Now.ToUnixTimeSeconds() || GetValueFromClaim(user.Claims, "token_name") != tokenName)
                return false;

            return true;
        }

        public string GetValueFromClaim(IEnumerable<Claim> claims, string claimType)
        {
            var claim = claims.Where(c => c.Type.Equals(claimType, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            if (claim == null)
                return String.Empty;

            return claim.Value;
        }
    }
}
