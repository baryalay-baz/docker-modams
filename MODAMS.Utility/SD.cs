using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace MODAMS.Utility
{
    public class SD
    {
        public const string Role_User = "User";
        public const string Role_StoreOwner = "StoreOwner";
        public const string Role_SeniorManagement = "SeniorManagement";
        public const string Role_Administrator = "Administrator";

        public const int Transfer_Pending = 1;
        public const int Transfer_SubmittedForAcknowledgement = 2;
        public const int Transfer_Completed = 3;
        public const int Transfer_Rejected = 4;
	}
}