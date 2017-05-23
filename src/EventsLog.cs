using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Landis.Library.Metadata;
using Landis.SpatialModeling;
using Landis.Core;

namespace Landis.Extension.DynamicFire
{
    public class EventsLog
    {

        [DataFieldAttribute(Desc = "Time")]
        public int Time { set; get; }

        [DataFieldAttribute(Desc = "Initial Site")]
        public Location InitSite { set; get; }

        [DataFieldAttribute(Desc = "Initial Fire Region")]
        public string InitFireRegion { set; get; }

        [DataFieldAttribute(Desc = "Initial Fuel")]
        public int InitFuel { set; get; }

        [DataFieldAttribute(Desc = "Initial Percentage Conifer")]
        public int InitPercentConifer { set; get; }

        [DataFieldAttribute(Desc = "Selected Size Or Duration")]
        public double SelectedSizeOrDuration { set; get; }

        [DataFieldAttribute(Desc = "Size Bin")]
        public double SizeBin { set; get; }

        [DataFieldAttribute(Desc = "Duration")]
        public double Duration { set; get; }

        [DataFieldAttribute(Desc = "Fire Season")]
        public SeasonName FireSeason { set; get; }

        [DataFieldAttribute(Desc = "Wind Speed")]
        public int WindSpeed { set; get; }

        [DataFieldAttribute(Desc = "Wind Direction")]
        public int WindDirection { set; get; }

        [DataFieldAttribute(Desc = "FFMC")]
        public int FFMC { set; get; }

        [DataFieldAttribute(Desc = "Build Up Index")]
        public int BUI { set; get; }

        [DataFieldAttribute(Desc = "Percent Curing")]
        public int PercentCuring { set; get; }

        [DataFieldAttribute(Desc = "ISI")]
        public int ISI { set; get; }

        [DataFieldAttribute(Desc = "Sites Checked")]
        public int SitesChecked { set; get; }

        [DataFieldAttribute(Desc = "Cohorts Killed")]
        public int CohortsKilled { set; get; }

        [DataFieldAttribute(Desc = "Mean Severity")]
        public double MeanSeverity { set; get; }

        [DataFieldAttribute(Desc = "Ecoregion Sites by MapCode", ColumnList = true)]
        public double[] EcoMaps_ { set; get; }

        [DataFieldAttribute(Desc = "Total Sites In Event")]
        public int TotalSitesInEvent { set; get; }
    }
}
