using System;
using System.ComponentModel;

namespace CodingTaskDavidSocketModels
{
    public class ListModel
    {
        [DisplayName("Stock Name")]
        public string stockName { get; set; }
        [DisplayName("Date & Time")]
        public string dateTime { get; set; }
        [DisplayName("Price")]
        public string price { get; set; }
    }
}
