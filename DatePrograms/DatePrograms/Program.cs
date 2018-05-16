using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatePrograms
{
    class Program
    {
        static void Main(string[] args)
        {
            ConsoleKeyInfo cki;
            while (true)
            {
                //Program obj = new Program();
                //obj.readdata();
                //obj.leap();

                DateFormats df = new DateFormats();
                df.DateFormat();

                cki = Console.ReadKey(true);
                if (cki.Key == ConsoleKey.X) break;

            }
            
        }
        int y;
        public void readdata()
        {
            Console.WriteLine("Enter the Year in Four Digits :");
            y = Convert.ToInt32(Console.ReadLine());
        }
        public void leap()
        {
            if((y%4==0 && y%100!=0)||(y%400 == 0))
            {
                Console.WriteLine("{0} is a Leap Year", y);
            }
            else
            {
                Console.WriteLine("{0} is not a Leap Year", y);
            }
        }
    }
}
