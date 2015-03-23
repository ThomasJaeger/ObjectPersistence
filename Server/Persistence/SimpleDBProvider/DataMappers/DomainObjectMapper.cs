using System;
using System.Collections.Generic;
using Amazon.SimpleDB.Model;
using DomainModel;
using PersistenceService;

namespace SimpleDBProvider.DataMappers
{
    public class DomainObjectMapper : AbstractMapper<DomainObject>
    {
        private static readonly DomainObjectMapper _instance = new DomainObjectMapper();

        private DomainObjectMapper()
        {
        }

        public static DomainObjectMapper Instance
        {
            get { return _instance; }
        }

        protected override DomainObject LoadRow(Item item)
        {
            // Check if the object exists in the IdentiyMap first
            if (IdentityMap.ContainsId(item.Name))
                return IdentityMap.Get(item.Name);

            var result = new DomainObject();
            result.Id = item.Name.Replace(_fullDomain,"");
            result.Created = Convert.ToDateTime(item.Attributes.Find(bk => bk.Name == "Created").Value);
            result.Active = bool.Parse(item.Attributes.Find(bk => bk.Name == "Active").Value);
            result.Name = item.Attributes.Find(bk => bk.Name == "Name").Value;
            result.Version = item.Attributes.Find(bk => bk.Name == "Version").Value;

            IdentityMap.Update(result);
            return result;
        }

        public override bool Save(DomainObject obj)
        {
            try
            {
                _sdb.PutAttribute<DomainObject>(obj.Id, "Active", true, obj.Active.ToString());
                _sdb.PutAttribute<DomainObject>(obj.Id, "Created", true, obj.Created.ToString("O"));
                _sdb.PutAttribute<DomainObject>(obj.Id, "Name", true, obj.Name);
                _sdb.PutAttribute<DomainObject>(obj.Id, "Version", true, DomainObject.CURRENT_DOMAIN_VERSION);
                IdentityMap.Remove(obj);
                return true;
            }
            catch (Exception ex)
            {
                // Log (ex.Message);
                return false;
            }
        }

        public override bool Delete(DomainObject obj)
        {
            var result = false;
            try
            {
                var list = new List<string>();
                list.Add("Version");
                list.Add("Created");
                list.Add("Active");
                list.Add("Name");
                _sdb.DeleteAttributes<DomainObject>(_domain, obj.Id, list);
                IdentityMap.Remove(obj);
                result = true;
            }
            catch (Exception ex)
            {
                // Log (ex.StackTrace);
            }
            return result;
        }

        public DomainObject GetByName(string domainObject)
        {
            DomainObject result = null;

            var selectRequestAction =
                new SelectRequest("select * from " + _domain + " WHERE Name = '" + domainObject + "'");
            var selectResponse = _sdb.Client.Select(selectRequestAction);
            if (selectResponse.Items.Count > 0)
            {
                var selectResult = selectResponse.SelectResult;
                foreach (var item in selectResult.Items)
                {
                    return LoadRow(item);
                }
                IdentityMap.Update(result);
            }
            return result;
        }
    }
}