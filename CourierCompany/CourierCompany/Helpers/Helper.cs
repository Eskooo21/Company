using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using CourierCompany.Model;

namespace CourierCompany.Helpers
{
    public static class Helper
    {
        /// <summary>
        /// Расчет растояния
        /// </summary>
        /// <param name="location1"></param>
        /// <param name="location2"></param>
        /// <returns></returns>
        public static double CalculationDistance(Location location1, Location location2)
        {
            return Math.Sqrt(Math.Pow(location2.Abscissa - location1.Abscissa, 2) +
                             Math.Pow(location2.Ordinate - location1.Ordinate, 2));
        }

        public static void Log(ConsoleColor color, string message)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }

    }
    
}
