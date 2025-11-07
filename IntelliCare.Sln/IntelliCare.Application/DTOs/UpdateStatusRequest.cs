using IntelliCare.Domain.Enums;

public class UpdateStatusRequest
{
    public int AppointmentId { get; set; }
    public AppointmentStatus NewStatus { get; set; }
}
