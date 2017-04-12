using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Landis.Library.Metadata;

namespace Landis.Extension.DynamicFire
{
    public class SummaryLog
    {

        [DataFieldAttribute(Desc = "Time")]
        public int Time { set; get; }

        [DataFieldAttribute(Desc = "Total Sites Burned")]
        public int TotalSitesBurned { set; get; }

        [DataFieldAttribute(Desc = "Number of Fires")]
        public int NumberFires { set; get; }

        [DataFieldAttribute(Desc = "Ecoregion Sites by Map Code", ColumnList = true)]
        public int[] EcoMaps_ { set; get; }
    }
}