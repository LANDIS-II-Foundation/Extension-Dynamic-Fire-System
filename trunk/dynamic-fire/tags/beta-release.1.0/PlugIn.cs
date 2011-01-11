//  Copyright 2005 University of Wisconsin
//  Authors:  Robert M. Scheller, James B. Domingo
//  License:  Available at  
//  http://landis.forest.wisc.edu/developers/LANDIS-IISourceCodeLicenseAgreement.pdf
//  Edited by Brian R. Miranda (BRM)

using Landis.AgeCohort;
using Landis.Ecoregions;
using Landis.Landscape;
using Landis.PlugIns;
using Landis.RasterIO;
using Landis.Util;
using System.Collections.Generic;
using System.IO;
using System;
using Edu.Wisc.Forest.Flel.Grids;
//-----Added by BRM-----
//-----to utilize Poisson distribution
using Troschuetz.Random;
//----------

namespace Landis.Fire
{


    ///<summary>
    /// A disturbance plug-in that simulates Fire disturbance.
    /// </summary>
    public class PlugIn
        : PlugIns.PlugIn, PlugIns.I2PhaseInitialization
    {
        public static readonly PlugInType Type = new PlugInType("disturbance:fire");

        private string mapNameTemplate;
        private StreamWriter log;
        private StreamWriter summaryLog;
        private int[] summaryEcoregionEventCount;
        private int summaryTotalSites;
        private int summaryEventCount;
        private SizeType fireSizeType;
        private bool bui;
        private ISeasonParameters[] seasonParameters;
        private IWindDirectionParameters[] windDirectionParameters;

        //---------------------------------------------------------------------

        public PlugIn()
            : base("Fire 2007", Type)
        {
        }


        //---------------------------------------------------------------------

        public override void Initialize(string        dataFile,
                                        PlugIns.ICore modelCore)
        {
            Model.Core = modelCore;

            SiteVars.Initialize();
            Model.Core.RegisterSiteVar(SiteVars.Ecoregion, "Fire.Ecoregion");

            ParameterParser.EcoregionsDataset = Model.Core.Ecoregions;
            ParameterParser parser = new ParameterParser();
            IParameters parameters = Data.Load<IParameters>(dataFile, parser);

            //foreach(FuelTypeParameters ftparms in parameters.FuelTypeParameters)
            //    UI.WriteLine("      InitProb = {0}.", ftparms.InitiationProbability);

            //foreach(FireParameters fparms in parameters.FireParameters)
            //    UI.WriteLine("      MeanSize = {0}.", fparms.MeanSize);

            Timestep = parameters.Timestep;
            fireSizeType = parameters.FireSizeType;
            bui = parameters.BUI;
            mapNameTemplate = parameters.MapNamesTemplate;
            
            Event.Initialize(parameters.SeasonParameters, parameters.FuelTypeParameters,parameters.FireDamages);


            seasonParameters = parameters.SeasonParameters;
            windDirectionParameters = parameters.WindDirectionParameters;
            
            summaryEcoregionEventCount = new int[Model.Core.Ecoregions.Count];

            foreach (IEcoregion ecoregion in Ecoregions.Dataset)
            {
                if (ecoregion.MoreEcoregionParameters == null)
                    UI.WriteLine("   Fire Parameters empty.");
                IMoreEcoregionParameters eventParms = ecoregion.MoreEcoregionParameters;

                UI.WriteLine("   FireSize={0}, SD={1}", eventParms.MeanSize, eventParms.StandardDeviation);
            }

            // Initialize list of sites per ecoregion:
            foreach (Site site in Model.Core.Landscape)
            {
                if (site.IsActive)
                {
                    IEcoregion ecoregion = SiteVars.Ecoregion[site];
                    ecoregion.MoreEcoregionParameters.EcoregionSites.Add(site.Location);
                }
            }

            UI.WriteLine("   Opening Fire log file \"{0}\" ...", parameters.LogFileName);
            log = Data.CreateTextFile(parameters.LogFileName);
            log.AutoFlush = true;
            //-----Edited by BRM-----
            //-----To add Duration
            log.Write("Time,InitSite,InitEcoregion,InitFuel,InitPercentConifer,SelectedSize,Duration,FireSeason,WindSpeed,WindDirection,FFMC,BUI,PercentCuring,SitesChecked,CohortsKilled,MeanSeverity,");
            //----------
            foreach (IEcoregion ecoregion in Ecoregions.Dataset)
            {
                  log.Write("{0},", ecoregion.Name);
            }
            log.Write("TotalSitesInEvent");
            log.WriteLine("");

            summaryLog = Data.CreateTextFile(parameters.SummaryLogFileName);
            summaryLog.AutoFlush = true;
            summaryLog.Write("TimeStep, TotalSitesBurned,");
            foreach (IEcoregion ecoregion in Ecoregions.Dataset)
            {
                    summaryLog.Write("{0},", ecoregion.Name);
            }
            summaryLog.Write("TotalNumberEvents");
            summaryLog.WriteLine("");


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
        
            UI.WriteLine("Processing landscape for Fire events ...");

            SiteVars.Event.SiteValues = null;
            SiteVars.Severity.ActiveSiteValues = 0;
            SiteVars.Disturbed.ActiveSiteValues = false;
            SiteVars.TravelTime.ActiveSiteValues = Double.PositiveInfinity;
            SiteVars.MinNeighborTravelTime.ActiveSiteValues = Double.PositiveInfinity;
            SiteVars.RateOfSpread.ActiveSiteValues = 0.0;
            
            // Fill in open types as needed:
            foreach (ActiveSite site in Model.Core.Landscape) 
            {
                if((FuelTypeCode) SiteVars.CFSFuelType[site] == FuelTypeCode.NoFuel)
                {
                    IEcoregion ecoregion = SiteVars.Ecoregion[site];
                    IMoreEcoregionParameters fireParms = ecoregion.MoreEcoregionParameters;
                    SiteVars.CFSFuelType[site] = (int) fireParms.OpenFuelType;
                }
            }


            foreach (IEcoregion ecoregion in Ecoregions.Dataset)
            {
                summaryEcoregionEventCount[ecoregion.Index] = 0;
            }

            summaryTotalSites = 0;
            summaryEventCount = 0;
            
            //-----Edited by BRM to incorporate Poisson selection of ignition #-----
            /*
            //-----This section is original method
            foreach (ActiveSite site in Model.Core.Landscape) 
            {
                IEcoregion ecoregion = SiteVars.Ecoregion[site];
                IMoreEcoregionParameters fireParms = ecoregion.MoreEcoregionParameters;
                
                if(Util.Random.GenerateUniform() <= fireParms.EcoIgnitionProb)
                {
                             
                    Event FireEvent = Event.Initiate(site, Timestep, fireSizeType, bui, seasonParameters, windDirectionParameters);
                    if (FireEvent != null) 
                    {
                        LogEvent(Model.Core.CurrentTime, FireEvent);
                        summaryEventCount++;
                    }
                }
            }
             //----------
             */
            
            //-----This section replaces removed section above-----
            foreach (IEcoregion ecoregion in Ecoregions.Dataset)
            {
                IMoreEcoregionParameters fireParms = ecoregion.MoreEcoregionParameters;
                if (fireParms.EcoIgnitionProb > 0)
                {
                    PoissonDistribution randVar = new PoissonDistribution(RandomNumberGenerator.Singleton);
                    double doubleLambda;
                    int ignGenerated = 0;
                    if (fireParms.EcoIgnitionProb < 1)
                    {
                        doubleLambda = fireParms.EcoIgnitionProb * Timestep;
                        if (doubleLambda < 1)
                        {
                            if (Util.Random.GenerateUniform() <= doubleLambda)
                            {
                                randVar.Lambda = 1;
                                ignGenerated = randVar.Next();
                            }
                            else
                            {
                                ignGenerated = 0;
                            }

                        }
                        else
                        {
                            randVar.Lambda = doubleLambda;
                            ignGenerated = randVar.Next();
                        }
                    }
                    else
                    {
                        doubleLambda = fireParms.EcoIgnitionProb;
                        bool boolLarge = false;
                        if (doubleLambda > 745)
                        {
                            doubleLambda = doubleLambda / 10;
                            boolLarge = true;
                        }
                        bool boolLambda = randVar.IsValidLambda(doubleLambda);
                        randVar.Lambda = doubleLambda;
                        
                        for (int i = 1; i <= Timestep; i++)
                        {
                            int annualFires = randVar.Next();
                            if (boolLarge)
                                annualFires = annualFires * 10;
                            ignGenerated += annualFires;
                        }
                    }
                    
                    List<Location> cellsPerEcoregion = ecoregion.MoreEcoregionParameters.EcoregionSites;
                    Landis.Util.Random.Shuffle(cellsPerEcoregion);
                    int fireCount = 0;

                    //Try to create poissonNumber of fires in each ecoregion.
                    //Fires should only initiate if a fire event has not previously occurred 
                    //at that site.

                    foreach (Location siteLocation in cellsPerEcoregion)
                    {

                        Site site = Model.Core.Landscape.GetSite(siteLocation);

                        ActiveSite asite = site as ActiveSite;

                        if (fireCount >= ignGenerated) continue;  //exit loop if the required number of fires has occurred.
                        if (SiteVars.Event[asite] == null)
                        {
                            fireCount++;
                            Event FireEvent = Event.Initiate(asite, Timestep, fireSizeType, bui, seasonParameters, windDirectionParameters);
                            if (FireEvent != null)
                            {  
                                LogEvent(Model.Core.CurrentTime, FireEvent);
                                summaryEventCount++;
                            }
                        }
                    }
                }

              
            }
 
            //----------
             

            //UI.WriteLine("  Fire events: {0}", summaryEventCount);

            //  Write Fire severity map
            string path = MapNames.ReplaceTemplateVars(mapNameTemplate, Model.Core.CurrentTime);
            IOutputRaster<SeverityPixel> map = CreateMap(path);
            using (map) {
                SeverityPixel pixel = new SeverityPixel();
                foreach (Site site in Model.Core.Landscape.AllSites) {
                    if (site.IsActive) {
                        if (SiteVars.Disturbed[site])
                            pixel.Band0 = (byte) (SiteVars.Severity[site] + 2);
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
            
            //  Write travel time map
            path = MapNames.ReplaceTemplateVars("./tests/Fire-2006/travel-time-{timestep}.gis", Model.Core.CurrentTime);
            //path = MapNames.ReplaceTemplateVars(mapNameTemplate, Model.Core.CurrentTime);
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

            //  Write topo map
            path = MapNames.ReplaceTemplateVars("./tests/Fire-2006/topo-{timestep}.gis", Model.Core.CurrentTime);
            //path = MapNames.ReplaceTemplateVars(mapNameTemplate, Model.Core.CurrentTime);
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
            //  Write wind speed map
            path = MapNames.ReplaceTemplateVars("./tests/Fire-2006/WSV-{timestep}.gis", Model.Core.CurrentTime);
            //path = MapNames.ReplaceTemplateVars(mapNameTemplate, Model.Core.CurrentTime);
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

            //  Write ROS map
            path = MapNames.ReplaceTemplateVars("./tests/Fire-2006/ROS-{timestep}.gis", Model.Core.CurrentTime);
            //path = MapNames.ReplaceTemplateVars(mapNameTemplate, Model.Core.CurrentTime);
            IOutputRaster<UShortPixel> rosmap = CreateTravelTimeMap(path);
            using (rosmap)
            {
                UShortPixel pixel = new UShortPixel();
                foreach (Site site in Model.Core.Landscape.AllSites)
                {
                    if (site.IsActive)
                    {
                        pixel.Band0 = (ushort)(SiteVars.RateOfSpread[site]);

                    }
                    else
                    {
                        //  Inactive site
                        pixel.Band0 = 0;
                    }
                    rosmap.WritePixel(pixel);
                }
            }

            //  Write AdjROS map
            path = MapNames.ReplaceTemplateVars("./tests/Fire-2006/AdjROS-{timestep}.gis", Model.Core.CurrentTime);
            //path = MapNames.ReplaceTemplateVars(mapNameTemplate, Model.Core.CurrentTime);
            IOutputRaster<UShortPixel> adjmap = CreateTravelTimeMap(path);
            using (adjmap)
            {
                UShortPixel pixel = new UShortPixel();
                foreach (Site site in Model.Core.Landscape.AllSites)
                {
                    if (site.IsActive)
                    {
                        pixel.Band0 = (ushort)((SiteVars.AdjROS[site]) * 100);

                    }
                    else
                    {
                        //  Inactive site
                        pixel.Band0 = 0;
                    }
                    adjmap.WritePixel(pixel);
                }
            }

            WriteSummaryLog(Model.Core.CurrentTime);

        }

        //---------------------------------------------------------------------

        private void LogEvent(int   currentTime,
                              Event fireEvent)
        {
        
        //HEADER:  Time,Initiation Site,Sites Checked,Cohorts Killed,Mean Severity,
            int totalSitesInEvent = 0;
            if (fireEvent.EventSeverity > 0) 
            {
                //-----Edited by BRM-----
                //-----to include MaxDuration
                log.Write("{0},\"{1}\",{2},{3},{4},{5:0.00},{6:0.00},{7},{8},{9},{10},{11},{12},{13},{14},{15:0.00}",
                          currentTime,
                          fireEvent.StartLocation,
                          fireEvent.InitiationEcoregion,
                          fireEvent.InitiationFuel,
                          fireEvent.InitiationPercentConifer,
                          fireEvent.MaxFireParameter,
                          fireEvent.MaxDuration,
                          fireEvent.FireSeason.NameOfSeason,
                          fireEvent.WindSpeed,
                          fireEvent.WindDirection,
                          fireEvent.FFMC,
                          fireEvent.BuildUpIndex,
                          fireEvent.FireSeason.PercentCuring,
                          fireEvent.NumSitesChecked,
                          fireEvent.CohortsKilled,
                          fireEvent.EventSeverity);
                //----------
                foreach (IEcoregion ecoregion in Ecoregions.Dataset)
                {
//                    if (ecoregion.Active)
//                    {
                        log.Write(",{0}", fireEvent.SitesInEvent[ecoregion.Index]);
                        totalSitesInEvent += fireEvent.SitesInEvent[ecoregion.Index];
                        summaryEcoregionEventCount[ecoregion.Index] += fireEvent.SitesInEvent[ecoregion.Index];
//                    }
                }
                summaryTotalSites += totalSitesInEvent;
                log.Write(", {0}", totalSitesInEvent);
                log.WriteLine("");
            }
        }

        //---------------------------------------------------------------------

        private void WriteSummaryLog(int   currentTime)
        {
            //int totalSitesInEvent = 0;
            summaryLog.Write("{0},{1},{2}", currentTime, summaryTotalSites, summaryEventCount);
            foreach (IEcoregion ecoregion in Ecoregions.Dataset)
            {
                    summaryLog.Write(",{0}", summaryEcoregionEventCount[ecoregion.Index]);
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
