using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using DomainModel;
using PersistenceService;

namespace SQLServerProvider.DataMappers
{
    public class AccountMapper : AbstractMapper<Account>
    {
        private const string SQL_TABLE_NAME = "Accounts";
        private const string SQL_COLUMNS = "ID, AccountNumber, AccountOwnerId";
        private const string SQL_SELECT = "SELECT " + SQL_COLUMNS + " FROM ";
        private const string SQL_INSERT = "INSERT INTO " + SQL_TABLE_NAME + " (" + SQL_COLUMNS + ") VALUES ('";
        private const string SQL_UPDATE = "UPDATE " + SQL_TABLE_NAME + " SET";
        private static readonly AccountMapper _instance = new AccountMapper();

        private AccountMapper()
        {
        }

        public static AccountMapper Instance
        {
            get { return _instance; }
        }

        protected override Account LoadRow(DataRow dataRow)
        {
            var domainObject = DomainObjectMapper.Instance.GetByID(dataRow[0].ToString());
            var result = GetPropertiesFromParentObject(domainObject);

            if (dataRow[1] != DBNull.Value)
                result.AccountNumber = dataRow[1].ToString();
            if (dataRow[2] != DBNull.Value)
                result.AccountOwner = AccountOwnerMapper.Instance.GetByID(dataRow[2].ToString());
            IdentityMap.Update(result);
            return result;
        }

        public override bool Save(Account obj)
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
                sb.Append(" AccountNumber = '");
                sb.Append(obj.AccountNumber);
                sb.Append("',");
                sb.Append(" AccountOwnerId = '");
                sb.Append(obj.AccountOwner.Id);
                sb.Append("'");
                sb.Append(" WHERE ID = '" + obj.Id + "'");
            }
            else
            {
                sb.Append(SQL_INSERT);
                sb.Append(obj.Id);
                sb.Append("', '");
                sb.Append(obj.AccountNumber);
                sb.Append("', '");
                sb.Append(obj.AccountOwner.Id);
                sb.Append("')");
            }

            SqlTransaction trans = null;
            var db2Command = new SqlCommand(sb.ToString(), Connection);

            try
            {
                if (Connection.State != ConnectionState.Open)
                    Connection.Open();
                trans = Connection.BeginTransaction();
                db2Command.Transaction = trans;
                var rows = db2Command.ExecuteNonQuery();
                if (rows > 0)
                {
                    result = DomainObjectMapper.Instance.Save(obj); 
                }
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

        public override bool Delete(Account obj)
        {
            IdentityMap.Remove(obj);
            return DeleteRow("DELETE FROM " + SQL_TABLE_NAME + " WHERE ID = '" + obj.Id + "'");
        }

        public override Account GetByID(string id)
        {
            return AbstractFind(id, SQL_SELECT + SQL_TABLE_NAME + " WHERE ID = '" + id + "'");
        }

        protected override string SelectAllSql
        {
            get { return SQL_SELECT + SQL_TABLE_NAME; }
        }

        public Account GetAccountByAccountNumber(string accountNumber)
        {
            return AbstractFind("",
                "select * from " + SQL_TABLE_NAME + " WHERE 'AccountNumber' = '" + accountNumber + "'");
        }

        public Account GetAccountByAccountOwnerEmail(string email)
        {
            Account result = null;
            var person = PersonMapper.Instance.GetPersonByEmail(email);
            var accountOwner = AccountOwnerMapper.Instance.GetByID(person.Id);
            result = GetByID(accountOwner.Account.Id);
            IdentityMap.Update(result);
            return result;
        }
    }
}