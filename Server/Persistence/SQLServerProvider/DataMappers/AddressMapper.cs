using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using DomainModel;
using PersistenceService;

namespace SQLServerProvider.DataMappers
{
    public class AddressMapper : AbstractMapper<Address>
    {
        private const string SQL_TABLE_NAME = "Addresses";
        private const string SQL_COLUMNS = "ID, City, Country, Line1, Line2, Line3, State, Zip, AddressTypeId";
        private const string SQL_SELECT = "SELECT " + SQL_COLUMNS + " FROM ";
        private const string SQL_INSERT = "INSERT INTO " + SQL_TABLE_NAME + " (" + SQL_COLUMNS + ") VALUES ('";
        private const string SQL_UPDATE = "UPDATE " + SQL_TABLE_NAME + " SET";
        private static readonly AddressMapper _instance = new AddressMapper();

        private AddressMapper()
        {
        }

        public static AddressMapper Instance
        {
            get { return _instance; }
        }

        protected override Address LoadRow(DataRow dataRow)
        {
            DomainObject domainObject = DomainObjectMapper.Instance.GetByID(dataRow[0].ToString());
            Address result = GetPropertiesFromParentObject(domainObject);

            if (dataRow[1] != DBNull.Value)
                result.City = dataRow[1].ToString();
            if (dataRow[2] != DBNull.Value)
                result.Country = dataRow[2].ToString();
            if (dataRow[3] != DBNull.Value)
                result.Line1 = dataRow[3].ToString();
            if (dataRow[4] != DBNull.Value)
                result.Line2 = dataRow[4].ToString();
            if (dataRow[5] != DBNull.Value)
                result.Line3 = dataRow[5].ToString();
            if (dataRow[6] != DBNull.Value)
                result.State = dataRow[6].ToString();
            if (dataRow[7] != DBNull.Value)
                result.Zip = dataRow[7].ToString();
            if (dataRow[8] != DBNull.Value)
                result.AddressType = AddressTypeMapper.Instance.GetByID(dataRow[8].ToString());

            IdentityMap.Update(result);
            return result;
        }

        public override bool Save(Address obj)
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
                sb.Append(" City = '");
                sb.Append(obj.City);
                sb.Append("', ");
                sb.Append(" Country = '");
                sb.Append(obj.Country);
                sb.Append("', ");
                sb.Append(" Line1 = '");
                sb.Append(obj.Line1);
                sb.Append("', ");
                sb.Append(" Line2 = '");
                sb.Append(obj.Line2);
                sb.Append("', ");
                sb.Append(" Line3 = '");
                sb.Append(obj.Line3);
                sb.Append("', ");
                sb.Append(" State = '");
                sb.Append(obj.State);
                sb.Append("', ");
                sb.Append(" Zip = '");
                sb.Append(obj.Zip);
                sb.Append("', ");
                sb.Append(" AddressTypeId = '");
                sb.Append(obj.AddressType.Id);
                sb.Append("'");
                sb.Append(" WHERE ID = '" + obj.Id + "'");
            }
            else
            {
                sb.Append(SQL_INSERT);
                sb.Append(obj.Id);
                sb.Append("', '");
                sb.Append(obj.City);
                sb.Append("', '");
                sb.Append(obj.Country);
                sb.Append("', '");
                sb.Append(obj.Line1);
                sb.Append("', '");
                sb.Append(obj.Line2);
                sb.Append("', '");
                sb.Append(obj.Line3);
                sb.Append("', '");
                sb.Append(obj.State);
                sb.Append("', '");
                sb.Append(obj.Zip);
                sb.Append("', '");
                sb.Append(obj.AddressType.Id);
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

        public override bool Delete(Address obj)
        {
            IdentityMap.Remove(obj);
            return DeleteRow("DELETE FROM " + SQL_TABLE_NAME + " WHERE ID = '" + obj.Id + "'");
        }

        public override Address GetByID(string id)
        {
            Address result = AbstractFind(id, SQL_SELECT + SQL_TABLE_NAME + " WHERE ID = '" + id + "'") ??
                             Address.NewInstance();
            return result;
        }

        protected override string SelectAllSql
        {
            get { return SQL_SELECT + SQL_TABLE_NAME; }
        }
    }
}