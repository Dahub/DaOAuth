﻿using DaOAuth.Domain;
using System;
using System.Collections.Generic;

namespace DaOAuth.Service
{
    public static class ExtensionMethods
    {
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

            foreach(var c in values)
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
                Name = value.Name
            };
        }

        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source != null && toCheck != null && source.IndexOf(toCheck, comp) >= 0;
        }
    }
}
