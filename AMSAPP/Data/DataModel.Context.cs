
namespace AMSAPP
{
    using AMSAPP.models;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Data.SqlServerCe;
    using System.Linq;
    
    public partial class DataContext 
    {

        private string ConnectionString { get; set; }

        public DataContext ()
        {
            ConnectionString = ConfigurationManager.ConnectionStrings["DataContext"].ToString();
        }

        public virtual IEnumerable<DayLogInfo> DayLogInfos
        {
            get
            {

                SqlCeConnection conn = null;
                List<DayLogInfo> list;
                using (conn = new SqlCeConnection(ConnectionString))
                {
                    conn.Open();

                    // Create a String to hold the query.
                    string query = "SELECT * FROM DayLogInfo";

                    // Create a SqlCommand object and pass the constructor the connection string and the query string.
                    SqlCeCommand queryCommand = new SqlCeCommand(query, conn);

                    // Use the above SqlCommand object to create a SqlDataReader object.
                    SqlCeDataReader queryCommandReader = queryCommand.ExecuteReader();

                    list = (new List<DayLogInfo>()).FromDataReader<DayLogInfo>(queryCommandReader).ToList();
                    // Close the connection
                    conn.Close();
                }
                return list;

            }
        }

        public virtual IEnumerable<ComputerEvent> ComputerEvents { get {
                
            SqlCeConnection conn = null;
                List<ComputerEvent> list;
                using (conn = new SqlCeConnection(ConnectionString))
                {
                    conn.Open();

                    // Create a String to hold the query.
                    string query = "SELECT * FROM ComputerEvents";

                    // Create a SqlCommand object and pass the constructor the connection string and the query string.
                    SqlCeCommand queryCommand = new SqlCeCommand(query, conn);

                    // Use the above SqlCommand object to create a SqlDataReader object.
                    SqlCeDataReader queryCommandReader = queryCommand.ExecuteReader();

                    list = (new List<ComputerEvent>()).FromDataReader<ComputerEvent>(queryCommandReader).ToList();
                    // Close the connection
                    conn.Close();
                }
                return list;

            }
        }
        public virtual IEnumerable<AMSEvent> AMSEvents
        {
            get
            {

                SqlCeConnection conn = null;
                List<AMSEvent> list;
                using (conn = new SqlCeConnection(ConnectionString))
                {
                    conn.Open();

                    // Create a String to hold the query.
                    string query = "SELECT * FROM AMSEvents";

                    // Create a SqlCommand object and pass the constructor the connection string and the query string.
                    SqlCeCommand queryCommand = new SqlCeCommand(query, conn);

                    // Use the above SqlCommand object to create a SqlDataReader object.
                    SqlCeDataReader queryCommandReader = queryCommand.ExecuteReader();

                    list = (new List<AMSEvent>()).FromDataReader<AMSEvent>(queryCommandReader).ToList();
                    // Close the connection
                    conn.Close();
                }
                return list;

            }
        }

        public  DayLogInfo GetDayLogForDay(DateTime date)
        {
            SqlCeConnection conn = null;
            List<DayLogInfo> list;
            using (conn = new SqlCeConnection(ConnectionString))
            {
                conn.Open();

                // Create a String to hold the query.
                string query = "SELECT * FROM DayLogInfo where [Date] = '"+ date.ToString("yyyy-MM-dd") +"'";

                // Create a SqlCommand object and pass the constructor the connection string and the query string.
                SqlCeCommand queryCommand = new SqlCeCommand(query, conn);

                // Use the above SqlCommand object to create a SqlDataReader object.
                SqlCeDataReader queryCommandReader = queryCommand.ExecuteReader();

                list = (new List<DayLogInfo>()).FromDataReader<DayLogInfo>(queryCommandReader).ToList();
                // Close the connection
                conn.Close();

                return list.FirstOrDefault();
            }
           

        }

        public void AddDayLog(DayLogInfo log)
        {
            var info = this.GetDayLogForDay(log.Date);

            if (info == null)
            {
                SqlCeConnection conn = null;
                using (conn = new SqlCeConnection(ConnectionString))
                {
                    conn.Open();

                    SqlCeCommand cmd = conn.CreateCommand();
                    cmd.CommandText = "INSERT INTO DayLogInfo ([Leave],[Comments],[Date],Buffer) Values('"
                                        + log.Leave + "','" + log.Comments + "','" + log.Date.ToString("yyyy-MM-dd HH:mm:ss") + "','" + log.Buffer + "')";

                    cmd.ExecuteNonQuery();

                    // Close the connection
                    conn.Close();
                }
            }else
            {
                log.Id = info.Id;
                UpdateDayLog(log);
            }

        }

        private void UpdateDayLog(DayLogInfo log)
        {
            SqlCeConnection conn = null;
            using (conn = new SqlCeConnection(ConnectionString))
            {
                conn.Open();

                SqlCeCommand cmd = conn.CreateCommand();
                var query = "update DayLogInfo set [Leave] = '{0}' , [Comments] = '{1}', Buffer = '{2}' where Id = {3}  ";

                cmd.CommandText = string.Format(query,log.Leave,log.Comments,log.Buffer, log.Id);

                cmd.ExecuteNonQuery();

                // Close the connection
                conn.Close();
            }

        }
     

        public void AddComputerEvent(ComputerEvent event1)
        {
            SqlCeConnection conn = null;
            using (conn = new SqlCeConnection(ConnectionString))
            {
                conn.Open();

                SqlCeCommand cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO ComputerEvents ([EventType],[EventOn]) Values('"
                                    + event1.EventType + "','" + event1.EventOn.ToString("yyyy-MM-dd HH:mm:ss") + "')";

                cmd.ExecuteNonQuery();

                // Close the connection
                conn.Close();
            }
    
        }

         public void AddAMSEvent(AMSEvent event1)
        {
            SqlCeConnection conn = null;
            using (conn = new SqlCeConnection(ConnectionString))
            {
                conn.Open();

                SqlCeCommand cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO AMSEvents ([EventType],[EventOn] ,[Description],[ElapsedTimeTxt],[IsActual]) Values('"
                                    + event1.EventType + "','" + event1.EventOn.ToString("yyyy-MM-dd HH:mm:ss") + "','"
                                    + event1.Description + "','" + event1.ElapsedTimeTxt + "'," + (event1.IsActual ? "1" : "0") + ")";

                cmd.ExecuteNonQuery();

                // Close the connection
                conn.Close();
            }
    
        }

        public void ClearEventLogs()
         {
             SqlCeConnection conn = null;
             using (conn = new SqlCeConnection(ConnectionString))
             {
                 conn.Open();

                 SqlCeCommand cmd = conn.CreateCommand();
                 cmd.CommandText = "Delete  from AMSEvents where EventOn < '"+ DateTime.Now.ToString("yyyy-MM-dd") + "'" ;

                 cmd.ExecuteNonQuery();

                 cmd.CommandText = "Delete  from ComputerEvents where EventOn < '" + DateTime.Now.ToString("yyyy-MM-dd") + "'";

                 cmd.ExecuteNonQuery();

                 // Close the connection
                 conn.Close();
             }
         }
    }

    public class Reflection
    {
        public void FillObjectWithProperty(ref object objectTo, string propertyName, object propertyValue)
        {
            Type tOb2 = objectTo.GetType();
            tOb2.GetProperty(propertyName).SetValue(objectTo, propertyValue,null);
        }
    }

    public static class IENumerableExtensions
    {
        public static IEnumerable<T> FromDataReader<T>(this IEnumerable<T> list, DbDataReader dr)
        {
            //Instance reflec object from Reflection class coded above
            Reflection reflec = new Reflection();
            //Declare one "instance" object of Object type and an object list
            Object instance;
            List<Object> lstObj = new List<Object>();

            //dataReader loop
            while (dr.Read())
            {
                //Create an instance of the object needed.
                //The instance is created by obtaining the object type T of the object
                //list, which is the object that calls the extension method
                //Type T is inferred and is instantiated
                instance = Activator.CreateInstance(list.GetType().GetGenericArguments()[0]);

                // Loop all the fields of each row of dataReader, and through the object
                // reflector (first step method) fill the object instance with the datareader values
                foreach (DataRow drow in dr.GetSchemaTable().Rows)
                {
                    reflec.FillObjectWithProperty(ref instance,
                            drow.ItemArray[0].ToString(), dr[drow.ItemArray[0].ToString()]);
                }

                //Add object instance to list
                lstObj.Add(instance);
            }

            List<T> lstResult = new List<T>();
            foreach (Object item in lstObj)
            {
                lstResult.Add((T)Convert.ChangeType(item, typeof(T)));
            }

            return lstResult;
        }
    }

}


