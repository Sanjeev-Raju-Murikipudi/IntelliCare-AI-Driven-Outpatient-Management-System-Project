namespace IntelliCare.Domain.Enums
{
    public enum AppointmentStatus
    {
        Available = 0,
        Booked = 1,
        Cancelled = 2,
        InProgress = 3,
        Completed = 4,
        Pending = 5,
        ReopenedFromCancellation = 6, 
        ReopenedFromReschedule = 7,
        NoShow = 8
    }

}
