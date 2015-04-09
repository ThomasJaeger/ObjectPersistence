using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using DomainModel;
using PersistenceService;

namespace SQLServerProvider.DataMappers
{
    public class PersonMapper : AbstractMapper<Person>
    {
        private const string SQL_TABLE_NAME = "Person";
        private const string SQL_COLUMNS = "ID, HomeAddressId, WorkAddressId, FirstName, LastName, Email";
        private const string SQL_SELECT = "SELECT " + SQL_COLUMNS + " FROM ";
        private const string SQL_INSERT = "INSERT INTO " + SQL_TABLE_NAME + " (" + SQL_COLUMNS + ") VALUES ('";
        private const string SQL_UPDATE = "UPDATE " + SQL_TABLE_NAME + " SET";
        private static readonly PersonMapper _instance = new PersonMapper();

        private PersonMapper()
        {
        }

        public static PersonMapper Instance
        {
            get { return _instance; }
        }

        protected override Person LoadRow(DataRow dataRow)
        {
            DomainObject domainObject = DomainObjectMapper.Instance.GetByID(dataRow[0].ToString());
            Person result = GetPropertiesFromParentObject(domainObject);
            result.HomeAddress = Address.NewInstance();
            result.WorkAddress = Address.NewInstance();

            if (dataRow[1] != DBNull.Value)
                result.HomeAddress = AddressMapper.Instance.GetByID(dataRow[1].ToString());
            if (dataRow[2] != DBNull.Value)
                result.WorkAddress = AddressMapper.Instance.GetByID(dataRow[2].ToString());
            if (dataRow[3] != DBNull.Value)
                result.FirstName = dataRow[3].ToString();
            if (dataRow[4] != DBNull.Value)
                result.LastName = dataRow[4].ToString();
            if (dataRow[5] != DBNull.Value)
                result.Email = dataRow[5].ToString();

            IdentityMap.Update(result);
            return result;
        }

        public override bool Save(Person obj)
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
                sb.Append(" HomeAddressId = '");
                sb.Append(obj.HomeAddress.Id);
                sb.Append("', ");
                sb.Append(" WorkAddressId = '");
                sb.Append(obj.WorkAddress.Id);
                sb.Append("', ");
                sb.Append(" FirstName = '");
                sb.Append(obj.FirstName);
                sb.Append("', ");
                sb.Append(" LastName = '");
                sb.Append(obj.LastName);
                sb.Append("', ");
                sb.Append(" Email = '");
                sb.Append(obj.Email);
                sb.Append("'");
                sb.Append(" WHERE ID = '" + obj.Id + "'");
            }
            else
            {
                sb.Append(SQL_INSERT);
                sb.Append(obj.Id);
                sb.Append("', '");
                sb.Append(obj.HomeAddress.Id);
                sb.Append("', '");
                sb.Append(obj.WorkAddress.Id);
                sb.Append("', '");
                sb.Append(obj.FirstName);
                sb.Append("', '");
                sb.Append(obj.LastName);
                sb.Append("', '");
                sb.Append(obj.Email);
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
                    AddressMapper.Instance.Save(obj.HomeAddress);
                    AddressMapper.Instance.Save(obj.WorkAddress);
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

        public override bool Delete(Person obj)
        {
            IdentityMap.Remove(obj);
            return DeleteRow("DELETE FROM " + SQL_TABLE_NAME + " WHERE ID = '" + obj.Id + "'");
        }

        public override Person GetByID(string id)
        {
            return AbstractFind(id, SQL_SELECT + SQL_TABLE_NAME + " WHERE ID = '" + id + "'");
        }

        protected override string SelectAllSql
        {
            get { return SQL_SELECT + SQL_TABLE_NAME; }
        }

        public Person GetPersonByEmail(string email)
        {
            Person result = null;

            string sql = SQL_SELECT + SQL_TABLE_NAME + " WHERE EMAIL = '" + email + "'";

            if (Connection.State != ConnectionState.Open)
                Connection.Open();

            try
            {
                var sqlCommand = new SqlCommand(sql, Connection);
                sqlCommand.CommandType = CommandType.Text;
                var dataAdapter = new SqlDataAdapter();
                dataAdapter.SelectCommand = sqlCommand;
                var dataSet = new DataSet();
                dataAdapter.Fill(dataSet);
                if (dataSet.Tables[0].Rows.Count > 0)
                {
                    string id = dataSet.Tables[0].Rows[0][0].ToString();
                    result = Load(id, dataSet);
                }
            }
            catch (Exception ex)
            {
                // Log (ex.StackTrace);
                return result;
            }
            finally
            {
                Connection.Close();
            }
            return result;
        }
    }
}