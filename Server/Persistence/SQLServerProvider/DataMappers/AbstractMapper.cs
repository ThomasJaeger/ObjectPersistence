using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using DomainModel;
using PersistenceService;

namespace SQLServerProvider.DataMappers
{
    public abstract class AbstractMapper<T> where T : DomainObject, new()
    {
        // That should be put into the provider custom properties in your config file
        public static readonly string SQL_CONNECTION_STRING = 
            //@"Data Source=(LocalDB)\v11.0;AttachDbFilename=|DataDirectory|\SQLServerProvider.mdf;Integrated Security=True";
            @"Data Source=(LocalDB)\v11.0;AttachDbFilename=C:\Source\ObjectPersistence\Server\Services\RESTService\App_Data\SQLServerProvider.mdf;Integrated Security=True";

        protected SqlConnection sqlConnection;

        protected AbstractMapper()
        {
            Connection = new SqlConnection(SQL_CONNECTION_STRING);
        }

        public static SqlConnection Connection { get; set; }
        public string FindStatement { get; set; }
        protected abstract T LoadRow(DataRow dataRow);
        public abstract bool Save(T obj);
        public abstract bool Delete(T obj);
        public abstract T GetByID(string id);
        protected abstract string SelectAllSql { get; }

        protected T AbstractFind(string id, string sql)
        {
            T result = null;
            try
            {
                if (IdentityMap.ContainsId(id))
                    return IdentityMap.Get(id) as T;

                FindStatement = sql;
                if (string.IsNullOrEmpty(FindStatement))
                    throw new Exception("Find Statement is null");

                // *****************************
                // Not in Identity Map, check DB
                // *****************************
                var conn = new SqlConnection(SQL_CONNECTION_STRING);
                var db2Command = new SqlCommand(FindStatement, conn);
                db2Command.CommandType = CommandType.Text;
                var dataAdapter = new SqlDataAdapter();
                dataAdapter.SelectCommand = db2Command;
                var dataSet = new DataSet();
                dataAdapter.Fill(dataSet);
                result = Load(id, dataSet);
            }
            catch (Exception ex)
            {
                // Log (ex.StackTrace);
                return result;
            }
            return result;
        }

        private T DoLoad(string id, DataSet dataSet)
        {
            T obj = null;
            DataRow dataRow;

            try
            {
                // ********************************************
                // Check if we wanted to load more than one row 
                // for a collection later on
                // ********************************************
                if (dataSet.Tables[0].Rows.Count > 1)
                {
                    var found = from
                        DataRow p in
                        dataSet.Tables[0].Rows
                        where
                            p[0].ToString() == id
                        select p;
                    foreach (var foundRow in found)
                    {
                        dataRow = foundRow;
                        obj = LoadRow(dataRow);
                        break;
                    }
                }
                else
                {
                    if (dataSet.Tables[0].Rows.Count > 0)
                        obj = LoadRow(dataSet.Tables[0].Rows[0]);
                }
            }
            catch
            {
                return obj;
            }
            return obj;
        }

        protected T Load(string id, DataSet dataSet)
        {
            T obj = null;
            try
            {
                if (IdentityMap.ContainsId(id))
                    return IdentityMap.Get(id) as T;
                obj = DoLoad(id, dataSet);
                IdentityMap.Update(obj);
            }
            catch
            {
                return obj;
            }
            return obj;
        }

        public virtual bool SaveAll(List<T> objs)
        {
            try
            {
                foreach (var p in objs)
                {
                    if (!Save(p))
                        return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                // log (ex.StackTrace);
                return false;
            }
        }

        public virtual bool DeleteAll(List<T> objs)
        {
            foreach (var obj in objs)
            {
                Delete(obj);
            }
            return true;
        }

        public virtual List<T> GetAll()
        {
            List<T> result = new List<T>();
            SqlCommand sqlCommand = new SqlCommand(SelectAllSql, Connection);
            sqlCommand.CommandType = CommandType.Text;
            SqlDataAdapter dataAdapter = new SqlDataAdapter();
            dataAdapter.SelectCommand = sqlCommand;
            DataSet dataSet = new DataSet();
            dataAdapter.Fill(dataSet);
            foreach (DataRow dataRow in dataSet.Tables[0].Rows)
            {
                string id = dataRow[0].ToString();
                result.Add(Load(id, dataSet));
            }
            return result;
        }

        public virtual T GetPropertiesFromParentObject<U>(U parentObject)
        {
            var result = new T();
            var dtoProperties = parentObject.GetType().GetProperties();
            var boProperties = typeof (T).GetProperties();
            foreach (var dtoProperty in dtoProperties)
            {
                var value = dtoProperty.GetValue(parentObject, null);
                var boProperty = boProperties.First(p => p.Name == dtoProperty.Name);
                if (value != null)
                    boProperty.SetValue(result, value, null);
            }
            return result;
        }

        protected virtual bool DeleteRow(string sql)
        {
            if (Connection.State != ConnectionState.Open)
                Connection.Open();
            try
            {
                SqlCommand cmd = new SqlCommand(sql, Connection);
                cmd.ExecuteScalar();
                return true;
            }
            catch (Exception ex)
            {
                // log (ex.StackTrace);
                return false;
            }
            finally
            {
                Connection.Close();
            }
        }
    }
}