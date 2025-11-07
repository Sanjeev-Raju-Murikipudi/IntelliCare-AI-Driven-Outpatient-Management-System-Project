using System.ComponentModel.DataAnnotations;

public class RescheduleAppointmentDto
{
    [Required]
    public int AppointmentId { get; set; }

    [Required]
    public DateTime NewDate_Time { get; set; }
}
