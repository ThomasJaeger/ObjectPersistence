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
    public class DomainObjectMapper : AbstractMapper<DomainObject>
    {
        private const string SQL_TABLE_NAME = "DomainObjects";
        private const string SQL_COLUMNS = "ID, ACTIVE, CREATED, NAME, VERSION";
        private const string SQL_SELECT = "SELECT " + SQL_COLUMNS + " FROM ";
        private const string SQL_INSERT = "INSERT INTO " + SQL_TABLE_NAME + " (" + SQL_COLUMNS + ") VALUES ('";
        private const string SQL_UPDATE = "UPDATE " + SQL_TABLE_NAME + " SET";
        private static readonly DomainObjectMapper _instance = new DomainObjectMapper();

        private DomainObjectMapper()
        {
        }

        public static DomainObjectMapper Instance
        {
            get { return _instance; }
        }

        protected override DomainObject LoadRow(DataRow dataRow)
        {
            var result = new DomainObject();
            result.Id = dataRow[0].ToString();
            if (dataRow[1] != DBNull.Value)
                result.Active = Convert.ToBoolean(dataRow[1]);
            if (dataRow[2] != DBNull.Value)
                result.Created = Convert.ToDateTime(dataRow[2]);
            if (dataRow[3] != DBNull.Value)
                result.Name = dataRow[3].ToString();
            if (dataRow[4] != DBNull.Value)
                result.Version = dataRow[4].ToString();
            IdentityMap.Update(result);
            return result;
        }

        public override bool Save(DomainObject obj)
        {
            IdentityMap.Remove(obj);

            bool result = false;
            StringBuilder sb = new StringBuilder();
            bool doUpdate = false;

            // *************************************
            // Decide if we need to insert or update
            // *************************************
            if (Connection.State != ConnectionState.Open)
                Connection.Open();

            SqlCommand cmd = new SqlCommand("SELECT ID FROM " + SQL_TABLE_NAME + " WHERE ID = '" + obj.Id + "'", Connection);
            object o = cmd.ExecuteScalar();
            if (o != null)
                doUpdate = true;

            if (doUpdate)
            {
                sb.Append(SQL_UPDATE);
                sb.Append(" ACTIVE = '");
                sb.Append(obj.Active);
                sb.Append("',");
                sb.Append(" CREATED = '");
                sb.Append(obj.Created);
                sb.Append("',");
                sb.Append(" NAME = '");
                sb.Append(obj.Name);
                sb.Append("',");
                sb.Append(" VERSION = '");
                sb.Append(obj.Version);
                sb.Append("'");
                sb.Append(" WHERE ID = '" + obj.Id + "'");
            }
            else
            {
                sb.Append(SQL_INSERT);
                sb.Append(obj.Id);
                sb.Append("', '");
                sb.Append(obj.Active);
                sb.Append("', '");
                sb.Append(obj.Created);
                sb.Append("', '");
                sb.Append(obj.Name);
                sb.Append("', '");
                sb.Append(obj.Version);
                sb.Append("')");
            }

            SqlTransaction trans = null;
            SqlCommand db2Command = new SqlCommand(sb.ToString(), Connection);

            try
            {
                if (Connection.State != ConnectionState.Open)
                    Connection.Open();
                trans = Connection.BeginTransaction();
                db2Command.Transaction = trans;
                int rows = db2Command.ExecuteNonQuery();
                if (rows > 0)
                    result = true;
                trans.Commit();
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

        public override bool Delete(DomainObject obj)
        {
            IdentityMap.Remove(obj);
            return DeleteRow("DELETE FROM " + SQL_TABLE_NAME + " WHERE ID = '" + obj.Id + "'");
        }

        public override DomainObject GetByID(string id)
        {
            return AbstractFind(id, SQL_SELECT + SQL_TABLE_NAME + " WHERE ID = '" + id + "'");
        }

        protected override string SelectAllSql
        {
            get { return SQL_SELECT + SQL_TABLE_NAME; }
        }

        public DomainObject GetByName(string name)
        {
            DomainObject result = null;

            // ********************************************
            // Check the cache with LINQ to get to the name
            // ********************************************
            var found = IdentityMap.Values().FirstOrDefault(o => o.Name == name);
            if (found != null)
                return found;

            // ****************************************************************
            // Object can't be found in the identity map, look for it in the db
            // ****************************************************************
            string sql = SQL_SELECT + SQL_TABLE_NAME + " WHERE NAME = '" + name + "'";

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