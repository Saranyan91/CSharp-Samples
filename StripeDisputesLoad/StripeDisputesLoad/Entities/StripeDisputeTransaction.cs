using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stripe.Infrastructure;

namespace StripeDisputesLoad.Entities
{
    public class StripeDisputeTransaction
    {
        public string DisputeId { get; set; }

        public string TransactionType { get; set; }

        public string Reason { get; set; }

        public string Status { get; set; }

        public string BalanceTransaction { get; set; }

        public string ChargeId { get; set; }

        public DateTime? CreatedTime { get; set; }

        public decimal? Amount { get; set; }

        public string Currency { get; set; }

        public string Evidence { get; set; }
       
        public string EvidenceDetails { get; set; }

        public bool? IsChargeRefundable { get; set; }

        public bool? LiveMode { get; set; }

        public string Metadata { get; set; }
    }
}
