using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatePrograms
{
    class DateFormats
    {
        public int DateFormat()
        {
            DateTime date = new DateTime(2018, 6, 23);
            Console.WriteLine("Some Date Formats :");
            Console.WriteLine("Date and Time: {0}", date);
            Console.WriteLine(date.ToString("yyyy-MM-dd"));
            Console.WriteLine(date.ToString("dd-MMM-yy"));
            Console.WriteLine(date.ToString("M/d/yyyy"));
            Console.Read();
            return 0;
        }
    }
}
