//  Copyright 2005 University of Wisconsin
//  Authors:  Robert M. Scheller, James B. Domingo
//  License:  Available at  
//  http://landis.forest.wisc.edu/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Edu.Wisc.Forest.Flel.Grids;
using Landis.AgeCohort;
using Landis.Ecoregions;
using Landis.Landscape;
using Landis.PlugIns;
using Landis.Species;
using Landis.Util;
using System.Collections.Generic;
//using System.Math;

namespace Landis.Fire
{
    public class FuelEffects
    {

        public static double InitialRateOfSpread(int fuelIndex, double ISI, 
                                                int PC, int PH, int PDF,
                                                ISeasonParameters season)
        {   

            FuelTypeCode siteFuelType = (FuelTypeCode) fuelIndex;
            
            //UI.WriteLine("Fuel Type Code = {0}.", siteFuelType.ToString());
            
            double RSI = 0.0;  
            
            if (siteFuelType == FuelTypeCode.C1 ||
                siteFuelType == FuelTypeCode.C2 ||
                siteFuelType == FuelTypeCode.C3 ||
                siteFuelType == FuelTypeCode.C4 ||
                siteFuelType == FuelTypeCode.C5 ||
                siteFuelType == FuelTypeCode.C6 ||
                siteFuelType == FuelTypeCode.C7)
            {
                double a = Event.fuelTypeParms[fuelIndex].A;
                double b = Event.fuelTypeParms[fuelIndex].B;
                double c = Event.fuelTypeParms[fuelIndex].C;
                
                double percentHard = (double) PH / 100.0;
                double percentConi = (double) PC / 100.0;
                
                RSI = CalculateRSI(a, b, c, ISI);
                
                if(PH > 0)
                {
                    double RSIconifer = RSI;
                    int dIndex = (int) FuelTypeCode.D1;
                    double RSIdecid = CalculateRSI(Event.fuelTypeParms[dIndex].A, Event.fuelTypeParms[dIndex].B, Event.fuelTypeParms[dIndex].C, ISI);
                
                    if(season.LeafStatus == LeafOnOff.LeafOn) 
                        RSI = (percentHard * RSIconifer) + (0.2 * percentHard * RSIdecid);
                    else 
                        RSI = (percentConi * RSIconifer) + (percentHard * RSIdecid);

                    //UI.WriteLine("Calculating ROSi for a MIXED type. PH={0}, PC={1}, LeafStatus={2}.", PH, PC, season.LeafStatus);
                    //UI.WriteLine("  RSIcon={0:0.0}, RSIdecid={1:0.0}, RSImix={2:0.000}.", RSIconifer, RSIdecid, RSI);
                    
                }
            }
                
            if( siteFuelType == FuelTypeCode.D1 ||
                siteFuelType == FuelTypeCode.S1 ||
                siteFuelType == FuelTypeCode.S2 ||
                siteFuelType == FuelTypeCode.S3)
            {
                //UI.WriteLine("Calculating ROSi for a DECIDUOUS or SLASH type.");

                double a = Event.fuelTypeParms[fuelIndex].A;
                double b = Event.fuelTypeParms[fuelIndex].B;
                double c = Event.fuelTypeParms[fuelIndex].C;
                    
                RSI = CalculateRSI(a, b, c, ISI);

                if(siteFuelType == FuelTypeCode.D1 && season.LeafStatus == LeafOnOff.LeafOn)
                    RSI *= 0.2;

            }
  /*
            if(siteFuelType == FuelTypeCode.M2)
            {

                int cIndex = (int) FuelTypeCode.C2;
                int dIndex = (int) FuelTypeCode.D1;
                
                double RSI_c2 = CalculateRSI(Event.fuelTypeParms[cIndex].A, Event.fuelTypeParms[cIndex].B, Event.fuelTypeParms[cIndex].C, ISI);
                double RSI_d1 = CalculateRSI(Event.fuelTypeParms[dIndex].A, Event.fuelTypeParms[dIndex].B, Event.fuelTypeParms[dIndex].C, ISI);
                
                RSI = ((PC/100 * RSI_c2) + (0.2 * PH/100 * RSI_d1));
            }
*/            
            if(PH > 0 && PDF > 0)
            {
                double a, b, c;
                if(season.LeafStatus == LeafOnOff.LeafOff)
                {
                    a = 170 * System.Math.Exp(-35 / PDF);
                    b = 0.082 * System.Math.Exp(-36 / PDF);
                    c = 1.698 - 0.00303 * PDF;
                } else 
                {
                    a = 170 * System.Math.Exp(-35 / PDF);
                    b = 0.0404;
                    c = 3.02 * System.Math.Exp(-0.00714 * PDF);
                }
                RSI = CalculateRSI(a, b, c, ISI);
            }
            /*
            if(siteFuelType == FuelTypeCode.M4)
            {
                double a = 170 * System.Math.Exp(-35 / PDF);
                double b = 0.0404;
                double c = 3.02 * System.Math.Exp(-0.00714 * PDF);
                
                RSI = CalculateRSI(a, b, c, ISI);
            }
            
            if(siteFuelType == FuelTypeCode.O1a || siteFuelType == FuelTypeCode.O1b)
            {
                double a = Event.fuelTypeParms[fuelIndex].A;
                double b = Event.fuelTypeParms[fuelIndex].B;
                double c = Event.fuelTypeParms[fuelIndex].C;
                
                double CF = (0.02 * percentCuring) - 1.0;
                RSI = CalculateRSI(a, b, c, ISI) * CF;
            }*/

            if(siteFuelType == FuelTypeCode.O1a)
            {
                double a, b, c;
                int percentCuring = season.PercentCuring;
                
                if(season.NameOfSeason == SeasonName.Spring)
                {
                    a = Event.fuelTypeParms[(int) FuelTypeCode.O1a].A;
                    b = Event.fuelTypeParms[(int) FuelTypeCode.O1a].B;
                    c = Event.fuelTypeParms[(int) FuelTypeCode.O1a].C;
                } else 
                {
                    a = Event.fuelTypeParms[(int) FuelTypeCode.O1b].A;
                    b = Event.fuelTypeParms[(int) FuelTypeCode.O1b].B;
                    c = Event.fuelTypeParms[(int) FuelTypeCode.O1b].C;
                }
                
                double CF = (0.02 * percentCuring) - 1.0;
                RSI = CalculateRSI(a, b, c, ISI);
                if(percentCuring > 50) 
                    RSI *= CF;
                else
                    RSI = 0;
            }
            
            if(siteFuelType == FuelTypeCode.NoFuel)
            {
                return 0;
            }

            
            return RSI;
        }


        public static double CalculateRSI(double a, double b, double c, double ISI)
        {
            double RSI = a * System.Math.Pow((1 - System.Math.Exp(-1 * b * ISI)), c);
            return RSI;
            
        }

        //---------------------------------------------------------------------
        
        public static double FuelMoistureBuildupIndex(double bui, double q, double bui_0)
        {

            double buildupEffect = 1.0;  
            buildupEffect = System.Math.Exp(50 * System.Math.Log(q) * ((1/bui)-(1/bui_0)));
            return buildupEffect;
            
        }
        
    }
}
