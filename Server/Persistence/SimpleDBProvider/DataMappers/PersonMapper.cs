using System;
using System.Collections.Generic;
using Amazon.SimpleDB.Model;
using DomainModel;
using PersistenceService;

namespace SimpleDBProvider.DataMappers
{
    public class PersonMapper : AbstractMapper<Person>
    {
        private static readonly PersonMapper _instance = new PersonMapper();

        private PersonMapper()
        {
        }

        public static PersonMapper Instance
        {
            get { return _instance; }
        }

        protected override Person LoadRow(Item item)
        {
            // Check if the object exists in the IdentiyMap first
            if (IdentityMap.ContainsId(item.Name))
                return IdentityMap.Get(item.Name) as Person;

            DomainObject domainObject = DomainObjectMapper.Instance.GetByID<DomainObject>(item.Name);
            Person result = GetPropertiesFromParentObject(domainObject);
            result.HomeAddress = AddressMapper.Instance.GetByID<Address>(item.Attributes.Find(bk => bk.Name == "HomeAddressId").Value);
            result.WorkAddress = AddressMapper.Instance.GetByID<Address>(item.Attributes.Find(bk => bk.Name == "WorkAddressId").Value);
            result.FirstName = item.Attributes.Find(bk => bk.Name == "FirstName").Value;
            result.LastName = item.Attributes.Find(bk => bk.Name == "LastName").Value;
            result.Email = item.Attributes.Find(bk => bk.Name == "Email").Value;

            IdentityMap.Update(result);
            return result;
        }

        public override bool Save(Person obj)
        {
            bool result = false;
            try
            {
                _sdb.PutAttribute<Person>(obj.Id, "HomeAddressId", true, obj.HomeAddress.Id);
                _sdb.PutAttribute<Person>(obj.Id, "WorkAddressId", true, obj.WorkAddress.Id);
                _sdb.PutAttribute<Person>(obj.Id, "FirstName", true, obj.FirstName);
                _sdb.PutAttribute<Person>(obj.Id, "LastName", true, obj.LastName);
                _sdb.PutAttribute<Person>(obj.Id, "Email", true, obj.Email);
                IdentityMap.Remove(obj);
                return DomainObjectMapper.Instance.Save(obj);
            }
            catch (Exception ex)
            {
                // Log (ex.Message);
                return false;
            }
        }

        public override bool Delete(Person obj)
        {
            bool result = false;
            try
            {
                DomainObjectMapper.Instance.Delete(obj);
                AddressMapper.Instance.Delete(obj.HomeAddress);
                AddressMapper.Instance.Delete(obj.WorkAddress);
                string itemName = obj.Id;
                List<string> list = new List<string>();
                list.Add("HomeAddressId");
                list.Add("WorkAddressId");
                list.Add("FirstName");
                list.Add("LastName");
                list.Add("Email");
                _sdb.DeleteAttributes<Person>(_domain, itemName, list);
                IdentityMap.Remove(obj);
                result = true;
            }
            catch (Exception ex)
            {
                // Log (ex.StackTrace);
            }
            return result;
        }

        public Person GetPersonByEmail(string email)
        {
            Person result = AbstractFind("select * from " + DOMAIN_PREFIX + _domain + " WHERE 'Email' = '" + email + "'");
            IdentityMap.Update(result);
            return result;
        }
    }
}