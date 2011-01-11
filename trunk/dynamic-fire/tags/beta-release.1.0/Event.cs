//  Copyright 2005 University of Wisconsin
//  Authors:  Robert M. Scheller, James B. Domingo
//  License:  Available at  
//  http://landis.forest.wisc.edu/developers/LANDIS-IISourceCodeLicenseAgreement.pdf
//  Edited by Brian R. Miranda (BRM)

using Edu.Wisc.Forest.Flel.Grids;
using Landis.AgeCohort;
using Landis.Ecoregions;
using Landis.Landscape;
using Landis.PlugIns;
using Landis.Species;
using Landis.Util;
using System.Collections.Generic;
using System;
using Troschuetz.Random;

namespace Landis.Fire
{

    public class Event
        : AgeCohort.ICohortDisturbance
    {
        public static IFuelTypeParameters[] fuelTypeParms;
        private static IDamageTable[] damages;
        private static ILandscapeCohorts cohorts;

        //My Test command

        private ActiveSite initiationSite;
        private double maxFireParameter;
        //-----Added by BRM-----
        private double maxDuration;
        //----------
        private int initiationEcoregion;
        private int initiationPercentConifer;
        private FuelTypeCode initiationFuel;
        private int totalSitesDamaged;
        private int cohortsKilled;
        private double eventSeverity;
        private int numSitesChecked;
        private int[] sitesInEvent;

        private ActiveSite currentSite; // current site where cohorts are being damaged
        private int siteSeverity;      // used to compute maximum cohort severity at a site

        private ISeasonParameters fireSeason;
        private int windSpeed;
        private int windDirection;
        private int fineFuelMoistureCode;
        private int buildUpIndex;
        private double lengthB;
        private double lengthA;
        private double lengthD;
        private double lbr;  //lenght:breadth ratio

        //---------------------------------------------------------------------
        static Event()
        {
        }

        //---------------------------------------------------------------------

        public Location StartLocation
        {
            get {
                return initiationSite.Location;
            }
        }

        //---------------------------------------------------------------------

        public double MaxFireParameter
        {
            get {
                return maxFireParameter;
            }
        }
        //---------------------------------------------------------------------
        //-----Added by BRM-----
        public double MaxDuration
        {
            get
            {
                return maxDuration;
            }
        }
        //----------
        //---------------------------------------------------------------------

        public int InitiationEcoregion
        {
            get {
                return initiationEcoregion;
            }
        }
        //---------------------------------------------------------------------

        public int InitiationPercentConifer
        {
            get {
                return initiationPercentConifer;
            }
        }
        //---------------------------------------------------------------------

        public FuelTypeCode InitiationFuel
        {
            get {
                return initiationFuel;
            }
        }
        //---------------------------------------------------------------------

        public int TotalSitesDamaged
        {
            get {
                return totalSitesDamaged;
            }
        }
        //---------------------------------------------------------------------

        public int[] SitesInEvent
        {
            get {
                return sitesInEvent;
            }
        }

        //---------------------------------------------------------------------

        public int NumSitesChecked
        {
            get {
                return numSitesChecked;
            }
        }
        //---------------------------------------------------------------------

        public int CohortsKilled
        {
            get {
                return cohortsKilled;
            }
        }

        //---------------------------------------------------------------------

        public double EventSeverity
        {
            get {
                return eventSeverity;
            }
        }

        //---------------------------------------------------------------------

        public int WindSpeed
        {
            get {
                return windSpeed;
            }
            set {
                windSpeed = value;
            }
        }
        //---------------------------------------------------------------------

        public int WindDirection
        {
            get {
                return windDirection;
            }
            set {
                windDirection = value;
            }
        }
        //---------------------------------------------------------------------

        public int FFMC
        {
            get {
                return fineFuelMoistureCode;
            }
        }
        
        //---------------------------------------------------------------------

        public int BuildUpIndex
        {
            get {
                return buildUpIndex;
            }
        }
        //---------------------------------------------------------------------

        public ISeasonParameters FireSeason
        {
            get {
                return fireSeason;
            }
        }
        //---------------------------------------------------------------------

        public double LengthB
        {
            get {
                return lengthB;
            }
            set {
                lengthB = value;
            }
        }
        //---------------------------------------------------------------------

        public double LengthA
        {
            get {
                return lengthA;
            }
            set {
                lengthA = value;
            }
        }
        //---------------------------------------------------------------------

        public double LengthD
        {
            get {
                return lengthD;
            }
            set {
                lengthD = value;
            }
        }
        //---------------------------------------------------------------------

        public double LB
        {
            get {
                return lbr;
            }
            set {
                lbr = value;
            }
        }
        //---------------------------------------------------------------------

        public IFuelTypeParameters[] FuelTypeParms
        {
            get {
                return fuelTypeParms;
            }
        }
        //---------------------------------------------------------------------

        PlugInType AgeCohort.IDisturbance.Type
        {
            get {
                return PlugIn.Type;
            }
        }

        //---------------------------------------------------------------------

        ActiveSite AgeCohort.IDisturbance.CurrentSite
        {
            get {
                return currentSite;
            }
        }

        //---------------------------------------------------------------------

        private Event(ActiveSite initiationSite, ISeasonParameters[] seasons, IWindDirectionParameters[] windDirs)
        {
            this.initiationSite = initiationSite;
            this.sitesInEvent = new int[Ecoregions.Dataset.Count];
            foreach(IEcoregion ecoregion in Ecoregions.Dataset)
                this.sitesInEvent[ecoregion.Index] = 0;
            this.cohortsKilled = 0;
            this.eventSeverity = 0;
            this.totalSitesDamaged = 0;
            this.lengthB = 0.0;
            this.lengthA = 0.0;
            this.lengthD = 0.0;
            
            this.fireSeason           = Weather.GenerateSeason(seasons);
            this.windSpeed            = Weather.GenerateWindSpeed(this.fireSeason);
            this.fineFuelMoistureCode = Weather.GenerateFineFuelMoistureCode(this.fireSeason);
            this.buildUpIndex         = Weather.GenerateBuildUpIndex(this.fireSeason);

            IWindDirectionParameters windDir = windDirs[(int) this.fireSeason.NameOfSeason];
            this.windDirection        = Weather.GenerateWindDirection(windDir);

            

        }

        //---------------------------------------------------------------------

        public static void Initialize(ISeasonParameters[] seasons, 
                                      IFuelTypeParameters[] fuelTypeParameters,
                                      IDamageTable[]    damages)
        {
            double totalSeasonFireProb = 0.0;
            foreach(ISeasonParameters season in seasons)
                totalSeasonFireProb += season.FireProbability;
            if (totalSeasonFireProb != 1.0)
                throw new System.ApplicationException("Error: Season Probabilities don't add to 1.0");
            Event.fuelTypeParms = fuelTypeParameters;
            Event.damages = damages;

            cohorts = Model.Core.SuccessionCohorts as ILandscapeCohorts;
            if (cohorts == null)
                throw new System.ApplicationException("Error: Cohorts don't support age-cohort interface");
        }
        //---------------------------------------------------------------------
        public static Event Initiate(ActiveSite site,
                                     int        timestep,
                                     SizeType   fireSizeType, 
                                     bool       bui,
                                     ISeasonParameters[] seasons, 
                                     IWindDirectionParameters[] windDirs)
        {

            
            //Adjust ignition probability (measured on an annual basis) for the 
            //user determined fire time step.
            int fuelIndex = SiteVars.CFSFuelType[site];
            //-----Edited by BRM-----
            //double initProb = fuelTypeParms[fuelIndex].InitiationProbability * timestep;
            double initProb = fuelTypeParms[fuelIndex].InitiationProbability;
            //---------

            //The initial site must exceed the probability of initiation and
            //have a severity > 0 and exceed the ignition threshold:
            if (Util.Random.GenerateUniform() <= initProb)
            {
                Event FireEvent = new Event(site, seasons, windDirs);
                
                if(!FireEvent.Spread(site, fireSizeType, bui))
                    return null;
                else 
                    return FireEvent;
            }
            
            return null;
        }

        //---------------------------------------------------------------------
        private bool Spread(ActiveSite initiationSite, SizeType fireSizeType, bool BUI)
        {
            //First, check for fire overlap:
            if(SiteVars.Event[initiationSite] != null) 
                return false;
            
            UI.WriteLine("   Fire spreading... ");

            IEcoregion ecoregion = SiteVars.Ecoregion[initiationSite];
            this.initiationEcoregion = (int) ecoregion.MapCode;
            IMoreEcoregionParameters fireParms = ecoregion.MoreEcoregionParameters;
            
            int totalSiteSeverities = 0;
            int siteCohortsKilled    = 0;
            totalSitesDamaged = 1;
            
            //Calculate Size or Duration:
            this.maxFireParameter = ComputeSize(fireParms.MeanSize, fireParms.StandardDeviation, fireSizeType);
            //FuelTypeCode activeFT = (FuelTypeCode) SiteVars.CFSFuelType[initiationSite];
            this.initiationFuel   = (FuelTypeCode) SiteVars.CFSFuelType[initiationSite];
            this.initiationPercentConifer = SiteVars.PercentConifer[initiationSite];

            //UI.WriteLine("      Calculated max fire size or duration = {0:0.0}", maxFireParameter);
            //UI.WriteLine("      Fuel Type = {0}", activeFT.ToString());
            
            //Next, calculate the fire area:
            List<Site> FireLocations = new List<Site>();
            //-----Edited by BRM-----
            FireLocations = EventRegion.SizeFireCostSurface(this, fireSizeType);
            //----------
            
            //Attach travel time weights here
            List<WeightedSites> FireCostSurface = new List<WeightedSites>(0);
            foreach(Site site in FireLocations)
            {
                FireCostSurface.Add(new WeightedSites(site, SiteVars.TravelTime[site]));
            }

            FireCostSurface.Sort(CompareWeights);
            FireLocations = new List<Site>();

            double cellArea = (Model.Core.CellLength * Model.Core.CellLength) / 10000; //convert to ha
            double totalArea = 0.0;
            int cellCnt = 0;
            //-----Added by BRM-----
            double durMax = 0;
            //----------
            int weightInd = 0;
            if (fireSizeType == SizeType.size_based) 
            {

                foreach(WeightedSites weighted in FireCostSurface)
                {
                    //weightCnt++;
                    cellCnt++;
                    if(totalArea > this.maxFireParameter) 
                    {
                        //-----Added by BRM-----
                        if (durMax == 0)
                        {
                            foreach (WeightedSites testweight in FireCostSurface)
                            {
                                if (weightInd == 1)
                                {
                                    durMax = SiteVars.TravelTime[testweight.Site];
                                }
                                else
                                {
                                    if (testweight == weighted)
                                    {
                                        weightInd = 1;
                                    }
                                }
                            }

                        }
                        //----------
                        SiteVars.TravelTime[weighted.Site] = Double.PositiveInfinity;
                        SiteVars.Event[weighted.Site] = null;
                    }
                    else
                    {
                        totalArea += cellArea;
                        FireLocations.Add(weighted.Site);
                        //-----Added by BRM-----
                        if (SiteVars.TravelTime[weighted.Site] > durMax)
                            durMax = SiteVars.TravelTime[weighted.Site];
                        //----------
                    }
                }
                //-----Added by BRM-----
                this.maxDuration = durMax;
                //----------
                //UI.WriteLine("      CellCnt = {0}, BurnedArea = {1:0.0} (ha), targetArea = {2:0.0} (ha).", cellCnt, totalArea, this.maxFireParameter);
                if(totalArea < this.maxFireParameter)
                    UI.WriteLine("POSSIBLE ERROR:  Check map for partial area!");
            }
            else if (fireSizeType == SizeType.duration_based) 
            {
                foreach(WeightedSites weighted in FireCostSurface)
                {
                    cellCnt++;
                    if(weighted.Weight > this.maxFireParameter) 
                    {
                        SiteVars.TravelTime[weighted.Site] = Double.PositiveInfinity;
                        SiteVars.Event[weighted.Site] = null;
                    }
                    else
                    {
                        totalArea += cellArea;
                        FireLocations.Add(weighted.Site);
                        //-----Added by BRM-----
                        if (SiteVars.TravelTime[weighted.Site] > durMax)
                            durMax = SiteVars.TravelTime[weighted.Site];
                        //----------
                    }
                }
                //-----Added by BRM-----
                this.maxDuration = durMax;
                //----------
                UI.WriteLine("      CellCnt = {0}, BurnedArea = {1:0.0} (ha), target duration = {2:0.0}.", cellCnt, totalArea, this.maxFireParameter);
            }

            foreach(Site site in FireLocations)
            {
                currentSite = site as ActiveSite;
                if(currentSite.IsActive)
                {
                    this.numSitesChecked++;
            
                    this.siteSeverity = FireSeverity.CalcFireSeverity(currentSite, this);
                    siteCohortsKilled = Damage(currentSite);
                //if (siteCohortsKilled > 0) 
                //{
                    //UI.WriteLine("      Damaged Cohorts = {0}, Severity = {1}.", siteCohortsKilled, siteSeverity);
                    this.totalSitesDamaged++;
                    totalSiteSeverities += this.siteSeverity;
                    IEcoregion siteEcoregion = SiteVars.Ecoregion[site];
                    sitesInEvent[siteEcoregion.Index]++;
                    SiteVars.Disturbed[currentSite] = true;
                    SiteVars.Severity[currentSite] = (byte) siteSeverity;
                //}
                }
            }

            if (this.totalSitesDamaged == 0)
                this.eventSeverity = 0;
            else
                this.eventSeverity = ((double) totalSiteSeverities) / this.totalSitesDamaged;
                
            return true;
        }
        //---------------------------------------------------------------------

        public static double ComputeSize(double meanSize, double sd, SizeType fireSizeType)
        {
            if (fireSizeType == SizeType.duration_based) 
            {

                //-----Edited by BRM-----
                LognormalDistribution randVar = new LognormalDistribution(RandomNumberGenerator.Singleton);
                //NormalDistribution randVar = new NormalDistribution(RandomNumberGenerator.Singleton);
                //GammaDistribution randVar = new GammaDistribution(RandomNumberGenerator.Singleton);
                //----------
                randVar.Mu = meanSize;      //randVar.Mu for Lognormal //randVar.Alpha for Gamma
                randVar.Sigma = sd;   //randVar.Sigma for Lognormal  //randVar.Theta for Gamma
                double sizeGenerated = randVar.NextDouble();
                if (sizeGenerated < 0)
                    return 0;
                else
                    return (sizeGenerated); 
            }
            else if (fireSizeType == SizeType.size_based) 
            {
                LognormalDistribution randVar = new LognormalDistribution(RandomNumberGenerator.Singleton);
                //NormalDistribution randVar = new NormalDistribution(RandomNumberGenerator.Singleton);
                //GammaDistribution randVar = new GammaDistribution(RandomNumberGenerator.Singleton);
                randVar.Mu = meanSize;      //randVar.Mu for Lognormal //randVar.Alpha for Gamma
                randVar.Sigma = sd;   //randVar.Sigma for Lognormal //randVar.Theta for Gamma
                double sizeGenerated = randVar.NextDouble();
                if (sizeGenerated <= 0)
                    return 0;
                return (sizeGenerated);
            
            }
            return 0.0;
        }
        //---------------------------------------------------------------------

        private int Damage(ActiveSite site)
        {
            int previousCohortsKilled = this.cohortsKilled;
            cohorts[site].DamageBy(this);
            return this.cohortsKilled - previousCohortsKilled;
        }

        //---------------------------------------------------------------------

        //  A filter to determine which cohorts are removed.

        bool AgeCohort.ICohortDisturbance.Damage(AgeCohort.ICohort cohort)
        {
            bool killCohort = false;

            //Fire Severity 5 kills all cohorts:
            if (siteSeverity == 5) 
            {
                killCohort = true;
            }
            else {
                //Otherwise, use damage table to calculate damage.
                //Read table backwards; most severe first.
                float ageAsPercent = (float) cohort.Age / (float) cohort.Species.Longevity;
                for (int i = damages.Length-1; i >= 0; --i) 
                {
                    IDamageTable damage = damages[i];
                    if (siteSeverity - cohort.Species.FireTolerance >= damage.SeverTolerDifference) 
                    {
                        if (damage.MaxAge >= ageAsPercent) {
                            killCohort = true;
                        }
                        break;  // No need to search further in the table
                    }
                }
            }

            if (killCohort) {
                this.cohortsKilled++;
            }
            return killCohort;
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Compares weights
        /// </summary>
        private static int CompareWeights(WeightedSites x,
                                          WeightedSites y)
        {
            if (x.Weight < y.Weight)
                return -1;
            else if (x.Weight > y.Weight)
                return 1;
            else
                return 0;
        }

    }


    public class WeightedSites
    {
        private Site site;
        private double weight;

        //---------------------------------------------------------------------
        public Site Site
        {
            get {
                return site;
            }
            set {
                site = value;
            }
        }
        
        public double Weight
        {
            get {
                return weight;
            }
            set {
                weight = value;
            }
        }
        
        public WeightedSites (Site site, double weight)
        {
            this.site = site;
            this.weight = weight;
        }

    }
    
    
}
