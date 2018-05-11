using DaOAuth.Domain;
using System;
using System.Linq;
using System.Collections.Generic;

namespace DaOAuth.Service
{
    public static class ExtensionMethods
    {
        public static UserClientDto ToDto(this UserClient value)
        {
            UserClientDto toReturn = new UserClientDto()
            {
                ClientDescription = value.Client.Description,
                ClientName = value.Client.Name,
                IsAuthorize = value.IsValid
            };

            if(value.Client.Scopes != null)
            {
                string[] scopes = value.Client.Scopes.Select(s => s.NiceWording).ToArray();
                toReturn.ScopesNiceWordings = scopes;
            }
            else
            {
                toReturn.ScopesNiceWordings = new string[] { };
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
                Scopes = value.Scopes != null ? value.Scopes.Select(s => s.Wording).ToArray() : new string[] { }
            };
        }

        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source != null && toCheck != null && source.IndexOf(toCheck, comp) >= 0;
        }
    }
}
