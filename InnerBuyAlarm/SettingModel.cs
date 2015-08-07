using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InnerBuyAlarm
{
    public class SettingModel
    {
        public string Url { set; get; }
        public string Interval { set; get; }
        public string SearchText { set; get; }
        public bool AutoBuy { set; get; }
        public string[] Products { set; get; }
        public string BuyerName { set; get; }
        public string BuyAmount { set; get; }
        public int BuyLimit { set; get; }
    }
}
