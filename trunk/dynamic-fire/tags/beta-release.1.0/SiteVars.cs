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
        private static ISiteVar<IEcoregion> ecoregions;
        private static ISiteVar<int> cfsFuelType;
        private static ISiteVar<int> percentConifer;
        private static ISiteVar<int> percentHardwood;
        private static ISiteVar<int> percentDeadFir;
        private static ISiteVar<byte> severity;
        private static ISiteVar<bool> disturbed;
        private static ISiteVar<double> travelTime;
        private static ISiteVar<double> minNeighborTravelTime;
        //private static ISiteVar<double> simpleTravelTime;
        private static ISiteVar<double> rateOfSpread;
        //-----Added by BRM-----
        private static ISiteVar<double> adjROS;
        private static ISiteVar<double> costTime;
        //----------
        private static ISiteVar<ushort> groundSlope;
        private static ISiteVar<ushort> uphillSlopeAzimuth;

        private static ISiteVar<ushort> siteWindSpeed;
        private static ISiteVar<ushort> siteWindDirection;

        //---------------------------------------------------------------------

        public static void Initialize()
        {
            eventVar       = Model.Core.Landscape.NewSiteVar<Event>(InactiveSiteMode.DistinctValues);
            ecoregions     = Model.Core.Landscape.NewSiteVar<IEcoregion>();
            percentDeadFir = Model.Core.Landscape.NewSiteVar<int>();
            severity       = Model.Core.Landscape.NewSiteVar<byte>();
            disturbed      = Model.Core.Landscape.NewSiteVar<bool>();
            travelTime     = Model.Core.Landscape.NewSiteVar<double>();
            minNeighborTravelTime     = Model.Core.Landscape.NewSiteVar<double>();
            //simpleTravelTime     = Model.Core.Landscape.NewSiteVar<double>();
            rateOfSpread   = Model.Core.Landscape.NewSiteVar<double>();
            //-----Added by BRM-----
            adjROS = Model.Core.Landscape.NewSiteVar<double>();
            costTime = Model.Core.Landscape.NewSiteVar<double>();
            //----------
            groundSlope = Model.Core.Landscape.NewSiteVar<ushort>();
            uphillSlopeAzimuth = Model.Core.Landscape.NewSiteVar<ushort>();

            siteWindSpeed = Model.Core.Landscape.NewSiteVar<ushort>();
            siteWindDirection = Model.Core.Landscape.NewSiteVar<ushort>();
            
            //Also initialize topography, will be overwritten if optional parameters provided:
            SiteVars.GroundSlope.ActiveSiteValues = 0;
            SiteVars.UphillSlopeAzimuth.ActiveSiteValues = 0;
            
        }

        //---------------------------------------------------------------------

        public static void InitializeFuelType()
        {
            UI.WriteLine("   Initializing Fuel Type.");
            
            cfsFuelType = Model.Core.GetSiteVar<int>("Fuels.CFSFuelType");
            percentConifer = Model.Core.GetSiteVar<int>("Fuels.PercentConifer");
            percentHardwood = Model.Core.GetSiteVar<int>("Fuels.PercentHardwood");
            
            if (SiteVars.CFSFuelType == null) 
                throw new System.ApplicationException("Error: CFS Fuel Type NOT Initialized.  Fuel extension MUST be active.");

            SiteVars.PercentDeadFir.ActiveSiteValues = 0;
            
        }

        //---------------------------------------------------------------------

        public static ISiteVar<IEcoregion> Ecoregion
        {
            get {
                return ecoregions;
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
/*
        public static ISiteVar<double> SimpleTravelTime
        {
            get {
                return simpleTravelTime;
            }
        }*/
        //---------------------------------------------------------------------

        public static ISiteVar<double> RateOfSpread
        {
            get {
                return rateOfSpread;
            }
        }
        //-----Added by BRM-----
        public static ISiteVar<double> AdjROS
        {
            get
            {
                return adjROS;
            }
        }
        public static ISiteVar<double> CostTime
        {
            get
            {
                return costTime;
            }
        }
        //----------

        public static ISiteVar<ushort> GroundSlope
        {
            get {
                return groundSlope;
            }
        }

        public static ISiteVar<ushort> UphillSlopeAzimuth
        {
            get {
                return uphillSlopeAzimuth;
            }
        }
        public static ISiteVar<ushort> SiteWindSpeed
        {
            get
            {
                return siteWindSpeed;
            }
        }

        public static ISiteVar<ushort> SiteWindDirection
        {
            get
            {
                return siteWindDirection;
            }
        }
    }
}
