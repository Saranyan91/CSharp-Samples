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


namespace CSVtoDatabase
{
    class ReadandWrite
    {
        static void Main(string[] args)
        {
            string SQLServerConnectionString = "Data Source=localhost;Initial Catalog=TestingDatabase;Integrated Security=True";

            string CSVPath = @"D:\OneDrive - Premium Parking Service, LLC\Saran's Projects\Data Warehousing\Parkeon";
            string CSVFileConnectionString = String.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};;Extended Properties=\"text;HDR=Yes;FMT=Delimited\";", CSVPath);

            var AllFiles = new DirectoryInfo(CSVPath).GetFiles("*.csv");
            string File_Name = string.Empty;

            foreach (var file in AllFiles)
            {
                try
                {
                    DataTable dt = new DataTable();
                    using (OleDbConnection con = new OleDbConnection(CSVFileConnectionString))
                    {

                    }
                 
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            Console.ReadLine();
        }
    }
}
