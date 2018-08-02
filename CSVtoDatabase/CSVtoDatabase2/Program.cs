using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Configuration;
using CSVtoDatabase2.Entities;

namespace CSVtoDatabase2
{
    public static class Program
    {
        static string SQLServerConnectionString = "Data Source=premiumsql.database.windows.net;Initial Catalog=ParkingAnalytics;User ID=premiumadmin;Password=Japonica844";

        static void Main(string[] args)
        {
            ParkeonRun();
            //FreshDeskRun();
            Console.ReadLine();
        }

        public static void ParkeonRun()
        {
            
            string CSVpath = @"D:\OneDrive - Premium Parking Service, LLC\Saran's Projects\Data Warehousing\Parkeon"; // CSV file Path
            string CSVFileConnectionString = String.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};;Extended Properties=\"text;HDR=Yes;FMT=Delimited\";", CSVpath);

            var AllFiles = new DirectoryInfo(CSVpath).GetFiles("*.CSV");
            string File_Name = string.Empty;

            foreach (var file in AllFiles)
            {
                try
                {
                    DataTable dt = new DataTable();
                    using (OleDbConnection con = new OleDbConnection(CSVFileConnectionString))
                    {
                        con.Open();
                        Console.WriteLine(file.Name.ToString());
                        var csvQuery = string.Format("select * from [{0}]", file.Name);
                        using (OleDbDataAdapter da = new OleDbDataAdapter(csvQuery, con))
                        {
                            da.Fill(dt);
                        }
                    }
                    //Console.WriteLine(dt.Rows[0][0].GetType());
                    foreach (DataColumn dataColumn in dt.Columns)
                    {
                        Console.WriteLine(dataColumn.ColumnName.ToString());
                    }
                    foreach (DataRow dataRow in dt.Rows)
                    {
                        //Console.WriteLine(dataRow["Amount"].ToString() + dataRow["Meter Code"].ToString());
                        var trans = DataTabletoTransactions(dataRow);
                        UpsertParkeonTransaction(trans);

                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

        }

        static ParkeonTransactions DataTabletoTransactions(DataRow datat)
        {
            var trans = new ParkeonTransactions()
            {
                SystemID = Int32.Parse(datat["System ID"].ToString()),
                PrintedID = Int32.Parse(datat["Printed ID"].ToString()),
                ServerTime = DateTime.Parse(datat["Server Time"].ToString()),
                TerminalDate = DateTime.Parse(datat["Terminal Date"].ToString()),
                EndDate = DateTime.Parse(datat["End Date"].ToString()),
                PaymentType = datat[0].ToString(),
                Amount = Decimal.Parse(datat["Amount"].ToString()),
                Currency = datat["Currency"].ToString(),
                CardType = datat["Card Type"].ToString(),
                CardNumber = datat["Card #:"].ToString(),
                ZoneDesc = datat["Zone Desc"].ToString(),
                CircuitDesc = datat["Circuit Desc"].ToString(),
                ParkCode = Int32.Parse(datat["Park Code"].ToString()),
                Park = datat["Park"].ToString(),
                MeterCode = Int32.Parse(datat["Meter Code"].ToString()),
                MeterDesc = datat["Meter Description"].ToString(), 
                Address = datat["Address"].ToString(), 
                Type = datat["Type"].ToString(), 
                UserType = datat["User Type:"].ToString(),
                TotalDuration = datat["Total Duration"].ToString(),
                PaidDuration = datat["Paid Duration"].ToString(),
                FreeDuration = datat["Free Duration:"].ToString(),
                TotalDurationinMins = Int32.Parse(datat["Total Duration in mins"].ToString()) ,
                PaidDurationinMins = Int32.Parse(datat["Paid Duration in mins"].ToString()),
                FreeDurationinMins = Int32.Parse(datat["Free Duration in mins"].ToString()),
                SpaceCount = datat["Space #"].ToString(),
                PlateNumber = datat["Plate #"].ToString()
            };
            return trans;
        }

        public static void UpsertParkeonTransaction(ParkeonTransactions trans)
        {
            string ParkeonupsertCommand = @"IF NOT EXISTS (SELECT 1 FROM [Parkeon].[ParkeonTransactions] WHERE [SystemID] = @SystemID)
BEGIN
  INSERT INTO [Parkeon].[ParkeonTransactions]
	 ([SystemID]
      ,[PrintedID]
      ,[ServerTime]
      ,[TerminalDate]
           ,[EndDate]
           ,[PaymentType]
           ,[Amount]
           ,[Currency]
           ,[BankingID]
           ,[CardType]
           ,[CardNumber]
           ,[ZoneDesc]
           ,[CircuitDesc]
           ,[ParkCode]
           ,[Park]
           ,[MeterCode]
           ,[MeterDesc]
           ,[Address]
           ,[Type]
           ,[UserType]
           ,[TotalDuration]
           ,[PaidDuration]
           ,[FreeDuration]
           ,[TotalDurationinMins]
           ,[PaidDurationinMins]
           ,[FreeDurationinMins]
           ,[SpaceCount]
           ,[Plate#])
  VALUES 
	  (@SystemID
	 ,@PrintedID 
	 ,@ServerTime 
	 ,@TerminalDate
	 ,@EndDate
	 ,@PaymentType
	 ,@Amount
	 ,@Currency 
	 ,@BankingID
	 ,@CardType
	 ,@CardNumber
	 ,@ZoneDesc 
	 ,@CircuitDesc
	 ,@ParkCode
	 ,@Park 
	 ,@MeterCode 
	 ,@MeterDesc
	 ,@Address
	 ,@Type
	 ,@UserType
     ,@TotalDuration
     ,@PaidDuration
     ,@FreeDuration
     ,@TotalDurationinMins
     ,@PaidDurationinMins
     ,@FreeDurationinMins
     ,@SpaceCount
     ,@PlateNumber
)
END
ELSE
BEGIN
UPDATE [Parkeon].[ParkeonTransactions]
	SET [PrintedID] = @PrintedID
      ,[ServerTime] = @ServerTime
      ,[TerminalDate] = @TerminalDate
           ,[EndDate] = @EndDate
           ,[PaymentType] = @PaymentType
           ,[Amount] = @Amount
           ,[Currency] = @Currency
           ,[BankingID] = @BankingID
           ,[CardType] = @CardType
           ,[CardNumber] = @CardNumber
           ,[ZoneDesc] = @ZoneDesc
           ,[CircuitDesc] = @CircuitDesc
           ,[ParkCode] = @ParkCode
           ,[Park] = @Park
           ,[MeterCode] = @MeterCode
           ,[MeterDesc] = @MeterDesc
           ,[Address] = @Address
           ,[Type] = @Type
           ,[UserType] = @UserType
           ,[TotalDuration] = @TotalDuration
           ,[PaidDuration] = @PaidDuration
           ,[FreeDuration] = @FreeDuration
           ,[TotalDurationinMins] = @TotalDurationinMins
           ,[PaidDurationinMins] = @PaidDurationinMins
           ,[FreeDurationinMins] = @FreeDurationinMins
           ,[SpaceCount] = @SpaceCount
           ,[Plate#] = @PlateNumber
           ,[record_updated_date] = @RecordUpdatedAt
WHERE [SystemID]  = @SystemID
END";
            try
            {

                using (var connection = new SqlConnection(SQLServerConnectionString))
                {
                    connection.Open();


                    using (SqlCommand cmd = new SqlCommand(ParkeonupsertCommand, connection))
                    {
                        cmd.Parameters.Add("@SystemID", SqlDbType.Int).Value = trans.SystemID;
                        cmd.Parameters.Add("@PrintedID", SqlDbType.Int).Value = trans.PrintedID;
                        cmd.Parameters.Add("@ServerTime ", SqlDbType.DateTime2).Value = trans.ServerTime;
                        cmd.Parameters.Add("@TerminalDate", SqlDbType.DateTime2).Value = trans.TerminalDate;
                        cmd.Parameters.Add("@EndDate", SqlDbType.DateTime2).Value = trans.EndDate;
                        cmd.Parameters.Add("@PaymentType", SqlDbType.NVarChar, 255).Value = trans.PaymentType ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Amount", SqlDbType.Decimal).Value = trans.Amount ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Currency", SqlDbType.NVarChar, 10).Value = trans.Currency ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@BankingID", SqlDbType.Int).Value = trans.BankingID;
                        cmd.Parameters.Add("@CardType", SqlDbType.NVarChar,50).Value = trans.CardType ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@CardNumber", SqlDbType.NVarChar, 25).Value = trans.CardNumber ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@ZoneDesc", SqlDbType.NVarChar, 255).Value = trans.ZoneDesc ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@CircuitDesc", SqlDbType.NVarChar, 255).Value = trans.CircuitDesc ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@ParkCode", SqlDbType.Int).Value = trans.ParkCode;
                        cmd.Parameters.Add("@Park", SqlDbType.NVarChar, 255).Value = trans.Park ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@MeterCode", SqlDbType.Int).Value = trans.MeterCode;
                        cmd.Parameters.Add("@MeterDesc", SqlDbType.NVarChar, 255).Value = trans.MeterDesc ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Address", SqlDbType.NVarChar, 255).Value = trans.Address ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Type", SqlDbType.NVarChar, 255).Value = trans.Type ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@UserType", SqlDbType.NVarChar, 50).Value = trans.UserType ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@TotalDuration", SqlDbType.NVarChar, 128).Value = trans.TotalDuration ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@PaidDuration", SqlDbType.NVarChar, 128).Value = trans.PaidDuration ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@FreeDuration", SqlDbType.NVarChar, 128).Value = trans.FreeDuration ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@TotalDurationinMins", SqlDbType.Int).Value = trans.TotalDurationinMins;
                        cmd.Parameters.Add("@PaidDurationinMins", SqlDbType.Int).Value = trans.PaidDurationinMins;
                        cmd.Parameters.Add("@FreeDurationinMins", SqlDbType.Int).Value = trans.FreeDurationinMins;
                        cmd.Parameters.Add("@SpaceCount", SqlDbType.NVarChar, 50).Value = trans.SpaceCount ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@PlateNumber", SqlDbType.NVarChar, 255).Value = trans.PlateNumber ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@RecordUpdatedAt", SqlDbType.DateTime2).Value = DateTime.UtcNow;
                        var resultCount = cmd.ExecuteNonQuery();
                        Console.WriteLine("RowsAffected: {0}", resultCount);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("InsertParkeonTransaction error", ex);
            }
        }

        public static void FreshDeskRun()
        {
            string CSVpath = @"D:\OneDrive - Premium Parking Service, LLC\Saran's Projects\Data Warehousing\Freshdesk_integration"; // CSV file Path
            string CSVFileConnectionString = String.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};;Extended Properties=\"text;HDR=Yes;FMT=Delimited\";", CSVpath);

            var AllFiles = new DirectoryInfo(CSVpath).GetFiles("*.CSV");
            string File_Name = string.Empty;

            foreach (var file in AllFiles)
            {
                try
                {
                    DataTable dt = new DataTable();
                    using (OleDbConnection con = new OleDbConnection(CSVFileConnectionString))
                    {
                        con.Open();
                        var csvQuery = string.Format("select * from [{0}]", file.Name);
                        using (OleDbDataAdapter da = new OleDbDataAdapter(csvQuery, con))
                        {
                            da.Fill(dt);
                        }
                    }
                    //Console.WriteLine(dt.Rows[0][0].GetType());
                    foreach (DataColumn dataColumn in dt.Columns)
                    {
                        Console.WriteLine(dataColumn.ColumnName.ToString());
                    }
                    foreach (DataRow dataRow in dt.Rows)
                    {
                        //Console.WriteLine(dataRow["Created Time"].GetType());
                        //Console.WriteLine(dataRow["Amount"].ToString() + dataRow["Meter Code"].ToString());
                        var trans = FreshDeskDataTabletoTransactions(dataRow);
                        UpsertFreshDeskTickets(trans);

                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            
        }

        static FreshDeskTickets FreshDeskDataTabletoTransactions(DataRow datat)
        {
            var trans = new FreshDeskTickets()
            {
                TicketID = Int32.Parse(datat["Ticket Id"].ToString()),
                Subject = CheckStringEmptyorNull(datat["Subject"].ToString()),
                Description = CheckStringEmptyorNull(datat["Description"].ToString()),
                Status = CheckStringEmptyorNull(datat["Status"].ToString()),
                Priority = CheckStringEmptyorNull(datat["Priority"].ToString()),
                Source = CheckStringEmptyorNull(datat["Source"].ToString()),
                Type = CheckStringEmptyorNull(datat["Type"].ToString()),
                Company = CheckStringEmptyorNull(datat["Company"].ToString()),
                RequesterName = CheckStringEmptyorNull(datat["Requester Name"].ToString()),
                RequesterEmail = CheckStringEmptyorNull(datat["Requester Email"].ToString()),
                RequesterPhone = CheckStringEmptyorNull(datat["Requester Phone"].ToString()),
                FacebookProfileID = CheckStringEmptyorNull(datat["Facebook Profile Id"].ToString()),
                Agent = CheckStringEmptyorNull(datat["Agent"].ToString()),
                Group = CheckStringEmptyorNull(datat["Group"].ToString()),
                CreatedTime = CheckStringEmptyorNull(datat["Created Time"].ToString()),
                DueByTime = CheckStringEmptyorNull(datat["Due by Time"].ToString()),
                ResolvedTime = CheckStringEmptyorNull(datat["Resolved Time"].ToString()),
                ClosedTime = CheckStringEmptyorNull(datat["Closed Time"].ToString()),
                LastUpdatedTime = CheckStringEmptyorNull(datat["Last Updated Time"].ToString()),
                InitialResponseTime = CheckStringEmptyorNull(datat["Initial Response Time"].ToString()),
                AgentInteractions = CheckStringEmptyorNull(datat["Agent interactions"].ToString()),
                CustomerInteractions = CheckStringEmptyorNull(datat["Customer interactions"].ToString()),
                ResolutionStatus = CheckStringEmptyorNull(datat["Resolution Status"].ToString()),
                FirstResponseStatus = CheckStringEmptyorNull(datat["First Response Status"].ToString()),
                Tags = CheckStringEmptyorNull(datat["Tags"].ToString()),
                SurveyResult = CheckStringEmptyorNull(datat["Survey Result"].ToString()),
                Product = CheckStringEmptyorNull(datat["Product"].ToString()) ,
                MarketRegion = CheckStringEmptyorNull(datat["Market/Region"].ToString()), 
                LocationNumber = CheckStringEmptyorNull(datat["Location #"].ToString()) ,
                CompanyToBill = CheckStringEmptyorNull(datat["Company to Bill"].ToString()),
                EntityType = CheckStringEmptyorNull(datat["Entity Type"].ToString()),
                Entity = CheckStringEmptyorNull(datat["Entity"].ToString()),
                BillByPriority = CheckStringEmptyorNull(datat["Bill by Priority"].ToString()),
                URLReferenceLink = CheckStringEmptyorNull(datat["URL Reference Link"].ToString()),
                DelegateTeam = CheckStringEmptyorNull(datat["Delegate Team"].ToString()),
                Delegate = CheckStringEmptyorNull(datat["Delegate"].ToString()),
                InventoryUsed = CheckStringEmptyorNull(datat["Inventory Used"].ToString())

            };
            return trans;
        }

        public static void UpsertFreshDeskTickets(FreshDeskTickets trans)
        {
            string FreshDeskupsertCommand = @"IF NOT EXISTS (SELECT 1 FROM [freshdesk].[Tickets] WHERE [Ticket_Id] = @TicketID)
BEGIN
  INSERT INTO [freshdesk].[Tickets]
	 ([Ticket_Id]
      ,[Subject]
      ,[Description]
      ,[Status]
           ,[Priority]
           ,[Source]
           ,[Type]
           ,[Company]
           ,[Requester_Name]
           ,[Requester_Email]
           ,[Requester_Phone]
           ,[Facebook_Profile_Id]
           ,[Agent]
           ,[Group]
           ,[Created_Time]
           ,[Due_by_Time]
           ,[Resolved_Time]
           ,[Closed_Time]
           ,[Last_Updated_Time]
           ,[Initial_Response_Time]
           ,[Agent_interactions]
           ,[Customer_interactions]
           ,[Resolution_Status]
           ,[First_Response_Status]
           ,[Tags]
           ,[Survey_Result]
           ,[Product]
           ,[Market_Region]
           ,[Location#]
           ,[Company_to_Bill]
           ,[Entity_Type]
           ,[Entity]
           ,[Bill_by_Priority]
           ,[URL_Reference_Link]
           ,[Delegate_Team]
           ,[Delegate]
           ,[Inventory_Used]
           )
  VALUES 
	  (@TicketID
	 ,@Subject 
	 ,@Description 
	 ,@Status
	 ,@Priority
	 ,@Source
	 ,@Type
	 ,@Company 
	 ,@RequesterName
	 ,@RequesterEmail
	 ,@RequesterPhone
	 ,@FacebookProfileID
	 ,@Agent
	 ,@Group
	 ,@CreatedTime
	 ,@DueByTime
	 ,@ResolvedTime
	 ,@ClosedTime
	 ,@LastUpdatedTime
	 ,@InitialResponseTime
     ,@AgentInteractions
     ,@CustomerInteractions
     ,@ResolutionStatus
     ,@FirstResponseStatus
     ,@Tags
     ,@SurveyResult
     ,@Product
     ,@MarketRegion
     ,@LocationNumber
     ,@CompanyToBill
     ,@EntityType
     ,@Entity
     ,@BillByPriority
     ,@URLReferenceLink
     ,@DelegateTeam
     ,@Delegate
     ,@InventoryUsed
)
END
ELSE
BEGIN
UPDATE [freshdesk].[Tickets]
	SET [Subject] = @Subject
      ,[Description] = @Description 
      ,[Status] = @Status
           ,[Priority] = @Priority
           ,[Source] = @Source
           ,[Type] = @Type
           ,[Company] = @Company 
           ,[Requester_Name] = @RequesterName
           ,[Requester_Email] = @RequesterEmail
           ,[Requester_Phone] = @RequesterPhone
           ,[Facebook_Profile_Id] = @FacebookProfileID
           ,[Agent] = @Agent
           ,[Group] = @Group
           ,[Created_Time] = @CreatedTime
           ,[Due_by_Time] = @DueByTime
           ,[Resolved_Time] = @ResolvedTime
           ,[Closed_Time] = @ClosedTime
           ,[Last_Updated_Time] = @LastUpdatedTime
           ,[Initial_Response_Time] = @InitialResponseTime
           ,[Agent_interactions] = @AgentInteractions
           ,[Customer_interactions] = @CustomerInteractions
           ,[Resolution_Status] =  @ResolutionStatus
           ,[First_Response_Status] = @FirstResponseStatus
           ,[Tags] = @Tags
           ,[Survey_Result] = @SurveyResult
           ,[Product] = @Product
           ,[Market_Region] = @MarketRegion
           ,[Location#] = @LocationNumber
           ,[Company_to_Bill] = @CompanyToBill
           ,[Entity_Type] = @EntityType
           ,[Entity] = @Entity
           ,[Bill_by_Priority] = @BillByPriority
           ,[URL_Reference_Link] = @URLReferenceLink
           ,[Delegate_Team] = @DelegateTeam
           ,[Delegate] = @Delegate
           ,[Inventory_Used] = @InventoryUsed
           ,[record_updated_date] = @RecordUpdatedAt
WHERE [Ticket_Id]  = @TicketID
END";
            try
            {

                using (var connection = new SqlConnection(SQLServerConnectionString))
                {
                    connection.Open();


                    using (SqlCommand cmd = new SqlCommand(FreshDeskupsertCommand, connection))
                    {
                        cmd.Parameters.Add("@TicketID", SqlDbType.Int).Value = trans.TicketID;
                        cmd.Parameters.Add("@Subject", SqlDbType.NVarChar).Value = trans.Subject ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Description", SqlDbType.NVarChar).Value = trans.Description ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Status", SqlDbType.NVarChar).Value = trans.Status ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Priority", SqlDbType.NVarChar).Value = trans.Priority ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Source", SqlDbType.NVarChar).Value = trans.Source ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Type", SqlDbType.NVarChar).Value = trans.Type ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Company", SqlDbType.NVarChar).Value = trans.Company ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@RequesterName", SqlDbType.NVarChar).Value = trans.RequesterName ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@RequesterEmail", SqlDbType.NVarChar).Value = trans.RequesterEmail ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@RequesterPhone", SqlDbType.NVarChar).Value = trans.RequesterPhone ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@FacebookProfileID", SqlDbType.NVarChar).Value = trans.FacebookProfileID ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Agent", SqlDbType.NVarChar).Value = trans.Agent ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Group", SqlDbType.NVarChar).Value = trans.Group ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@CreatedTime", SqlDbType.DateTime2).Value = trans.CreatedTime ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@DueByTime", SqlDbType.DateTime2).Value = trans.DueByTime ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@ResolvedTime", SqlDbType.DateTime2).Value = trans.ResolvedTime ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@ClosedTime", SqlDbType.DateTime2).Value = trans.ClosedTime ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@LastUpdatedTime", SqlDbType.DateTime2).Value = trans.LastUpdatedTime ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@InitialResponseTime", SqlDbType.DateTime2).Value = trans.InitialResponseTime ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@AgentInteractions", SqlDbType.NVarChar).Value = trans.AgentInteractions ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@CustomerInteractions", SqlDbType.NVarChar).Value = trans.CustomerInteractions ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@ResolutionStatus", SqlDbType.NVarChar).Value = trans.ResolutionStatus ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@FirstResponseStatus", SqlDbType.NVarChar).Value = trans.FirstResponseStatus ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Tags", SqlDbType.NVarChar).Value = trans.Tags ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@SurveyResult", SqlDbType.NVarChar).Value = trans.SurveyResult ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Product", SqlDbType.NVarChar).Value = trans.Product ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@MarketRegion", SqlDbType.NVarChar).Value = trans.MarketRegion ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@LocationNumber", SqlDbType.NVarChar).Value = trans.LocationNumber ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@CompanyToBill", SqlDbType.NVarChar).Value = trans.CompanyToBill ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@EntityType", SqlDbType.NVarChar).Value = trans.EntityType ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Entity", SqlDbType.NVarChar).Value = trans.Entity ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@BillByPriority", SqlDbType.NVarChar).Value = trans.BillByPriority ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@URLReferenceLink", SqlDbType.NVarChar).Value = trans.URLReferenceLink ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@DelegateTeam", SqlDbType.NVarChar).Value = trans.DelegateTeam ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@Delegate", SqlDbType.NVarChar).Value = trans.Delegate ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@InventoryUsed", SqlDbType.NVarChar).Value = trans.InventoryUsed ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@RecordUpdatedAt", SqlDbType.DateTime2).Value = DateTime.UtcNow;
                        var resultCount = cmd.ExecuteNonQuery();
                        Console.WriteLine("RowsAffected: {0}", resultCount);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Insert Freshdesk Tickets error");
                Console.WriteLine(ex);
            }
        }

        public static String CheckStringEmptyorNull(String str)
    {
         if (String.IsNullOrEmpty(str))
            {
                return null;
            }
            else
            {
                return str;
            }
        }
 ///New Functions after this line
 ///

    }
}
