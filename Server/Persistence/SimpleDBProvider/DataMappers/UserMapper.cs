using System;
using System.Collections.Generic;
using Amazon.SimpleDB.Model;
using DomainModel;
using PersistenceService;
using Attribute = Amazon.SimpleDB.Model.Attribute;

namespace SimpleDBProvider.DataMappers
{
    public class UserMapper : AbstractMapper<User>
    {
        private static readonly UserMapper _instance = new UserMapper();

        private UserMapper()
        {
        }

        public static UserMapper Instance
        {
            get { return _instance; }
        }

        protected override User LoadRow(Item item)
        {
            // Check if the object exists in the IdentiyMap first
            if (IdentityMap.ContainsId(item.Name))
                return IdentityMap.Get(item.Name) as User;

            Person parent = PersonMapper.Instance.GetByID<Person>(item.Name);
            User result = GetPropertiesFromParentObject(parent);
            result.Password = item.Attributes.Find(bk => bk.Name == "Password").Value;

            List<Attribute> list = item.Attributes.FindAll(bk => bk.Name == "ApplicationRoleId");
            foreach (Attribute attribute in list)
            {
                result.ApplicationRoles.Add(ApplicationRoleMapper.Instance.GetByID<ApplicationRole>(attribute.Value));
            }

            IdentityMap.Update(result);
            return result;
        }

        public override bool Save(User obj)
        {
            bool result = false;
            try
            {
                _sdb.PutAttribute<User>(obj.Id, "Password", true, obj.Password);
                foreach (var applicationRole in obj.ApplicationRoles)
                {
                    _sdb.PutAttribute<User>(obj.Id, "ApplicationRoleId", false, applicationRole.Id);
                }
                ApplicationRoleMapper.Instance.SaveAll(obj.ApplicationRoles);
                IdentityMap.Remove(obj);
                return PersonMapper.Instance.Save(obj);
            }
            catch (Exception ex)
            {
                // Log (ex.Message);
                return false;
            }
        }

        public override bool Delete(User obj)
        {
            bool result = false;
            try
            {
                PersonMapper.Instance.Delete(obj);
                string itemName = obj.Id;
                List<string> list = new List<string>();
                list.Add("Password");
                list.Add("ApplicationRoleId");
                _sdb.DeleteAttributes<User>(_domain, itemName, list);
                IdentityMap.Remove(obj);
                result = true;
            }
            catch (Exception ex)
            {
                // Log (ex.StackTrace);
            }
            return result;
        }

        public User GetUserByEmail(string email)
        {
            User result = null;
            Person person = PersonMapper.Instance.GetPersonByEmail(email);
            result = GetByID<User>(person.Id);
            IdentityMap.Update(result);
            return result;
        }
    }
}