using ApiObjects;

namespace ApiLogic.Users.Security
{
    public interface IUserEncryptionKeyStorage
    {
        byte[] GenerateRandomEncryptionKey(EncryptionType encryptionType);

        /// <returns>false - if the group already has some key</returns>
        bool AddEncryptionKey(EncryptionKey encryptionKey, long updaterId);

        /// <returns>
        /// true and the key - if username encryption is enabled for the group
        /// false and null - otherwise
        /// </returns>
        (bool, EncryptionKey) GetEncryptionKey(int groupId);
    }
}
