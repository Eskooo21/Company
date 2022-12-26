using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Threading.Tasks;
using CourierCompany.Helpers;

namespace CourierCompany.Model
{
    /// <summary>
    /// Курьер
    /// </summary>
    public abstract class Courier
    {
        //private const double PricePerDistance = 1;
        //private const double PriceOnScooter = 2;

        /// <summary>
        /// Наименование курьера
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Грузоподъемность
        /// </summary>
        public double LoadCapacity { get; set; }

        /// <summary>
        /// Скорость курьера
        /// </summary>
        public double Speed { get; set; }

        /// <summary>
        /// Цена за единицу перемещения
        /// </summary>
        public double Price { get; set; }
        
        /// <summary>
        /// Местоположение курьера
        /// </summary>
        public Location Location { get; set; }

        /// <summary>
        /// График работы курьера
        /// </summary>
        public Schedule Schedule { get; set; }

        /// <summary>
        /// Недоступности курьера
        /// </summary>
        public List<Unavailable> Unavailables { get; set; }

        /// <summary>
        /// Расписание курьера
        /// </summary>
        public LinkedList<ScheduleItem> ScheduleItems { get; set; } = new LinkedList<ScheduleItem>();

        /// <summary>
        /// Проверка доступности курьера по графику работы с учетом недоступности
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public bool AvailabilityAccordingToWorkSchedule(Order order)
        {
            if (Schedule.WorkinDaysOfWeek.Contains(order.DeliveryPeriod.DayOfWeek)
                && Schedule.WorkingHoursFrom < order.DeliveryPeriod.TimeOfDay
                && Schedule.WorkingHoursTo > order.DeliveryPeriod.TimeOfDay)
            {
                if (Unavailables != null)
                {
                    foreach (var courierUnavailable in Unavailables)
                    {
                        if (courierUnavailable.UnavailableDate == order.DeliveryPeriod.Date
                            && courierUnavailable.UnavailableSince >= order.DeliveryPeriod.TimeOfDay
                            && courierUnavailable.UnavailableBy <= order.DeliveryPeriod.TimeOfDay)
                        {
                            Console.WriteLine(this.Name + " имеет недоступнопность");
                            return false;
                        }
                    }
                }

                return true;
            }
            else
            {
                Console.WriteLine(this.Name + " не соответствует график работы");
                return false;
            }
        }

        /// <summary>
        /// Может поднять вес посылки
        /// </summary>
        public bool CanLift(Order order)
        {
            if (this.LoadCapacity < order.Mass)
            {
                Helper.Log(ConsoleColor.Green, Name + " не может поднять заказ");
            }

            
            return this.LoadCapacity >= order.Mass;
        }

        /// <summary>
        /// Расчет стоимости заказа (предложение от курьера)
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public double CalculationCost(Order order)
        {
            return CalculationFullDistance(order) * Price;
        }
        //TODO Поменять текущую позицию курьера в зависимости от элемента расписания
        /// <summary>
        /// Расчет расстояния от курьера до первоначального места заказа и до места доставки
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public double CalculationFullDistance(Order order)
        {
            //TODO 
            return Helper.CalculationDistance(this.GetInitialLocation(order), order.FromLocation) + order.Distance;
            //return Helper.CalculationFullDistance(this.GetInitialLocation(order.ToScheduleItem(this)), order.FromLocation) + order.Distance;
        }

        //TODO Доделать текущее время и уточнить стр. 125
        /// <summary>
        /// Может доставить ко времени
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public bool CanDeliverOnTime(Order order)
        {
            var currentTime = new TimeSpan(10, 0, 0);
            return order.DeliveryPeriod.TimeOfDay - currentTime >
                   this.CalculationDeliveryTime(order);
        }

        /// <summary>
        /// Проверка возможности выполнения заказа курьером
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public bool GetPossibilityCourier(Order order)
        {
            return this.CanLift(order) && this.AvailabilityAccordingToWorkSchedule(order)/* && this.CanDeliverOnTime(order)*/;
        }

        /// <summary>
        /// Расчет времени доставки
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public TimeSpan CalculationDeliveryTime(Order order)
        {
            return TimeSpan.FromHours(CalculationFullDistance(order) / Speed);
        }

        /// <summary>
        /// Расчет максимального времени начала доставки (работы)
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public TimeSpan CalculationStartTime(Order order)
        {
            return order.DeliveryPeriod.TimeOfDay - this.CalculationDeliveryTime(order);
        }

        /// <summary>
        /// Получение текущего местоположения курьера
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public Location GetInitialLocation(Order order)
        {
            var location = Location;

            foreach (var scheduleItem in ScheduleItems)
            {
                if (order.DeliveryPeriod.TimeOfDay > scheduleItem.RightTime)
                    location = scheduleItem.EndLocation;
                else return location;
            }

            return location;
        }

        /// <summary>
        /// Может ли разместиться заказ, если да то размещает
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public Answer ChangeSchedule(Order order, double delta)
        {
            if (CanDeliverOnTime(order))
            {
                var scheduleItem = new ScheduleItem(order)
                {
                    Profit = order.GetProfit(this) + delta,
                    InitialLocation = this.GetInitialLocation(order),
                    LeftTime = this.CalculationStartTime(order),
                    CurrentCourier = this
                };

                if (ScheduleItems.Count == 0)
                {
                    ScheduleItems.AddFirst(scheduleItem);

                    Helper.Log(ConsoleColor.Green,
                        $"\t Курьер {Name} разместил заказ {order.Name} первым в своем расписании");
                    
                    return new Answer
                        { AnswerEnum = AnswerEnum.Ok, ConflictScheduleItems = null, CurrentScheduleItem = scheduleItem };
                }

                foreach (var item in ScheduleItems)
                {

                    if (order.DeliveryPeriod.TimeOfDay > item.RightTime
                        && (ScheduleItems.Find(item)?.Next?.Value == null
                            || order.DeliveryPeriod.TimeOfDay < ScheduleItems.Find(item)?.Next?.Value.LeftTime))
                    {
                        ScheduleItems.AddAfter(ScheduleItems.Find(item), scheduleItem);

                        Helper.Log(ConsoleColor.Green, $"\t Курьер {Name} разместил заказ {order.Name} в своем расписании");
                        
                        return new Answer { AnswerEnum = AnswerEnum.Ok, ConflictScheduleItems = null };
                    }

                }

                var conflictOrders = this.ConflictingOrders(order);
                if (conflictOrders.Count == 1)
                {

                    Helper.Log(ConsoleColor.Green,
                        $"\t Курьер {Name} передает заказу {order.Name}, что существует конфликтный элемент расписания {conflictOrders[0].Order.Name} с {conflictOrders[0].LeftTime} по {conflictOrders[0].RightTime}");
                    

                    return new Answer
                    {
                        AnswerEnum = AnswerEnum.BusyOneOrder,
                        ConflictScheduleItems = conflictOrders,
                        CurrentScheduleItem = scheduleItem
                    };
                }

                if (conflictOrders.Count > 1)
                {
                    Helper.Log(ConsoleColor.Green,
                        $"\t Курьер {Name} передает заказу {order.Name}, что существуют 2 или более конфликтных элементов расписания");
                    
                    return new Answer
                    {
                        AnswerEnum = AnswerEnum.BusyManyOrder,
                        ConflictScheduleItems = conflictOrders,
                        CurrentScheduleItem = scheduleItem
                    };
                }
            }

            Helper.Log(ConsoleColor.Green, Name + " не успевает ко времени");
           

            return new Answer { AnswerEnum = AnswerEnum.No, ConflictScheduleItems = null, CurrentScheduleItem = null };
        }

        public List<ScheduleItem> ConflictingOrders(Order order)
        {
            var conflictingOrders = new List<ScheduleItem>();
            var startTime = this.CalculationStartTime(order);
            foreach (var scheduleItem in ScheduleItems)
            {
                if (order.DeliveryPeriod.TimeOfDay >= scheduleItem.LeftTime
                    && order.DeliveryPeriod.TimeOfDay <= scheduleItem.RightTime
                    || startTime >= scheduleItem.LeftTime
                    && startTime <= scheduleItem.RightTime
                    || startTime < scheduleItem.LeftTime
                    && order.DeliveryPeriod.TimeOfDay > scheduleItem.RightTime)
                {
                    conflictingOrders.Add(scheduleItem);
                }
            }

            return conflictingOrders;

        }

        /// <summary>
        /// Информация о курьере
        /// </summary>
        /// <returns></returns>
        public string GetInfo()
        {
            return string.Format("\t Курьер: {0}" +
                                 " Скорость: {1}" +
                                 " Грузоподъемность: {2}" +
                                 " Стартовая позиция: {3}",
                Name, Speed, LoadCapacity, Location);
        }

        public void WriteScheduleItems()
        {
            Console.WriteLine(this.Name + " расписание");
            foreach (var scheduleItem in ScheduleItems)
                Console.WriteLine("\t{0} из {1} в {2} с {3} по {4} профит = {5}", scheduleItem.Order.Name,
                    scheduleItem.Order.FromLocation.ToString(), scheduleItem.Order.ToLocation.ToString(),
                    scheduleItem.LeftTime, scheduleItem.RightTime, scheduleItem.Profit);
        }

    }

    /// <summary>
    /// Курьер пеший
    /// </summary>
    public class CourierOnFoot : Courier
    {
        public CourierOnFoot()
        {
            Price = Company.PricePerDistance * 0.1;
            Speed = Company.SpeedOnFoot;
        }
    }

    /// <summary>
    /// Курьер на самокате
    /// </summary>
    public class CourierOnScooter : Courier
    {
        public CourierOnScooter()
        {
            Price = Company.PricePerDistance * 0.11;
            Speed = Company.SpeedOnScooter;
        }
    }

    /// <summary>
    /// Вспомогательный класс для ответа на заказ
    /// </summary>
    public class CourierAndCost
    {
        public Courier Courier { get; set; }
        public double Cost { get; set; }
        
    }

    public enum AnswerEnum
    {
        Ok,
        BusyOneOrder,
        BusyManyOrder,
        No,
    }

    public class Answer
    {
        public AnswerEnum AnswerEnum { get; set; }
        /// <summary>
        /// Конфликтные элементы расписания
        /// </summary>
        public List<ScheduleItem>? ConflictScheduleItems { get; set; }

        /// <summary>
        /// Текущий элемент расписания
        /// </summary>
        public ScheduleItem? CurrentScheduleItem { get; set; }
    }
}
