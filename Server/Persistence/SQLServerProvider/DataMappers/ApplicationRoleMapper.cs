using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using DomainModel;
using PersistenceService;

namespace SQLServerProvider.DataMappers
{
    public class ApplicationRoleMapper : AbstractMapper<ApplicationRole>
    {
        private const string SQL_TABLE_NAME = "ApplicationRoles";
        private const string SQL_COLUMNS = "ID, Description";
        private const string SQL_SELECT = "SELECT " + SQL_COLUMNS + " FROM ";
        private const string SQL_INSERT = "INSERT INTO " + SQL_TABLE_NAME + " (" + SQL_COLUMNS + ") VALUES ('";
        private const string SQL_UPDATE = "UPDATE " + SQL_TABLE_NAME + " SET";
        private static readonly ApplicationRoleMapper _instance = new ApplicationRoleMapper();

        private ApplicationRoleMapper()
        {
        }

        public static ApplicationRoleMapper Instance
        {
            get { return _instance; }
        }

        protected override ApplicationRole LoadRow(DataRow dataRow)
        {
            var domainObject = DomainObjectMapper.Instance.GetByID(dataRow[0].ToString());
            var result = GetPropertiesFromParentObject(domainObject);

            if (dataRow[1] != DBNull.Value)
                result.Description = dataRow[1].ToString();
            LoadApplicationPermissions(result);
            IdentityMap.Update(result);
            return result;
        }

        /// <summary>
        /// Load all permissions for the application role from
        /// a third table.
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private void LoadApplicationPermissions(ApplicationRole obj)
        {
            List<ApplicationPermission> list = new List<ApplicationPermission>();

            string sql = "SELECT ApplicationPermissionId FROM ApplicationPermissionsApplicationRoles WHERE ApplicationRoleId = '" + obj.Id + "'";
            SqlCommand sqlCommand = new SqlCommand(sql, Connection);
            sqlCommand.CommandType = CommandType.Text;
            SqlDataAdapter dataAdapter = new SqlDataAdapter();
            dataAdapter.SelectCommand = sqlCommand;
            DataSet dataSet = new DataSet();
            dataAdapter.Fill(dataSet);
            foreach (DataRow dataRow in dataSet.Tables[0].Rows)
            {
                string id = dataRow[0].ToString();
                list.Add(ApplicationPermissionMapper.Instance.GetByID(id));
            }

            obj.ApplicationPermissions = list;
        }

        public override bool Save(ApplicationRole obj)
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
                sb.Append(" Description = '");
                sb.Append(obj.Description);
                sb.Append("'");
                sb.Append(" WHERE ID = '" + obj.Id + "'");
            }
            else
            {
                sb.Append(SQL_INSERT);
                sb.Append(obj.Id);
                sb.Append("', '");
                sb.Append(obj.Description);
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
                    // Save to third table
                    result = SavePermissions(obj);
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

        /// <summary>
        /// Save all permissions for the application role into
        /// a third table.
        /// </summary>
        /// <param name="applicationRole"></param>
        /// <returns></returns>
        private bool SavePermissions(ApplicationRole obj)
        {
            if (Connection.State != ConnectionState.Open)
                Connection.Open();

            bool result = true;
            string sql = "";
            SqlTransaction trans = Connection.BeginTransaction();

            if (!DeleteExistingApplicationRoleIds(obj, trans))
                return false;

            foreach (var applicationPermission in obj.ApplicationPermissions)
            {
                sql =
                    "INSERT INTO ApplicationPermissionsApplicationRoles (ApplicationPermissionId, ApplicationRoleId) VALUES ('" +
                    applicationPermission.Id + "', '" + obj.Id + "')";
                var cmd = new SqlCommand(sql, Connection);
                try
                {
                    cmd.Transaction = trans;
                    var rows = cmd.ExecuteNonQuery();
                    if (rows > 0)
                        result = DomainObjectMapper.Instance.Save(obj); 
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
            }
            return result;
        }

        /// <summary>
        /// Delete all existing ApplicationRoleIds
        /// </summary>
        private bool DeleteExistingApplicationRoleIds(ApplicationRole obj, SqlTransaction trans)
        {
            try
            {
                var cmd =
                    new SqlCommand(
                        "DELETE * FROM ApplicationPermissionsApplicationRoles WHERE ApplicationRoleId = '" +
                        obj.Id + "'", Connection);
                cmd.Transaction = trans;
                var o = cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception)
            {
                trans.Rollback();
                return false;
            }
        }

        public override bool Delete(ApplicationRole obj)
        {
            IdentityMap.Remove(obj);
            return DeleteRow("DELETE FROM " + SQL_TABLE_NAME + " WHERE ID = '" + obj.Id + "'");
        }

        public override ApplicationRole GetByID(string id)
        {
            return AbstractFind(id, SQL_SELECT + SQL_TABLE_NAME + " WHERE ID = '" + id + "'");
        }

        protected override string SelectAllSql
        {
            get { return SQL_SELECT + SQL_TABLE_NAME; }
        }

        public override List<ApplicationRole> GetAll()
        {
            List<ApplicationRole> result = base.GetAll();

            foreach (var applicationRole in result)
            {
                LoadApplicationPermissions(applicationRole);
            }

            return result;
        }
    }
}