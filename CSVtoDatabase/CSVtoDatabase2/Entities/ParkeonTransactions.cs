using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVtoDatabase2.Entities
{
    public class ParkeonTransactions

    {
        public Int32 SystemID { get; set; }
        public Int32 PrintedID { get; set; }
        public DateTime ServerTime { get; set; }
        public DateTime TerminalDate { get; set; }
        public DateTime EndDate { get; set; }
        public string PaymentType { get; set; }
        public decimal? Amount { get; set; }
        public string Currency { get; set; }
        public Int32 BankingID { get; set; }
        public string CardType { get; set; }
        public string CardNumber { get; set; }
        public string ZoneDesc { get; set; }
        public string CircuitDesc { get; set; }
        public Int32 ParkCode { get; set; }
        public string Park { get; set; }
        public Int32 MeterCode { get; set; }
        public string MeterDesc { get; set; }
        public string Address { get; set; }
        public string Type { get; set; }
        public string UserType { get; set; }
        public string TotalDuration { get; set; }
        public string PaidDuration { get; set; }
        public string FreeDuration { get; set; }
        public Int32 TotalDurationinMins { get; set; }
        public Int32 PaidDurationinMins { get; set; }
        public Int32 FreeDurationinMins { get; set; }
        public string SpaceCount { get; set; }
        public string PlateNumber { get; set; }

    }
}
