using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using DomainModel;
using PersistenceService;

namespace SQLServerProvider.DataMappers
{
    public class UserMapper : AbstractMapper<User>
    {
        private const string SQL_TABLE_NAME = "Users";
        private const string SQL_COLUMNS = "ID, Password";
        private const string SQL_SELECT = "SELECT " + SQL_COLUMNS + " FROM ";
        private const string SQL_INSERT = "INSERT INTO " + SQL_TABLE_NAME + " (" + SQL_COLUMNS + ") VALUES ('";
        private const string SQL_UPDATE = "UPDATE " + SQL_TABLE_NAME + " SET";
        private static readonly UserMapper _instance = new UserMapper();

        private UserMapper()
        {
        }

        public static UserMapper Instance
        {
            get { return _instance; }
        }

        protected override User LoadRow(DataRow dataRow)
        {
            Person parent = PersonMapper.Instance.GetByID(dataRow[0].ToString());
            User result = GetPropertiesFromParentObject(parent);

            if (dataRow[1] != DBNull.Value)
                result.Password = dataRow[1].ToString();
            LoadApplicationRoles(result);
            IdentityMap.Update(result);
            return result;
        }

        /// <summary>
        /// Load all permissions for the application role from
        /// a third table.
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private void LoadApplicationRoles(User obj)
        {
            List<ApplicationRole> list = new List<ApplicationRole>();

            string sql = "SELECT ApplicationRoleId FROM UserApplicationRoles WHERE UserId = '" + obj.Id + "'";
            SqlCommand sqlCommand = new SqlCommand(sql, Connection);
            sqlCommand.CommandType = CommandType.Text;
            SqlDataAdapter dataAdapter = new SqlDataAdapter();
            dataAdapter.SelectCommand = sqlCommand;
            DataSet dataSet = new DataSet();
            dataAdapter.Fill(dataSet);
            foreach (DataRow dataRow in dataSet.Tables[0].Rows)
            {
                string id = dataRow[0].ToString();
                list.Add(ApplicationRoleMapper.Instance.GetByID(id));
            }

            obj.ApplicationRoles = list;
        }

        public override bool Save(User obj)
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
                sb.Append(" Password = '");
                sb.Append(obj.Password);
                sb.Append("'");
                sb.Append(" WHERE ID = '" + obj.Id + "'");
            }
            else
            {
                sb.Append(SQL_INSERT);
                sb.Append(obj.Id);
                sb.Append("', '");
                sb.Append(obj.Password);
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
                    result = SaveApplicationRoles(obj);
                    PersonMapper.Instance.Save(obj);
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
        /// Save all application roles for the user into
        /// a third table.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool SaveApplicationRoles(User obj)
        {
            if (Connection.State != ConnectionState.Open)
                Connection.Open();

            bool result = true;
            string sql = "";
            SqlTransaction trans = Connection.BeginTransaction();

            if (!DeleteExistingUserIds(obj, trans))
                return false;

            foreach (var applicationRole in obj.ApplicationRoles)
            {
                sql =
                    "INSERT INTO UserApplicationRoles (UserId, ApplicationRoleId) VALUES ('" +
                    obj.Id + "', '" + applicationRole.Id + "')";
                var cmd = new SqlCommand(sql, Connection);
                try
                {
                    cmd.Transaction = trans;
                    var rows = cmd.ExecuteNonQuery();
                    if (rows > 0)
                        result = true;
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
        /// Delete all existing UserIds
        /// </summary>
        private bool DeleteExistingUserIds(User obj, SqlTransaction trans)
        {
            try
            {
                var cmd =
                    new SqlCommand(
                        "DELETE * FROM UserApplicationRoles WHERE UserId = '" +
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

        public override bool Delete(User obj)
        {
            IdentityMap.Remove(obj);
            return DeleteRow("DELETE FROM " + SQL_TABLE_NAME + " WHERE ID = '" + obj.Id + "'");
        }

        public override User GetByID(string id)
        {
            return AbstractFind(id, SQL_SELECT + SQL_TABLE_NAME + " WHERE ID = '" + id + "'");
        }

        protected override string SelectAllSql
        {
            get { return SQL_SELECT + SQL_TABLE_NAME; }
        }

        public User GetUserByEmail(string email)
        {
            User result = null;
            Person person = PersonMapper.Instance.GetPersonByEmail(email);
            result = GetByID(person.Id);
            IdentityMap.Update(result);
            return result;
        }

        public override List<User> GetAll()
        {
            List<User> result = base.GetAll();

            foreach (var user in result)
            {
                LoadApplicationRoles(user);
            }

            return result;
        }
    }
}