using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using System.Configuration;
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;
using Stripe;
using DisputedChargeBacksFromStripe.Entities;
using System.Linq;
using System.Collections.Generic;

namespace DisputedChargeBacksFromStripe
{
    public static class StripeDisputes
    {
        static string cmdGetLatestCreatedTime = "SELECT ISNULL(MAX(CreatedTime),DATEADD(year,-1,GETDATE())) FROM [dbo].[StripeTransactions]";
        static string upsertCommand = @"IF NOT EXISTS (SELECT 1 FROM [dbo].[StripeTransactions] WHERE [APITransactionID] = @APITransactionID)
BEGIN
  INSERT INTO [dbo].[StripeTransactions]
	 ([APITransactionID]
	 ,[TransactionType]
	 ,[Description]
	 ,[CreatedTime]
	 ,[Amount]
	 ,[Currency]
	 ,[FeeAmount]
	 ,[FeeDetails]
	 ,[TaxAmount]
	 ,[LiveMode]
	 ,[Status]
	 ,[CustomerStripeID]
	 ,[Captured]
	 ,[Card]
	 ,[InvoiceID]
	 ,[CardBrand]
	 ,[DestinationAccountId]
	 ,[TransferID]
	 ,[TransferGroup]
	 ,[Metadata]
     ,[RefundChargeId])
  VALUES 
	  (@APITransactionID 
	 ,@TransactionType 
	 ,@Description 
	 ,@CreatedTime
	 ,@Amount
	 ,@Currency
	 ,@FeeAmount
	 ,@FeeDetails 
	 ,@TaxAmount
	 ,@LiveMode
	 ,@Status 
	 ,@CustomerStripeID 
	 ,@Captured
	 ,@Card 
	 ,@InvoiceID 
	 ,@CardBrand 
	 ,@DestinationAccountId
	 ,@TransferID
	 ,@TransferGroup
	 ,@Metadata
     ,@RefundChargeId)
END
ELSE
BEGIN
UPDATE [dbo].[StripeTransactions]
	SET 
	 [TransactionType]      = @TransactionType 
	,[Description]		   = @Description 
	,[CreatedTime]		   = @CreatedTime
	,[Amount]			   = @Amount
	,[Currency]			   = @Currency
	,[FeeAmount]			   = @FeeAmount
	,[FeeDetails]		   = @FeeDetails 
	,[TaxAmount]			   = @TaxAmount
	,[LiveMode]			   = @LiveMode
	,[Status]			   = @Status 
	,[CustomerStripeID]	   = @CustomerStripeID 
	,[Captured]			   = @Captured
	,[Card]				   = @Card 
	,[InvoiceID]			   = @InvoiceID 
	,[CardBrand]			   = @CardBrand 
	,[DestinationAccountId] = @DestinationAccountId
	,[TransferID]		   = @TransferID
	,[TransferGroup]		   = @TransferGroup
	,[Metadata]			   = @Metadata
    ,[RefundChargeId]       = @RefundChargeId
WHERE [APITransactionID]  = @APITransactionID
END";

        [FunctionName("StripeDisputes")]
        public static void Run([TimerTrigger("0 */15 * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            if (myTimer.IsPastDue)
            {
                log.Info("Timer is running late!");
            }
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

            StripeChargeService chargeService = new StripeChargeService();
            chargeService.ExpandBalanceTransaction = true;
            chargeService.ExpandApplicationFee = true;
            chargeService.ExpandDestination = true;
            chargeService.ExpandInvoice = true;
            StripeConfiguration.SetApiKey(ConfigurationManager.AppSettings["StripeApiKey"]);

            GetRefundsForLast24Hours(chargeService, log);
        }

        static void GetRefundsForLast24Hours(StripeChargeService chargeService, TraceWriter log)
        {
            StripeRefundService refundService = new StripeRefundService();
            refundService.ExpandBalanceTransaction = true;
            refundService.ExpandCharge = true;

            StripeRefundListOptions refundListOptions = new StripeRefundListOptions()
            {
                Limit = 10
            };

            DateTime greaterEqualCreated = DateTime.UtcNow.AddHours(-24);

            IEnumerable<StripeRefund> refunds;
            string lastObjectId = null;
            DateTime? lastRefundCreated = null;
            do
            {
                refunds = refundService.List(refundListOptions);
                foreach (var r in refunds)
                {
                    //var c = chargeService.Get(r.ChargeId);
                    //var trans = StripeChargeToStripeTransaction(c);

                    var trans = StripeRefundToStripeTransaction(r);
                    UpsertStripeTransaction(trans, log);
                }
                lastObjectId = refunds.LastOrDefault()?.Id;
                lastRefundCreated = refunds.LastOrDefault()?.Created;
                refundListOptions.StartingAfter = lastObjectId;
                log.Info($"Refund last ObjectId: {lastObjectId}. Created: {lastRefundCreated} ");
            } while (refunds.Count() == 10 && lastRefundCreated.HasValue && lastRefundCreated.Value >= greaterEqualCreated);
        }

        static StripeTransaction StripeRefundToStripeTransaction(StripeRefund r)
        {
            var trans = new StripeTransaction()
            {
                APITransactionId = r.Id,
                TransactionType = r.Object,
                Description = r.Description,
                CreatedTime = r.Created.ToUniversalTime(),
                Amount = (-1) * r.Amount / 100m,
                Currency = r.Currency,
                FeeAmount = r.BalanceTransaction?.Fee / 100m,
                FeeDetails = JsonConvert.SerializeObject(r.BalanceTransaction?.FeeDetails),
                TaxAmount = r.Charge?.Invoice?.Tax / 100m,
                LiveMode = r.Charge?.LiveMode,
                Status = r.Status,
                CustomerStripeId = r.Charge?.CustomerId,
                Captured = r.Charge?.Captured,
                Card = JsonConvert.SerializeObject(r.Charge?.Source?.Card),
                InvoiceId = r.Charge?.InvoiceId,
                CardBrand = r.Charge?.Source?.Card?.Brand,
                DestinationAccountId = r.Charge?.Destination?.Id,
                TransferId = r.Charge?.TransferId,
                TransferGroup = r.Charge?.TransferGroup,
                Metadata = JsonConvert.SerializeObject(r.Metadata),
                RefundChargeId = r.ChargeId
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

        public static void UpsertStripeTransaction(StripeTransaction trans, TraceWriter log)
        {
            try
            {
                var cnnString = ConfigurationManager.ConnectionStrings["PP_ConnectionString"].ConnectionString;

                using (var connection = new SqlConnection(cnnString))
                {
                    connection.Open();


                    using (SqlCommand cmd = new SqlCommand(upsertCommand, connection))
                    {
                        cmd.Parameters.Add("@APITransactionID", SqlDbType.VarChar, 50).Value = trans.APITransactionId ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@TransactionType", SqlDbType.VarChar, 25).Value = trans.TransactionType ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Description", SqlDbType.VarChar, 255).Value = trans.Description ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@CreatedTime", SqlDbType.DateTime).Value = trans.CreatedTime;
                        cmd.Parameters.Add("@Amount", SqlDbType.Decimal).Value = trans.Amount ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Currency", SqlDbType.VarChar, 10).Value = trans.Currency ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@FeeAmount", SqlDbType.Decimal).Value = trans.FeeAmount ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@FeeDetails", SqlDbType.NVarChar).Value = trans.FeeDetails ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@TaxAmount", SqlDbType.Decimal).Value = trans.TaxAmount ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@LiveMode", SqlDbType.Bit).Value = trans.LiveMode ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Status", SqlDbType.VarChar, 50).Value = trans.Status ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@CustomerStripeID", SqlDbType.VarChar, 25).Value = trans.CustomerStripeId ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Captured", SqlDbType.Bit).Value = trans.Captured ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Card", SqlDbType.NVarChar).Value = trans.Card ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@InvoiceID", SqlDbType.VarChar, 50).Value = trans.InvoiceId ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@CardBrand", SqlDbType.VarChar, 25).Value = trans.CardBrand ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@DestinationAccountId", SqlDbType.VarChar, 50).Value = trans.DestinationAccountId ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@TransferID", SqlDbType.VarChar, 50).Value = trans.TransferId ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@TransferGroup", SqlDbType.VarChar, 50).Value = trans.TransferGroup ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Metadata", SqlDbType.NVarChar).Value = trans.Metadata ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@RefundChargeId", SqlDbType.VarChar, 50).Value = trans.RefundChargeId ?? (object)DBNull.Value;
                        var resultCount = cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                //dberrorhandler.inserterror(trans, ex, log);
                log.Error("InsertStripeTransaction error", ex);
            }
        }

    }
}
