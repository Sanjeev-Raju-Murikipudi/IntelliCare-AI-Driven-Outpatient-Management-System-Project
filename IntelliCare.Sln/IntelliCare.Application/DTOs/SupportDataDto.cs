namespace IntelliCare.Application.DTOs
{
    public class SupportDataDto
    {
        public string DataType { get; set; }
        public int RefPatientID { get; set; }
        public string FileName { get; set; }

        public byte[] FileContent { get; set; }

        // ✅ Add these fields to match your database schema
        public string Metrics { get; set; } = "N/A";
        public string PredictionDetails { get; set; } = "N/A";
    }
}
