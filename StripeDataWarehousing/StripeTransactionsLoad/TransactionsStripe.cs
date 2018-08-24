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
using StripeTransactionsLoad.Entities;

namespace StripeTransactionsLoad
{
    public static class TransactionsStripe
    {
        static string cmdGetLatestCreatedTime = "SELECT ISNULL(MAX(CreatedTime),DATEADD(year,-1,GETDATE())) FROM [Stripe].[Charges]";
        static string cmdGetMinCreatedTime = "SELECT ISNULL(MIN(CreatedTime),DATEADD(year,-1,GETDATE())) FROM [Stripe].[Charges]";
        static string cmdGetLatestBankAccountChargeTime = "SELECT ISNULL(MIN(CreatedTime),DATEADD(year,-1,GETDATE())) FROM [Stripe].[Charges] where SourceType = 'Bank Account'";
        static string upsertCommand = @"IF NOT EXISTS (SELECT 1 FROM [Stripe].[Charges] WHERE [ChargeID] = @ChargeID)
BEGIN
  INSERT INTO [Stripe].[Charges]
	 ([ChargeID]
	 ,[TransactionType]
	 ,[Amount]
	 ,[Fee]
	 ,[FeeDetails]
     ,[TaxAmount]
	 ,[ApplicationId]
	 ,[Application]
	 ,[ApplicationFeeId]
	 ,[ApplicationFee]
	 ,[BalanceTransactionId]
	 ,[BalanceTransaction]
	 ,[Captured]
	 ,[CreatedTime]
	 ,[Currency]
	 ,[CustomerID]
	 ,[Description]
	 ,[DisputeId]
	 ,[InvoiceId]
	 ,[LiveMode]
	 ,[Metadata]
     ,[PremiumPaymentID]
     ,[OnBehalfOfId]
     ,[Paid]
     ,[ReceiptEmail]
     ,[ReceiptNumber]
     ,[Refunded]
     ,[SourceType]
     ,[SourceID]
     ,[Source]
     ,[Card] 
	 ,[BankAccount] 
	 ,[Account]
     ,[Status]
     ,[OutcomeMessage]
     ,[OutcomeType]
     ,[TransferId])
  VALUES 
	  (@ChargeID
	 ,@TransactionType
	 ,@Amount
	 ,@Fee
	 ,@FeeDetails
     ,@TaxAmount
	 ,@ApplicationId
	 ,@Application
	 ,@ApplicationFeeId
	 ,@ApplicationFee
	 ,@BalanceTransactionId
	 ,@BalanceTransaction
	 ,@Captured
	 ,@CreatedTime
	 ,@Currency
	 ,@CustomerID
	 ,@Description
	 ,@DisputeId
	 ,@InvoiceId
	 ,@LiveMode
     ,@Metadata
     ,@PremiumPaymentID
     ,@OnBehalfOfId
     ,@Paid
     ,@ReceiptEmail
     ,@ReceiptNumber
     ,@Refunded
     ,@SourceType
     ,@SourceID
     ,@Source
     ,@Card 
	 ,@BankAccount
	 ,@Account
     ,@Status
     ,@OutcomeMessage
     ,@OutcomeType
     ,@TransferId)
END
ELSE
BEGIN
UPDATE [Stripe].[Charges]
	SET 
	 [TransactionType] = @TransactionType
	 ,[Amount] = @Amount
	 ,[Fee] = @Fee
	 ,[FeeDetails] = @FeeDetails
     ,[TaxAmount] = @TaxAmount
	 ,[ApplicationId] = @ApplicationId
	 ,[Application] = @Application
	 ,[ApplicationFeeId] = @ApplicationFeeId
	 ,[ApplicationFee] = @ApplicationFee
	 ,[BalanceTransactionId] = @BalanceTransactionId
	 ,[BalanceTransaction] = @BalanceTransaction
	 ,[Captured] = @Captured
	 ,[CreatedTime] = @CreatedTime
	 ,[Currency] = @Currency
	 ,[CustomerID] = @CustomerID
	 ,[Description] = @Description
	 ,[DisputeId] = @DisputeId
	 ,[InvoiceId] = @InvoiceId
	 ,[LiveMode] = @LiveMode
	 ,[Metadata] = @Metadata
     ,[PremiumPaymentID] = @PremiumPaymentID
     ,[OnBehalfOfId] = @OnBehalfOfId
     ,[Paid] = @Paid
     ,[ReceiptEmail] = @ReceiptEmail
     ,[ReceiptNumber] = @ReceiptNumber 
     ,[Refunded] = @Refunded
     ,[SourceType] = @SourceType
     ,[SourceID] = @SourceID
     ,[Source] = @Source
     ,[Card] = @Card 
	 ,[BankAccount] = @BankAccount
	 ,[Account] = @Account
     ,[Status] = @Status
     ,[OutcomeMessage] = @OutcomeMessage
     ,[OutcomeType] = @OutcomeType
     ,[TransferId] = @TransferId
     ,[record_updated_date] = @RecordUpdatedDate
WHERE [ChargeID]  = @ChargeID
END";

        [FunctionName("StripeCharges")]
        public static void Run([TimerTrigger("0 */10 * * * *")]TimerInfo myTimer, TraceWriter log)
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

            GetNewCharges(chargeService, log);
            //GetAllCharges(chargeService, log);
        }

        static void GetNewCharges(StripeChargeService chargeService, TraceWriter log)
        {
            string lastObjectId = null;
            StripeList<StripeCharge> response = null;

            var greaterThanCreatedFilter = GetLatestCreatedTime();
            log.Info($"Latest Created Time: {greaterThanCreatedFilter}");

            var listOptions = new StripeChargeListOptions()
            {
                Limit = 100,
                Created = new StripeDateFilter { GreaterThan = greaterThanCreatedFilter },
                StartingAfter = lastObjectId
            };

            do
            {
                response = chargeService.List(listOptions);

                foreach (var c in response.Data)
                {
                    var trans = StripeChargeToStripeTransaction(c);
                    UpsertStripeTransaction(trans, log);
                }
                lastObjectId = response.Data.LastOrDefault()?.Id;
                listOptions.StartingAfter = lastObjectId;
                log.Info($"Charge last ObjectId: {lastObjectId}. More responses? {response.HasMore}");
            }
            while (response.HasMore);
        }

        static void GetAllCharges(StripeChargeService chargeService, TraceWriter log)
        {
            string lastObjectId = null;
            DateTime? lastObjectCreated = null;
            StripeList<StripeCharge> response = null;

            var lesserThanCreatedFilter = GetMinCreatedTime();
            log.Info($"Min Created Time: {lesserThanCreatedFilter}");

            var listOptions = new StripeChargeListOptions()
            {
                Limit = 100,
                Created = new StripeDateFilter { LessThan = lesserThanCreatedFilter },
                StartingAfter = lastObjectId
            };

            do
            {
                response = chargeService.List(listOptions);

                foreach (var c in response.Data)
                {
                    var trans = StripeChargeToStripeTransaction(c);
                    UpsertStripeTransaction(trans, log);
                }
                lastObjectId = response.Data.LastOrDefault()?.Id;
                lastObjectCreated = response.Data.LastOrDefault()?.Created;
                listOptions.StartingAfter = lastObjectId;
                listOptions.Created = new StripeDateFilter { LessThan = lastObjectCreated };
                log.Info($"Charge last ObjectId: {lastObjectId}. Created Time: {lastObjectCreated}.More responses? {response.HasMore}");
            }
            while (response.HasMore);
        }

        static void GetBankAccountCharges(StripeChargeService chargeService, TraceWriter log)
        {
            string lastObjectId = null;
            DateTime? lastObjectCreated = null;
            StripeList<StripeCharge> response = null;
            
            var greaterThanCreatedFilter = GetLatestBankAccountChargeTime();
            log.Info($"Max Back Account Charge Created Time: {greaterThanCreatedFilter}");

            var listOptions = new StripeChargeListOptions()
            {
                Limit = 100,
                Created = new StripeDateFilter { LessThan = greaterThanCreatedFilter },
                StartingAfter = lastObjectId
            };

            var sourceListOptions = new StripeSourceListOptions()
            {
                Type = "Bank Account"
            };

            do
            {
                response = chargeService.List(listOptions);

                foreach (var c in response.Data)
                {
                    var trans = StripeChargeToStripeTransaction(c);
                    UpsertStripeTransaction(trans, log);
                }
                lastObjectId = response.Data.LastOrDefault()?.Id;
                lastObjectCreated = response.Data.LastOrDefault()?.Created;
                listOptions.StartingAfter = lastObjectId;
                log.Info($"Charge last ObjectId: {lastObjectId}. Created Time: {lastObjectCreated}.More responses? {response.HasMore}");
            }
            while (response.HasMore);
        }

        static StripeTransaction StripeChargeToStripeTransaction(StripeCharge c)
        {
            var trans = new StripeTransaction()
            {
                ChargeID = c.Id,
                TransactionType = c.Object,
                Amount = c.Amount / 100m,
                Fee = c.BalanceTransaction?.Fee / 100m,
                FeeDetails = processNull(JsonConvert.SerializeObject(c.BalanceTransaction?.FeeDetails)),
                TaxAmount = c.Invoice?.Tax / 100m,
                ApplicationId = c.ApplicationId,
                Application = processNull(JsonConvert.SerializeObject(c.Application)),
                ApplicationFeeId = c.ApplicationFeeId,
                ApplicationFee = processNull(JsonConvert.SerializeObject(c.ApplicationFee)),
                BalanceTransactionId = c.BalanceTransactionId,
                BalanceTransaction = processNull(JsonConvert.SerializeObject(c.BalanceTransaction)),
                Captured = c.Captured,
                CreatedTime = c.Created.ToUniversalTime(),
                Currency = c.Currency,
                CustomerID = c.CustomerId,
                Description = c.Description,
                DisputeId = c.DisputeId,
                InvoiceId = c.InvoiceId,
                LiveMode = c.LiveMode,
                Metadata = processNull(JsonConvert.SerializeObject(c.Metadata)),
                PremiumPaymentID = Convert.ToInt32(getPaymentId(c.Metadata)),
                OnBehalfOfId = c.OnBehalfOfId,
                Paid = c.Paid,
                ReceiptEmail = c.ReceiptEmail,
                ReceiptNumber = c.ReceiptNumber,
                Refunded = c.Refunded,
                SourceType = getSourceType(c.Source.Type),
                SourceID = c.Source?.Id,
                Source = JsonConvert.SerializeObject(c.Source),
                Card = processNull(JsonConvert.SerializeObject(c.Source?.Card)),
                BankAccount = processNull(JsonConvert.SerializeObject((c.Source?.BankAccount))),
                Account = processNull(JsonConvert.SerializeObject(c.Source?.Account)),
                Status = c.Status,
                OutcomeMessage = c.Outcome?.SellerMessage,
                OutcomeType = c.Outcome?.Type,
                TransferId = c.TransferId
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

        static DateTime GetMinCreatedTime()
        {
            DateTime minCreatedTime;
            var cnnString = ConfigurationManager.ConnectionStrings["PP_ConnectionString"].ConnectionString;

            using (var connection = new SqlConnection(cnnString))
            {
                connection.Open();

                using (SqlCommand cmd = new SqlCommand(cmdGetMinCreatedTime, connection))
                {
                    minCreatedTime = (DateTime)cmd.ExecuteScalar();
                    minCreatedTime = DateTime.SpecifyKind(minCreatedTime, DateTimeKind.Utc);
                }
            }
            return minCreatedTime;
        }


        static DateTime GetLatestBankAccountChargeTime()
        {
            DateTime maxCreatedTime;
            var cnnString = ConfigurationManager.ConnectionStrings["PP_ConnectionString"].ConnectionString;

            using (var connection = new SqlConnection(cnnString))
            {
                connection.Open();

                using (SqlCommand cmd = new SqlCommand(cmdGetLatestBankAccountChargeTime, connection))
                {
                    maxCreatedTime = (DateTime)cmd.ExecuteScalar();
                    maxCreatedTime = DateTime.SpecifyKind(maxCreatedTime, DateTimeKind.Utc);
                }
            }
            return maxCreatedTime;
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
                        cmd.Parameters.Add("@ChargeID", SqlDbType.VarChar, 50).Value = trans.ChargeID ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@TransactionType", SqlDbType.VarChar, 25).Value = trans.TransactionType ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Amount", SqlDbType.Decimal).Value = trans.Amount ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Fee", SqlDbType.Decimal).Value = trans.Fee ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@FeeDetails", SqlDbType.NVarChar).Value = trans.FeeDetails ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@TaxAmount", SqlDbType.Decimal).Value = trans.TaxAmount ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@ApplicationId", SqlDbType.VarChar, 50).Value = trans.ApplicationId ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Application", SqlDbType.NVarChar).Value = trans.Application?? (object)DBNull.Value;
                        cmd.Parameters.Add("@ApplicationFeeId", SqlDbType.VarChar, 50).Value = trans.ApplicationFeeId ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@ApplicationFee", SqlDbType.NVarChar).Value = trans.ApplicationFee ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@BalanceTransactionId", SqlDbType.VarChar, 50).Value = trans.BalanceTransactionId ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@BalanceTransaction", SqlDbType.NVarChar).Value = trans.BalanceTransaction ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Captured", SqlDbType.Bit).Value = trans.Captured ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@CreatedTime", SqlDbType.DateTime2).Value = trans.CreatedTime;
                        cmd.Parameters.Add("@Currency", SqlDbType.VarChar, 10).Value = trans.Currency ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@CustomerID", SqlDbType.VarChar, 50).Value = trans.CustomerID ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Description", SqlDbType.NVarChar).Value = trans.Description ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@DisputeId", SqlDbType.VarChar, 50).Value = trans.DisputeId ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@InvoiceID", SqlDbType.VarChar, 50).Value = trans.InvoiceId ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@LiveMode", SqlDbType.Bit).Value = trans.LiveMode ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Metadata", SqlDbType.NVarChar).Value = trans.Metadata ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@PremiumPaymentID", SqlDbType.Int).Value = trans.PremiumPaymentID ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@OnBehalfOfId", SqlDbType.VarChar, 50).Value = trans.OnBehalfOfId ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Paid", SqlDbType.Bit).Value = trans.Paid ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@ReceiptEmail", SqlDbType.VarChar, 50).Value = trans.ReceiptEmail?? (object)DBNull.Value;
                        cmd.Parameters.Add("@ReceiptNumber", SqlDbType.VarChar, 50).Value = trans.ReceiptNumber ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Refunded", SqlDbType.Bit).Value = trans.Refunded ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@SourceType", SqlDbType.VarChar, 50).Value = trans.SourceType ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@SourceID", SqlDbType.VarChar, 50).Value = trans.SourceID ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Source", SqlDbType.NVarChar).Value = trans.Source ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Card", SqlDbType.NVarChar).Value = trans.Card ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@BankAccount", SqlDbType.NVarChar).Value = trans.BankAccount ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Account", SqlDbType.NVarChar).Value = trans.Account ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Status", SqlDbType.VarChar, 50).Value = trans.Status ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@OutcomeMessage", SqlDbType.VarChar, 50).Value = trans.OutcomeMessage ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@OutcomeType", SqlDbType.VarChar, 50).Value = trans.OutcomeType ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@TransferId", SqlDbType.VarChar, 50).Value = trans.TransferId ?? (object)DBNull.Value;
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

        static string getPaymentId(Dictionary<string,string> Metadata)
        {
            if (Metadata.TryGetValue("payment_id", out string myValue))
            {
                return myValue;
            }
            else if (Metadata.TryGetValue("confirmation_number", out string myValue2))
            {
                return myValue2;
            }
            else
            {
                return null;
            }
            
        } 

        static string getSourceType(Stripe.SourceType sourceNum)
        {
            if (sourceNum == Stripe.SourceType.Account)
            {
                return Convert.ToString("Account");
            }
            else if (sourceNum == Stripe.SourceType.BankAccount)
            {
                return Convert.ToString("Bank Account");
            }
            else if (sourceNum == Stripe.SourceType.Card)
            {
                return Convert.ToString("Card");
            }

            else if (sourceNum == Stripe.SourceType.Deleted)
            {
                return Convert.ToString("Deleted");
            }
            else
            {
                return Convert.ToString("Source");
            }
        }

        static string processNull(string data)
        {
            if (data == "null")
            {
                return null;
            }
            else
            {
                return Convert.ToString(data);
            }
        }


    }
}
