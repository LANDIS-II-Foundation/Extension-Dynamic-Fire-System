//  Copyright 2005 University of Wisconsin
//  Authors:  Robert M. Scheller, James B. Domingo
//  License:  Available at
//  http://landis.forest.wisc.edu/developers/LANDIS-IISourceCodeLicenseAgreement.pdf
//  Edited by Brian R. Miranda (BRM)

using Landis.AgeCohort;
using Landis.Landscape;

namespace Landis.Fire
{
    public static class SiteVars
    {
        private static ISiteVar<Event> eventVar;
        private static ISiteVar<int> timeOfLastFire;
        private static ISiteVar<IFireRegion> fire_regions;
        private static ISiteVar<int> cfsFuelType;
        private static ISiteVar<int> decidFuelType;
        private static ISiteVar<int> percentConifer;
        private static ISiteVar<int> percentHardwood;
        private static ISiteVar<int> percentDeadFir;
        private static ISiteVar<byte> severity;
        private static ISiteVar<byte> lastSeverity;
        private static ISiteVar<bool> disturbed;
        private static ISiteVar<double> travelTime;
        private static ISiteVar<double> minNeighborTravelTime;
        //private static ISiteVar<double> simpleTravelTime;
        private static ISiteVar<double> rateOfSpread;
        private static ISiteVar<double> isi;
        private static ISiteVar<double> adjROS;
        private static ISiteVar<double> costTime;
        private static ISiteVar<ushort> groundSlope;
        private static ISiteVar<ushort> uphillSlopeAzimuth;

        private static ISiteVar<ushort> siteWindSpeed;
        private static ISiteVar<ushort> siteWindDirection;

        //---------------------------------------------------------------------

        public static void Initialize(ILandscapeCohorts cohorts)
        {
            eventVar            = Model.Core.Landscape.NewSiteVar<Event>(InactiveSiteMode.DistinctValues);
            timeOfLastFire      = Model.Core.Landscape.NewSiteVar<int>();
            fire_regions          = Model.Core.Landscape.NewSiteVar<IFireRegion>();
            percentDeadFir      = Model.Core.Landscape.NewSiteVar<int>();
            severity            = Model.Core.Landscape.NewSiteVar<byte>();
            lastSeverity        = Model.Core.Landscape.NewSiteVar<byte>();
            disturbed           = Model.Core.Landscape.NewSiteVar<bool>();
            travelTime          = Model.Core.Landscape.NewSiteVar<double>();
            minNeighborTravelTime     = Model.Core.Landscape.NewSiteVar<double>();
            rateOfSpread        = Model.Core.Landscape.NewSiteVar<double>();
            adjROS              = Model.Core.Landscape.NewSiteVar<double>();
            isi                 = Model.Core.Landscape.NewSiteVar<double>();
            costTime            = Model.Core.Landscape.NewSiteVar<double>();
            groundSlope         = Model.Core.Landscape.NewSiteVar<ushort>();
            uphillSlopeAzimuth  = Model.Core.Landscape.NewSiteVar<ushort>();
            siteWindSpeed       = Model.Core.Landscape.NewSiteVar<ushort>();
            siteWindDirection   = Model.Core.Landscape.NewSiteVar<ushort>();

            //Also initialize topography, will be overwritten if optional parameters provided:
            SiteVars.GroundSlope.ActiveSiteValues = 0;
            SiteVars.UphillSlopeAzimuth.ActiveSiteValues = 0;

            //Initialize TimeSinceLastFire to the maximum cohort age:
            foreach (ActiveSite site in Model.Core.Landscape)
            {
                ushort maxAge = AgeCohort.Util.GetMaxAge(cohorts[site]);
                timeOfLastFire[site] = Model.Core.StartTime - maxAge;
            }


            Model.Core.RegisterSiteVar(SiteVars.FireRegion, "Fire.FireRegion");
            Model.Core.RegisterSiteVar(SiteVars.Severity, "Fire.Severity");
            Model.Core.RegisterSiteVar(SiteVars.LastSeverity, "Fire.LastSeverity");
            Model.Core.RegisterSiteVar(SiteVars.TimeOfLastFire, "Fire.TimeOfLastEvent");
        }

        //---------------------------------------------------------------------

        public static void InitializeFuelType()
        {
            UI.WriteLine("   Initializing Fuel Type.");

            cfsFuelType     = Model.Core.GetSiteVar<int>("Fuels.CFSFuelType");
            decidFuelType   = Model.Core.GetSiteVar<int>("Fuels.DecidFuelType");
            percentConifer  = Model.Core.GetSiteVar<int>("Fuels.PercentConifer");
            percentHardwood = Model.Core.GetSiteVar<int>("Fuels.PercentHardwood");
            percentDeadFir  = Model.Core.GetSiteVar<int>("Fuels.PercentDeadFir");

            if (SiteVars.CFSFuelType == null)
                throw new System.ApplicationException("Error: CFS Fuel Type NOT Initialized.  Fuel extension MUST be active.");

            //SiteVars.PercentDeadFir.ActiveSiteValues = 0;

        }

        //---------------------------------------------------------------------

        public static ISiteVar<IFireRegion> FireRegion
        {
            get {
                return fire_regions;
            }
        }

        //---------------------------------------------------------------------

        public static ISiteVar<int> TimeOfLastFire
        {
            get {
                return timeOfLastFire;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<Event> Event
        {
            get {
                return eventVar;
            }
        }

        //---------------------------------------------------------------------

        public static ISiteVar<int> CFSFuelType
        {
            get {
                return cfsFuelType;
            }
        }
        //---------------------------------------------------------------------

        public static ISiteVar<int> DecidFuelType
        {
            get {
                return decidFuelType;
            }
        }
        //---------------------------------------------------------------------

        public static ISiteVar<int> PercentConifer
        {
            get {
                return percentConifer;
            }
        }

        //---------------------------------------------------------------------

        public static ISiteVar<int> PercentHardwood
        {
            get {
                return percentHardwood;
            }
        }
        //---------------------------------------------------------------------

        public static ISiteVar<int> PercentDeadFir
        {
            get {
                return percentDeadFir;
            }
        }
        //---------------------------------------------------------------------

        public static ISiteVar<byte> Severity
        {
            get {
                return severity;
            }
        }

        //---------------------------------------------------------------------
        public static ISiteVar<byte> LastSeverity
        {
            get
            {
                return lastSeverity;
            }
        }

        //---------------------------------------------------------------------

        public static ISiteVar<bool> Disturbed
        {
            get {
                return disturbed;
            }
        }
        //---------------------------------------------------------------------

        public static ISiteVar<double> TravelTime
        {
            get {
                return travelTime;
            }
        }
        //---------------------------------------------------------------------

        public static ISiteVar<double> MinNeighborTravelTime
        {
            get {
                return minNeighborTravelTime;
            }
        }
        //---------------------------------------------------------------------

        public static ISiteVar<double> RateOfSpread
        {
            get {
                return rateOfSpread;
            }
        }
        //---------------------------------------------------------------------

        public static ISiteVar<double> ISI
        {
            get {
                return isi;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<double> AdjROS
        {
            get
            {
                return adjROS;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<double> CostTime
        {
            get
            {
                return costTime;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<ushort> GroundSlope
        {
            get {
                return groundSlope;
            }
        }
        //---------------------------------------------------------------------

        public static ISiteVar<ushort> UphillSlopeAzimuth
        {
            get {
                return uphillSlopeAzimuth;
            }
        }
        //---------------------------------------------------------------------
        public static ISiteVar<ushort> SiteWindSpeed
        {
            get
            {
                return siteWindSpeed;
            }
        }

        //---------------------------------------------------------------------
        public static ISiteVar<ushort> SiteWindDirection
        {
            get
            {
                return siteWindDirection;
            }
        }
    }
}
