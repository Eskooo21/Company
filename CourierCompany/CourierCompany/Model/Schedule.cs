using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourierCompany.Model
{
    /// <summary>
    /// График работы
    /// </summary>
    public class Schedule
    {
        /// <summary>
        /// Время работы с
        /// </summary>
        public TimeSpan WorkingHoursFrom { get; set; }
        /// <summary>
        /// Время работы по
        /// </summary>
        public TimeSpan WorkingHoursTo { get; set; }
        /// <summary>
        /// Рабочие дни недели
        /// </summary>
        public List<DayOfWeek> WorkinDaysOfWeek { get; set; }
    }
}
