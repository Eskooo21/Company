using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourierCompany.Model
{
    /// <summary>
    /// Местоположение
    /// </summary>
    public class Location /*: ZCoordinates*/
    {
        /// <summary>
        /// Координата по оси Х
        /// </summary>
        public double Abscissa { get; set; }
        /// <summary>
        /// Координата по оси Y
        /// </summary>
        public double Ordinate { get; set; }

        public Location()
        {
        }

        /// <summary>
        /// Создает новое местоположение по координатам
        /// </summary>
        /// <param name="abscissa"></param>
        /// <param name="ordinate"></param>
        public Location(double abscissa, double ordinate)
        {
            Abscissa = abscissa;
            Ordinate = ordinate;
        }

        public override string ToString()
        {
            return $"({Abscissa}, {Ordinate})";
        }
    }
}
