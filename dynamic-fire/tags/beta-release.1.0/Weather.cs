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
using Troschuetz.Random;

namespace Landis.Fire
{

    public class Weather
    {

        //---------------------------------------------------------------------

        public static ISeasonParameters GenerateSeason(ISeasonParameters[] seasons)
        {
            double randNum = Util.Random.GenerateUniform();
            double bottom = 0.0;
            double top = 0.0;
            foreach (ISeasonParameters season in seasons)
            {
                top += season.FireProbability;
                if(randNum >= bottom && randNum <= top)
                    return season;
                bottom += season.FireProbability;
            }
            return null;
        }

        //---------------------------------------------------------------------

        public static int GenerateWindSpeed(ISeasonParameters season)
        {
            double windSpeed = 0.0;
            windSpeed = GenerateRandomNum(season.WSVDist, season.WSVP1, season.WSVP2);
            if(windSpeed < 0) windSpeed = 0;
            
            return (int) windSpeed;
        }
        //---------------------------------------------------------------------

        public static int GenerateWindDirection(IWindDirectionParameters windDir)
        {
            double randNum = Util.Random.GenerateUniform();
            int primeWindDirection = 0;
            double bottom = 0.0;
            double top = 0.0;
            for (int i = 0; i <= 7; i++)
            {
                top += windDir.WindDirections[i];
                if(randNum >= bottom && randNum <= top)
                {
                    primeWindDirection = i * 45;  //45 degrees per slice
                    break;
                }
                bottom += windDir.WindDirections[i];
            }
            
            //Next, randomize around cardinal direction:
            int wiggle = (int) ((Util.Random.GenerateUniform() * 45.0) - 22.5);
            
            //return primeWindDirection + wiggle;
            return primeWindDirection;
        }

        //---------------------------------------------------------------------

        public static int GenerateFineFuelMoistureCode(ISeasonParameters season)
        {
            double ffmc = 0.0;
            ffmc = GenerateRandomNum(season.FFMCDist, season.FFMCP1, season.FFMCP2);
            if(ffmc < 0) ffmc = 0;
            if(ffmc > 100) ffmc = 100;
            
            return (int) ffmc;
        }

        //---------------------------------------------------------------------

        public static int GenerateBuildUpIndex(ISeasonParameters season)
        {
            double bui = 0.0;
            bui = GenerateRandomNum(season.BUIDist, season.BUIP1, season.BUIP2);
            if(bui < 0) bui = 0;
            if(bui > 200) bui = 200;
            
            return (int) bui;
        }

        //---------------------------------------------------------------------

        public static int GenerateFoliarMoistureContent(ISeasonParameters season, IEcoregion ecoregion)
        {
            double bui = 0.0;
            bui = GenerateRandomNum(season.BUIDist, season.BUIP1, season.BUIP2);
            if(bui < 0) bui = 0;
            if(bui > 200) bui = 200;
            
            return (int) bui;
        }


        //---------------------------------------------------------------------

        public static double CalculateFuelMoistureEffect(double FFMC)
        {
            // FBP 45 and 46:
            double f_F = 0.0;
            double m = (147.2 * (101 - FFMC)) / (59.5 + FFMC);
            
            f_F = 91.9 * System.Math.Exp(-0.1386 * m) *
                            (1 + (System.Math.Pow(m, 5.31) / 49300000));
            return f_F;
        }
        //---------------------------------------------------------------------
        public static double CalculateWindEffect(double WSV)
        {
            // FBP 53 and 53a:
            double f_W = 0.0;
            if(WSV  <= 40)  //0.06 = convert back to km/hr
                f_W = System.Math.Exp(0.05039 * WSV);
            else
                f_W = 12 * (1 - System.Math.Exp(-0.0818 * (WSV - 28)));
            return f_W;
        }
        //---------------------------------------------------------------------
        public static double CalculateBackWindEffect(double WSV)
        {
            // FBP 75:
            double f_W = System.Math.Exp(-1 * 0.05039 * WSV);
            return f_W;
        }
        //---------------------------------------------------------------------

        private static double GenerateRandomNum(Distribution dist, double parameter1, double parameter2)
        {
            double randomNum = 0.0;
            if(dist == Distribution.normal)
            {
                NormalDistribution randVar = new NormalDistribution(RandomNumberGenerator.Singleton);
                randVar.Mu = parameter1;      // mean
                randVar.Sigma = parameter2;   // std dev
                randomNum = randVar.NextDouble();
            }
            if(dist == Distribution.lognormal)
            {
                LognormalDistribution randVar = new LognormalDistribution(RandomNumberGenerator.Singleton);
                randVar.Mu = parameter1;      // mean
                randVar.Sigma = parameter2;   // std dev
                randomNum = randVar.NextDouble();
            }
            if(dist == Distribution.gamma)
            {
                GammaDistribution randVar = new GammaDistribution(RandomNumberGenerator.Singleton);
                randVar.Alpha = parameter1;      // mean
                randVar.Theta = parameter2;   // std dev
                randomNum = randVar.NextDouble();
            }
            if(dist == Distribution.Weibull)
            {
                WeibullDistribution randVar = new WeibullDistribution(RandomNumberGenerator.Singleton);
                randVar.Alpha = parameter1;      // mean
                randVar.Lambda = parameter2;   // std dev
                randomNum = randVar.NextDouble();
            }
            return randomNum;
        }


    }
}
