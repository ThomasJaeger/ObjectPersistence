using System;
using System.Collections.Generic;
using Amazon.SimpleDB.Model;
using DomainModel;
using PersistenceService;
using Attribute = Amazon.SimpleDB.Model.Attribute;

namespace SimpleDBProvider.DataMappers
{
    public class ApplicationRoleMapper : AbstractMapper<ApplicationRole>
    {
        private static readonly ApplicationRoleMapper _instance = new ApplicationRoleMapper();

        private ApplicationRoleMapper()
        {
        }

        public static ApplicationRoleMapper Instance
        {
            get { return _instance; }
        }

        protected override ApplicationRole LoadRow(Item item)
        {
            // Check if the object exists in the IdentiyMap first
            if (IdentityMap.ContainsId(item.Name))
                return IdentityMap.Get(item.Name) as ApplicationRole;

            var domainObject = DomainObjectMapper.Instance.GetByID<DomainObject>(item.Name);
            var result = GetPropertiesFromParentObject(domainObject);
            result.Description = item.Attributes.Find(bk => bk.Name == "Description").Value;

            List<Attribute> list = item.Attributes.FindAll(bk => bk.Name == "ApplicationPermissionId");
            foreach (Attribute attribute in list)
            {
                result.ApplicationPermissions.Add(ApplicationPermissionMapper.Instance.GetByID<ApplicationPermission>(attribute.Value));
            }

            IdentityMap.Update(result);
            return result;
        }

        public override bool Save(ApplicationRole obj)
        {
            var result = false;
            try
            {
                _sdb.PutAttribute<ApplicationRole>(obj.Id, "Description", true, obj.Description);
                foreach (var applicationPermission in obj.ApplicationPermissions)
                {
                    _sdb.PutAttribute<User>(obj.Id, "ApplicationPermissionId", false, applicationPermission.Id);
                }
                ApplicationPermissionMapper.Instance.SaveAll(obj.ApplicationPermissions);
                IdentityMap.Remove(obj);
                return DomainObjectMapper.Instance.Save(obj);
            }
            catch (Exception ex)
            {
                // Log (ex.Message);
                return false;
            }
        }

        public override bool Delete(ApplicationRole obj)
        {
            var result = false;
            try
            {
                DomainObjectMapper.Instance.Delete(obj);
                var itemName = obj.Id;
                var list = new List<string>();
                list.Add("Description");
                list.Add("ApplicationPermissionId");
                _sdb.DeleteAttributes<ApplicationRole>(_domain, itemName, list);
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