using System.Collections.Generic;

namespace ApiObjects
{
    public class PasswordHistory
    {
        public List<Password> History { get; set; } = new List<Password>();

        public PasswordHistory() { }

        public PasswordHistory(List<Password> history)
        {
            History = history;
        }

        public void Add(Password password, int maxCount)
        {
            History.Add(password);

            var currentCount = History.Count;
            if (currentCount > maxCount)
            {
                History.RemoveRange(0, currentCount - maxCount);
            }
        }
    }

    public class Password
    {
        public string HashedPassword { get; set; }
        public string Salt { get; set; }

        public Password(string hashedPassword, string salt)        
        {
            HashedPassword = hashedPassword;
            Salt = salt;
        }
    }
}