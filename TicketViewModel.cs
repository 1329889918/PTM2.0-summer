namespace PTM2._0.Models
{
    public class TicketViewModel
    {
        public Ticket Ticket { get; set; }
        public int SoldQuantity { get; set; }
        public double SoldPercentage { get; set; }
        public int? VenueCapacity { get; set; }
    }
}
