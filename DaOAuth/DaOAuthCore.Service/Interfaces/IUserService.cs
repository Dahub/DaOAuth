namespace DaOAuthCore.Service
{
    public interface IUserService
    {
        UserDto Find(string userName, string password);
        bool CheckIfUserExist(string userName);
        UserDto CreateUser(UserDto toCreate, string password);
    }
}
