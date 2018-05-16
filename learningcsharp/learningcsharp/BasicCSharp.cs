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

        public int ReverseNumber(int num)
        {
            int reverse = 0;
            while (num != 0)
            {
                reverse = reverse * 10;
                reverse = reverse + num % 10;
                num = num / 10;
            }
            return reverse;
        }

        public void BinaryTriangle(int input)
        {
            int p, lastInt = 0;
            for (int i = 1; i <= input; i++)
            {
                for (p = 1; p <= i; p++)
                {
                    if(lastInt == 1)
                    {
                        Console.Write("0");
                        lastInt = 0;
                    }
                    if(lastInt == 0)
                    {
                        Console.Write("1");
                        lastInt = 1;
                    }
                }Console.Write("\n");

            }
        }
        static void Main(string[] args)
        {
            ConsoleKeyInfo cki;
            int num;
            BasicCSharp dial = new BasicCSharp();
            while (true)
            {

                //Console.WriteLine("Enter a Number : ");
                //num = int.Parse(Console.ReadLine());
                //Console.WriteLine("Reverse of the Number {0} is {1}, Press 'C' to Continue or 'X' to exit", num, dial.ReverseNumber(num));
                Console.WriteLine("Enter the Number of Rows: ");
                num = int.Parse(Console.ReadLine());
                Console.WriteLine("Binary Triangle of {0} Rows", num);
                dial.BinaryTriangle(num);
                cki = Console.ReadKey(true);
                if (cki.Key == ConsoleKey.X) break;
            }            
        }
    }
}
