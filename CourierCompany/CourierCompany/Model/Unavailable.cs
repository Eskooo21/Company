using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourierCompany.Model
{
    /// <summary>
    /// Недоступность
    /// </summary>
    public class Unavailable
    {
        /// <summary>
        /// Дата недоступности
        /// </summary>
        public DateTime UnavailableDate { get; set; }
        /// <summary>
        /// Недоступность с
        /// </summary>
        public TimeSpan UnavailableSince { get; set; }
        /// <summary>
        /// Недоступность по
        /// </summary>
        public TimeSpan UnavailableBy { get; set; }
    }
}
