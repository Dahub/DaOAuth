using DaOAuthCore.Domain;
using System;
using System.Linq;
using System.Collections.Generic;

namespace DaOAuthCore.Service
{
    public static class ExtensionMethods
    {
        public static UserClientDto ToDto(this UserClient value)
        {
            UserClientDto toReturn = new UserClientDto()
            {
                ClientId = value.Client.PublicId,
                ClientDescription = value.Client.Description,
                ClientName = value.Client.Name,
                IsAuthorize = value.IsValid
            };

            if (value.Client.ClientsScopes != null && value.Client.ClientsScopes.Count() > 0)
            {
                string[] scopes = value.Client.ClientsScopes.Select(cs => cs.Scope.NiceWording).ToArray();
                toReturn.ScopesNiceWordings = scopes;
            }
            else
            {
                toReturn.ScopesNiceWordings = Array.Empty<string>();
            }

            return toReturn;
        }

        public static IList<UserClientDto> ToDto(this IEnumerable<UserClient> values)
        {
            IList<UserClientDto> toReturn = new List<UserClientDto>();

            foreach (var c in values)
            {
                toReturn.Add(c.ToDto());
            }

            return toReturn;
        }

        public static UserDto ToDto(this User value)
        {
            return new UserDto()
            {
                BirthDate = value.BirthDate,
                FullName = value.FullName,
                Id = value.Id,
                UserName = value.UserName
            };
        }

        public static IList<ClientDto> ToDto(this IEnumerable<Client> values)
        {
            IList<ClientDto> toReturn = new List<ClientDto>();

            foreach (var c in values)
            {
                toReturn.Add(c.ToDto());
            }

            return toReturn;
        }

        public static ClientDto ToDto(this Client value)
        {
            return new ClientDto()
            {
                IsValid = value.IsValid,
                Name = value.Name,
                PublicId = value.PublicId,
                Description = value.Description,
                Scopes = value.ClientsScopes != null ? value.ClientsScopes.Select(s => s.Scope.Wording).ToArray() : Array.Empty<string>()
            };
        }

        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source != null && toCheck != null && source.IndexOf(toCheck, comp) >= 0;
        }
    }
}
