// Models/ChartDataModels.cs
namespace PTM2._0.Models
{
    public class DailySalesData
    {
        public string[] Dates { get; set; }
        public decimal[] Amounts { get; set; }
    }

    public class PerformanceSalesData
    {
        public string[] Names { get; set; }
        public decimal[] Values { get; set; }
    }

    public class UserPurchaseData
    {
        public string[] Names { get; set; }
        public decimal[] Values { get; set; }
    }
}