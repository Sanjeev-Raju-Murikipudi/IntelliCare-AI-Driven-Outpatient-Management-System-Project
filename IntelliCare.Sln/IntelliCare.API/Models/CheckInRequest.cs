namespace IntelliCare.API.Models
{
    public class CheckInRequest
    {
        public int AppointmentId { get; set; }
        public int QueuePosition { get; set; }
    }
}
