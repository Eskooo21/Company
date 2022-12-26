using CourierCompany.Helpers;
using static System.String;


namespace CourierCompany.Model
{
    /// <summary>
    /// Заказ
    /// </summary>
    public class Order
    {
        /// <summary>
        /// Шаг переторговки
        /// </summary>
        private const double OvertrandingStep = 5.0;
        /// <summary>
        /// Наименование заказа
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Масса заказа
        /// </summary>
        public double Mass { get; set; }

        /// <summary>
        /// Максимальная стоимость
        /// </summary>
        public double? MaxCost { get; set; }
        //{
        //    get
        //    {
        //        return GetMaxOrderPrice();
        //    }
        //}
        /// <summary>
        /// Срок доставки
        /// </summary>
        public DateTime DeliveryPeriod { get; set; }
        /// <summary>
        /// Местоположение доставки заказа
        /// </summary>
        public Location ToLocation { get; set; }
        /// <summary>
        /// Первоночальное местоположение заказа
        /// </summary>
        public Location FromLocation { get; set; }
        /// <summary>
        /// Растояние от первоначального местоположения заказа до места доставки
        /// </summary>
        public double Distance
        {
            get
            {
                return Helper.CalculationDistance(ToLocation, FromLocation);
            }
            
        }

        public Company Company { get; set; }
        /// <summary>
        /// Расчет растояния от первоначального местоположения заказа до места доставки
        /// </summary>
        //public void CalculationFullDistance()
        //{
        //    Distance = Helper.Helper.CalculationFullDistance(ToLocation, FromLocation);
        //}

        /// <summary>
        /// Расчет максимальной стоимости заказа
        /// </summary>
        public double? GetMaxOrderPrice()
        {
            if (MaxCost == null)
            {
                MaxCost = Distance * Company.PricePerDistance;
            }

            return MaxCost;
        }

        
        //TODO Болванка
        public List<CourierAndCost> GetAvailableCouriers(List<Courier> allCouriers)
        {
            return allCouriers
                .Where( x => x.GetPossibilityCourier(this))
                .Select(x => new CourierAndCost{ Courier = x, Cost = x.CalculationCost(this)})
                .OrderBy( x => x.Cost)
                .Take(10)
                .ToList();
        }

        //ToDo уточнить
        /// <summary>
        /// Профит от заказа
        /// </summary>
        /// <param name="courier"></param>
        /// <returns></returns>
        public double GetProfit(Courier courier)
        {
            return MaxCost != null ? MaxCost.Value - courier.Price * courier.CalculationFullDistance(this) : 0;
        }

        /// <summary>
        /// Может произойти переторговка, если да, то меняет элемент расписания
        /// </summary>
        /// <param name="conflictScheduleItems"></param>
        /// <param name="currentCourier"></param>
        /// <param name="currentScheduleItem"></param>
        /// <returns></returns>
        public bool Overtrading(List<ScheduleItem> conflictScheduleItems, Courier currentCourier, ScheduleItem currentScheduleItem, Courier nextCourier, double delta)
        {
            Helper.Log(ConsoleColor.Yellow,
                $"Запуск процесса переторговки заказа {currentScheduleItem.Order.Name} (профит с курьером {currentCourier.Name} = {currentScheduleItem.Profit + delta}) с заказом {conflictScheduleItems[0].Order.Name} (профит с курьером {currentCourier.Name} = {conflictScheduleItems[0].Profit})");
            
            foreach (var scheduleItem in conflictScheduleItems)
            {
                var step = OvertrandingStep;
                var temp1 = this.GetProfit(currentCourier) + delta;
                var temp2 = this.GetProfit(nextCourier);
                var deltaOffer = this.GetProfit(currentCourier) + delta - this.GetProfit(nextCourier);
                var offerOvertrading = 0.0;
                while (step < deltaOffer)
                {
                    offerOvertrading += OvertrandingStep;

                    Helper.Log(ConsoleColor.Yellow, $"\t Заказ {Name} предлагает уступку = {offerOvertrading} у.е.");
                    
                    if (scheduleItem.Order.AgreementOvertrading(Company.Couriers, currentCourier, offerOvertrading))
                    {
                        var tempScheduleItem = currentCourier.ScheduleItems.Find(scheduleItem)?.Value;
                        var tempPreviousScheduleItem = currentCourier.ScheduleItems.Find(scheduleItem)?.Previous?.Value;
                        if (tempScheduleItem != null)
                        {
                            currentScheduleItem.Order.MaxCost -= offerOvertrading;
                            currentScheduleItem.Profit -= offerOvertrading;

                            Helper.Log(ConsoleColor.Yellow,
                                $"\t Заказ {Name} размещен у курьера {currentCourier.Name} с профитом {currentScheduleItem.Profit}");

                            if (tempPreviousScheduleItem != null)
                                currentCourier.ScheduleItems.AddAfter(currentCourier.ScheduleItems.Find(tempPreviousScheduleItem),
                                    currentScheduleItem);
                            else
                                currentCourier.ScheduleItems.AddFirst(currentScheduleItem);
                            currentCourier.ScheduleItems.Remove(tempScheduleItem);
                        }

                        return true;
                    }

                    step +=OvertrandingStep;
                }
            }

            Helper.Log(ConsoleColor.Yellow,
                $"Переторговка не удалась");
            return false;
        }

        /// <summary>
        /// Принятие переторговки
        /// </summary>
        /// <param name="allCouriers"></param>
        /// <param name="currentCourier"></param>
        /// <param name="offerOvertrading"></param>
        /// <returns></returns>
        //TODO изменение профита и смена эл расписания
        public bool AgreementOvertrading(List<Courier> allCouriers, Courier currentCourier, double offerOvertrading)
        {
            var arrCouriers = this.GetRemainingCouriers(allCouriers, currentCourier);
            if (arrCouriers == null)
            {
                Helper.Log(ConsoleColor.Cyan, $"\t Заказ {Name} отказывается из-за недостатка подходящих курьеров");
                
                return false;
            }
            foreach (var nextCourier in arrCouriers)
            {
                var deltaAgreement = this.GetProfit(currentCourier) - this.GetProfit(nextCourier);
                
                if (offerOvertrading > deltaAgreement)
                {
                    Helper.Log(ConsoleColor.Cyan,
                        $"\t Попытка размещения заказа {Name} у следующего курьера {nextCourier.Name}");
                    
                    var answer = nextCourier.ChangeSchedule(this, offerOvertrading);
                    if (answer.AnswerEnum == AnswerEnum.Ok)
                    {
                        this.MaxCost = this.GetMaxOrderPrice() + offerOvertrading;

                        Helper.Log(ConsoleColor.Cyan,
                            $"\t Заказ {Name} размещен у курьера {nextCourier.Name} с профитом = {answer.CurrentScheduleItem.Profit} у.е.");

                        return true;
                    }
                    else if (answer.AnswerEnum == AnswerEnum.BusyOneOrder)
                    {
                        if (this.Overtrading(answer.ConflictScheduleItems, currentCourier, answer.CurrentScheduleItem, nextCourier, offerOvertrading))
                        {
                            answer.CurrentScheduleItem.Order.MaxCost += offerOvertrading;

                            return true;
                        }
                        else continue;
                    }
                    else continue;
                }
            }

            Helper.Log(ConsoleColor.Cyan,
                $"\t Заказу {Name} не интересно");
            return false;
        }

        /// <summary>
        /// Получить оставшихся курьеров
        /// </summary>
        /// <param name="allCouriers"></param>
        /// <param name="currentCourier"></param>
        /// <returns></returns>
        public Courier[]? GetRemainingCouriers(List<Courier> allCouriers, Courier currentCourier)
        {
            var couriersAndCost = this.GetAvailableCouriers(allCouriers);
            var couriers = couriersAndCost
                .Select(x => x.Courier)
                .ToArray();
            var index = Array.IndexOf(couriers, currentCourier);
            if (index != couriers.Length - 1)
            {
                //Todo exeption
                var arrCouriers = new Courier[couriersAndCost.Count - index - 1];
                Array.Copy(couriers, index + 1, arrCouriers, 0, couriersAndCost.Count - index - 1);
                return arrCouriers;
            }

            return null;
        }

        //Todo Answer
        public bool Planning(List<CourierAndCost> couriersAndCost)
        {
            Helper.Log(ConsoleColor.Yellow,
                $"Начато планирование заказа {Name} из {FromLocation.ToString()} в {ToLocation.ToString()} ко времени {DeliveryPeriod} с ценой {MaxCost}");
            
            foreach (var courierAndCost in couriersAndCost)
            {
                var answer = courierAndCost.Courier.ChangeSchedule(this, 0);
                switch (answer.AnswerEnum)
                {
                    case AnswerEnum.Ok:

                        Helper.Log(ConsoleColor.Yellow,
                            $"Заказ {Name} запланирован у курьера {courierAndCost.Courier.Name}");
                        
                        return true;

                    case AnswerEnum.BusyOneOrder:
                        var index = couriersAndCost.IndexOf(courierAndCost);
                      
                        if (index != couriersAndCost.Count - 1
                            && this.Overtrading(answer.ConflictScheduleItems,
                                courierAndCost.Courier, answer.CurrentScheduleItem, couriersAndCost[index + 1].Courier, 0.0))
                        {
                            return true;
                        }
                        break;

                    case AnswerEnum.BusyManyOrder: 
                        continue;
                    
                    case AnswerEnum.No:
                        continue;

                    default:
                        continue;

                }
            }

            return false;
        }

        /// <summary>
        /// Информация о закзазе
        /// </summary>
        /// <returns></returns>
        public string GetInfo()
        {
            return Format("\t Заказ: {0}" +
                          " Из: {1}" +
                          " В: {2}" +
                          " Ко времени: {3}" +
                          " Масса {4}" +
                          " Стоимость {5}",
                Name, FromLocation, ToLocation, DeliveryPeriod, Mass, MaxCost);
        }

    }
}
