using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using DomainModel;
using PersistenceService;

namespace SQLServerProvider.DataMappers
{
    public class AddressTypeMapper : AbstractMapper<AddressType>
    {
        private const string SQL_TABLE_NAME = "AddressTypes";
        private const string SQL_COLUMNS = "ID, Value, DisplayName";
        private const string SQL_SELECT = "SELECT " + SQL_COLUMNS + " FROM ";
        private const string SQL_INSERT = "INSERT INTO " + SQL_TABLE_NAME + " (" + SQL_COLUMNS + ") VALUES ('";
        private const string SQL_UPDATE = "UPDATE " + SQL_TABLE_NAME + " SET";
        private static readonly AddressTypeMapper _instance = new AddressTypeMapper();

        private AddressTypeMapper()
        {
        }

        public static AddressTypeMapper Instance
        {
            get { return _instance; }
        }

        protected override AddressType LoadRow(DataRow dataRow)
        {
            DomainObject domainObject = DomainObjectMapper.Instance.GetByID(dataRow[0].ToString());
            AddressType result = GetPropertiesFromParentObject(domainObject);

            if (dataRow[1] != DBNull.Value)
                result.Value = Convert.ToInt32(dataRow[1].ToString());
            if (dataRow[2] != DBNull.Value)
                result.DisplayName = dataRow[2].ToString();

            IdentityMap.Update(result);
            return result;
        }

        public override bool Save(AddressType obj)
        {
            IdentityMap.Remove(obj);

            var result = false;
            var sb = new StringBuilder();
            var doUpdate = false;

            // *************************************
            // Decide if we need to insert or update
            // *************************************
            if (Connection.State != ConnectionState.Open)
                Connection.Open();

            var cmd = new SqlCommand("SELECT ID FROM " + SQL_TABLE_NAME + " WHERE ID = '" + obj.Id + "'", Connection);
            var o = cmd.ExecuteScalar();
            if (o != null)
                doUpdate = true;

            if (doUpdate)
            {
                sb.Append(SQL_UPDATE);
                sb.Append(" Value = ");
                sb.Append(obj.Value);
                sb.Append(", '");
                sb.Append(" DisplayName = '");
                sb.Append(obj.DisplayName);
                sb.Append("'");
                sb.Append(" WHERE ID = '" + obj.Id + "'");
            }
            else
            {
                sb.Append(SQL_INSERT);
                sb.Append(obj.Id);
                sb.Append("', ");
                sb.Append(obj.Value);
                sb.Append(", '");
                sb.Append(obj.DisplayName);
                sb.Append("')");
            }

            SqlTransaction trans = null;
            cmd = new SqlCommand(sb.ToString(), Connection);

            try
            {
                if (Connection.State != ConnectionState.Open)
                    Connection.Open();
                trans = Connection.BeginTransaction();
                cmd.Transaction = trans;
                var rows = cmd.ExecuteNonQuery();
                if (rows > 0)
                {
                    DomainObjectMapper.Instance.Save(obj);
                    result = true;
                }
                if (result)
                    trans.Commit();
                else
                    trans.Rollback();
            }
            catch
            {
                trans.Rollback();
                result = false;
            }
            finally
            {
                Connection.Close();
            }
            return result;
        }

        public override bool Delete(AddressType obj)
        {
            IdentityMap.Remove(obj);
            return DeleteRow("DELETE FROM " + SQL_TABLE_NAME + " WHERE ID = '" + obj.Id + "'");
        }

        public override AddressType GetByID(string id)
        {
            return AbstractFind(id, SQL_SELECT + SQL_TABLE_NAME + " WHERE ID = '" + id + "'");
        }

        protected override string SelectAllSql
        {
            get { return SQL_SELECT + SQL_TABLE_NAME; }
        }
    }
}