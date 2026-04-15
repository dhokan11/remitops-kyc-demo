namespace RemitOps.API.Models.Admin
{
    public class DailyVolumePointDto
    {
        public DateTime Date { get; set; }
        public decimal Volume { get; set; }
        public int Count { get; set; }
    }
}