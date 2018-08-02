using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVtoDatabase2.Entities
{
    public class FreshDeskTickets

    {
        public Int32 TicketID { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public string Source { get; set; }
        public string Type { get; set; }
        public string Company { get; set; }
        public string RequesterName { get; set; }
        public string RequesterEmail { get; set; }
        public string RequesterPhone { get; set; }
        public string FacebookProfileID { get; set; }
        public string Agent { get; set; }
        public string Group { get; set; }
        public string CreatedTime { get; set; }
        public string DueByTime { get; set; }
        public string ResolvedTime { get; set; }
        public string ClosedTime { get; set; }
        public string LastUpdatedTime { get; set; }
        public string InitialResponseTime { get; set; }
        public string TimeTracked { get; set; }
        public string FirstResponseTimeinHrs { get; set; }
        public string ResolutionTimeinHrs { get; set; }
        public string AgentInteractions { get; set; }
        public string CustomerInteractions { get; set; }
        public string ResolutionStatus { get; set; }
        public string FirstResponseStatus { get; set; }
        public string Tags { get; set; }
        public string SurveyResult { get; set; }
        public string Product { get; set; }
        public string MarketRegion { get; set; }
        public string LocationNumber { get; set; }
        public string CompanyToBill { get; set; }
        public string EntityType { get; set; }
        public string Entity { get; set; }
        public string BillByPriority { get; set; }
        public string URLReferenceLink { get; set; }
        public string DelegateTeam { get; set; }
        public string Delegate { get; set; }
        public string InventoryUsed { get; set; }

    }
}
