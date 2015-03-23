using System;
using System.Collections.Generic;
using Amazon.SimpleDB.Model;
using DomainModel;
using PersistenceService;

namespace SimpleDBProvider.DataMappers
{
    public class AccountOwnerMapper : AbstractMapper<AccountOwner>
    {
        private static readonly AccountOwnerMapper _instance = new AccountOwnerMapper();

        private AccountOwnerMapper()
        {
        }

        public static AccountOwnerMapper Instance
        {
            get { return _instance; }
        }

        protected override AccountOwner LoadRow(Item item)
        {
            // Check if the object exists in the IdentiyMap first
            if (IdentityMap.ContainsId(item.Name))
                return IdentityMap.Get(item.Name) as AccountOwner;

            DomainObject domainObject = DomainObjectMapper.Instance.GetByID<DomainObject>(item.Name);
            AccountOwner result = GetPropertiesFromParentObject(domainObject);
            result.Account = AccountMapper.Instance.GetByID<Account>(item.Attributes.Find(bk => bk.Name == "AccountId").Value);

            IdentityMap.Update(result);
            return result;
        }

        public override bool Save(AccountOwner obj)
        {
            bool result = false;
            try
            {
                _sdb.PutAttribute<AccountOwner>(obj.Id, "AccountId", true, obj.Account.Id);
                UserMapper.Instance.Save(obj);
                IdentityMap.Remove(obj);
                return true;
            }
            catch (Exception ex)
            {
                // Log (ex.Message);
                return false;
            }
        }

        public override bool Delete(AccountOwner obj)
        {
            bool result = false;
            try
            {
                string itemName = obj.Id;
                List<string> list = new List<string>();
                list.Add("AccountId");
                _sdb.DeleteAttributes<AddressType>(_domain, itemName, list);
                IdentityMap.Remove(obj);
                result = true;
            }
            catch (Exception ex)
            {
                // Log (ex.StackTrace);
            }
            return result;
        }
    }
}