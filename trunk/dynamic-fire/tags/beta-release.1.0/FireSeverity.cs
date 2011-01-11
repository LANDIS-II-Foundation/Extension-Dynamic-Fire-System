//  Copyright 2005 University of Wisconsin
//  Authors:  Robert M. Scheller, James B. Domingo
//  License:  Available at  
//  http://landis.forest.wisc.edu/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

//using Edu.Wisc.Forest.Flel.Grids;
//using Landis.AgeCohort;
//using Landis.Ecoregions;
using Landis.Landscape;
//using Landis.PlugIns;
using Landis.Species;
using Landis.Util;
using System.Collections.Generic;
using System;

namespace Landis.Fire
{
    public class FireSeverity
    {

        public static int CalcFireSeverity(ActiveSite site, Event fireEvent)
        {

            IEcoregion ecoregion = SiteVars.Ecoregion[site];
            IMoreEcoregionParameters fireParms = ecoregion.MoreEcoregionParameters;
            int PH = SiteVars.PercentHardwood[site];  //Percent Hardwood
            int PDF = SiteVars.PercentDeadFir[site];
            
            int fuelIndex = SiteVars.CFSFuelType[site];
            
            int CBH = fireEvent.FuelTypeParms[fuelIndex].CBH;
            int FFMC = fireEvent.FFMC;
            int BUI = fireEvent.BuildUpIndex;
            double SFC = SurfaceFuelConsumption(fuelIndex, FFMC, BUI, PH, PDF);
            int severity = 0;
        
            int FMC = 0;  //Foliar Moisture Content
            if(fireEvent.FireSeason.NameOfSeason == SeasonName.Spring)
            {
                if (Util.Random.GenerateUniform() < fireParms.SpringFMCHiProp)
                    FMC = fireParms.SpringFMCHi;
                else
                    FMC = fireParms.SpringFMCLo;
            }
            if(fireEvent.FireSeason.NameOfSeason == SeasonName.Summer)
            {
                if (Util.Random.GenerateUniform() < fireParms.SummerFMCHiProp)
                    FMC = fireParms.SummerFMCHi;
                else
                    FMC = fireParms.SummerFMCLo;
            }
            if(fireEvent.FireSeason.NameOfSeason == SeasonName.Fall)
            {
                if (Util.Random.GenerateUniform() < fireParms.FallFMCHiProp)
                    FMC = fireParms.FallFMCHi;
                else
                    FMC = fireParms.FallFMCLo;
            }

            //-----Edited by BRM-----
            //double ROS = SiteVars.RateOfSpread[site];
            double ROS = SiteVars.AdjROS[site];
            //----------
            double CSI = 0.001 * Math.Pow(CBH,1.5) * Math.Pow((460 + 25.9 * FMC),1.5);
            double RSO = CSI / 300 * SFC;
            double CFB = 1 - Math.Exp(-0.23 * (ROS - RSO));
            
            
            // Finally, calculate SEVERITY:
            double lowThreshold = (RSO + 0.458)/2.0;
            
            if (CFB >= 0.9) severity = 5;
            if (CFB < 0.9 && CFB >= 0.495) severity = 4;
            if (CFB < 0.495 && CFB >= 0.1) severity = 3;
            if (CFB < 0.1 && ROS >= lowThreshold) severity = 2;
            if (CFB < 0.1 && ROS < lowThreshold) severity = 1;
            
            //UI.WriteLine("      Severity = {0}.  CSI={1}, RSO={2}, ROS={3}, CFB={4}.", severity, CSI, RSO, ROS, CFB);

            return severity;
        }

        ///<summary>
        /// This method calculates the surface fuel consumption.
        ///</summary>
        private static double SurfaceFuelConsumption(int fuelIndex, int FFMC, int BUI, int PH, int PDF)
        {
            double SFC = 0.0;
            FuelTypeCode siteFuelType = (FuelTypeCode) fuelIndex;
            
            if (siteFuelType == FuelTypeCode.C1)
            {
                SFC = 1.5 * (1.0 - Math.Exp(-0.223 * (FFMC - 81)));
                if (SFC < 0) SFC = 0;

                if(PH > 0)
                {
                    double SFC_d1 = 1.5 * (1.0 - Math.Exp(-0.0183 * BUI));
                    SFC = (((100-PH)/100 * SFC) + (PH/100 * SFC_d1));
                }
            }
            
            if (siteFuelType == FuelTypeCode.C2 || 
            //This is now the equivalent of fuel type M-3:
                (PH > 0 && PDF > 0))
            {
                SFC = 5.0 * (1.0 - Math.Exp(-0.0115 * BUI));
                /*if(PH > 0)  //If Percent Hardwood > 0, then it is mixed
                {
                    SFC_d1 = 1.5 * (1.0 - Math.Exp(-0.0183 * BUI));
                    SFC = (((100-PH)/100 * SFC) + (PH/100 * SFC_d1));
                }*/
            }
            if (siteFuelType == FuelTypeCode.C3 || 
                siteFuelType == FuelTypeCode.C4)
            {
                SFC = 5.0 * Math.Pow((1.0 - Math.Exp(-0.0164 * BUI)), 2.24);
                if(PH > 0)
                {
                    double SFC_d1 = 1.5 * (1.0 - Math.Exp(-0.0183 * BUI));
                    SFC = (((100-PH)/100 * SFC) + (PH/100 * SFC_d1));
                }
            }
            if (siteFuelType == FuelTypeCode.C5 || 
                siteFuelType == FuelTypeCode.C6)
            {
                SFC = 5.0 * Math.Pow((1.0 - Math.Exp(-0.0149 * BUI)), 2.48);
                if(PH > 0)
                {
                    double SFC_d1 = 1.5 * (1.0 - Math.Exp(-0.0183 * BUI));
                    SFC = (((100-PH)/100 * SFC) + (PH/100 * SFC_d1));
                }
            }
            if (siteFuelType == FuelTypeCode.C7)
            {
                double FFC = 2.0 * (1.0 - Math.Exp(-0.104 * (FFMC - 70)));
                if (FFC < 0) FFC = 0;
                double WFC = 1.5 * (1.0 - Math.Exp(-0.0201 * BUI));
                SFC = FFC + WFC;
                
                if(PH > 0)
                {
                    double SFC_d1 = 1.5 * (1.0 - Math.Exp(-0.0183 * BUI));
                    SFC = (((100-PH)/100 * SFC) + (PH/100 * SFC_d1));
                }
            }
            if (siteFuelType == FuelTypeCode.D1)
            {
                SFC = 1.5 * (1.0 - Math.Exp(-0.0183 * BUI));
            }
            if (siteFuelType == FuelTypeCode.O1a || siteFuelType == FuelTypeCode.O1b )
            {
                SFC = 0.3;
            }
            if (siteFuelType == FuelTypeCode.S1)
            {
                double FFC = 4.0 * (1.0 - Math.Exp(-0.025 * BUI));
                double WFC = 4.0 * (1.0 - Math.Exp(-0.034 * BUI));
                SFC = FFC + WFC;
            }
            if (siteFuelType == FuelTypeCode.S2)
            {
                double FFC = 10.0 * (1.0 - Math.Exp(-0.013 * BUI));
                double WFC = 6.0 * (1.0 - Math.Exp(-0.060 * BUI));
                SFC = FFC + WFC;
            }
            if (siteFuelType == FuelTypeCode.S3)
            {
                double FFC = 12.0 * (1.0 - Math.Exp(-0.0166 * BUI));
                double WFC = 20.0 * (1.0 - Math.Exp(-0.021 * BUI));
                SFC = FFC + WFC;
            }

            return SFC;
        }

    }
}
