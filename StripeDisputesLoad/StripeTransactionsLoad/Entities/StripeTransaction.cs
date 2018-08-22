using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StripeTransactionsLoad.Entities
{
    public class StripeTransaction
    {
        public string ChargeID { get; set; }
        public string TransactionType { get; set; }
        public decimal? Amount { get; set; }
        public decimal? Fee { get; set; }
        public string FeeDetails { get; set; }
        public decimal? TaxAmount { get; set; }
        public string ApplicationId { get; set; }
        public string Application { get; set; }
        public string ApplicationFeeId { get; set; }
        public string ApplicationFee { get; set; }
        public string BalanceTransactionId { get; set; }
        public string BalanceTransaction { get; set; }
        public bool? Captured { get; set; }
        public DateTime CreatedTime { get; set; }
        public string Currency { get; set; }
        public string CustomerID { get; set; }
        public string Description { get; set; }
        public string DisputeId { get; set; }
        public string InvoiceId { get; set; }
        public bool? LiveMode { get; set; }
        public string Metadata { get; set; }
        public int? PremiumPaymentID { get; set; }
        public string OnBehalfOfId { get; set; }
        public bool? Paid { get; set; }
        public string ReceiptEmail { get; set; }
        public string ReceiptNumber { get; set; }
        public bool? Refunded { get; set; }
        public string SourceType { get; set; }
        public string SourceID { get; set; }
        public string Source { get; set; }
        public string Card { get; set; }
        public string BankAccount { get; set; }
        public string Account { get; set; }
        public string Status { get; set; }
        public string OutcomeMessage { get; set; }
        public string OutcomeType { get; set; }
        public string TransferId { get; set; }
    }
}
