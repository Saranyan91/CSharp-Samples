using System;
using System.Configuration;
using System.Data.SqlClient;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Data;
using Stripe;
using System.Linq;
using System.Collections.Generic;
using StripeDisputesLoad.Entities;

namespace StripeDisputesLoad
{
    public static class DisputesStripe
    {
        
        static string cmdGetLatestCreatedTime = "SELECT ISNULL(MAX(CreatedTime),DATEADD(year,-1,GETDATE())) FROM [dbo].[StripeDisputes] where TransactionType = 'dispute'";
        static string upsertCommand = @"IF NOT EXISTS (SELECT 1 FROM [dbo].[StripeDisputes] WHERE [DisputeID] = @DisputeID)
BEGIN
  INSERT INTO [dbo].[StripeDisputes]
	 ([DisputeID]
	 ,[TransactionType]
	 ,[Reason]
	 ,[Status]
	 ,[BalanceTransaction]
	 ,[ChargeID]
	 ,[CreatedTime]
	 ,[Amount]
	 ,[Currency]
	 ,[Evidence]
	 ,[EvidenceDetails]
	 ,[IsChargeRefundable]
	 ,[LiveMode]
	 ,[Metadata])
  VALUES 
	  (@DisputeID 
	 ,@TransactionType 
	 ,@Reason 
	 ,@Status
	 ,@BalanceTransaction
	 ,@ChargeID
	 ,@CreatedTime
	 ,@Amount 
	 ,@Currency
	 ,@Evidence
	 ,@EvidenceDetails 
	 ,@IsChargeRefundable 
	 ,@LiveMode
	 ,@Metadata)
END
ELSE
BEGIN
UPDATE [dbo].[StripeDisputes]
	SET 
	 [TransactionType]      = @TransactionType 
	,[Reason]		        = @Reason 
	,[Status]		        = @Status
	,[BalanceTransaction]	= @BalanceTransaction
	,[ChargeID]			    = @ChargeID
	,[CreatedTime]			= @CreatedTime
	,[Amount]		        = @Amount 
	,[Currency]			    = @Currency
	,[Evidence]			    = @Evidence
	,[EvidenceDetails]		= @EvidenceDetails 
	,[IsChargeRefundable]	= @IsChargeRefundable 
	,[LiveMode]			   = @LiveMode
	,[Metadata]			   = @Metadata
    ,[record_updated_date] = @RecordUpdatedDate
WHERE [DisputeID]  = @DisputeID
END";
        [FunctionName("DisputesStripe")]
        public static void Run([TimerTrigger("0 0 8 * * *")]TimerInfo myTimer, TraceWriter log)
        {
            if (myTimer.IsPastDue)
            {
                log.Info("Timer is running late!");
            }
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");
            StripeDisputeService chargeService = new StripeDisputeService();
            chargeService.ExpandCharge = true; 
            StripeConfiguration.SetApiKey(ConfigurationManager.AppSettings["StripeApiKey"]);

            GetDisputedCharges(chargeService, log);
        }

        static void GetDisputedCharges(StripeDisputeService chargeService, TraceWriter log)
        {
            string lastObjectId = null;
            StripeList<StripeDispute> response = null;

            //var greaterThanCreatedFilter = GetLatestCreatedTime();
            var listOptions = new StripeDisputeListOptions()
            {
                Limit = 100,
                StartingAfter = lastObjectId
            };

            do
            {
                response = chargeService.List(listOptions);

                foreach (var d in response.Data)
                {
                    var disputes = DisputesResponseToStripeDisputes(d);
                    UpsertStripeDispute(disputes,log);
                    log.Info($"Dispute Updated: {disputes.DisputeId.ToString()}");
                }
                lastObjectId = response.Data.LastOrDefault()?.Id;
                listOptions.StartingAfter = lastObjectId;
                log.Info($"Charge last ObjectId: {lastObjectId}. More responses? {response.HasMore}");
            }
            while (response.HasMore);
        }

        static StripeDisputeTransaction DisputesResponseToStripeDisputes(StripeDispute d)
        {
            var disp = new StripeDisputeTransaction()
            {
                DisputeId = d.Id,
                TransactionType = d.Object,
                Reason = d.Reason,
                Status = d.Status,
                ChargeId = d.ChargeId,
                BalanceTransaction = JsonConvert.SerializeObject(d.BalanceTransactions),
                CreatedTime = d.Created,
                Amount = (-1) * d.Amount / 100m,
                Currency = d.Currency,
                Evidence = JsonConvert.SerializeObject(d.Evidence),
                EvidenceDetails = JsonConvert.SerializeObject(d.EvidenceDetails),
                IsChargeRefundable = d.IsChargeRefundable,
                LiveMode = d.LiveMode,
                Metadata = JsonConvert.SerializeObject(d.Metadata)

            };
            return disp;
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

        public static void UpsertStripeDispute(StripeDisputeTransaction disputes, TraceWriter log)
        {
            try
            {
                var cnnString = ConfigurationManager.ConnectionStrings["PP_ConnectionString"].ConnectionString;

                using (var connection = new SqlConnection(cnnString))
                {
                    connection.Open();


                    using (SqlCommand cmd = new SqlCommand(upsertCommand, connection))
                    {
                        cmd.Parameters.Add("@DisputeID", SqlDbType.NVarChar, 50).Value = disputes.DisputeId ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@TransactionType", SqlDbType.NVarChar, 25).Value = disputes.TransactionType ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Reason", SqlDbType.NVarChar, 255).Value = disputes.Reason ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 255).Value = disputes.Status ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@BalanceTransaction", SqlDbType.NVarChar).Value = disputes.BalanceTransaction ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@ChargeID", SqlDbType.NVarChar, 50).Value = disputes.ChargeId ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@CreatedTime", SqlDbType.DateTime2).Value = disputes.CreatedTime ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Amount", SqlDbType.Decimal).Value = disputes.Amount ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Currency", SqlDbType.NVarChar, 10).Value = disputes.Currency ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Evidence", SqlDbType.NVarChar).Value = disputes.Evidence ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@EvidenceDetails ", SqlDbType.NVarChar).Value = disputes.EvidenceDetails ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@IsChargeRefundable ", SqlDbType.Bit).Value = disputes.IsChargeRefundable ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@LiveMode", SqlDbType.Bit).Value = disputes.LiveMode ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Metadata", SqlDbType.NVarChar).Value = disputes.Metadata ?? (object)DBNull.Value;
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
