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
        public void CheckIfClientIsValidTest()
        {
            Assert.False(cs.CheckIfClientIsValid(String.Empty));
            Assert.False(cs.CheckIfClientIsValid("abc"));
            Assert.False(cs.CheckIfClientIsValid("public"));
            Assert.True(cs.CheckIfClientIsValid("id-valide"));
        }

        [Fact]
        public void CreateNewClientTest()
        {
            Assert.Throws<DaOauthServiceException>(() => cs.CreateNewClient("", ""));
            Assert.Equal<int>(16, cs.CreateNewClient("test", "").Id);
        }

        [Fact]
        public void AddCodeToClientTest()
        {
            Assert.Throws<DaOauthServiceException>(() => cs.AddCodeToClient("abc"));
            Assert.Equal<int>(32, cs.AddCodeToClient("test").Id);
            Assert.Equal(16, cs.AddCodeToClient("test").ClientId);
            Assert.Equal<int>(24, cs.AddCodeToClient("test").CodeValue.Length);
            Assert.True(cs.AddCodeToClient("test").ExpirationTimeStamp <= new DateTimeOffset(DateTime.Now.AddMinutes(10)).ToUnixTimeSeconds());
        }
    }
}
