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
        static string upsertCommand = @"IF NOT EXISTS (SELECT 1 FROM [Parkeon].[ParkeonTransactions] WHERE [SystemID] = @SystemID)
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
        static string SQLServerConnectionString = "Data Source=localhost;Initial Catalog=TestingDatabase;Integrated Security=True";

        static void Main(string[] args)
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
                        var csvQuery = string.Format("select * from [{0}]", file.Name);
                        using (OleDbDataAdapter da = new OleDbDataAdapter(csvQuery, con))
                        {
                            da.Fill(dt);
                        }
                    }
                    //Console.WriteLine(dt.Rows[0][0].GetType());
                    foreach(DataColumn dataColumn in dt.Columns)
                    {
                        Console.WriteLine(dataColumn.ColumnName.ToString());
                    }
                    foreach (DataRow dataRow in dt.Rows)
                    {
                        //Console.WriteLine(dataRow["Amount"].ToString() + dataRow["Meter Code"].ToString());
                        var trans = DataTabletoTransactions(dataRow);
                        UpsertParkeonTransaction(trans);

                    }


                    //using (sqlbulkcopy bulkcopy = new sqlbulkcopy(sqlserverconnectionstring))
                    //{
                    //    bulkcopy.columnmappings.add(0, "mygroup");
                    //    bulkcopy.columnmappings.add(1, "id");
                    //    bulkcopy.columnmappings.add(2, "name");
                    //    bulkcopy.columnmappings.add(3, "address");
                    //    bulkcopy.columnmappings.add(4, "country");
                    //    bulkcopy.destinationtablename = "allemployees";
                    //    bulkcopy.batchsize = 0;
                    //    bulkcopy.writetoserver(dt);
                    //    bulkcopy.close();
                    //}

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            Console.ReadLine();

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
            try
            {

                using (var connection = new SqlConnection(SQLServerConnectionString))
                {
                    connection.Open();


                    using (SqlCommand cmd = new SqlCommand(upsertCommand, connection))
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
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("InsertStripeTransaction error", ex);
            }
        }
    }
}
