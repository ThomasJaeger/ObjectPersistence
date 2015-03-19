using System;

namespace DomainModel
{
    public class Account: DomainObject
    {
        public string AccountNumber { get; set; }
        public AccountOwner AccountOwner { get; set; }

        public Account()
        {
        }

        public Account(string name) : base(name)
        {
        }

        public static Account NewInstance(AccountOwner accountOwner)
        {
            if (accountOwner == null)
                throw new ApplicationException("accountOwner can not be null");

            Account account = new Account();
            account.AccountNumber = CreditCardNumberGenerator.GenerateAccountNumber();
            account.AccountOwner = accountOwner;
            return account;
        }

        public override string ToString()
        {
            return AccountNumber;
        }
    }
}
