using System;
using Amazon.SimpleDB.Model;
using DomainModel;
using PersistenceService;

namespace SimpleDBProvider.DataMappers
{
    public class ApplicationPermissionMapper : AbstractMapper<ApplicationPermission>
    {
        private static readonly ApplicationPermissionMapper _instance = new ApplicationPermissionMapper();

        private ApplicationPermissionMapper()
        {
        }

        public static ApplicationPermissionMapper Instance
        {
            get { return _instance; }
        }

        protected override ApplicationPermission LoadRow(Item item)
        {
            // Check if the object exists in the IdentiyMap first
            if (IdentityMap.ContainsId(item.Name))
                return IdentityMap.Get(item.Name) as ApplicationPermission;

            var domainObject = DomainObjectMapper.Instance.GetByID<DomainObject>(item.Name);
            var result = GetPropertiesFromParentObject(domainObject);

            IdentityMap.Update(result);
            return result;
        }

        public override bool Save(ApplicationPermission obj)
        {
            var result = false;
            try
            {
                IdentityMap.Remove(obj);
                return DomainObjectMapper.Instance.Save(obj);
            }
            catch (Exception ex)
            {
                // Log (ex.Message);
                return false;
            }
        }

        public override bool Delete(ApplicationPermission obj)
        {
            var result = false;
            try
            {
                DomainObjectMapper.Instance.Delete(obj);
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