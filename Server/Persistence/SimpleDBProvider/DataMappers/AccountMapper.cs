using System;
using System.Collections.Generic;
using Amazon.SimpleDB.Model;
using DomainModel;
using PersistenceService;

namespace SimpleDBProvider.DataMappers
{
    public class AccountMapper : AbstractMapper<Account>
    {
        private static readonly AccountMapper _instance = new AccountMapper();

        private AccountMapper()
        {
        }

        public static AccountMapper Instance
        {
            get { return _instance; }
        }

        protected override Account LoadRow(Item item)
        {
            // Check if the object exists in the IdentiyMap first
            if (IdentityMap.ContainsId(item.Name))
                return IdentityMap.Get(item.Name) as Account;

            DomainObject domainObject = DomainObjectMapper.Instance.GetByID<DomainObject>(item.Name);
            Account result = GetPropertiesFromParentObject(domainObject);
            result.AccountNumber = item.Attributes.Find(bk => bk.Name == "AccountNumber").Value;
            result.AccountOwner = AccountOwnerMapper.Instance.GetByID<AccountOwner>(item.Attributes.Find(bk => bk.Name == "AccountOwnerId").Value);

            IdentityMap.Update(result);
            return result;
        }

        public override bool Save(Account obj)
        {
            bool result = false;
            try
            {
                _sdb.PutAttribute<Account>(obj.Id, "AccountNumber", true, obj.AccountNumber);
                _sdb.PutAttribute<Account>(obj.Id, "AccountOwnerId", true, obj.AccountOwner.Id);
                IdentityMap.Remove(obj);
                return DomainObjectMapper.Instance.Save(obj);
            }
            catch (Exception ex)
            {
                // Log (ex.Message);
                return false;
            }
        }

        public override bool Delete(Account obj)
        {
            bool result = false;
            try
            {
                DomainObjectMapper.Instance.Delete(obj);
                string itemName = obj.Id;
                List<string> list = new List<string>();
                list.Add("AccountNumber");
                list.Add("AccountOwnerId");
                _sdb.DeleteAttributes<Account>(_domain, itemName, list);
                IdentityMap.Remove(obj);
                result = true;
            }
            catch (Exception ex)
            {
                // Log (ex.StackTrace);
            }
            return result;
        }

        public Account GetAccountByAccountNumber(string accountNumber)
        {
            return AbstractFind("select * from " + DOMAIN_PREFIX + _domain + " WHERE 'AccountNumber' = '" + accountNumber + "'");
        }

        public Account GetAccountByAccountOwnerEmail(string email)
        {
            Account result = null;
            Person person = PersonMapper.Instance.GetPersonByEmail(email);
            AccountOwner accountOwner = AccountOwnerMapper.Instance.GetByID<AccountOwner>(person.Id);
            result = GetByID<Account>(accountOwner.Account.Id);
            IdentityMap.Update(result);
            return result;
        }
    }
}