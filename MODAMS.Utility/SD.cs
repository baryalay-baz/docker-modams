using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace MODAMS.Utility
{
    public class SD
    {
        public const string Role_User = "User";
        public const string Role_StoreOwner = "StoreOwner";
        public const string Role_SeniorManagement = "SeniorManagement";
        public const string Role_Administrator = "Administrator";

        public const int Asset_Available = 1;
        public const int Asset_HandedOver = 2;
        public const int Asset_Disposed = 3;


        public const int Transfer_Pending = 1;
        public const int Transfer_SubmittedForAcknowledgement = 2;
        public const int Transfer_Completed = 3;
        public const int Transfer_Rejected = 4;

        public const int Transaction_Registration = 1;
        public const int Transaction_Transfer = 2;
        public const int Transaction_Handover = 3;
        public const int Transaction_Disposal = 4;
        public const int Transaction_Verification = 5;

        public const string WebAddress = "http://misportal.ddns.net:85";
        public const string WebAddressLocal = "https://localhost:44398";

        public const int NS_NewRegistration = 1;
        public const int NS_ForgotPassword = 2;
        public const int NS_Transfer = 3;


        
    }

    
}