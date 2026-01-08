using System;

namespace SimpleHelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine();
            Console.WriteLine("Hello, World!");

            // Interpolated string for current time
            DateTime now = DateTime.Now;
            Console.WriteLine($"The current time is {now:M/d/yyyy h:mm:ss tt}.");

            // Calculate days until next Christmas
            DateTime today = DateTime.Today;
            int year = today.Month == 12 && today.Day > 25 ? today.Year + 1 : today.Year;
            DateTime christmas = new DateTime(year, 12, 25);
            int daysUntilChristmas = (christmas - today).Days;

            // Interpolated string for Christmas countdown
            Console.WriteLine($"There are {daysUntilChristmas} days until the next Christmas.");
            Console.WriteLine();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            
        }
    }
}