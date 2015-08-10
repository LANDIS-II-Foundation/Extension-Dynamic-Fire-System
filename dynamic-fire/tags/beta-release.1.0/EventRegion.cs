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

namespace Landis.Fire
{
    public class EventRegion
    {

        private static int siteWindSpeed;
        private static int siteWindDirection;

        public static List<Location> DurationBasedSize(ActiveSite site, double ROS)
        {
            List<Location> newFireList = new List<Location>(0);
            newFireList.Add(site.Location);  //Add ignition site first.
            
            return newFireList;
        }
        //---------------------------------------------------------------------
        //-----Edited by BRM-----
        //-----To pass fireSizeType
        public static List<Site> SizeFireCostSurface(Event fireEvent, SizeType fireSizeType)
        //----------
        {
            
            Site initiationSite = Model.Core.Landscape.GetSite(fireEvent.StartLocation);
            
            MaximumFuelDistance(fireEvent);
            
            //Adjusted for SIZE or DURATION-----BRM-----
            if (fireSizeType == SizeType.size_based)
            {
                double ellipseAdjust = ((fireEvent.LengthB * 2) - fireEvent.LengthD) / fireEvent.LengthD;
                if (ellipseAdjust < 2)
                    ellipseAdjust = 2;
                double ratioBD = fireEvent.LengthB/fireEvent.LengthD;
                double ellipseArea = fireEvent.MaxFireParameter * Model.Core.CellLength * Model.Core.CellLength * ellipseAdjust;
                fireEvent.LengthA = Math.Sqrt(ellipseArea / (Math.PI * fireEvent.LB));
                fireEvent.LengthB = fireEvent.LengthA * fireEvent.LB;
                fireEvent.LengthD = fireEvent.LengthB/ratioBD;
            }
            else
            {
                //FBP 86;  Area in ha
                double ellipseArea = (((Math.PI) / (4 * fireEvent.LB)) * Math.Pow(fireEvent.LengthB * 2 * fireEvent.MaxFireParameter, 2) / 10000);
                fireEvent.LengthD = fireEvent.LengthD * fireEvent.MaxFireParameter;
                fireEvent.LengthB = fireEvent.LengthB * fireEvent.MaxFireParameter;
                fireEvent.LengthA = fireEvent.LengthA * fireEvent.MaxFireParameter;              
             }
             //double ellipseArea = fireEvent.LengthB * fireEvent.LengthA * Math.PI;
            //double ellipseAdjust = 2 * Math.Sqrt((double) fireEvent.MaxFireParameter / ellipseArea);

            //These are the parameters necessary for the calculating eliptical lengths:
            //double lengthD = fireEvent.LengthD * Model.Core.CellLength * ellipseAdjust;
            double lengthC = (fireEvent.LengthB - fireEvent.LengthD);
            
            //UI.WriteLine("      Max Distance = {0:0.0}.  Calculating burn area...", maxDistance);
            UI.WriteLine("      Ellipse: LengthD = {0:0.0} (m).  LengthC = {1:0.0} (m).", fireEvent.LengthD, lengthC);
            
            List<Site> newFireList = new List<Site>();
            SiteVars.TravelTime[initiationSite] = 0.0;
            SiteVars.MinNeighborTravelTime[initiationSite] = 0.0;
            SiteVars.Event[initiationSite] = fireEvent;
            
            //Assign a ROS to the initiation site:
            double initiationTravelTime = CalculateTravelTime(initiationSite, initiationSite, fireEvent);
            SiteVars.TravelTime[initiationSite] = initiationTravelTime;

            //Create a queue of neighboring sites to which the fire will spread:
            Queue<Site> sitesToConsider = new Queue<Site>();
            sitesToConsider.Enqueue(initiationSite);

            while (sitesToConsider.Count > 0) 
            {
                
                //UI.WriteLine("      SiteToConsider = {0}.", sitesToConsider.Count);
                Site srcSite = sitesToConsider.Dequeue();  //Source site
                newFireList.Add(srcSite);

                //Next, add site's neighbors to the list of
                //sites to consider.  The neighbors cannot be part of
                //any other Fire event in the current timestep, and
                //cannot already be on the list.

                List<Site> destinations = Get8Neighbors(srcSite);
                if (destinations.Count > 0)
                {
                    foreach (Site destSite in destinations) 
                    {
                        if (SiteVars.Event[destSite] != null)
                            continue;
                        if (sitesToConsider.Contains(destSite))
                            continue;
                        if(ElipseDistanceBetweenSites(initiationSite, destSite, lengthC, fireEvent.LengthD, fireEvent.WindDirection))
                        {
                            //Complex Cost Surface (Scheller's algorithm)
                            SiteVars.CostTime[destSite] = CalculateTravelTime(destSite, srcSite, fireEvent);
                            if (srcSite == initiationSite)
                            {
                                SiteVars.TravelTime[destSite] = (SiteVars.CostTime[destSite] / 2) * CellDistanceBetweenSites(destSite, srcSite)
                                                           + SiteVars.TravelTime[srcSite];
                            }
                            else
                            {
                                SiteVars.TravelTime[destSite] = (SiteVars.CostTime[destSite] / 2 + SiteVars.CostTime[srcSite] / 2) * CellDistanceBetweenSites(destSite, srcSite)
                                                               + SiteVars.TravelTime[srcSite];
                                //SiteVars.TravelTime[destSite] = SiteVars.CostTime[destSite] * CellDistanceBetweenSites(destSite, srcSite) 
                                //                                + SiteVars.TravelTime[srcSite];
                            }
                            SiteVars.MinNeighborTravelTime[destSite] = SiteVars.TravelTime[srcSite];
                            sitesToConsider.Enqueue(destSite);
                            SiteVars.Event[destSite] = fireEvent;
                        }
                    }
                }
            }
            
            //Finally,  check neighbor travel 10 times to optimize the shortest travel
            //distance.
            
            bool finished = false;
            int count = 1;

            while (!finished)
            {
                UI.WriteLine("      Re-Calculating optimal travel time:  {0} times.", count++);
                finished = true;
                
                // Loop through list and check each for a new, shorter travel time.
                foreach (Site destSite in newFireList)  //Fire destination
                {
                    Site srcSite = CalculateMinNeighbor(destSite, fireEvent);  // Where is the fire coming from?
                    if(srcSite != null)
                    {
                        double oldTravelTime = SiteVars.TravelTime[destSite];
                        //double newTravelTime = (CalculateTravelTime(destSite, srcSite, fireEvent)
                        //                        * CellDistanceBetweenSites(destSite, srcSite))
                        //                        + SiteVars.TravelTime[srcSite];
                        double newCostTime = CalculateTravelTime(destSite, srcSite, fireEvent);
                        double newTravelTime = ((newCostTime / 2 + SiteVars.CostTime[srcSite] / 2) *
                                                    CellDistanceBetweenSites(destSite, srcSite)) + SiteVars.TravelTime[srcSite];
                        if(newTravelTime < oldTravelTime)
                        {
                            SiteVars.CostTime[destSite] = newCostTime;
                            SiteVars.TravelTime[destSite] = newTravelTime;
                            //UI.WriteLine("      OldTT = {0:0.00}, NewTT = {1:0.00}, OldMinNeighborTT = {2:0.00}, MinNeighborTT = {3:0.00}.", 
                            //oldTravelTime, SiteVars.TravelTime[destSite], 
                            SiteVars.MinNeighborTravelTime[destSite]= SiteVars.TravelTime[srcSite];
                            if ((oldTravelTime - newTravelTime) > 0.5)
                            finished = false;
                        }
                    }

                }
                
                if(count > 2000) finished = true;
            }

            return newFireList;
        }
        //-------------------------------------------------------
        private static Site CalculateMinNeighbor(Site site, Event fireEvent)
        {
            List<Site> neighborhood = Get8Neighbors(site);

            Site lowestNeighbor = null;
            
            double minNeighborTravelTime = SiteVars.MinNeighborTravelTime[site]; 
            
            foreach (Site relneighbor in neighborhood) 
            {
                if( //relneighbor != null && 
                    SiteVars.Event[relneighbor] == fireEvent &&
                    !Double.IsPositiveInfinity(SiteVars.TravelTime[relneighbor]) && 
                    SiteVars.TravelTime[relneighbor] < minNeighborTravelTime)
                {
                    lowestNeighbor = relneighbor;
                    SiteVars.MinNeighborTravelTime[site] = SiteVars.TravelTime[relneighbor];
                    //UI.WriteLine("Location = {0},{1}.  MinTravelTime = {2}.", lowestNeighbor.Row, lowestNeighbor.Column, SiteVars.TravelTime[relneighbor]);
                }
            }
            
            return lowestNeighbor;

        }

        private static double CalculateTravelTime(Site site, Site firesource, Event fireEvent)
        {

            //Calculate Fire regime size adjustment:
            IEcoregion ecoregion = SiteVars.Ecoregion[site];
            IMoreEcoregionParameters eventParms = ecoregion.MoreEcoregionParameters;
            double FRUA = eventParms.MeanSize;
            
            ecoregion = SiteVars.Ecoregion[firesource];
            eventParms = ecoregion.MoreEcoregionParameters;
            FRUA = FRUA / eventParms.MeanSize;
            
            
            int fuelIndex = SiteVars.CFSFuelType[site];
            int percentConifer = SiteVars.PercentConifer[site];
            int percentHardwood = SiteVars.PercentHardwood[site];
            int percentDeadFir = SiteVars.PercentDeadFir[site];
            List<int> siteWindList = new List<int>();
            int siteWindSpeed, siteWindDirection;

            FuelTypeCode temp = (FuelTypeCode) fuelIndex;
            //UI.WriteLine("         Fuel Type Code = {0}.", temp.ToString());
            
            IFuelTypeParameters[] fuelTypeParms = fireEvent.FuelTypeParms;
            ISeasonParameters season = fireEvent.FireSeason;

            double f_F = Weather.CalculateFuelMoistureEffect(fireEvent.FFMC);
            double ISZ = 0.208 * f_F;  //No wind
            double RSZ = FuelEffects.InitialRateOfSpread(fuelIndex, ISZ,
                                                        percentConifer,
                                                        percentHardwood,
                                                        percentDeadFir,
                                                        season);
            
            siteWindList = CalculateSlopeEffect(fireEvent.WindSpeed, fireEvent.WindDirection, site, RSZ, f_F, season);
            if (siteWindList.Count > 0)
            {
                siteWindSpeed = siteWindList[0];
                siteWindDirection = siteWindList[1];
            }
            else
            {
                siteWindSpeed = fireEvent.WindSpeed;
                siteWindDirection = fireEvent.WindDirection;
            }

            double f_backW = Weather.CalculateBackWindEffect(siteWindSpeed);
            double f_W = Weather.CalculateWindEffect(siteWindSpeed);

            double ISI = 0.208 * f_F * f_W;
            double BISI = 0.208 * f_F * f_backW;

            double BROSi = FuelEffects.InitialRateOfSpread(fuelIndex, BISI, 
                                                        percentConifer,
                                                        percentHardwood,
                                                        percentDeadFir,
                                                        season);
           
            double ROSi = FuelEffects.InitialRateOfSpread(fuelIndex, ISI, 
                                                        percentConifer,
                                                        percentHardwood,
                                                        percentDeadFir,
                                                        season );
            
            BROSi *= FRUA;
            ROSi  *= FRUA;
            SiteVars.RateOfSpread[site] = ROSi;

            double LB = CalculateLengthBreadthRatio(siteWindSpeed, fuelIndex);
            
            double FROSi = (ROSi + BROSi)/(LB * 2.0);//        (FBP; 89)

            double alpha = siteWindDirection;
            double A = FROSi;
            double beta = Beta(firesource, site, alpha);

            //Randomize around 8 neighbor directions
            //int wiggle = (int)((Util.Random.GenerateUniform() * 45.0) - 22.5);
            //beta = beta + wiggle;
            

            double B = A * LB; //fireEvent.LB; 
            double C = B - BROSi;

            //  Equations below for e, p, b, c, a utlize the polar equation
            //  for an ellipse, given at:
            //  http://www.du.edu/~jcalvert/math/ellipse.htm

            double e = C / B;
            double dist = CellDistanceBetweenSites(site, firesource) * Model.Core.CellLength;
            double r = ROSi;
            if (dist != 0)
            {
                double p = dist * (1 + e * Math.Cos(Math.PI - beta));
                double b = p / (1 - e * e);
                double c = e * b;
                r = (dist / (b + c)) * ROSi;
             
            //double a = (1 / LB) * b;           

            //Equations below for theta, r come from Finney 2002, eq [3], [5]

            //double cosSqBeta = CosSquared(beta);
            //double sinSqBeta = SinSquared(beta);
            //double termA = a * Math.Cos(beta) * Math.Pow(((a*a * cosSqBeta) + ((b*b)-(c*c)) * sinSqBeta),0.5);
            //double termB = b * c * sinSqBeta;
            //double termC = ((a*a) * cosSqBeta) + ((b*b) * sinSqBeta);
            //double theta = Math.Acos((termA - termB) / termC);
            
            //r = (a * ((c * Math.Cos(theta)) + b)) /
            //            Math.Sqrt((a * a * CosSquared(theta)) + (b * b * SinSquared(theta)));
            }
                        
            //-----Added by BRM-----
            SiteVars.AdjROS[site] = r;
            //----------      
            
            //UI.WriteLine("      FROSi = {0}, BROSi = {1}.", FROSi, BROSi);
            //UI.WriteLine("      beta = {0:0.00}, theta = {1:0.00}, alpha = {2:0.00}, Travel time = {3:0.000000}.", beta, theta, alpha, 1/r);
            //UI.WriteLine("      Travel time = {0:0.000000}.  R = {1}.", 1/r, r);
            if (site == firesource)
            {
                double rate = 1.0 / r;  //units = minutes / meter
                double cost = rate * Model.Core.CellLength / 2;     //units = minutes
                return cost;
            }
            else
            {
                double rate = 1.0 / r;  //units = minutes / meter
                double cost = rate * Model.Core.CellLength;     //units = minutes
                return cost;
            }
        }
        //-------------------------------------------------------
        private static double Beta(Site sourcesite, Site site, double windDirection)
        {   
            double row = (double) sourcesite.Location.Row - (double) site.Location.Row;
            double column = (double) sourcesite.Location.Column - (double) site.Location.Column;
            
            double atan = 0.0;
            if (row == 0)
            {
                if (column == 0)
                {
                    return atan;
                }
                row = 0.0000001;
            }
            
            atan = Math.Atan(column / row) * 180 / Math.PI;// Convert to degrees
            
            if(Math.Sign(row) == -1) atan -= 180;

            double beta = atan - windDirection;
            
            //Randomize around 8 neighbor directions
            double wiggle = (Util.Random.GenerateUniform() * 45.0) - 22.5;
            beta = beta + wiggle;
            //
            beta = beta * Math.PI / 180;  //Convert to radians
            return beta;

            //atan = atan * Math.PI / 180;  //Back to radians
            
            //UI.WriteLine("Column = {0}, Row = {1}, Atan = {2}.", column, row, atan);
            //return atan - (windDirection * Math.PI / 180);
        }
        
        private static void MaximumFuelDistance(Event fireEvent)
        {
            UI.WriteLine("      Calculating maximum RSI...");
            IFuelTypeParameters[] fuelTypeParms = fireEvent.FuelTypeParms;

            ISeasonParameters season = fireEvent.FireSeason;
            ushort tempSlope, maxSlope = 0;
            foreach (Site site in Model.Core.Landscape.AllSites)
            {
                if (site.IsActive)
                {
                    tempSlope = SiteVars.GroundSlope[site];
                    if (tempSlope > maxSlope)
                        maxSlope = tempSlope;
                }
            }
            
            double f_F = Weather.CalculateFuelMoistureEffect(fireEvent.FFMC);
            double SF = CalculateSF(maxSlope);
            double f_WZero = Weather.CalculateWindEffect(0);

            double ISZ = 0.208 * f_F * f_WZero;

            double ISI = 0;
            double f_backW = Weather.CalculateBackWindEffect(fireEvent.WindSpeed);
            double BISI = 0.208 * f_F * f_backW;
            double RSZ = 0;
            double RSF = 0;
            double WSE = 0;
            double WSV = 0;
            double f_W = 0;

            
            //M2 and M4 (leaf on) not included in maximum list because they will always be lower
            //than M1 and M3 (leaf off), respectively.
            double ROS = 0.0;
            double ROSmax = 0.0;
            double BROSmax = 0.0;
            double FROSmax = 0.0;
            int maxfuelIndex = -1; 

            int fuelIndex = (int) FuelTypeCode.C1;
            RSZ = FuelEffects.InitialRateOfSpread(fuelIndex, ISZ, 100, 0, 0, season);
            RSF = RSZ * SF;
            WSE = CalculateWSE(fuelIndex, RSF, f_F, 100, season);
            WSV = WSE + fireEvent.WindSpeed;
            f_W = Weather.CalculateWindEffect(WSV);
            ISI = 0.208 * f_F * f_W;
            ROS = FuelEffects.InitialRateOfSpread(fuelIndex, ISI, 100, 0, 0, season) * fuelTypeParms[fuelIndex].MaxBE;
            if(ROS >= ROSmax)
            {
                ROSmax = ROS;
                maxfuelIndex = fuelIndex;
                BROSmax = FuelEffects.InitialRateOfSpread(fuelIndex, BISI, 100, 0, 0, season);
                fireEvent.LB = CalculateLengthBreadthRatio(fireEvent.WindSpeed, fuelIndex);
                FROSmax = (ROSmax + BROSmax) / (fireEvent.LB * 2);
                //FROSmax = FuelEffects.InitialRateOfSpread(fuelIndex, RSZ, 100, 0, 0, season);
                maxfuelIndex = fuelIndex;
            }
            fuelIndex = (int) FuelTypeCode.C2;
            RSZ = FuelEffects.InitialRateOfSpread(fuelIndex, ISZ, 100, 0, 0, season);
            RSF = RSZ * SF;
            WSE = CalculateWSE(fuelIndex, RSF, f_F, 100, season);
            WSV = WSE + fireEvent.WindSpeed;
            f_W = Weather.CalculateWindEffect(WSV);
            ISI = 0.208 * f_F * f_W;
            ROS = FuelEffects.InitialRateOfSpread(fuelIndex, ISI, 100, 0, 0, season) * fuelTypeParms[fuelIndex].MaxBE;
            if(ROS >= ROSmax)
            {
                ROSmax = ROS;
                maxfuelIndex = fuelIndex;
                BROSmax = FuelEffects.InitialRateOfSpread(fuelIndex, BISI, 100, 0, 0, season);
                fireEvent.LB = CalculateLengthBreadthRatio(fireEvent.WindSpeed, fuelIndex);
                FROSmax = (ROSmax + BROSmax) / (fireEvent.LB * 2);
                //FROSmax = FuelEffects.InitialRateOfSpread(fuelIndex, RSZ, 100, 0, 0, season);
                maxfuelIndex = fuelIndex;
            }
            fuelIndex = (int) FuelTypeCode.C3;
            RSZ = FuelEffects.InitialRateOfSpread(fuelIndex, ISZ, 100, 0, 0, season);
            RSF = RSZ * SF;
            WSE = CalculateWSE(fuelIndex, RSF, f_F, 100, season);
            WSV = WSE + fireEvent.WindSpeed;
            f_W = Weather.CalculateWindEffect(WSV);
            ISI = 0.208 * f_F * f_W;
            ROS = FuelEffects.InitialRateOfSpread(fuelIndex, ISI, 100, 0, 0, season) * fuelTypeParms[fuelIndex].MaxBE;
            if(ROS >= ROSmax)
            {
                ROSmax = ROS;
                maxfuelIndex = fuelIndex;
                BROSmax = FuelEffects.InitialRateOfSpread(fuelIndex, BISI, 100, 0, 0, season);
                fireEvent.LB = CalculateLengthBreadthRatio(fireEvent.WindSpeed, fuelIndex);
                FROSmax = (ROSmax + BROSmax) / (fireEvent.LB * 2);
                //FROSmax = FuelEffects.InitialRateOfSpread(fuelIndex, RSZ, 100, 0, 0, season);
                maxfuelIndex = fuelIndex;
            }
            fuelIndex = (int) FuelTypeCode.C4;
            RSZ = FuelEffects.InitialRateOfSpread(fuelIndex, ISZ, 100, 0, 0, season);
            RSF = RSZ * SF;
            WSE = CalculateWSE(fuelIndex, RSF, f_F, 100, season);
            WSV = WSE + fireEvent.WindSpeed;
            f_W = Weather.CalculateWindEffect(WSV);
            ISI = 0.208 * f_F * f_W;
            ROS = FuelEffects.InitialRateOfSpread(fuelIndex, ISI, 100, 0, 0, season) * fuelTypeParms[fuelIndex].MaxBE;
            if(ROS >= ROSmax)
            {
                ROSmax = ROS;
                BROSmax = FuelEffects.InitialRateOfSpread(fuelIndex, BISI, 100, 0, 0, season);
                fireEvent.LB = CalculateLengthBreadthRatio(fireEvent.WindSpeed, fuelIndex);
                FROSmax = (ROSmax + BROSmax) / (fireEvent.LB * 2);
                //FROSmax = FuelEffects.InitialRateOfSpread(fuelIndex, RSZ, 100, 0, 0, season);
                maxfuelIndex = fuelIndex;
            }
            fuelIndex = (int) FuelTypeCode.C5;
            RSZ = FuelEffects.InitialRateOfSpread(fuelIndex, ISZ, 100, 0, 0, season);
            RSF = RSZ * SF;
            WSE = CalculateWSE(fuelIndex, RSF, f_F, 100, season);
            WSV = WSE + fireEvent.WindSpeed;
            f_W = Weather.CalculateWindEffect(WSV);
            ISI = 0.208 * f_F * f_W;
            ROS = FuelEffects.InitialRateOfSpread(fuelIndex, ISI, 100, 0, 0, season) * fuelTypeParms[fuelIndex].MaxBE;
            if(ROS >= ROSmax)
            {
                ROSmax = ROS;
                BROSmax = FuelEffects.InitialRateOfSpread(fuelIndex, BISI, 100, 0, 0, season);
                fireEvent.LB = CalculateLengthBreadthRatio(fireEvent.WindSpeed, fuelIndex);
                FROSmax = (ROSmax + BROSmax) / (fireEvent.LB * 2);
                //FROSmax = FuelEffects.InitialRateOfSpread(fuelIndex, RSZ, 100, 0, 0, season);
                maxfuelIndex = fuelIndex;
            }
            fuelIndex = (int) FuelTypeCode.C6;
            RSZ = FuelEffects.InitialRateOfSpread(fuelIndex, ISZ, 100, 0, 0, season);
            RSF = RSZ * SF;
            WSE = CalculateWSE(fuelIndex, RSF, f_F, 100, season);
            WSV = WSE + fireEvent.WindSpeed;
            f_W = Weather.CalculateWindEffect(WSV);
            ISI = 0.208 * f_F * f_W;
            ROS = FuelEffects.InitialRateOfSpread(fuelIndex, ISI, 100, 0, 0, season) * fuelTypeParms[fuelIndex].MaxBE;
            if(ROS >= ROSmax)
            {
                ROSmax = ROS;
                BROSmax = FuelEffects.InitialRateOfSpread(fuelIndex, BISI, 100, 0, 0, season);
                fireEvent.LB = CalculateLengthBreadthRatio(fireEvent.WindSpeed, fuelIndex);
                FROSmax = (ROSmax + BROSmax) / (fireEvent.LB * 2);
                //FROSmax = FuelEffects.InitialRateOfSpread(fuelIndex, RSZ, 100, 0, 0, season);
                maxfuelIndex = fuelIndex;
            }
            fuelIndex = (int) FuelTypeCode.C7;
            RSZ = FuelEffects.InitialRateOfSpread(fuelIndex, ISZ, 100, 0, 0, season);
            RSF = RSZ * SF;
            WSE = CalculateWSE(fuelIndex, RSF, f_F, 100, season);
            WSV = WSE + fireEvent.WindSpeed;
            f_W = Weather.CalculateWindEffect(WSV);
            ISI = 0.208 * f_F * f_W;
            ROS = FuelEffects.InitialRateOfSpread(fuelIndex, ISI, 100, 0, 0, season) * fuelTypeParms[fuelIndex].MaxBE;
            if(ROS >= ROSmax)
            {
                ROSmax = ROS;
                BROSmax = FuelEffects.InitialRateOfSpread(fuelIndex, BISI, 100, 0, 0, season);
                fireEvent.LB = CalculateLengthBreadthRatio(fireEvent.WindSpeed, fuelIndex);
                FROSmax = (ROSmax + BROSmax) / (fireEvent.LB * 2);
                //FROSmax = FuelEffects.InitialRateOfSpread(fuelIndex, RSZ, 100, 0, 0, season);
                maxfuelIndex = fuelIndex;
            }
            fuelIndex = (int) FuelTypeCode.D1;
            RSZ = FuelEffects.InitialRateOfSpread(fuelIndex, ISZ, 0, 0, 0, season);
            RSF = RSZ * SF;
            WSE = CalculateWSE(fuelIndex, RSF, f_F, 100, season);
            WSV = WSE + fireEvent.WindSpeed;
            f_W = Weather.CalculateWindEffect(WSV);
            ISI = 0.208 * f_F * f_W;
            ROS = FuelEffects.InitialRateOfSpread(fuelIndex, ISI, 0, 100, 0, season) * fuelTypeParms[fuelIndex].MaxBE;
            if(ROS >= ROSmax)
            {
                ROSmax = ROS;
                BROSmax = FuelEffects.InitialRateOfSpread(fuelIndex, BISI, 0, 100, 0, season);
                fireEvent.LB = CalculateLengthBreadthRatio(fireEvent.WindSpeed, fuelIndex);
                FROSmax = (ROSmax + BROSmax) / (fireEvent.LB * 2);
                //FROSmax = FuelEffects.InitialRateOfSpread(fuelIndex, RSZ, 0, 100, 0, season);
                maxfuelIndex = fuelIndex;
            }
            fuelIndex = (int) FuelTypeCode.S1;
            RSZ = FuelEffects.InitialRateOfSpread(fuelIndex, ISZ, 0, 0, 0, season);
            RSF = RSZ * SF;
            WSE = CalculateWSE(fuelIndex, RSF, f_F, 100, season);
            WSV = WSE + fireEvent.WindSpeed;
            f_W = Weather.CalculateWindEffect(WSV);
            ISI = 0.208 * f_F * f_W;
            ROS = FuelEffects.InitialRateOfSpread(fuelIndex, ISI, 0, 0, 0, season) * fuelTypeParms[fuelIndex].MaxBE;
            if(ROS >= ROSmax)
            {
                ROSmax = ROS;
                BROSmax = FuelEffects.InitialRateOfSpread(fuelIndex, BISI, 0, 0, 0, season);
                fireEvent.LB = CalculateLengthBreadthRatio(fireEvent.WindSpeed, fuelIndex);
                FROSmax = (ROSmax + BROSmax) / (fireEvent.LB * 2);
                //FROSmax = FuelEffects.InitialRateOfSpread(fuelIndex, RSZ, 0, 0, 0, season);
                maxfuelIndex = fuelIndex;
            }
            fuelIndex = (int) FuelTypeCode.S2;
            RSZ = FuelEffects.InitialRateOfSpread(fuelIndex, ISZ, 0, 0, 0, season);
            RSF = RSZ * SF;
            WSE = CalculateWSE(fuelIndex, RSF, f_F, 100, season);
            WSV = WSE + fireEvent.WindSpeed;
            f_W = Weather.CalculateWindEffect(WSV);
            ISI = 0.208 * f_F * f_W;
            ROS = FuelEffects.InitialRateOfSpread(fuelIndex, ISI, 0, 0, 0, season) * fuelTypeParms[fuelIndex].MaxBE;
            if(ROS >= ROSmax)
            {
                ROSmax = ROS;
                BROSmax = FuelEffects.InitialRateOfSpread(fuelIndex, BISI, 0, 0, 0, season);
                fireEvent.LB = CalculateLengthBreadthRatio(fireEvent.WindSpeed, fuelIndex);
                FROSmax = (ROSmax + BROSmax) / (fireEvent.LB * 2);
                //FROSmax = FuelEffects.InitialRateOfSpread(fuelIndex, RSZ, 0, 0, 0, season);
                maxfuelIndex = fuelIndex;
            }
            fuelIndex = (int) FuelTypeCode.S3;
            RSZ = FuelEffects.InitialRateOfSpread(fuelIndex, ISZ, 0, 0, 0, season);
            RSF = RSZ * SF;
            WSE = CalculateWSE(fuelIndex, RSF, f_F, 100, season);
            WSV = WSE + fireEvent.WindSpeed;
            f_W = Weather.CalculateWindEffect(WSV);
            ISI = 0.208 * f_F * f_W;
            ROS = FuelEffects.InitialRateOfSpread(fuelIndex, ISI, 0, 0, 0, season) * fuelTypeParms[fuelIndex].MaxBE;
            if(ROS >= ROSmax)
            {
                ROSmax = ROS;
                BROSmax = FuelEffects.InitialRateOfSpread(fuelIndex, BISI, 0, 0, 0, season);
                fireEvent.LB = CalculateLengthBreadthRatio(fireEvent.WindSpeed, fuelIndex);
                FROSmax = (ROSmax + BROSmax) / (fireEvent.LB * 2);
                //FROSmax = FuelEffects.InitialRateOfSpread(fuelIndex, RSZ, 0, 0, 0, season);
                maxfuelIndex = fuelIndex;
            }
            /*fuelIndex = (int) FuelTypeCode.M1;
            ROS = FuelEffects.InitialRateOfSpread(fuelIndex, ISI, 100, 0, 0, season) * fuelTypeParms[fuelIndex].MaxBE;
            if(ROS >= ROSmax)
            {
                ROSmax = ROS;
                BROSmax = FuelEffects.InitialRateOfSpread(fuelIndex, BISI, 100, 0, 0, season);
                FROSmax = FuelEffects.InitialRateOfSpread(fuelIndex, RSZ, 100, 0, 0, season);
                maxfuelIndex = fuelIndex;
            }
            fuelIndex = (int) FuelTypeCode.M3;
            ROS = FuelEffects.InitialRateOfSpread(fuelIndex, ISI, 0, 0, 100, season) * fuelTypeParms[fuelIndex].MaxBE;
            if(ROS >= ROSmax)
            {
                ROSmax = ROS;
                BROSmax = FuelEffects.InitialRateOfSpread(fuelIndex, BISI, 0, 0, 100, season);
                FROSmax = FuelEffects.InitialRateOfSpread(fuelIndex, RSZ, 0, 0, 100, season);
                maxfuelIndex = fuelIndex;
            }*/
            fuelIndex = (int) FuelTypeCode.O1a;
            RSZ = FuelEffects.InitialRateOfSpread(fuelIndex, ISZ, 0, 0, 0, season);
            RSF = RSZ * SF;
            WSE = CalculateWSE(fuelIndex, RSF, f_F, 100, season);
            WSV = WSE + fireEvent.WindSpeed;
            f_W = Weather.CalculateWindEffect(WSV);
            ISI = 0.208 * f_F * f_W;
            ROS = FuelEffects.InitialRateOfSpread(fuelIndex, ISI, 0, 0, 0, season) * fuelTypeParms[fuelIndex].MaxBE;
            if(ROS >= ROSmax)
            {
                ROSmax = ROS;
                BROSmax = FuelEffects.InitialRateOfSpread(fuelIndex, BISI, 0, 0, 0, season);
                //Open fuel types have narrow LB, so to make sure max ellipse covers enough area we force it
                // to use the wider LB of the other types
                fireEvent.LB = CalculateLengthBreadthRatio(fireEvent.WindSpeed, (int) FuelTypeCode.D1);  
                FROSmax = (ROSmax + BROSmax) / (fireEvent.LB * 2);
                //FROSmax = FuelEffects.InitialRateOfSpread(fuelIndex, RSZ, 0, 0, 0, season);
                maxfuelIndex = fuelIndex;
            }
           /* fuelIndex = (int) FuelTypeCode.O1b;
            ROS = FuelEffects.InitialRateOfSpread(fuelIndex, ISI, 0, 0, 0, season) * fuelTypeParms[fuelIndex].MaxBE;
            if(ROS >= ROSmax)
            {
                ROSmax = ROS;
                BROSmax = FuelEffects.InitialRateOfSpread(fuelIndex, BISI, 0, 0, 0, season);
                FROSmax = FuelEffects.InitialRateOfSpread(fuelIndex, RSZ, 0, 0, 0, season);
                maxfuelIndex = fuelIndex;
            }*/
            
            if(maxfuelIndex == -1)
                UI.WriteLine("   Error:  FFMC={0}, WSV={1}.", fireEvent.FFMC, fireEvent.WindSpeed);
            
            
            BROSmax *= fuelTypeParms[maxfuelIndex].MaxBE;

            // Use the Length to Breadth ratio to determine the FLANK rate of spread (FROS).
            //double LB = CalculateLengthBreadthRatio(fireEvent.WindSpeed, maxfuelIndex);
            //FROSmax = (ROSmax + BROSmax)/(LB * 2.0);//        (FBP; 89)
            FROSmax *= fuelTypeParms[maxfuelIndex].MaxBE;
            
            fireEvent.LengthA = FROSmax;
            //fireEvent.LengthB = FROSmax * LB; //((ROSmax + BROSmax) / 2);
            fireEvent.LengthB = ((ROSmax + BROSmax) / 2);
            fireEvent.LengthD = BROSmax;
            //fireEvent.LB = fireEvent.LengthB / fireEvent.LengthA;
            
            UI.WriteLine("      ROSmax = {0:0.000}, FROSmax = {1:0.000}, BROSmax = {2:0.000}.", ROSmax, FROSmax, BROSmax);
            UI.WriteLine("      LBmax = {0:0.000}, WSV = {1}, maxfuelType = {2}", fireEvent.LB, fireEvent.WindSpeed, (FuelTypeCode) maxfuelIndex);
            UI.WriteLine("      LengthA = {0:0.000}, LengthB = {1:0.000}, LengthD = {2:0.000}.", fireEvent.LengthA, fireEvent.LengthB, fireEvent.LengthD);
            UI.WriteLine("      BISI = {0:0.000}, ISI = {1:0.000}, RSZ = {2:0.000}, f(F) = {3:0.000}, f(W) = {4:0.000}.", BISI, ISI, RSZ, f_F, f_W);

            return; 
        }

        private static double CalculateLengthBreadthRatio(int WSV, int fuelIndex)
        {
            // Use the Length to Breadth ratio to determine the FLANK rate of spread (FROS).
            // WSV = wind speed velocity (Event.WindSpeed)
            
            double lengthBreadthRatio = 0.0;  
            //For all types except O-1:
            if((FuelTypeCode) fuelIndex != FuelTypeCode.O1a)
                lengthBreadthRatio = 1.0 + (8.729 * Math.Pow((1.0 - Math.Exp(-0.030 * (double) WSV)), 2.155));//    (FBP; 79)
            //For O-1:
            else
            {
                if (WSV < 1.0)  
                    lengthBreadthRatio = 1.0; //(FBP; 81)
                else
                    lengthBreadthRatio = 1.1 + Math.Pow((double)WSV, 0.464); //(FBP; 80)
            }
            return lengthBreadthRatio;
        }

        private static bool ElipseDistanceBetweenSites(Site igSite, Site burnSite, double C, double D, double fireDirection)
        {
            bool lessThan = false;
            
            //In the landscape, 'higher' rows have lower numbers, the inverse of what you 
            //would expect if the ignition site is located at (0,0).
            double dYburn = ((double) igSite.Location.Row - (double) burnSite.Location.Row);
            double dXburn = ((double) burnSite.Location.Column - (double) igSite.Location.Column);
        
            C = C / Model.Core.CellLength;
            D = D / Model.Core.CellLength;
            
            //Assume that ignition site (F0) is at location 0,0.
            double LengthA = Math.Sqrt((dXburn*dXburn) + (dYburn*dYburn));
        
            //Where is second focal site?
            double dYf1 = C * 2 * Math.Sin((90 + fireDirection) * Math.PI / 180);
            double dXf1 = C * 2 * Math.Cos((90 + fireDirection) * Math.PI / 180);
            
            double dXburn_f1 = dXburn - dXf1;
            double dYburn_f1 = dYburn - dYf1;
            
            double LengthB = Math.Sqrt((dXburn_f1 * dXburn_f1) + (dYburn_f1 * dYburn_f1));

            if(LengthA + LengthB <= ((C + D) * 2.0) )  
                lessThan = true;
            //UI.WriteLine("LengthA={0:0.0}, LengthB={1:0.0}, C={2:0.0}, D={3:0.0}.", LengthA, LengthB, C,D);
            //UI.WriteLine("dXf1={0:0.0}, dYf1={1:0.0}, dXburn={2:0.0}, dYburn={3:0.0}.", dXf1, dYf1,dXburn,dYburn);
            //return true;
            return lessThan;
        }



        //-------------------------------------------------------
        //Calculate the distance (in units of cells) from a location to a center
        //point (row and column = 0).
        private static double CellDistanceBetweenSites(Site asite, Site bsite)
        {
            double row = ((double) asite.Location.Row - (double) bsite.Location.Row);
            double column = ((double) asite.Location.Column - (double) bsite.Location.Column);
             double cSq = column * column;
             double rSq = row * row;
             return Math.Sqrt(cSq + rSq);
        }

        private static List<Site> Get8Neighbors(Site site)
        {
            List<Site> neighbors = new List<Site>();
        
            RelativeLocation[] neighborhood = new RelativeLocation[] 
            {
                new RelativeLocation(-1,  0),  // north
                new RelativeLocation(-1,  1),  // northeast
                new RelativeLocation( 0,  1),  // east
                new RelativeLocation( 1,  1),  // southeast
                new RelativeLocation( 1,  0),  // south
                new RelativeLocation( 1, -1),  // southwest
                new RelativeLocation( 0, -1),  // west
                new RelativeLocation(-1, -1),  // northwest
            };

            foreach (RelativeLocation relativeLoc in neighborhood) 
            {
                Site neighbor = site.GetNeighbor(relativeLoc);
                if(neighbor != null && neighbor.IsActive)
                    neighbors.Add(neighbor);
            }
            Landis.Util.Random.Shuffle(neighbors);
        
            return neighbors;
        }
        
        private static double CosSquared(double num)
        {
            return (Math.Cos(num) * Math.Cos(num));
        }
        private static double SinSquared(double num)
        {
            return (Math.Sin(num) * Math.Sin(num));
        }
        private static List<int> CalculateSlopeEffect(int windSpeed, int windDirection, 
                                                Site site, double RSZ, double f_F,
                                                ISeasonParameters season)
        {
            List<int> siteWindList = new List<int>();
            if (SiteVars.GroundSlope[site] == 0)
            {
                siteWindSpeed = windSpeed;
                siteWindDirection = windDirection;
                //nothing is changed
            }
            else
            {
                FuelTypeCode siteFuelType = (FuelTypeCode)SiteVars.CFSFuelType[site]; ;

                //Walk through the equations from FBP:

               
                double SF = CalculateSF(SiteVars.GroundSlope[site]);  //FBP 39

                double RSF = RSZ * SF;  //FBP 40

                double ISF = 0.0;
                double a, b, c;
                int fuelIndex = SiteVars.CFSFuelType[site];

                if (siteFuelType == FuelTypeCode.O1a)
                {
                    int percentCuring = season.PercentCuring;
                    double CF = (0.02 * percentCuring) - 1.0;
                    if (season.NameOfSeason == SeasonName.Spring)
                    {
                        a = Event.fuelTypeParms[(int)FuelTypeCode.O1a].A;
                        b = Event.fuelTypeParms[(int)FuelTypeCode.O1a].B;
                        c = Event.fuelTypeParms[(int)FuelTypeCode.O1a].C;
                    }
                    else
                    {
                        a = Event.fuelTypeParms[(int)FuelTypeCode.O1b].A;
                        b = Event.fuelTypeParms[(int)FuelTypeCode.O1b].B;
                        c = Event.fuelTypeParms[(int)FuelTypeCode.O1b].C;
                    }

                    ISF = Math.Log((1 - Math.Pow((RSF / (CF * a)), (1 / c)))) / (-1 * b); //FBP 43
                }
                else
                {
                    a = Event.fuelTypeParms[fuelIndex].A;
                    b = Event.fuelTypeParms[fuelIndex].B;
                    c = Event.fuelTypeParms[fuelIndex].C;

                    if (SiteVars.PercentHardwood[site] > 0)
                    {
                        int PC = SiteVars.PercentConifer[site];
                        ISF = Math.Log((1 - Math.Pow(((100 - RSF) / (PC * a)), (1 / c)))) / (-1 * b); //FBP 42
                    }
                    else
                    {
                        ISF = Math.Log((1 - Math.Pow(((RSF) / (a)), (1 / c)))) / (-1 * b); //FBP 41
                    }
                }
                double WSE = (Math.Log((ISF) / (.208 * f_F))) / (0.05039); //FBP 43: The effect the % slope would have on ROS if it were a wind speed
                double WAZ = windDirection * Math.PI / 180;  //wind direction/azimuth in radians
                double SAZ = SiteVars.UphillSlopeAzimuth[site] * Math.PI / 180;  //uphill slope azimuth in radians
                
                double WSX = (windSpeed * Math.Sin(WAZ)) + (WSE * Math.Sin(SAZ));  //FBP 47 
                double WSY = (windSpeed * Math.Cos(WAZ)) + (WSE * Math.Cos(SAZ));  //FBP 48

                 
                double WSV = Math.Sqrt(Math.Pow(WSX, 2) + Math.Pow(WSY, 2));  //FBP 49
                siteWindSpeed = (int)WSV;

                double RAZ = Math.Acos(WSY / WSV);  //FBP 50

                if (WSX < 0) RAZ = 360 - RAZ;
                siteWindDirection = (int)RAZ;
            }
            siteWindList.Add(siteWindSpeed);
            siteWindList.Add(siteWindDirection);
            SiteVars.SiteWindSpeed[site] = (ushort)siteWindSpeed;
            SiteVars.SiteWindDirection[site] = (ushort)siteWindDirection;

            return siteWindList;
            
            
            
        }
        
        private static double CalculateSF(ushort groundSlope)
        {
            return Math.Pow(Math.E, 3.533 * Math.Pow(((double)groundSlope / 100),1.2));  //FBP 39
        }

        private static double CalculateWSE(int fuelIndex, double RSF, double f_F, int PC, ISeasonParameters season)
        {
            FuelTypeCode siteFuelType = (FuelTypeCode) fuelIndex;
            double ISF, WSE;
            double a = Event.fuelTypeParms[fuelIndex].A;
            double b = Event.fuelTypeParms[fuelIndex].B;
            double c = Event.fuelTypeParms[fuelIndex].C;
            if (siteFuelType == FuelTypeCode.O1a)
            {
                double CF = (0.02 * season.PercentCuring) - 1;

                ISF = Math.Log(1 - Math.Pow((RSF/(CF * a)),(1/c)))/(-1 * b);
            }
            else
            {
                ISF = Math.Log(1 - Math.Pow((RSF / ( a)), (1 / c))) / (-1 * b);
            }
            WSE = Math.Log(ISF / (0.208 * f_F)) / 0.05039;
            return WSE;
        }
    }
}
