namespace Mango.Services.OrderAPI.Utility
{
    public class SD
    {
        //status of order
        public const string Status_Pending = "Pending";
        public const string Status_Approved = "Approved";
        public const string Status_ReadyForPackup = "ReadyForPackup";
        public const string Status_Completed = "Completed";
        public const string Status_Refuned = "Refuned";
        public const string Status_Cancelled = "Cancelled";

        //role of user
        public const string RoleAdmin = "ADMIN";
        public const string RoleCustomer = "CUSTOMER";
    }
}
