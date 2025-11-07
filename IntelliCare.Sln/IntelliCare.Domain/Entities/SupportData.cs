// SupportData.cs (IntelliCare.Domain)

using System;
using System.ComponentModel.DataAnnotations;
// This 'using' is CRITICAL to resolve the Patient entity reference.
// If Patient is not in a subfolder, you may need to try 'using IntelliCare.Domain;'
using IntelliCare.Domain;

namespace IntelliCare.Domain
{
    public class SupportData
    {
        [Key]
        public int DataID { get; set; }

        
        public string Type { get; set; } = string.Empty;

        
        public string DataType { get; set; } = string.Empty; 
        
        public string MetricsJson { get; set; } = string.Empty;
        public string DetailedDataJson { get; set; } = string.Empty;

       

        public int? DoctorID { get; set; } 

        public string FileName { get; set; } = string.Empty; 

        
        public byte[]? FileContent { get; set; } 

        public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;

        
        public Patient? Patient { get; set; }


    }
}