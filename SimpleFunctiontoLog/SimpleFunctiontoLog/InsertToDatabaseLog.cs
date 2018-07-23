using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Collections.Generic;
using Dapper;

namespace SimpleFunctiontoLog
{
    public static class InsertToDatabaseLog
    {
        [FunctionName("InsertToDatabaseLog")]
        public static void Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

            var cnnString = ConfigurationManager.ConnectionStrings["PP_ConnectionString"].ConnectionString;

            using (var connection = new SqlConnection(cnnString))
            {
                connection.Open();

                var rLog = "Some Log";

                // insert a log to the database
                // NOTE: Execute is an extension method from Dapper library
                connection.Execute("INSERT INTO [dbo].[TimerTriggerSqlDatabase] ([Log]) VALUES (@Log)", rLog);
                log.Info("Log added to database successfully!");
            }
        }
    }

    public class TimerTriggerSqlDatabase
    {
        public int Id { get; set; }
        public string Log { get; set; }
    }
}
