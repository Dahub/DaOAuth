using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace DaOAuthCore.Service
{
    public interface IJwtService
    {
        string GenerateToken(int minutesLifeTime, string tokenName, string clientId, string scope, Guid? userPublicId);
        string GenerateToken(int minutesLifeTime, string tokenName, string userName, string clientId, string scope, Guid? userPublicId);
        bool CheckIfTokenIsValid(string token, string tokenName, out long expire, out ClaimsPrincipal user);
        string GetValueFromClaim(IEnumerable<Claim> claims, string claimType);
    }
}
