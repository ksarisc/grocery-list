using System;

namespace GroceryList.Models
{
    public class DataRequest
    {
        public string HomeId { get; set; }
        public string StoreName { get; set; }
        public string ActionName { get; set; }
    }

    public class DataRequestInfo : DataRequest
    {
        /// <summary>
        /// information about the request (including created/last write time in UTC)
        /// </summary>
        public DateTime CreatedTime { get; set; }
    }
}
