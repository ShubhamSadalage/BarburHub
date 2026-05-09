namespace BarberHub.Web.Domain.Entities;

public enum BarberApprovalStatus
{
    NotApplicable = 0,   // Regular customer or SuperAdmin
    Pending = 1,         // Submitted barber registration, awaiting approval
    Approved = 2,        // Approved — full barber privileges
    Rejected = 3         // Rejected — cannot use barber features
}
