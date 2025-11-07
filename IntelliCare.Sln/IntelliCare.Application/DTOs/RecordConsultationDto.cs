
using System.ComponentModel.DataAnnotations; 

public class RecordConsultationDto
{
   
    [Required]
    public int AppointmentId { get; set; }

    public string Notes { get; set; }

    public string Diagnosis { get; set; }

   
    public string Medication { get; set; }

    
}