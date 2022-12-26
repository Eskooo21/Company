using CourierCompany.Model;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace CourierCompany
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        static void Main()
        {
            
            var courier1 = new CourierOnFoot
                { 
                    Name = "Катя", 
                    Location = new Location(0, 0), 
                    LoadCapacity = 10, 
                    Schedule = new Schedule
                    {
                        WorkinDaysOfWeek = new List<DayOfWeek>
                        {
                            DayOfWeek.Monday, 
                            DayOfWeek.Tuesday, 
                            DayOfWeek.Thursday, 
                            DayOfWeek.Wednesday, 
                            DayOfWeek.Friday

                        }, 
                        WorkingHoursFrom = new TimeSpan(10,00,00), 
                        WorkingHoursTo = new TimeSpan(19,00,00)

                    }

                };

            var courier2 = new CourierOnScooter()
            {
                Name = "Катюха", 
                Location = new Location(0, 11), 
                LoadCapacity = 10,
                Schedule = new Schedule
                {
                    WorkinDaysOfWeek = new List<DayOfWeek>
                    {
                        DayOfWeek.Monday,
                        DayOfWeek.Tuesday,
                        DayOfWeek.Thursday,
                        DayOfWeek.Wednesday,
                        DayOfWeek.Friday

                    },
                    WorkingHoursFrom = new TimeSpan(10, 00, 00),
                    WorkingHoursTo = new TimeSpan(19, 00, 00)

                }

            };


            
            //var courier3 = new CourierOnScooter
            //    { Name = "Катяха", Location = new Location(4, 1), LoadCapacity = 19 };
            //var courier4 = new CourierOnScooter
            //    { Name = "Катечка", Location = new Location(4, 9), LoadCapacity = 26 };
            
            List <Courier> couriers = new List<Courier>(){courier1, courier2};
            var company = new Company
            {
                Name = @"Курьерская компания Катюх ККК", 
                Couriers = couriers
            };

            var order1 = new Order
            {
                Name = "Заказ 1",
                Company = company,
                DeliveryPeriod = new DateTime(2022, 12, 21, 15, 00, 00),
                FromLocation = new Location(2, 0),
                ToLocation = new Location(2, 6),
                Mass = 5
            };

            var order2 = new Order
            {
                Name = "Заказ 2",
                Company = company,
                DeliveryPeriod = new DateTime(2022, 12, 21, 15, 00, 00),
                FromLocation = new Location(1, 0),
                ToLocation = new Location(1, 6),
                Mass = 5
            };



            //var order3 = new Order
            //{
            //    Name = "Заказ 3",
            //    Company = company,
            //    DeliveryPeriod = new DateTime(2022, 12, 21, 16, 00, 00),
            //    FromLocation = new Location(1, 1),
            //    ToLocation = new Location(5, 5),
            //    Mass = 5
            //};

            //var order4 = new Order
            //{
            //    Name = "Заказ 4",
            //    Company = company,
            //    DeliveryPeriod = new DateTime(2022, 12, 21, 16, 00, 00),
            //    FromLocation = new Location(1, 1),
            //    ToLocation = new Location(4, 4),
            //    Mass = 5
            //};

            var orders = new List<Order> {order1, order2};

            Console.WriteLine("Курьеры компании:");
            foreach (var courier in couriers)
            {
                Console.WriteLine(courier.GetInfo());
            }

            Console.WriteLine("Заказы компании:");
            foreach (var order in orders)
            {
                Console.WriteLine(order.GetInfo());
            }

            var orderQueue = new Queue<Order>();
            foreach (var order in orders.OrderByDescending(x => x.GetMaxOrderPrice()))
            {
                order.MaxCost = order.GetMaxOrderPrice();
                orderQueue.Enqueue(order);
            }

            while (orderQueue.Count > 0)
            {
                var currentOrder = orderQueue.Dequeue();
                if (currentOrder.Planning(currentOrder.GetAvailableCouriers(company.Couriers)))
                {
                    Console.WriteLine($"Планирование завершено для заказа {currentOrder.Name}: успешно");
                }
                else
                {
                    Console.WriteLine($"Планирование завершено для заказа {currentOrder.Name}: не успешно");
                }
            }

            foreach (var courier in couriers)
            {
                courier.WriteScheduleItems();
            }


            //var options = new JsonSerializerOptions
            //{
            //    WriteIndented = true,
            //    //Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.All)
            //    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            //};

            //string json = JsonSerializer.Serialize(courierCompany, options);
            //Console.WriteLine(json);
            //Console.WriteLine();
            //string json2 = JsonSerializer.Serialize(couriers, options);
            //Console.WriteLine(json2);
            //Console.ReadLine();
            //Company company = new Company();

        }

        //public static List<CurrentScheduleItem> PlanningOrder(Company company)
        //{
        //    var orderQueue = new Queue<Order>();
        //    var scheduleItems = new List<CurrentScheduleItem>();

        //    foreach( var e in company.Orders.OrderByDescending(x => x.MaxCost))
        //        orderQueue.Enqueue(e);

        //    while (orderQueue.Count > 0)
        //    {
        //        var currentOrder = orderQueue.Dequeue();
        //        var couriers = currentOrder
        //            .GetAvailableCouriers(company.Couriers)
        //            .OrderBy(x => x.Cost)
        //            .Take(10)
        //            .ToList();
        //    }

        //    return scheduleItems;
        //}
    }
}
