
using System;
using System.ComponentModel.DataAnnotations;

public class PrescriptionStatusUpdateDto
{
    [Required]
    [RegularExpression("^(Ready to Deliver|Not yet Delivered|Delivered)$", ErrorMessage = "NewStatus Status is invalid.")]
    public string NewStatus { get; set; } 

    
    public DateTime? DeliveryETA { get; set; }
}