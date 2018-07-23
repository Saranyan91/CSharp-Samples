using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DisputedChargeBacksFromStripe.Entities
{
    public class StripeTransaction
    {
        public string APITransactionId { get; set; }
        public string TransactionType { get; set; }
        public string Description { get; set; }

        public DateTime CreatedTime { get; set; }

        public decimal? Amount { get; set; }

        public string Currency { get; set; }

        public decimal? FeeAmount { get; set; }

        public string FeeDetails { get; set; }

        public decimal? TaxAmount { get; set; }

        public bool? LiveMode { get; set; }

        public string Status { get; set; }

        public string CustomerStripeId { get; set; }

        public bool? Captured { get; set; }

        public string Card { get; set; }

        public string InvoiceId { get; set; }

        public string CardBrand { get; set; }

        public string DestinationAccountId { get; set; }

        public string TransferId { get; set; }
        public string TransferGroup { get; set; }

        public string Metadata { get; set; }

        public string RefundChargeId { get; set; }
    }
}
