using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StripeRefundsLoad.Entities
{
    public class StripeRefundTransaction
    {
        public string RefundId { get; set; }

        public string TransactionType { get; set; }

        public decimal? Amount { get; set; }

        public string Status { get; set; }
        public string Reason { get; set; }

        public string BalanceTransactionId { get; set; }
        public string BalanceTransaction { get; set; }

        public string FailureBalanceTransactionId { get; set; }
        public string FailureBalanceTransaction { get; set; }
        public string FailureReason { get; set; }

        public string ChargeID { get; set; }
        public string Charge { get; set; }

        public DateTime CreatedTime { get; set; }

        public string Currency { get; set; }

        public string Description { get; set; }

        public string Metadata { get; set; }

        public string ReceiptNumber { get; set; }
    }
}
