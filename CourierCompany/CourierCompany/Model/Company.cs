using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourierCompany.Model
{
    /// <summary>
    /// Курьерская компания
    /// </summary>
    public class Company
    {
        /// <summary>
        /// Цена за единицу расстояния пешего курьера
        /// </summary>
        public const double PricePerDistance = 100;
        
        /// <summary>
        /// Скорость пешего курьера
        /// </summary>
        public const double SpeedOnFoot = 2;

        /// <summary>
        /// Скорость курьера на самокате
        /// </summary>
        public const double SpeedOnScooter = 4;
        
        /// <summary>
        /// Наименование курьерской компании
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Курьеры компании
        /// </summary>
        public List<Courier> Couriers { get; set; }
        
        /// <summary>
        /// Очередь заказов компании
        /// </summary>
        public List<Order> Orders { get; set; }

    }
}
