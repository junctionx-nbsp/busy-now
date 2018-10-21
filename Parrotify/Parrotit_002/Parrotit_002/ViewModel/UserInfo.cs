using System.Collections.Generic;

namespace Parrotify.ViewModel
{
    /// <summary>
    /// Describes the structure of UserInfo
    /// </summary>
    public static class UserInfo
    {
        public static string LatestPartnerNumber { get; set; }  

        public static bool IsConnected { get; set; }

        public static string Number { get; set; }

        public static List<Message> Messages { get; set; }  
    }
}
