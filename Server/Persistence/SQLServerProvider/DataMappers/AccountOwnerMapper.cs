using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using DomainModel;
using PersistenceService;

namespace SQLServerProvider.DataMappers
{
    public class AccountOwnerMapper : AbstractMapper<AccountOwner>
    {
        private const string SQL_TABLE_NAME = "AccountOwners";
        private const string SQL_COLUMNS = "ID, AccountId";
        private const string SQL_SELECT = "SELECT " + SQL_COLUMNS + " FROM ";
        private const string SQL_INSERT = "INSERT INTO " + SQL_TABLE_NAME + " (" + SQL_COLUMNS + ") VALUES ('";
        private const string SQL_UPDATE = "UPDATE " + SQL_TABLE_NAME + " SET";
        private static readonly AccountOwnerMapper _instance = new AccountOwnerMapper();

        private AccountOwnerMapper()
        {
        }

        public static AccountOwnerMapper Instance
        {
            get { return _instance; }
        }

        protected override AccountOwner LoadRow(DataRow dataRow)
        {
            DomainObject domainObject = DomainObjectMapper.Instance.GetByID(dataRow[0].ToString());
            AccountOwner result = GetPropertiesFromParentObject(domainObject);

            if (dataRow[1] != DBNull.Value)
                result.Account = AccountMapper.Instance.GetByID(dataRow[1].ToString());

            IdentityMap.Update(result);
            return result;
        }

        public override bool Save(AccountOwner obj)
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
                sb.Append(" AccountId = ");
                sb.Append(obj.Account.Id);
                sb.Append("'");
                sb.Append(" WHERE ID = '" + obj.Id + "'");
            }
            else
            {
                sb.Append(SQL_INSERT);
                sb.Append(obj.Id);
                sb.Append("', ");
                sb.Append(obj.Account.Id);
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
                    AccountMapper.Instance.Save(obj.Account);
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

        public override bool Delete(AccountOwner obj)
        {
            IdentityMap.Remove(obj);
            return DeleteRow("DELETE FROM " + SQL_TABLE_NAME + " WHERE ID = '" + obj.Id + "'");
        }

        public override AccountOwner GetByID(string id)
        {
            return AbstractFind(id, SQL_SELECT + SQL_TABLE_NAME + " WHERE ID = '" + id + "'");
        }

        protected override string SelectAllSql
        {
            get { return SQL_SELECT + SQL_TABLE_NAME; }
        }
    }
}