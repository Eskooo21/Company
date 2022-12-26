namespace CourierCompany.Model;

/// <summary>
/// Элемент расписания
/// </summary>
public class ScheduleItem
{
    
    /// <summary>
    /// Запланированный заказ
    /// </summary>
    public Order Order { get; set; }

    /// <summary>
    /// Время начала элемента расписания
    /// </summary>
    public TimeSpan LeftTime { get; set; }

    /// <summary>
    /// Время окончания элемента расписания
    /// </summary>
    public TimeSpan RightTime
    {
        get
        {
            return Order.DeliveryPeriod.TimeOfDay;
        }
    }

    /// <summary>
    /// Начальное местоположение
    /// </summary>
    public Location? InitialLocation { get; set; }

    /// <summary>
    /// Конечное положение
    /// </summary>
    public Location EndLocation
    {
        get
        {
            return Order.ToLocation;
        }
    }

    /// <summary>
    /// Профит
    /// </summary>
    public double Profit { get; set; }

    public Courier CurrentCourier { get; set; }

    public ScheduleItem(Order order)
    {

        Order = order;
        LeftTime = order.DeliveryPeriod.TimeOfDay;

    }

}