namespace Greenix.Domain.Enums
{
    public enum ComplaintStatus : byte
    {
        Submitted = 0,
        UnderReview = 1,
        Assigned = 2,
        WorkInProgress = 3,
        Resolved = 4,
        Closed = 5,
        Rejected = 6
    }
}
