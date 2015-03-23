using System;
using System.Collections.Generic;
using Amazon.SimpleDB.Model;
using DomainModel;
using PersistenceService;

namespace SimpleDBProvider.DataMappers
{
    public class AddressMapper : AbstractMapper<Address>
    {
        private static readonly AddressMapper _instance = new AddressMapper();

        private AddressMapper()
        {
        }

        public static AddressMapper Instance
        {
            get { return _instance; }
        }

        protected override Address LoadRow(Item item)
        {
            // Check if the object exists in the IdentiyMap first
            if (IdentityMap.ContainsId(item.Name))
                return IdentityMap.Get(item.Name) as Address;

            DomainObject domainObject = DomainObjectMapper.Instance.GetByID<DomainObject>(item.Name);
            Address result = GetPropertiesFromParentObject(domainObject);
            result.AddressType = AddressTypeMapper.Instance.GetByID<AddressType>(item.Attributes.Find(bk => bk.Name == "AddressTypeId").Value);
            result.City = item.Attributes.Find(bk => bk.Name == "City").Value;
            result.Country = item.Attributes.Find(bk => bk.Name == "Country").Value;
            result.Line1 = item.Attributes.Find(bk => bk.Name == "Line1").Value;
            result.Line2 = item.Attributes.Find(bk => bk.Name == "Line2").Value;
            result.Line3 = item.Attributes.Find(bk => bk.Name == "Line3").Value;
            result.State = item.Attributes.Find(bk => bk.Name == "State").Value;
            result.Zip = item.Attributes.Find(bk => bk.Name == "Zip").Value;

            IdentityMap.Update(result);
            return result;
        }

        public override bool Save(Address obj)
        {
            bool result = false;
            try
            {
                _sdb.PutAttribute<Address>(obj.Id, "AddressTypeId", true, obj.AddressType.Id);
                _sdb.PutAttribute<Address>(obj.Id, "City", true, obj.City);
                _sdb.PutAttribute<Address>(obj.Id, "Country", true, obj.Country);
                _sdb.PutAttribute<Address>(obj.Id, "Line1", true, obj.Line1);
                _sdb.PutAttribute<Address>(obj.Id, "Line2", true, obj.Line2);
                _sdb.PutAttribute<Address>(obj.Id, "Line3", true, obj.Line3);
                _sdb.PutAttribute<Address>(obj.Id, "State", true, obj.State);
                _sdb.PutAttribute<Address>(obj.Id, "Zip", true, obj.Zip);
                IdentityMap.Remove(obj);
                return DomainObjectMapper.Instance.Save(obj);
            }
            catch (Exception ex)
            {
                // Log (ex.Message);
                return false;
            }
        }

        public override bool Delete(Address obj)
        {
            bool result = false;
            try
            {
                DomainObjectMapper.Instance.Delete(obj);
                string itemName = obj.Id;
                List<string> list = new List<string>();
                // Note: Since AddressType is a lookup/reference/enumartion type,
                // we do not want to delete it.
                list.Add("City");
                list.Add("Country");
                list.Add("Line1");
                list.Add("Line2");
                list.Add("Line3");
                list.Add("State");
                list.Add("Zip");
                _sdb.DeleteAttributes<Address>(_domain, itemName, list);
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