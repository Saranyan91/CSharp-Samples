using System;
using System.Configuration;
using System.Data.SqlClient;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Data;
using Stripe;
using SimpleRowAdd.Entities;
using System.Linq;
using System.Collections.Generic;

namespace SimpleRowAdd
{
    public class DbErrorHandler
    {
        static string insertCommand = @"INSERT INTO [dbo].[ImportErrors] ([APITransactionId],[Source],[TransactionJson], [ExceptionMessage]) VALUES (@APITransactionId,@Source,@TransactionJson, @ExceptionMessage)";

        public static void InsertError(ErrorRow row, TraceWriter log)
        {
            try
            {
                var cnnString = ConfigurationManager.ConnectionStrings["PP_ConnectionStringLocal"].ConnectionString;

                using (var connection = new SqlConnection(cnnString))
                {
                    connection.Open();

                    using (SqlCommand cmd = new SqlCommand(insertCommand, connection))
                    {
                        cmd.Parameters.Add("@APITransactionId", SqlDbType.VarChar, 50).Value = row.APITransactionId ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Source", SqlDbType.VarChar, 50).Value = row.Source ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@TransactionJson", SqlDbType.NVarChar).Value = row.TransactionJson ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@ExceptionMessage", SqlDbType.NVarChar).Value = row.ExceptionMessage ?? (object)DBNull.Value;

                        var resultCount = cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("InsertError error", ex);
            }
        }


        internal static void InsertError(StripeTransaction trans, Exception ex, TraceWriter log)
        {
            if (null != trans)
            {
                ErrorRow row = new ErrorRow();
                row.APITransactionId = trans.APITransactionId;
                row.Source = "Stripe";
                if (null != ex && null != ex.Message)
                {
                    row.ExceptionMessage = ex.Message;
                }

                try
                {
                    row.TransactionJson = JsonConvert.SerializeObject(trans);
                }
                catch { }
                finally
                {
                    InsertError(row, log);
                }
            }
        }

    }
}
