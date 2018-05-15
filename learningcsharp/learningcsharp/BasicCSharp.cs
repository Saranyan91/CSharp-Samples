using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace learningcsharp
{
    class BasicCSharp
    {

        public void CheckOddorEven()
        {
            int i;
            Console.Write("Enter a Number : ");
            i = int.Parse(Console.ReadLine());
            if (i % 2 == 0)
            {
                Console.Write("Entered Number is an Even Number");
                Console.Read();
            }
            else
            {
                Console.Write("Entered Number is Odd Number");
                Console.Read();
            }
        }

        public void SwapNumber()
        {
            int num1, num2, temp;
            Console.Write("Enter Number1:");
            num1 = int.Parse(Console.ReadLine());
            Console.Write("Enter Number2:");
            num2 = int.Parse(Console.ReadLine());
            temp = num1;
            num1 = num2;
            num2 = temp;
            Console.Write("\nAfter Swapping:");
            Console.Write("\nFirst Number: " + num1);
            Console.Write("\nSecond Number:" + num2);
            Console.Read();
        }

        public void TestReadLine()
        {
            string m1 = "\nType a string of text then press Enter. " + "Type '+' anywhere in the text to quit:\n";
            string m2 = "\n Character '{0}' is hexadecimal 0x{1:x4}.";
            string m3 = "\n Character       is hexadecimal 0x{0:x4}.";
            char ch;
            int x;

            //
            Console.WriteLine(m1);
            do
            {
                x = Console.Read();
                try
                {
                    ch = Convert.ToChar(x);
                    if (Char.IsWhiteSpace(ch))
                    {
                        Console.WriteLine(m3, x);
                        if (ch == 0x0a)
                            Console.WriteLine(m2, ch, x);
                    }
                    else
                        Console.WriteLine(m2, ch, x);
                }
                catch (OverflowException e)
                {
                    Console.WriteLine("{0} Value read = {1}.", e.Message, x);
                    ch = Char.MinValue;
                    Console.WriteLine(m1);
                }
            } while (ch != '+');
        }

        public int SumofDigits(int num)
        {
            int lnum = num, sum = 0, r;
            while(lnum != 0)
            {
                r = lnum % 10;
                lnum = lnum / 10;
                sum = sum + r;
            }
            return sum;
        }

        static void Main(string[] args)
        {
            ConsoleKeyInfo cki;
            int num;
            BasicCSharp dial = new BasicCSharp();
            while (true)
            {
                
                Console.WriteLine("Enter a Number : ");
                num = int.Parse(Console.ReadLine());
                Console.WriteLine("Sum of the digits in Number {0}, Press 'C' to Continue or 'X' to exit", dial.SumofDigits(num));
                cki = Console.ReadKey(true);
                if (cki.Key == ConsoleKey.X) break;
            }            
        }
    }
}
