using System;
using System.Collections.Generic;
using Amazon.SimpleDB.Model;
using DomainModel;
using PersistenceService;

namespace SimpleDBProvider.DataMappers
{
    public class AddressTypeMapper : AbstractMapper<AddressType>
    {
        private static readonly AddressTypeMapper _instance = new AddressTypeMapper();

        private AddressTypeMapper()
        {
        }

        public static AddressTypeMapper Instance
        {
            get { return _instance; }
        }

        protected override AddressType LoadRow(Item item)
        {
            // Check if the object exists in the IdentiyMap first
            if (IdentityMap.ContainsId(item.Name))
                return IdentityMap.Get(item.Name) as AddressType;

            DomainObject domainObject = DomainObjectMapper.Instance.GetByID<DomainObject>(item.Name);
            AddressType result = GetPropertiesFromParentObject(domainObject);
            result.Value = Convert.ToInt32(item.Attributes.Find(bk => bk.Name == "Value").Value);
            result.DisplayName = item.Attributes.Find(bk => bk.Name == "DisplayName").Value;

            IdentityMap.Update(result);
            return result;
        }

        public override bool Save(AddressType obj)
        {
            bool result = false;
            try
            {
                _sdb.PutAttribute<AddressType>(obj.Id, "Value", true, obj.Value.ToString());
                _sdb.PutAttribute<AddressType>(obj.Id, "DisplayName", true, obj.DisplayName);
                IdentityMap.Remove(obj);
                return DomainObjectMapper.Instance.Save(obj);
            }
            catch (Exception ex)
            {
                // Log (ex.Message);
                return false;
            }
        }

        public override bool Delete(AddressType obj)
        {
            bool result = false;
            try
            {
                DomainObjectMapper.Instance.Delete(obj);
                string itemName = obj.Id;
                List<string> list = new List<string>();
                list.Add("Value");
                list.Add("DisplayName");
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