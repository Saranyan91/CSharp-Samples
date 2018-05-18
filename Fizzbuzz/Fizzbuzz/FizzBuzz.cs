using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fizzbuzz
{
    class FizzBuzz
    {
        static void Main(string[] args)
        {
            List<int> count3 = new List<int>();
            List<int> count5 = new List<int>();
            List<int> count35 = new List<int>();

            for (int i =1; i<= 100; i++)
            {
                string str = "";
                if (i % 3 == 0)
                {
                    str += "Fizz";
                    count3.Add(i);
                }

                if (i % 5 == 0)
                {
                    str += "Buzz";
                    count5.Add(i);
                }

                if (str.Length == 0)
                {
                    str = i.ToString();
                }

                if (str == "FizzBuzz")
                {
                    count35.Add(i);
                }
                Console.WriteLine(str);
                
            }
            count3.ForEach(item => Console.Write(item + ","));
            Console.WriteLine();
            count5.ForEach(item => Console.Write(item + ","));
            Console.WriteLine();
            count35.ForEach(item => Console.Write(item + ","));
            Console.WriteLine();
            Console.WriteLine("Average of Fizz {0}", count3.Average().ToString());
            
            Console.WriteLine("Average of Buzz {0}", count5.Average().ToString());
            Console.WriteLine("Average of FizzBuzz {0}", count35.Average().ToString());
            Console.ReadLine();
        }
    }
}
