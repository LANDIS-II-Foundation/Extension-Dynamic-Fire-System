//  Copyright 2006-2008 USFS Northern Research Station, Conservation Biology Institute, University of Wisconsin
//  Authors:  Robert M. Scheller, Brian R. Miranda
//  License:  Available at
//  http://www.landis-ii.org/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Edu.Wisc.Forest.Flel.Grids;
using Landis.AgeCohort;
using Landis.Landscape;
using Landis.PlugIns;
using Landis.RasterIO;
using Landis.Util;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Reflection;
using Troschuetz.Random;

namespace Landis.Fire
{


    ///<summary>
    /// A disturbance plug-in that simulates Fire disturbance.
    /// </summary>
    public class PlugIn
        : PlugIns.PlugIn, PlugIns.I2PhaseInitialization
    {
        //private static readonly ILog debugLog = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly bool isDebugEnabled = false; //debugLog.IsDebugEnabled;

        public static readonly PlugInType Type = new PlugInType("disturbance:fire");

        private string mapNameTemplate;
        private StreamWriter log;
        private StreamWriter summaryLog;
        private int[] summaryFireRegionEventCount;
        private int[] summaryFireRegionSeverity;
        private int[] ecoregionSitesCount;

        private int summaryTotalSites;
        private int summaryEventCount;

        private SizeType fireSizeType;
        private bool bui;
        private double severityCalibrate;

        private ISeasonParameters[] seasonParameters;
        private List<IDynamicFireRegion> dynamicEcos;
        private List<IDynamicWeather> dynamicWeather;
        private ILandscapeCohorts cohorts;

        public static DataTable WeatherDataTable;

        public static int WeatherRandomizer = 0;

        //---------------------------------------------------------------------

        public PlugIn()
            : base("Dynamic Fire System", Type)
        {
        }


        //---------------------------------------------------------------------

        public override void Initialize(string        dataFile,
                                        PlugIns.ICore modelCore)
        {
            if (isDebugEnabled)
                UI.WriteLine("Initializing {0} ...", Name);

            Model.Core = modelCore;

            cohorts = Model.Core.SuccessionCohorts as ILandscapeCohorts;
            if (cohorts == null)
                throw new ApplicationException("Error: Cohorts don't support age-cohort interface");

            SiteVars.Initialize(cohorts);

            InputParameterParser parser = new InputParameterParser();
            IInputParameters parameters = Data.Load<IInputParameters>(dataFile, parser);

            Timestep = parameters.Timestep;
            fireSizeType = parameters.FireSizeType;
            bui = parameters.BUI;

            mapNameTemplate = parameters.MapNamesTemplate;
            dynamicWeather = parameters.DynamicWeather;
            severityCalibrate = parameters.SeverityCalibrate;

            WeatherDataTable = Weather.ReadWeatherFile(parameters.InitialWeatherPath, FireRegions.Dataset, parameters.SeasonParameters);

            UI.WriteLine("   Initializing Fire Events...");
            Event.Initialize(parameters.SeasonParameters, parameters.FuelTypeParameters, parameters.FireDamages);


            seasonParameters = parameters.SeasonParameters;

            dynamicEcos = parameters.DynamicFireRegions;

            summaryFireRegionEventCount = new int[FireRegions.Dataset.Count];
            summaryFireRegionSeverity = new int[FireRegions.Dataset.Count];
            ecoregionSitesCount = new int[FireRegions.Dataset.Count];

            //foreach (IFireRegion fire_region in FireRegions.Dataset)
            //UI.WriteLine("   FireSize={0}, SD={1}", fire_region.MeanSize, fire_region.StandardDeviation);

            // Count the number of sites per fire_region:
            foreach (Site site in Model.Core.Landscape)
            {
                if (site.IsActive)
                {
                    IFireRegion fire_region = SiteVars.FireRegion[site];
                    ecoregionSitesCount[fire_region.Index] ++;
                }
            }

            UI.WriteLine("   Opening and Initializing Fire log files \"{0}\" and \"{1}\"...", parameters.LogFileName, parameters.SummaryLogFileName);
            try {
                log = Data.CreateTextFile(parameters.LogFileName);
            }
            catch (Exception err) {
                string mesg = string.Format("{0}", err.Message);
                throw new System.ApplicationException(mesg);
            }

            log.AutoFlush = true;
            log.Write("Time,InitSite,InitFireRegion,InitFuel,InitPercentConifer,SelectedSizeOrDuration,SizeBin,Duration,FireSeason,WindSpeed,WindDirection,FFMC,BUI,PercentCuring,ISI,SitesChecked,CohortsKilled,MeanSeverity,");
            foreach (IFireRegion fire_region in FireRegions.Dataset)
            {
                  log.Write("eco-{0},", fire_region.MapCode);
            }
            log.Write("TotalSitesInEvent");
            log.WriteLine("");

            try {
                summaryLog = Data.CreateTextFile(parameters.SummaryLogFileName);
            }
            catch (Exception err) {
                string mesg = string.Format("{0}", err.Message);
                throw new System.ApplicationException(mesg);
            }

            summaryLog.AutoFlush = true;
            summaryLog.Write("TimeStep, TotalSitesBurned, NumberFires");
            foreach (IFireRegion fire_region in FireRegions.Dataset)
            {
                    summaryLog.Write(", eco-num-sites-{0}", fire_region.MapCode);
            }
            foreach (IFireRegion fire_region in FireRegions.Dataset)
            {
                summaryLog.Write(", eco-mean-severity-{0}", fire_region.MapCode);
            }
            summaryLog.WriteLine("");
            summaryLog.Write("0, 0, 0");
            foreach (IFireRegion fire_region in FireRegions.Dataset)
            {
                    summaryLog.Write(", {0}", ecoregionSitesCount[fire_region.Index]);
            }
            foreach (IFireRegion fire_region in FireRegions.Dataset)
            {
                summaryLog.Write(", 0.0");
            }
            summaryLog.WriteLine("");

            if (isDebugEnabled)
                UI.WriteLine("Initialization done");
        }

        //---------------------------------------------------------------------

        void PlugIns.I2PhaseInitialization.InitializePhase2()
        {
            SiteVars.InitializeFuelType();
        }

        //---------------------------------------------------------------------

        ///<summary>
        /// Run the plug-in at a particular timestep.
        ///</summary>
        public override void Run()
        {
            if (isDebugEnabled)
                UI.WriteLine("Running {0} at time = {1}", Name, Model.Core.CurrentTime);

            UI.WriteLine("   Processing landscape for Fire events ...");

            SiteVars.Event.SiteValues = null;
            SiteVars.Severity.ActiveSiteValues = 0;
            SiteVars.Disturbed.ActiveSiteValues = false;
            SiteVars.TravelTime.ActiveSiteValues = Double.PositiveInfinity;
            SiteVars.MinNeighborTravelTime.ActiveSiteValues = Double.PositiveInfinity;
            SiteVars.RateOfSpread.ActiveSiteValues = 0.0;

            foreach (IFireRegion fire_region in FireRegions.Dataset)
            {
                summaryFireRegionEventCount[fire_region.Index] = 0;
                summaryFireRegionSeverity[fire_region.Index] = 0;
            }

            summaryTotalSites = 0;
            summaryEventCount = 0;

            // Update the FireRegions Map as necessary:
            //UI.WriteLine("    Dynamic Fire:  Loading Dynamic Fire Regions...");
            foreach(IDynamicFireRegion dyneco in dynamicEcos)
            {
                 if(dyneco.Year == Model.Core.CurrentTime)
                 {
                    UI.WriteLine("   Reading in new Fire FireRegions Map {0}.", dyneco.MapName);
                     foreach (IFireRegion fire_region in FireRegions.Dataset)
                     {
                         fire_region.FireRegionSites = new List<Location>();
                     }
                    FireRegions.ReadMap(dyneco.MapName); //Sites added to their respective fire_region lists
                 }
            }

            //Update the weather table as necessary:
            //UI.WriteLine("    Dynamic Fire:  Loading Dynamic Weather ...");
            foreach (IDynamicWeather dynweather in dynamicWeather)
            {
                if (dynweather.Year == Model.Core.CurrentTime)
                {
                    UI.WriteLine("  Reading in new Weather Table {0}", dynweather.FileName);
                    WeatherDataTable = Weather.ReadWeatherFile(dynweather.FileName, FireRegions.Dataset, seasonParameters);
                    //Weather.ReadFileName(, seasonParameters, FireRegions.Dataset);
                }
            }



            // Fill in open types as needed:
            if (isDebugEnabled)
                UI.WriteLine("Filling open types as needed ...");

            UI.WriteLine("      Dynamic Fire:  Filling open types as needed ...");
            foreach (ActiveSite site in Model.Core.Landscape)
            {
                IFireRegion fire_region = SiteVars.FireRegion[site];

                if(fire_region == null)
                    throw new System.ApplicationException("Error: SiteVars.FireRegion is empty.");

                //if(SiteVars.CFSFuelType[site] == 0)
                //    throw new System.ApplicationException("Error: SiteVars.CFSFuelType is empty.");

                if(Event.FuelTypeParms[SiteVars.CFSFuelType[site]] == null)
                {
                    UI.WriteLine("Error:  SiteVars.CFSFuelType[site]={0}.", SiteVars.CFSFuelType[site]);
                    throw new System.ApplicationException("Error: Event BaseFuel Empty.");
                }

                if(Event.FuelTypeParms[SiteVars.CFSFuelType[site]].BaseFuel == BaseFuelType.NoFuel)
                {
                    if(SiteVars.PercentDeadFir[site] == 0)
                        SiteVars.CFSFuelType[site] = fire_region.OpenFuelType;
                }
            }
            if (isDebugEnabled)
                UI.WriteLine("Done filling open types");

            UI.WriteLine("      Dynamic Fire:  Igniting Fires ...");
            foreach (IFireRegion fire_region in FireRegions.Dataset)
            {
                if (fire_region.EcoIgnitionNum > 0)
                {
                    PoissonDistribution randVar = new PoissonDistribution(RandomNumberGenerator.Singleton);
                    double doubleLambda;
                    int ignGenerated = 0;

                    if (isDebugEnabled)
                        UI.WriteLine("{0}: EcoIgnitionNum = {1}, computing ignGenerated ...",
                                             fire_region.Name, fire_region.EcoIgnitionNum);
                    if (fire_region.EcoIgnitionNum < 1)
                    {
                        // Adjust ignition probability for multiple years
                        // (The inverse of the probability of NOT having any ignition for the time period.)
                        // P = 1 - (1-Pignition)^timestep
                        //doubleLambda = 1 - System.Math.Pow(1.0 - fire_region.EcoIgnitionNum, Timestep);

                        for (int i = 1; i <= Timestep; i++)
                        {
                            int annualFires = 0;
                            if (Util.Random.GenerateUniform() <= fire_region.EcoIgnitionNum)
                            {
                                annualFires = 1;
                            }
                            ignGenerated += annualFires;
                        }
                    }
                    else
                    {
                        doubleLambda = fire_region.EcoIgnitionNum;
                        bool boolLarge = false;

                        // 745 is the upper limit for valid Poisson lambdas.  If greater than
                        // 745, divide by 10 and readjust back up below.
                        if (doubleLambda > 745)
                        {
                            doubleLambda = doubleLambda / 10;
                            boolLarge = true;
                        }
                        //bool boolLambda = randVar.IsValidLambda(doubleLambda);
                        randVar.Lambda = doubleLambda;

                        for (int i = 1; i <= Timestep; i++)
                        {
                            int annualFires = randVar.Next();
                            if (boolLarge)
                                annualFires = annualFires * 10;  //readjust if necessary.
                            ignGenerated += annualFires;
                        }
                    }
                    if (isDebugEnabled)
                        UI.WriteLine("  ignGenerated = {0}; Shuffling {0} cells ...",
                                             ignGenerated, fire_region.FireRegionSites.Count);

                    List<Location> cellsPerFireRegion = fire_region.FireRegionSites;
                    Landis.Util.Random.Shuffle(cellsPerFireRegion);
                    int fireCount = 0;

                    //Try to create poissonNumber of fires in each fire_region.
                    //Fires should only initiate if a fire event has not previously occurred
                    //at that site.
                    if (isDebugEnabled)
                    {
                        UI.WriteLine("  Trying to create fires... ");
                        UI.WriteLine("    there are {0} site locations in fire region {1}", cellsPerFireRegion.Count, fire_region.Name);
                    }

                    foreach (Location siteLocation in cellsPerFireRegion)
                    {

                        Site site = Model.Core.Landscape.GetSite(siteLocation);

                        ActiveSite asite = site as ActiveSite;

                        if (fireCount >= ignGenerated) continue;  //exit loop if the required number of fires has occurred.
                        if (SiteVars.Event[asite] == null)
                        {
                            fireCount++;
                            if (isDebugEnabled)
                                UI.WriteLine("    fireCount = {0}", fireCount);
                            Event FireEvent = Event.Initiate(asite, Timestep, fireSizeType, bui, seasonParameters, severityCalibrate);
                            if (isDebugEnabled)
                                UI.WriteLine("    fire event {0}started at {1}",
                                                     FireEvent == null ? "not ": "",
                                                     asite.Location);
                            if (FireEvent != null)
                            {
                                LogEvent(Model.Core.CurrentTime, FireEvent);
                                summaryEventCount++;
                            //fireCount++;  //RMS test
                            }
                        }
                    }
                }


            }

            // Track the time of last fire; registered in SiteVars.cs for other extensions to access.
            if (isDebugEnabled)
                UI.WriteLine("Assigning TimeOfLastFire site var ...");
            foreach (Site site in Model.Core.Landscape.AllSites)
                if(SiteVars.Disturbed[site])
                    SiteVars.TimeOfLastFire[site] = Model.Core.CurrentTime;


            //  Write Fire severity map
            UI.WriteLine("      Dynamic Fire:  Write Severity Map ...");

            string path = MapNames.ReplaceTemplateVars(mapNameTemplate, Model.Core.CurrentTime);
            IOutputRaster<SeverityPixel> map = CreateMap(path);
            using (map) {
                SeverityPixel pixel = new SeverityPixel();
                foreach (Site site in Model.Core.Landscape.AllSites) {
                    if (site.IsActive) {
                        if (SiteVars.Disturbed[site])
                        {
                            pixel.Band0 = (byte)(SiteVars.Severity[site] + 2);
                            summaryFireRegionSeverity[SiteVars.FireRegion[site].Index] += SiteVars.Severity[site];
                        }
                        else
                            pixel.Band0 = 1;
                    }
                    else {
                        //  Inactive site
                        pixel.Band0 = 0;
                    }
                    map.WritePixel(pixel);
                }
            }
            /*
            //  Write travel time map

            path = MapNames.ReplaceTemplateVars("./DFFS-output/travel-time-{timestep}.gis", Model.Core.CurrentTime);
            IOutputRaster<UShortPixel> umap = CreateTravelTimeMap(path);
            using (umap) {
                UShortPixel pixel = new UShortPixel();
                          foreach (Site site in Model.Core.Landscape.AllSites) {
                    if (site.IsActive) {
                        if (!Double.IsPositiveInfinity(SiteVars.TravelTime[site]))
                        //if (SiteVars.Event[site] != null)
                            pixel.Band0 = (ushort) ((SiteVars.TravelTime[site]) + 2);
                        else
                            pixel.Band0 = 1;
                    }
                    else {
                        //  Inactive site
                        pixel.Band0 = 0;
                    }
                    umap.WritePixel(pixel);
                }
            }
            */
            /*
            //  Write topo map
            path = MapNames.ReplaceTemplateVars("./DFFS-output/topo-{timestep}.gis", Model.Core.CurrentTime);
            IOutputRaster<TopoPixel> tmap = CreateTopoMap(path);
            using (tmap)
            {
                TopoPixel pixel = new TopoPixel();
                foreach (Site site in Model.Core.Landscape.AllSites)
                {
                    if (site.IsActive)
                    {
                        pixel.Band0 = (ushort)(SiteVars.GroundSlope[site]);

                    }
                    else
                    {
                        //  Inactive site
                        pixel.Band0 = 0;
                    }
                    tmap.WritePixel(pixel);
                }
            }
            */
            /*
            //  Write wind speed map

            path = MapNames.ReplaceTemplateVars("./DFFS-output/WSV-{timestep}.gis", Model.Core.CurrentTime);
            IOutputRaster<UShortPixel> wsvmap = CreateTravelTimeMap(path);
            using (wsvmap)
            {
                UShortPixel pixel = new UShortPixel();
                foreach (Site site in Model.Core.Landscape.AllSites)
                {
                    if (site.IsActive)
                    {
                        pixel.Band0 = (ushort)(SiteVars.SiteWindSpeed[site]);

                    }
                    else
                    {
                        //  Inactive site
                        pixel.Band0 = 0;
                    }
                    wsvmap.WritePixel(pixel);
                }
            }
            */
            /*
            //  Write wind direction map

            path = MapNames.ReplaceTemplateVars("./DFFS-output/WindDir-{timestep}.gis", Model.Core.CurrentTime);
            IOutputRaster<UShortPixel> winddirmap = CreateTravelTimeMap(path);
            using (winddirmap)
            {
                UShortPixel pixel = new UShortPixel();
                foreach (Site site in Model.Core.Landscape.AllSites)
                {
                    if (site.IsActive)
                    {
                        pixel.Band0 = (ushort)(SiteVars.SiteWindDirection[site]);

                    }
                    else
                    {
                        //  Inactive site
                        pixel.Band0 = 0;
                    }
                    winddirmap.WritePixel(pixel);
                }
            }
            */
            /*
            //  Write ROS map

            /*path = MapNames.ReplaceTemplateVars("./DFFS-output/ROS-{timestep}.gis", Model.Core.CurrentTime);
            IOutputRaster<UShortPixel> rosmap = CreateTravelTimeMap(path);
            using (rosmap)
            {
                UShortPixel pixel = new UShortPixel();
                foreach (Site site in Model.Core.Landscape.AllSites)
                {
                    if (site.IsActive)
                    {
                        pixel.Band0 = (ushort)((SiteVars.RateOfSpread[site]) + 1);

                    }
                    else
                    {
                        //  Inactive site
                        pixel.Band0 = 0;
                    }
                    rosmap.WritePixel(pixel);
                }
            }*/

            /*
            //  Write AdjROS map

            path = MapNames.ReplaceTemplateVars("./DFFS-output/AdjROS-{timestep}.gis", Model.Core.CurrentTime);
            IOutputRaster<UShortPixel> adjmap = CreateTravelTimeMap(path);
            using (adjmap)
            {
                UShortPixel pixel = new UShortPixel();
                foreach (Site site in Model.Core.Landscape.AllSites)
                {
                    if (site.IsActive)
                    {
                        pixel.Band0 = (ushort)(((SiteVars.AdjROS[site]) * 100) + 1);

                    }
                    else
                    {
                        //  Inactive site
                        pixel.Band0 = 0;
                    }
                    adjmap.WritePixel(pixel);
                }
            }
            */

            //  Write TimeOfLastFire map

            path = MapNames.ReplaceTemplateVars("./DFFS-output/TimeOfLastFire-{timestep}.gis", Model.Core.CurrentTime);
            IOutputRaster<UShortPixel> tolfMap = CreateTravelTimeMap(path);
            using (tolfMap)
            {
                UShortPixel pixel = new UShortPixel();
                foreach (Site site in Model.Core.Landscape.AllSites)
                {
                    if (site.IsActive)
                    {
                        pixel.Band0 = (ushort)(SiteVars.TimeOfLastFire[site]);

                    }
                    else
                    {
                        //  Inactive site
                        pixel.Band0 = 0;
                    }
                    tolfMap.WritePixel(pixel);
                }
            }

            WriteSummaryLog(Model.Core.CurrentTime);

            if (isDebugEnabled)
                UI.WriteLine("Done running extension");
        }

        //---------------------------------------------------------------------

        private void LogEvent(int   currentTime,
                              Event fireEvent)
        {

        //HEADER:  Time,Initiation Site,Sites Checked,Cohorts Killed,Mean Severity,
            int totalSitesInEvent = 0;
            if (fireEvent.EventSeverity > 0)
            {
                log.Write("{0},\"{1}\",{2},{3},{4},{5:0.00},{6},{7:0.00},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17:0.00}",
                          currentTime,
                          fireEvent.StartLocation,
                          fireEvent.InitiationFireRegion.Name,
                          fireEvent.InitiationFuel,
                          fireEvent.InitiationPercentConifer,
                          fireEvent.MaxFireParameter,
                          fireEvent.SizeBin,
                          fireEvent.MaxDuration,
                          fireEvent.FireSeason.NameOfSeason,
                          fireEvent.WindSpeed,
                          fireEvent.WindDirection,
                          fireEvent.FFMC,
                          fireEvent.BuildUpIndex,
                          fireEvent.FireSeason.PercentCuring,
                          fireEvent.ISI,
                          fireEvent.NumSitesChecked,
                          fireEvent.CohortsKilled,
                          fireEvent.EventSeverity);
                //----------
                foreach (IFireRegion fire_region in FireRegions.Dataset)
                {
                        log.Write(",{0}", fireEvent.SitesInEvent[fire_region.Index]);
                        totalSitesInEvent += fireEvent.SitesInEvent[fire_region.Index];
                        summaryFireRegionEventCount[fire_region.Index] += fireEvent.SitesInEvent[fire_region.Index];
                }
                summaryTotalSites += totalSitesInEvent;
                log.Write(", {0}", totalSitesInEvent);
                log.WriteLine("");
            }
        }

        //---------------------------------------------------------------------

        private void WriteSummaryLog(int   currentTime)
        {
            summaryLog.Write("{0},{1},{2}", currentTime, summaryTotalSites, summaryEventCount);
            foreach (IFireRegion fire_region in FireRegions.Dataset)
            {
                    summaryLog.Write(",{0}", summaryFireRegionEventCount[fire_region.Index]);
            }
            foreach (IFireRegion fire_region in FireRegions.Dataset)
            {
                summaryLog.Write(",{0:0.00}", ((double) summaryFireRegionSeverity[fire_region.Index]) / (double) summaryFireRegionEventCount[fire_region.Index]);
            }
            summaryLog.WriteLine("");
        }
        //---------------------------------------------------------------------

        private IOutputRaster<SeverityPixel> CreateMap(string path)
        {
            UI.WriteLine("Writing Fire severity map to {0} ...", path);
            return Model.Core.CreateRaster<SeverityPixel>(path,
                                                          Model.Core.Landscape.Dimensions,
                                                          Model.Core.LandscapeMapMetadata);
        }

        private IOutputRaster<UShortPixel> CreateTravelTimeMap(string path)
        {
            UI.WriteLine("Writing Travel Time output (with ushort values) map to {0} ...", path);
            return Model.Core.CreateRaster<UShortPixel>(path,
                                                          Model.Core.Landscape.Dimensions,
                                                          Model.Core.LandscapeMapMetadata);
        }


        private IOutputRaster<TopoPixel> CreateTopoMap(string path)
        {
            UI.WriteLine("Writing Topo map to {0} ...", path);
            return Model.Core.CreateRaster<TopoPixel>(path,
                                                          Model.Core.Landscape.Dimensions,
                                                          Model.Core.LandscapeMapMetadata);
        }



    }
}
