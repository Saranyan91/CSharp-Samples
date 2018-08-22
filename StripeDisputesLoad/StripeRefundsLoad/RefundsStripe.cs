using System;
using System.Configuration;
using System.Data.SqlClient;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Data;
using Stripe;
using StripeRefundsLoad.Entities;
using System.Linq;
using System.Collections.Generic;

namespace StripeRefundsLoad
{
    public static class RefundsStripe
    {
        static string cmdGetLatestCreatedTime = "SELECT ISNULL(MAX(CreatedTime),DATEADD(year,-1,GETDATE())) FROM [Stripe].[Refunds]";
        static string upsertCommand = @"IF NOT EXISTS (SELECT 1 FROM [Stripe].[Refunds] WHERE [RefundID] = @RefundID)
BEGIN
  INSERT INTO [Stripe].[Refunds]
	 ([RefundID]
	 ,[TransactionType]
	 ,[Amount]
	 ,[Reason]
	 ,[Status]
	 ,[BalanceTransactionId]
	 ,[BalanceTransaction]
	 ,[FailureBalanceTransactionId]
     ,[FailureBalanceTransaction]
	 ,[FailureReason]
	 ,[ChargeID]
	 ,[Charge]
	 ,[CreatedTime]
	 ,[Currency]
	 ,[Description]
	 ,[Metadata]
	 ,[Receipt_Number])
  VALUES 
	  (@RefundID
	 ,@TransactionType
	 ,@Amount
	 ,@Reason
	 ,@Status
	 ,@BalanceTransactionId
	 ,@BalanceTransaction
	 ,@FailureBalanceTransactionId
     ,@FailureBalanceTransaction
	 ,@FailureReason
	 ,@ChargeID
	 ,@Charge
	 ,@CreatedTime
	 ,@Currency
	 ,@Description
	 ,@Metadata
	 ,@ReceiptNumber)
END
ELSE
BEGIN
UPDATE [Stripe].[Refunds]
	SET
      [TransactionType] = @TransactionType
	 ,[Amount] = @Amount
	 ,[Reason] = @Reason
	 ,[Status] = @Status
	 ,[BalanceTransactionId] = @BalanceTransactionId
	 ,[BalanceTransaction] = @BalanceTransaction
	 ,[FailureBalanceTransactionId] = @FailureBalanceTransactionId
     ,[FailureBalanceTransaction] = @FailureBalanceTransaction
	 ,[FailureReason] = @FailureReason
	 ,[ChargeID] = @ChargeID
	 ,[Charge] = @Charge
	 ,[CreatedTime] = @CreatedTime
	 ,[Currency] = @Currency
	 ,[Description] = @Description
	 ,[Metadata] = @Metadata
	 ,[Receipt_Number] = @ReceiptNumber
     ,[record_updated_date] = @RecordUpdatedDate
WHERE [RefundID]  = @RefundID
END";

        [FunctionName("StripeRefunds")]
        public static void Run([TimerTrigger("0 0 7 * * *")]TimerInfo myTimer, TraceWriter log)
        {
            if (myTimer.IsPastDue)
            {
                log.Info("Timer is running late!");
            }

            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

            StripeRefundService refundService = new StripeRefundService();
            refundService.ExpandBalanceTransaction = true;
            refundService.ExpandFailureBalanceTransaction = true;
            refundService.ExpandCharge = true;
            StripeConfiguration.SetApiKey(ConfigurationManager.AppSettings["StripeApiKey"]);

            GetChargeRefunds48hrs(refundService, log);

        }

        static void GetChargeRefunds48hrs(StripeRefundService refundService, TraceWriter log)
        {
            string lastObjectId = null;
            StripeList<StripeRefund> refundItems = null;

            DateTime greaterEqualCreated = DateTime.UtcNow.AddHours(-48);

            var listOptions = new StripeRefundListOptions()
             {
                Limit = 100,
                StartingAfter = lastObjectId,
                
            };
            
            DateTime? lastRefundCreated = null;
            do
            {
                refundItems = refundService.List(listOptions);
                foreach (var r in refundItems.Data)
                {
                    
                    //log.Info(r.ToString());
                    var Refunds = StripeRefundToStripeTransaction(r);
                    UpsertStripeRefunds(Refunds, log);
                }
                lastObjectId = refundItems.Data.LastOrDefault()?.Id;
                lastRefundCreated = refundItems.Data.LastOrDefault()?.Created;
                listOptions.StartingAfter = lastObjectId;
                log.Info($"Refund last ObjectId: {lastObjectId}. Created: {lastRefundCreated} ");
            } while (refundItems.Count() == 10 && lastRefundCreated.HasValue && lastRefundCreated.Value >= greaterEqualCreated);
        }

        static void GetChargeRefunds(StripeRefundService refundService, TraceWriter log)
        {
            string lastObjectId = null;
            StripeList<StripeRefund> refundItems = null;

            DateTime greaterEqualCreated = DateTime.UtcNow.AddHours(-48);

            var listOptions = new StripeRefundListOptions()
            {
                Limit = 100,
                StartingAfter = lastObjectId
            };

            DateTime? lastRefundCreated = null;
            do
            {
                refundItems = refundService.List(listOptions);
                foreach (var r in refundItems.Data)
                {

                    //log.Info(r.ToString());
                    var Refunds = StripeRefundToStripeTransaction(r);
                    UpsertStripeRefunds(Refunds, log);
                }
                lastObjectId = refundItems.Data.LastOrDefault()?.Id;
                lastRefundCreated = refundItems.Data.LastOrDefault()?.Created;
                listOptions.StartingAfter = lastObjectId;
                log.Info($"Refund last ObjectId: {lastObjectId}. Created: {lastRefundCreated} ");
            } while (refundItems.HasMore);
        }


        static StripeRefundTransaction StripeRefundToStripeTransaction(StripeRefund r)
        {
            var trans = new StripeRefundTransaction()
            {
                RefundId = r.Id,
                TransactionType = r.Object,
                Amount = (-1) * r.Amount / 100m,
                Reason = r.Reason,
                Status = r.Status,
                BalanceTransactionId = r.BalanceTransactionId,
                BalanceTransaction = JsonConvert.SerializeObject(r.BalanceTransaction),
                FailureBalanceTransactionId = r.FailureBalanceTransactionId,
                FailureBalanceTransaction = JsonConvert.SerializeObject(r.FailureBalanceTransaction),
                FailureReason = r.FailureReason,
                ChargeID = r.ChargeId,
                Charge = JsonConvert.SerializeObject(r.Charge),
                CreatedTime = r.Created.ToUniversalTime(),
                Currency = r.Currency,
                Description = r.Description,
                Metadata = JsonConvert.SerializeObject(r.Metadata),
                ReceiptNumber = r.ReceiptNumber
            };
            return trans;
        }

        static DateTime GetLatestCreatedTime()
        {
            DateTime latestCreatedTime;
            var cnnString = ConfigurationManager.ConnectionStrings["PP_ConnectionString"].ConnectionString;

            using (var connection = new SqlConnection(cnnString))
            {
                connection.Open();

                using (SqlCommand cmd = new SqlCommand(cmdGetLatestCreatedTime, connection))
                {
                    latestCreatedTime = (DateTime)cmd.ExecuteScalar();
                    latestCreatedTime = DateTime.SpecifyKind(latestCreatedTime, DateTimeKind.Utc);
                }
            }
            return latestCreatedTime;
        }

        public static void UpsertStripeRefunds(StripeRefundTransaction trans, TraceWriter log)
        {
            try
            {
                var cnnString = ConfigurationManager.ConnectionStrings["PP_ConnectionString"].ConnectionString;

                using (var connection = new SqlConnection(cnnString))
                {
                    connection.Open();


                    using (SqlCommand cmd = new SqlCommand(upsertCommand, connection))
                    {
                        cmd.Parameters.Add("@RefundID", SqlDbType.VarChar, 50).Value = trans.RefundId ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@TransactionType", SqlDbType.VarChar, 25).Value = trans.TransactionType ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Amount", SqlDbType.Decimal).Value = trans.Amount ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Reason", SqlDbType.VarChar).Value = trans.Reason ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Status", SqlDbType.VarChar).Value = trans.Status ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@BalanceTransactionId", SqlDbType.VarChar).Value = trans.BalanceTransactionId ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@BalanceTransaction", SqlDbType.VarChar).Value = trans.BalanceTransaction ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@FailureBalanceTransactionId", SqlDbType.VarChar).Value = trans.FailureBalanceTransactionId ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@FailureBalanceTransaction", SqlDbType.VarChar).Value = trans.FailureBalanceTransaction ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@FailureReason", SqlDbType.VarChar).Value = trans.FailureReason ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@ChargeID", SqlDbType.VarChar, 50).Value = trans.ChargeID ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Charge", SqlDbType.NVarChar).Value = trans.Charge ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@CreatedTime", SqlDbType.DateTime2).Value = trans.CreatedTime;
                        cmd.Parameters.Add("@Currency", SqlDbType.VarChar, 10).Value = trans.Currency ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Description", SqlDbType.NVarChar).Value = trans.Description?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Metadata", SqlDbType.NVarChar).Value = trans.Metadata ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@ReceiptNumber", SqlDbType.VarChar, 50).Value = trans.ReceiptNumber ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@RecordUpdatedDate", SqlDbType.DateTime2, 50).Value = DateTime.UtcNow;
                        var resultCount = cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("InsertStripeTransaction error", ex);
            }
        }
    }
}
