//  Copyright 2006-2010 USFS Portland State University, Northern Research Station, University of Wisconsin
//  Authors:  Robert M. Scheller, Brian R. Miranda 

using Landis.Library.AgeOnlyCohorts;
using Landis.SpatialModeling;
using Landis.Core;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Reflection;

namespace Landis.Extension.DynamicFire
{


    ///<summary>
    /// A disturbance plug-in that simulates Fire disturbance.
    /// </summary>
    public class PlugIn
        : ExtensionMain 
    {
        private static readonly bool isDebugEnabled = false; 

        public static readonly ExtensionType Type = new ExtensionType("disturbance:fire");
        public static readonly string ExtensionName = "Dynamic Fire System";

        private string mapNameTemplate;
        private StreamWriter log;
        private StreamWriter summaryLog;
        private int[] summaryFireRegionEventCount;
        private int[] ecoregionSitesCount;

        private int summaryTotalSites;
        private int summaryEventCount;

        private SizeType fireSizeType;
        private bool bui;
        private double severityCalibrate;

        private ISeasonParameters[] seasonParameters;
        private List<IDynamicFireRegion> dynamicEcos;
        private List<IDynamicWeather> dynamicWeather;
        
        public static DataTable WeatherDataTable;

        public static int WeatherRandomizer = 0;

        private static IInputParameters parameters;

        private static ICore modelCore;

        //---------------------------------------------------------------------

        public PlugIn()
            : base(ExtensionName, Type)
        {
        }

        //---------------------------------------------------------------------

        public static ICore ModelCore
        {
            get
            {
                return modelCore;
            }
        }
        
        //---------------------------------------------------------------------

        public override void LoadParameters(string dataFile,
                                            ICore mCore)
        {
            modelCore = mCore;
            SiteVars.Initialize();
            InputParameterParser parser = new InputParameterParser();
            parameters = modelCore.Load<IInputParameters>(dataFile, parser);
            WeatherDataTable = Weather.ReadWeatherFile(parameters.InitialWeatherPath, FireRegions.Dataset, parameters.SeasonParameters);

        }

        //---------------------------------------------------------------------

        public override void Initialize()
        {
            Timestep            = parameters.Timestep;
            fireSizeType        = parameters.FireSizeType;
            bui                 = parameters.BUI;
            mapNameTemplate     = parameters.MapNamesTemplate;
            dynamicWeather      = parameters.DynamicWeather;
            severityCalibrate   = parameters.SeverityCalibrate;

            
            modelCore.Log.WriteLine("   Initializing Fire Events...");
            Event.Initialize(parameters.SeasonParameters, parameters.FuelTypeParameters, parameters.FireDamages);


            seasonParameters = parameters.SeasonParameters;

            dynamicEcos = parameters.DynamicFireRegions;

            summaryFireRegionEventCount = new int[FireRegions.Dataset.Count];
            ecoregionSitesCount        = new int[FireRegions.Dataset.Count];

            //foreach (IFireRegion fire_region in FireRegions.Dataset)
            //modelCore.Log.WriteLine("   FireSize={0}, SD={1}", fire_region.MeanSize, fire_region.StandardDeviation);

            // Count the number of sites per fire_region:
            foreach (Site site in modelCore.Landscape)
            {
                if (site.IsActive)
                {
                    IFireRegion fire_region = SiteVars.FireRegion[site];
                    ecoregionSitesCount[fire_region.Index] ++;
                }
            }

            modelCore.Log.WriteLine("   Opening and Initializing Fire log files \"{0}\" and \"{1}\"...", parameters.LogFileName, parameters.SummaryLogFileName);
            try {
                log = modelCore.CreateTextFile(parameters.LogFileName);
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
                summaryLog = modelCore.CreateTextFile(parameters.SummaryLogFileName);
            }
            catch (Exception err) {
                string mesg = string.Format("{0}", err.Message);
                throw new System.ApplicationException(mesg);
            }

            summaryLog.AutoFlush = true;
            summaryLog.Write("TimeStep, TotalSitesBurned, NumberFires");
            foreach (IFireRegion fire_region in FireRegions.Dataset)
            {
                    summaryLog.Write(", eco-{0}", fire_region.MapCode);
            }
            summaryLog.WriteLine("");
            summaryLog.Write("0, 0, 0");
            foreach (IFireRegion fire_region in FireRegions.Dataset)
            {
                    summaryLog.Write(", {0}", ecoregionSitesCount[fire_region.Index]);
            }
            summaryLog.WriteLine("");

            if (isDebugEnabled)
                modelCore.Log.WriteLine("Initialization done");
        }

        
        //---------------------------------------------------------------------

        ///<summary>
        /// Run the plug-in at a particular timestep.
        ///</summary>
        public override void Run()
        {

            SiteVars.InitializeFuelType();
            modelCore.Log.WriteLine("   Processing landscape for Fire events ...");

            if (FireRegions.Dataset.Count == 0)
                throw new ApplicationException("Fire region data set is empty.");

            SiteVars.Event.SiteValues = null;
            SiteVars.Severity.ActiveSiteValues = 0;
            SiteVars.Disturbed.ActiveSiteValues = false;
            SiteVars.TravelTime.ActiveSiteValues = Double.PositiveInfinity;
            SiteVars.MinNeighborTravelTime.ActiveSiteValues = Double.PositiveInfinity;
            SiteVars.RateOfSpread.ActiveSiteValues = 0.0;

            foreach (IFireRegion fire_region in FireRegions.Dataset)
            {
                summaryFireRegionEventCount[fire_region.Index] = 0;
            }

            summaryTotalSites = 0;
            summaryEventCount = 0;

            // Update the FireRegions Map as necessary:
            //modelCore.Log.WriteLine("    Dynamic Fire:  Loading Dynamic Fire Regions...");
            foreach(IDynamicFireRegion dyneco in dynamicEcos)
            {
                 if(dyneco.Year == modelCore.CurrentTime)
                 {
                    modelCore.Log.WriteLine("   Reading in new Fire FireRegions Map {0}.", dyneco.MapName);
                     foreach (IFireRegion fire_region in FireRegions.Dataset)
                     {
                         fire_region.FireRegionSites.Clear(); // = new List<Location>();
                     }
                    FireRegions.ReadMap(dyneco.MapName); //Sites added to their respective fire_region lists
                 }
            }

            //Update the weather table as necessary:
            foreach (IDynamicWeather dynweather in dynamicWeather)
            {
                if (dynweather.Year == modelCore.CurrentTime)
                {
                    modelCore.Log.WriteLine("  Reading in new Weather Table {0}", dynweather.FileName);
                    WeatherDataTable = Weather.ReadWeatherFile(dynweather.FileName, FireRegions.Dataset, seasonParameters);
                }
            }

            // Fill in open types as needed:
            modelCore.Log.WriteLine("   Dynamic Fire:  Filling open types as needed ...");
            foreach (ActiveSite site in modelCore.Landscape)
            {
                
                IFireRegion fire_region = SiteVars.FireRegion[site];

                if(fire_region == null)
                    throw new System.ApplicationException("Error: SiteVars.FireRegion is empty.");

                //if(SiteVars.CFSFuelType[site] == 0)
                //    throw new System.ApplicationException("Error: SiteVars.CFSFuelType is empty.");


                if(Event.FuelTypeParms[SiteVars.CFSFuelType[site]] == null)
                {
                    modelCore.Log.WriteLine("Error:  SiteVars.CFSFuelType[site]={0}.", SiteVars.CFSFuelType[site]);
                    throw new System.ApplicationException("Error: Event BaseFuel Empty.");
                }

                if(Event.FuelTypeParms[SiteVars.CFSFuelType[site]].BaseFuel == BaseFuelType.NoFuel)
                {
                    if(SiteVars.PercentDeadFir[site] == 0)
                        SiteVars.CFSFuelType[site] = fire_region.OpenFuelType;
                }
            }
            if (isDebugEnabled)
                modelCore.Log.WriteLine("Done filling open types");

            modelCore.Log.WriteLine("   Dynamic Fire:  Igniting Fires ...");
            foreach (IFireRegion fire_region in FireRegions.Dataset)
            {
                if (isDebugEnabled)
                    modelCore.Log.WriteLine("   There are {0} site locations in fire region {1}", fire_region.FireRegionSites.Count, fire_region.Name);
                if (fire_region.EcoIgnitionNum > 0)
                {
                    //PoissonDistribution randVar = new PoissonDistribution(RandomNumberGenerator.Singleton);
                    double doubleLambda;
                    int ignGenerated = 0;

                    if (isDebugEnabled)
                        modelCore.Log.WriteLine("{0}: EcoIgnitionNum = {1}, computing ignGenerated ...", fire_region.Name, fire_region.EcoIgnitionNum);
                    if (fire_region.EcoIgnitionNum < 1)
                    {
                        // Adjust ignition probability for multiple years
                        // (The inverse of the probability of NOT having any ignition for the time period.)
                        // P = 1 - (1-Pignition)^timestep
                        //doubleLambda = 1 - System.Math.Pow(1.0 - fire_region.EcoIgnitionNum, Timestep);

                        for (int i = 1; i <= Timestep; i++)
                        {
                            int annualFires = 0;
                            if (modelCore.GenerateUniform() <= fire_region.EcoIgnitionNum)
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
                        PlugIn.ModelCore.PoissonDistribution.Lambda = doubleLambda;
                        //randVar.Lambda = doubleLambda;

                        for (int i = 1; i <= Timestep; i++)
                        {
                            int annualFires = PlugIn.ModelCore.PoissonDistribution.Next();
                            if (boolLarge)
                                annualFires = annualFires * 10;  //readjust if necessary.
                            ignGenerated += annualFires;
                        }
                    }
                    if (isDebugEnabled)
                        modelCore.Log.WriteLine("  Ignitions generated = {0}; Shuffling {0} cells ...", ignGenerated, fire_region.FireRegionSites.Count);

                    List<Location> cellsPerFireRegion = new List<Location>(0);
                    foreach (Location location in fire_region.FireRegionSites)
                        cellsPerFireRegion.Add(location);

                    cellsPerFireRegion = Shuffle(cellsPerFireRegion);
                    int fireCount = 0;

                    //Try to create poissonNumber of fires in each fire_region.
                    //Fires should only initiate if a fire event has not previously occurred
                    //at that site.

                    foreach (Location siteLocation in cellsPerFireRegion)
                    {

                        Site site = modelCore.Landscape.GetSite(siteLocation);

                        ActiveSite asite = (ActiveSite) site;

                        if (fireCount >= ignGenerated) continue;  //exit loop if the required number of fires has occurred.
                        if (SiteVars.Event[asite] == null)
                        {
                            fireCount++;
                            if (isDebugEnabled)
                                modelCore.Log.WriteLine("    fireCount = {0}", fireCount);
                            Event FireEvent = Event.Initiate(asite, Timestep, fireSizeType, bui, seasonParameters, severityCalibrate);
                            if (isDebugEnabled)
                                modelCore.Log.WriteLine("    fire event {0} started at {1}",
                                                     FireEvent == null ? "not ": "",
                                                     asite.Location);
                            if (FireEvent != null)
                            {
                                LogEvent(modelCore.CurrentTime, FireEvent);
                                summaryEventCount++;
                            //fireCount++;  //RMS test
                            }
                        }
                    }
                }


            }

            // Track the time of last fire; registered in SiteVars.cs for other extensions to access.
            if (isDebugEnabled)
                modelCore.Log.WriteLine("Assigning TimeOfLastFire site var ...");
            foreach (Site site in modelCore.Landscape.AllSites)
                if(SiteVars.Disturbed[site])
                    SiteVars.TimeOfLastFire[site] = modelCore.CurrentTime;


            //  Write Fire severity map
            string path = MapNames.ReplaceTemplateVars(mapNameTemplate, modelCore.CurrentTime);
            modelCore.Log.WriteLine("   Writing Fire severity map to {0} ...", path);
            using (IOutputRaster<UShortPixel> outputRaster = modelCore.CreateRaster<UShortPixel>(path, modelCore.Landscape.Dimensions))
            {
                UShortPixel pixel = outputRaster.BufferPixel;
                foreach (Site site in modelCore.Landscape.AllSites)
                {
                    if (site.IsActive) {
                        if (SiteVars.Disturbed[site])
                            pixel.MapCode.Value = (byte) (SiteVars.Severity[site] + 2);
                        else
                            pixel.MapCode.Value = 1;
                    }
                    else {
                        //  Inactive site
                        pixel.MapCode.Value = 0;
                    }
                    outputRaster.WriteBufferPixel();
                }
            }

            path = MapNames.ReplaceTemplateVars("./DFFS-output/TimeOfLastFire-{timestep}.img", modelCore.CurrentTime);
            modelCore.Log.WriteLine("   Writing Travel Time output (with ushort values) map to {0} ...", path);
            using (IOutputRaster<UShortPixel> outputRaster = modelCore.CreateRaster<UShortPixel>(path, modelCore.Landscape.Dimensions))
            {
                UShortPixel pixel = outputRaster.BufferPixel;
                foreach (Site site in modelCore.Landscape.AllSites)
                {
                    if (site.IsActive)
                        pixel.MapCode.Value = (ushort)(SiteVars.TimeOfLastFire[site]);
                    else
                        pixel.MapCode.Value = 0;
                    outputRaster.WriteBufferPixel();
                }
            }

            WriteSummaryLog(modelCore.CurrentTime);

            if (isDebugEnabled)
                modelCore.Log.WriteLine("Done running extension");

        
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
            summaryLog.WriteLine("");
        }

        private static List<Location> Shuffle<Location>(List<Location> list)
        {
            List<Location> shuffledList = new List<Location>();

            int randomIndex = 0;
            while (list.Count > 0)
            {
                //randomIndex = modelCore.GenerateUniform(list.Count); //Choose a random object in the list
                randomIndex = (int) (list.Count * PlugIn.ModelCore.GenerateUniform());
                shuffledList.Add(list[randomIndex]); //add it to the new, random list
                list.RemoveAt(randomIndex); //remove to avoid duplicates
            }

            return shuffledList;
        }


    }
}
