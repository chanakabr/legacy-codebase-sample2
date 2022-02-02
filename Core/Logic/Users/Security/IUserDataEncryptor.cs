using ApiObjects;

namespace ApiLogic.Users.Security
{
    public interface IUserDataEncryptor
    {
        EncryptionType? GetUsernameEncryptionType(int groupId);
        string CorrectUsernameCase(EncryptionType? encryptionType, string clearUsername);
        string EncryptUsername(int groupId, EncryptionType? encryptionType, string clearUsername);
        string DecryptUsername(int groupId, EncryptionType? encryptionType, string encryptedUsername);
        string DecryptUsername(int groupId, string encryptedUsername);
    }
}
