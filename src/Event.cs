//  Authors:  Robert M. Scheller, Brian R. Miranda 

using Landis.Library.UniversalCohorts;
using Landis.SpatialModeling;
using Landis.Core;
using System;
using System.Collections.Generic;

namespace Landis.Extension.DynamicFire
{

    public class Event
        : IDisturbance
    {
        private static readonly bool isDebugEnabled = false; //debugLog.IsDebugEnabled;

        public static IFuelType[] FuelTypeParms;
        public static double SF;
        private static List<IFireDamage> damages;

        private ActiveSite initiationSite;
        private int sizeBin;
        private ActiveSite currentSite; // current site where cohorts are being damaged
        private int siteSeverity;      // used to compute maximum cohort severity at a site
                                       //private double lengthB;
                                       //private double lengthA;
                                       //private double lengthD;
                                       //private double lbr;  //lenght:breadth ratio

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

        public double MaxFireParameter { get; }
        //---------------------------------------------------------------------

        public double SizeBin
        {
            get
            {
                return sizeBin;
            }
        }
        //---------------------------------------------------------------------
        public double MaxDuration { get; private set; }
        //---------------------------------------------------------------------

        public IFireRegion InitiationFireRegion { get; }
        ////---------------------------------------------------------------------

        public int InitiationPercentConifer { get; private set; }
        //---------------------------------------------------------------------

        public int InitiationFuel { get; private set; }
        //---------------------------------------------------------------------

        //public int[] SitesInEvent { get; }

        ////---------------------------------------------------------------------

        public int NumSitesChecked { get; private set; }
        public int NumberDamagedSites { get; private set; }
        //---------------------------------------------------------------------

        public int CohortsKilled { get; private set; }

        ////---------------------------------------------------------------------

        public double EventSeverity { get; private set; }

        //---------------------------------------------------------------------

        public int WindSpeed { get; set; }
        ////---------------------------------------------------------------------

        public int WindDirection { get; set; }
        //---------------------------------------------------------------------

        public int FFMC { get; }

        //---------------------------------------------------------------------

        public int BuildUpIndex { get; }

        //---------------------------------------------------------------------

        public int FMC { get; }
        //---------------------------------------------------------------------

        public int ISI { get; private set; }
        //---------------------------------------------------------------------

        public ISeasonParameters FireSeason { get; }
        //---------------------------------------------------------------------

        ExtensionType IDisturbance.Type
        {
            get {
                return PlugIn.type;
            }
        }

        //---------------------------------------------------------------------

        ActiveSite IDisturbance.CurrentSite
        {
            get {
                return currentSite;
            }
        }

        //---------------------------------------------------------------------

        private Event(ActiveSite initiationSite, ISeasonParameters fireSeason, SizeType fireSizeType)
        {
            this.initiationSite = initiationSite;
            //this.SitesInEvent = new int[FireRegions.Dataset.Count];
            //PlugIn.ModelCore.UI.WriteLine("   initialzing siteInEvent ...");

            //foreach(IFireRegion fire_region in FireRegions.Dataset)
            //    this.SitesInEvent[fire_region.Index] = 0;
            this.CohortsKilled = 0;
            this.EventSeverity = 0;
            this.NumberDamagedSites = 0;
            //this.lengthB = 0.0;
            //this.lengthA = 0.0;
            //this.lengthD = 0.0;
            IFireRegion eco = SiteVars.FireRegion[initiationSite];
            this.InitiationFireRegion = eco;
            this.MaxFireParameter = ComputeSize(eco.MeanSize, eco.StandardDeviation, eco.MaxSize); //fireSizeType);
            this.sizeBin = ComputeSizeBin(eco.MeanSize, eco.StandardDeviation, this.MaxFireParameter);
            this.FireSeason         = fireSeason; //Weather.GenerateSeason(seasons);
            System.Data.DataRow weatherRow = Weather.GenerateDataRow(this.FireSeason, eco, this.sizeBin);
            
            this.WindSpeed            = Weather.GenerateWindSpeed(weatherRow);
            this.FFMC = Weather.GenerateFineFuelMoistureCode(weatherRow);
            this.BuildUpIndex         = Weather.GenerateBuildUpIndex(weatherRow);
            this.WindDirection        = Weather.GenerateWindDirection(weatherRow);
            this.FMC = Weather.GenerateFMC(this.FireSeason, eco);

        }

        //---------------------------------------------------------------------

        public static void Initialize(ISeasonParameters[] seasons,
                                      IFuelType[] fuelTypeParameters,
                                      List<IFireDamage>    damages)
        {
            if (isDebugEnabled)
                PlugIn.ModelCore.UI.WriteLine("Initializing event parameters ...");

            if(seasons == null || fuelTypeParameters == null || damages == null)
            {
                if(seasons == null)
                    PlugIn.ModelCore.UI.WriteLine("Error:  Seasons table empty.");
                if(fuelTypeParameters == null)
                    PlugIn.ModelCore.UI.WriteLine("Error:  FuelTypeParameters table empty.");
                if(damages == null)
                    PlugIn.ModelCore.UI.WriteLine("Error:  Damages table empty.");
                throw new System.ApplicationException("Error: Event class could not be initialized.");
            }

            float totalSeasonFireProb = 0.0F;
            foreach(ISeasonParameters season in seasons)
                totalSeasonFireProb += (float) season.FireProbability;

            if (totalSeasonFireProb != 1.0)
                throw new System.ApplicationException("Error: Season Probabilities don't add to 1.0");

            Event.FuelTypeParms = fuelTypeParameters;
            Event.damages = damages;

            int tempSlope, sumSlope = 0, cellCount = 0, meanSlope = 0;
            foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
            {
                if (site.IsActive)
                {
                    tempSlope = SiteVars.GroundSlope[site];
                    sumSlope += tempSlope;
                    cellCount++;
                }
            }

            if(sumSlope > 0)
            {
                meanSlope = sumSlope / cellCount;
                if (meanSlope > 60)
                    meanSlope = 60;
                Event.SF = CalculateSF(meanSlope);
            }
        }

        //---------------------------------------------------------------------
        public static Event Initiate(ActiveSite site,
                                     int        timestep,
                                     SizeType   fireSizeType,
                                     bool       bui,
                                     ISeasonParameters[] seasons,
                                     double severityCalibrate)
        {


            //Adjust ignition probability (measured on an annual basis) for the
            //user determined fire time step.
            int fuelIndex = SiteVars.CFSFuelType[site];

            double initProb = FuelTypeParms[fuelIndex].InitiationProbability;

            //If mixed type, need to use weighted average initProb
            if (FuelTypeParms[fuelIndex].BaseFuel == BaseFuelType.Conifer && SiteVars.PercentHardwood[site] > 0) //Mixed type
            {
                double decidInitProb;
                int decidFuelIndex = SiteVars.DecidFuelType[site];
                if (decidFuelIndex > 0)
                {
                    decidInitProb = FuelTypeParms[decidFuelIndex].InitiationProbability;
                    initProb = (initProb * SiteVars.PercentConifer[site] + decidInitProb * SiteVars.PercentHardwood[site]) / 100;
                }
            }

            //The initial site must exceed the probability of initiation and
            //have a severity > 0 and exceed the ignition threshold:
            double randomNum = PlugIn.ModelCore.GenerateUniform();

            ISeasonParameters fireSeason = Weather.GenerateSeason(seasons);

            if (SiteVars.PercentDeadFir[site] > 0) // If M3 or M4 type, use initProb if greater
            {
                if (SiteVars.PercentConifer[site] == 100 ||
                    fireSeason.LeafStatus == LeafOnOff.LeafOff)
                {
                    //find the fuelindex with surfacefuel M3
                    foreach (FuelType listFuel in FuelTypeParms)
                    {
                        if (listFuel.SurfaceFuel == SurfaceFuelType.M3)
                            if (listFuel.InitiationProbability > initProb) //Only use M3 initprob if > c-type
                                initProb = listFuel.InitiationProbability;
                    }

                }
                else
                {
                    //find the fuel index with surfacefuel M4
                    foreach (FuelType listFuel in FuelTypeParms)
                    {
                        if (listFuel.SurfaceFuel == SurfaceFuelType.M4)
                            if (listFuel.InitiationProbability > initProb) //Only use M4 initprob if > c-type
                                initProb = listFuel.InitiationProbability;
                    }
                }
            }
            if (randomNum <= initProb)
            {
                if (isDebugEnabled)
                    PlugIn.ModelCore.UI.WriteLine("   Generating a new fire event...");

                Event fireEvent = new Event(site, fireSeason, fireSizeType); //Must create event to determine season


                // Test that adequate weather data was retrieved:
                if (fireEvent.WindSpeed == 0 && fireEvent.FFMC == 0 && fireEvent.BuildUpIndex == 0)
                {
                    PlugIn.ModelCore.UI.WriteLine("   No weather data available:  {0}; fire_region = {1}.", fireEvent.FireSeason.NameOfSeason, fireEvent.InitiationFireRegion.Name);
                    return null;
                }

                if (fireEvent.FireSeason.PercentCuring == 0 && FuelTypeParms[fuelIndex].BaseFuel == BaseFuelType.Open)
                    return null;

                if (!fireEvent.Spread(site, fireSizeType, bui, severityCalibrate))
                    return null;
                else
                    return fireEvent;
            }
            else
            {
                if (isDebugEnabled)
                    PlugIn.ModelCore.UI.WriteLine("   Fire Event failed to initiate due to fuel type initiation probability");
                //return null;
            }
            return null;
        }

        //---------------------------------------------------------------------
        private bool Spread(ActiveSite initiationSite, SizeType fireSizeType, bool BUI, double severityCalibrate)
        {
            //First, check for fire overlap:
            if(SiteVars.Event[initiationSite] != null)
                return false;

            if (isDebugEnabled)
                PlugIn.ModelCore.UI.WriteLine("   Spreading fire event started at {0} ...", initiationSite.Location);

            IFireRegion fire_region = SiteVars.FireRegion[initiationSite];

            int totalSiteSeverities = 0;
            int siteCohortsKilled    = 0;
            int totalISI = 0;
            NumberDamagedSites = 1;

            this.InitiationFuel   = SiteVars.CFSFuelType[initiationSite];
            this.InitiationPercentConifer = SiteVars.PercentConifer[initiationSite];

            //PlugIn.ModelCore.UI.WriteLine("      Calculated max fire size or duration = {0:0.0}", maxFireParameter);
            //PlugIn.ModelCore.UI.WriteLine("      Fuel Type = {0}", activeFT.ToString());

            //Next, calculate the fire area:
            List<Site> FireLocations = new List<Site>();

            if (isDebugEnabled) PlugIn.ModelCore.UI.WriteLine("  Calling SizeFireCostSurface ...");

            FireLocations = EventRegion.SizeFireCostSurface(this, fireSizeType, BUI);

            if (isDebugEnabled) PlugIn.ModelCore.UI.WriteLine("    FireLocations.Count = {0}", FireLocations.Count);

            if (FireLocations.Count == 0) return false;

            //Attach travel time weights here
            if (isDebugEnabled)
                PlugIn.ModelCore.UI.WriteLine("  Computing SizeFireCostSurface ...");
            List<WeightedSite> FireCostSurface = new List<WeightedSite>(0);
            foreach(Site site in FireLocations)
            {
                double myWeight = SiteVars.TravelTime[site];
                if ((Double.IsNaN(myWeight))||(Double.IsInfinity(myWeight))) { }
                else
                {
                   FireCostSurface.Add(new WeightedSite(site, myWeight));
                }
            }
            WeightComparer weightComp = new WeightComparer();
            FireCostSurface.Sort(weightComp);
            FireLocations = new List<Site>();

            double cellArea = (PlugIn.ModelCore.CellLength * PlugIn.ModelCore.CellLength) / 10000; //convert to ha
            double totalArea = 0.0;
            int cellCnt = 0;
            double durMax = 0;

            if (isDebugEnabled)
                PlugIn.ModelCore.UI.WriteLine("  Determining cells burned ...");
            if (fireSizeType == SizeType.size_based)
            {

                foreach(WeightedSite weighted in FireCostSurface)
                {
                    //weightCnt++;
                    cellCnt++;
                    if(totalArea > this.MaxFireParameter)
                    {
                        SiteVars.Event[weighted.Site] = null;
                    }
                    else
                    {
                        totalArea += cellArea;
                        FireLocations.Add(weighted.Site);
                        if (SiteVars.TravelTime[weighted.Site] > durMax)
                            durMax = SiteVars.TravelTime[weighted.Site];
                    }
                }
                this.MaxDuration = durMax;
                //PlugIn.ModelCore.UI.WriteLine("   Fire Summary:  Cells Checked={0}, BurnedArea={1:0.0} (ha), Target Area={2:0.0} (ha).", cellCnt, totalArea, this.maxFireParameter);
                //if(totalArea < this.maxFireParameter)
                //    PlugIn.ModelCore.UI.WriteLine("      NOTE:  Partial fire burn; fire may have spread to the edge of the active area.");
            }
            else if (fireSizeType == SizeType.duration_based)
            {
                double durationAdj = this.MaxFireParameter;
                if (durationAdj >= 1440)
                    durationAdj = durationAdj * this.FireSeason.DayLengthProp;


                foreach(WeightedSite weighted in FireCostSurface)
                {
                    cellCnt++;
                    if (weighted.Site == this.initiationSite)
                    {
                        totalArea += cellArea;
                        FireLocations.Add(weighted.Site);
                        if (SiteVars.TravelTime[weighted.Site] > durMax)
                            durMax = SiteVars.TravelTime[weighted.Site];
                    }
                    else
                    {
                        if (weighted.Weight > durationAdj)
                        {
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
                }
                this.MaxDuration = durMax;

                //PlugIn.ModelCore.UI.WriteLine("   Fire Summary:  Cells Checked={0}, BurnedArea={1:0.0} (ha), Target Duration={2:0.0}, Adjusted Duration = {3:0.0}.", cellCnt, totalArea, this.maxFireParameter, durationAdj);
                //if(durationAdj - durMax > 5.0)
                //    PlugIn.ModelCore.UI.WriteLine("      NOTE:  Partial fire burn; fire may have spread to the edge of the active area.");
            }
            if (isDebugEnabled)
                PlugIn.ModelCore.UI.WriteLine("  FireLocations.Count = {0}", FireLocations.Count);
            int FMC = this.FMC;  //Foliar Moisture Content

            if (FireLocations.Count == 0) return false;

            if (isDebugEnabled)
                PlugIn.ModelCore.UI.WriteLine("  Damaging cohorts at burned sites ...");
            foreach(Site site in FireLocations)
            {
                currentSite = (ActiveSite) site;
                if(site.IsActive)
                {
                    this.NumSitesChecked++;

                    this.siteSeverity = FireSeverity.CalcFireSeverity(currentSite, this, severityCalibrate, FMC);
                    SiteVars.Severity[currentSite] = (byte)siteSeverity;
                    siteCohortsKilled = Damage(currentSite);

                    this.NumberDamagedSites++;
                    totalSiteSeverities += this.siteSeverity;
                    totalISI += (int) SiteVars.ISI[site];

                    IFireRegion siteFireRegion = SiteVars.FireRegion[site];
                    //SitesInEvent[siteFireRegion.Index]++;

                    SiteVars.Disturbed[currentSite] = true;
                    

                    if(siteSeverity > 0)
                        SiteVars.LastSeverity[currentSite] = (byte)siteSeverity;
                }
            }

            if (this.NumberDamagedSites == 0)
                this.EventSeverity = 0;
            else
                this.EventSeverity = ((double) totalSiteSeverities) / (double)this.NumberDamagedSites;

            this.ISI = (int) ((double) totalISI / (double) this.NumberDamagedSites);

            if (isDebugEnabled)
                PlugIn.ModelCore.UI.WriteLine("  Done spreading");
            return true;
        }
        //---------------------------------------------------------------------

        public static double ComputeSize(double meanSize, double sd, double maxSize)
        {

            double sizeGenerated = maxSize * 2.0;
            double minSize = 0.0;

            while(sizeGenerated > maxSize || sizeGenerated <= minSize)
            {
                PlugIn.ModelCore.LognormalDistribution.Mu = meanSize;      
                PlugIn.ModelCore.LognormalDistribution.Sigma = sd;   
                sizeGenerated = PlugIn.ModelCore.LognormalDistribution.NextDouble();
            }
            return sizeGenerated;
        }

        public static int ComputeSizeBin(double meanSize, double sd, double sizeGenerated)
        {
            // Percentile cutoffs from MN DNR (www.dnr.state.mn.us/forestry/fire/reports/canadian_indexes_o.html)
            double size5 = Math.Exp(meanSize + sd * 1.9600); // 97.5th percentil
            double size4 = Math.Exp(meanSize + sd * 1.2816); // 90th percentile
            double size3 = Math.Exp(meanSize + sd * 0.722);  // 76.5th percentile
            double size2 = Math.Exp(meanSize + sd * (-0.087)); // 46.5th percentile
            int sizeBin = 0;
            if (sizeGenerated >= size5)
                sizeBin = 5;
            else if (sizeGenerated >= size4)
                sizeBin = 4;
            else if (sizeGenerated >= size3)
                sizeBin = 3;
            else if (sizeGenerated >= size2)
                sizeBin = 2;
            else
                sizeBin = 1;
            return sizeBin;
        }

        //---------------------------------------------------------------------

        private int Damage(ActiveSite site)
        {
            int previousCohortsKilled = this.CohortsKilled;
            SiteVars.Cohorts[site].ReduceOrKillCohorts(this);
            return this.CohortsKilled - previousCohortsKilled;
        }

        //---------------------------------------------------------------------

        //  A filter to determine which cohorts are removed.

        int IDisturbance.ReduceOrKillMarkedCohort(ICohort cohort)
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
                float ageAsPercent = (float) cohort.Data.Age / (float) cohort.Species.Longevity;
                foreach(IFireDamage damage in damages)
                {
                    //IFireDamage damage = damages[i];
                    if (siteSeverity - SpeciesData.FireTolerance[cohort.Species] >= damage.SeverTolerDifference)
                    {
                        if (damage.MaxAge >= ageAsPercent)
                        {
                            killCohort = true;

                            return cohort.Data.Biomass; // No need to search further in the table
                        }
                    }
                }
            }

            if (killCohort) {
                this.CohortsKilled++;
            }
            return 0;
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Compares weights
        /// </summary>

        public class WeightComparer : IComparer<WeightedSite>
        {
            public int Compare(WeightedSite x,
                                              WeightedSite y)
            {
                int myCompare = x.Weight.CompareTo(y.Weight);
                return myCompare;
            }

        }

        private static double CalculateSF(int groundSlope)
        {
            return Math.Pow(Math.E, 3.533 * Math.Pow(((double)groundSlope / 100),1.2));  //FBP 39
        }


    }


    public class WeightedSite
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

        public WeightedSite (Site site, double weight)
        {
            this.site = site;
            this.weight = weight;
        }

    }


}
