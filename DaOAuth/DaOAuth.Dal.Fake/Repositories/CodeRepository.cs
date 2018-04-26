using System;
using System.Collections.Generic;
using DaOAuth.Dal.Interface;
using DaOAuth.Domain;

namespace DaOAuth.Dal.Fake
{
    internal class CodeRepository : ICodeRepository
    {
        public IContext Context { get; set; }

        public void Add(Code toAdd)
        {
            toAdd.Id = 32;
        }

        public void Delete(Code toDelete)
        {
            
        }

        public IEnumerable<Code> GetAllByClientId(string clientPublicId)
        {
            return new List<Code>() {
                new Code() { ClientId = 16, ExpirationTimeStamp = new DateTimeOffset(DateTime.Now.AddMinutes(10)).ToUnixTimeSeconds(), Id = 1, IsValid = true, CodeValue = "code_correct" },
                new Code() { ClientId = 16, ExpirationTimeStamp = new DateTimeOffset(DateTime.Now.AddMinutes(-10)).ToUnixTimeSeconds(), Id = 2, IsValid = true, CodeValue = "code_expiré" },
                new Code() { ClientId = 16, ExpirationTimeStamp = new DateTimeOffset(DateTime.Now.AddMinutes(-10)).ToUnixTimeSeconds(), Id = 3, IsValid = false, CodeValue = "code_invalide" }
            };
        }

        public void Update(Code toUpdate)
        {
            
        }
    }
}
