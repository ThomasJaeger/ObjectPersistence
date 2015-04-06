using System;
using System.Data;
using DomainModel;
using PersistenceService;

namespace SQLServerProvider.DataMappers
{
    public class ApplicationPermissionMapper : AbstractMapper<ApplicationPermission>
    {
        private const string SQL_TABLE_NAME = "Accounts";
        private const string SQL_COLUMNS = "ID";
        private const string SQL_SELECT = "SELECT " + SQL_COLUMNS + " FROM ";
        private const string SQL_INSERT = "INSERT INTO " + SQL_TABLE_NAME + " (" + SQL_COLUMNS + ") VALUES ('";
        private const string SQL_UPDATE = "UPDATE " + SQL_TABLE_NAME + " SET";

        private static readonly ApplicationPermissionMapper _instance = new ApplicationPermissionMapper();

        private ApplicationPermissionMapper()
        {
        }

        public static ApplicationPermissionMapper Instance
        {
            get { return _instance; }
        }

        protected override ApplicationPermission LoadRow(DataRow dataRow)
        {
            var domainObject = DomainObjectMapper.Instance.GetByID(dataRow[0].ToString());
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

        public override ApplicationPermission GetByID(string id)
        {
            return AbstractFind(id, SQL_SELECT + SQL_TABLE_NAME + " WHERE ID = '" + id + "'");
        }

        protected override string SelectAllSql
        {
            get { return SQL_SELECT + SQL_TABLE_NAME; }
        }
    }
}