using DaOAuth.Dal.Fake;
using System;
using Xunit;

namespace DaOAuth.Service.Test
{
    public class ClientServiceTest
    {
        private ClientService cs = new ClientService()
        {
            ConnexionString = "test connexion string",
            Factory = new FakeRepositoriesFactory()
        };

        [Fact]
        public void IsClientValidForAuthorizationCodeGrantTest()
        {
            Assert.False(cs.IsClientValidForAuthorization(String.Empty, "http://www.google.fr", "code"));
            Assert.False(cs.IsClientValidForAuthorization("abc", "http://www.google.fr", "code"));
            Assert.False(cs.IsClientValidForAuthorization("public", "http://www.google.fr", "code"));
            Assert.True(cs.IsClientValidForAuthorization("id-valide", "http://www.google.fr", "code"));
            Assert.False(cs.IsClientValidForAuthorization("id-valide", String.Empty, "code"));
            Assert.False(cs.IsClientValidForAuthorization("id-valide", "http://www.perdu.com", "code"));
        }

        [Fact]
        public void IsCodeValidForAuthorizationCodeGrantTest()
        {
            //Assert.False(cs.IsCodeValidForAuthorizationCodeGrant(String.Empty, "code_correct"), "le client id ne doit pas être null");      
            //Assert.False(cs.IsCodeValidForAuthorizationCodeGrant("id-valide", String.Empty), "le code ne doit pas être vide");
            //Assert.False(cs.IsCodeValidForAuthorizationCodeGrant("id-valide", "code_invalide"), "le code ne doit pas être invalide");
            //Assert.False(cs.IsCodeValidForAuthorizationCodeGrant("id-valide", "code_expiré"), "le code ne doit pas avoir expiré");
            //Assert.True(cs.IsCodeValidForAuthorizationCodeGrant("id-valide", "code_correct"), "client valide, url correcte, code correct, doit être vrai");
        }

        [Fact]
        public void CreateNewClientTest()
        {
            //Assert.Throws<DaOauthServiceException>(() => cs.CreateNewClient("", ""));
            //Assert.Equal<int>(16, cs.CreateNewClient("test", "").Id);
        }

        [Fact]
        public void AddCodeToClientTest()
        {
            //Assert.Throws<DaOauthServiceException>(() => cs.GenerateAndAddCodeToClient("abc", "david", ""));
            //Assert.Equal<int>(32, cs.GenerateAndAddCodeToClient("test", "david", "").Id);
            //Assert.Equal(16, cs.GenerateAndAddCodeToClient("test", "david", "").ClientId);
            //Assert.Equal<int>(24, cs.GenerateAndAddCodeToClient("test", "david", "").CodeValue.Length);
            //Assert.True(cs.GenerateAndAddCodeToClient("test", "david", "").ExpirationTimeStamp <= new DateTimeOffset(DateTime.Now.AddMinutes(10)).ToUnixTimeSeconds());
        }
    }
}
