using System.ComponentModel.DataAnnotations;

public class BookAppointmentDto
{
    [Required(ErrorMessage = "DoctorID is required")]
    public int DoctorID { get; set; }

    [Required(ErrorMessage = "Appointment date/time is required")]
    public DateTime Date_Time { get; set; }

    public string? Reason { get; set; }

    public decimal? Fee { get; set; }
}
