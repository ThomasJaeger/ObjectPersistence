using System;
using UtilityFunctions;

namespace DomainModel
{
    public class AccountOwner: User
    {
        public Account Account { get; set; }

        public AccountOwner()
        {
        }

        public AccountOwner(string name) : base(name)
        {
        }

        public static AccountOwner NewInstance(string email, string password)
        {
            if ((string.IsNullOrEmpty(email)) || (string.IsNullOrEmpty(password)))
                throw new ApplicationException("email and/or password must not be null");

            if (!Utility.ValidateEmail(email))
                throw new ApplicationException("email address is invalid");

            AccountOwner accountOwner = new AccountOwner();
            accountOwner.Account = Account.NewInstance(accountOwner);
            accountOwner.Email = email;
            
            // We do not want to store the actual password for security reasons.
            // We only store the hash of the password. When we compare passwords
            // at a later point, we will use the hash supplied with the hash
            // that we have stored in order to validate the user.
            accountOwner.Password = Utility.CalculateSHA1Hash(password);
            
            return accountOwner;
        }
    }
}
