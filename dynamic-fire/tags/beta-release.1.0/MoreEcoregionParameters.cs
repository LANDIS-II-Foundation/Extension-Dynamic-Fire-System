//  Copyright 2005 University of Wisconsin
//  Authors:  Robert M. Scheller, James B. Domingo
//  License:  Available at  
//  http://landis.forest.wisc.edu/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Landis.Landscape;
using System.Collections.Generic;
using Edu.Wisc.Forest.Flel.Grids;


namespace Landis.Fire
{
    /// <summary>
    /// Size and frequency parameters for Fire events in an ecoregion.
    /// </summary>
    public interface IMoreEcoregionParameters
    {
        double MeanSize{get;}
        double StandardDeviation{get;}
        int SpringFMCLo{get;}
        int SpringFMCHi{get;}
        double SpringFMCHiProp{get;}
        int SummerFMCLo{get;}
        int SummerFMCHi{get;}
        double SummerFMCHiProp{get;}
        int FallFMCLo{get;}
        int FallFMCHi{get;}
        double FallFMCHiProp{get;}
        FuelTypeCode OpenFuelType{get;}
        double EcoIgnitionProb{get;}

        //List of site (row, col):
        List<Location> EcoregionSites {get;}

    }
}


namespace Landis.Fire
{
    public class MoreEcoregionParameters
        : IMoreEcoregionParameters
    {
        private double meanSize;
        private double standardDeviation;
        private int springFMCLo;
        private int springFMCHi;
        private double springFMCHiProp;
        private int summerFMCLo;
        private int summerFMCHi;
        private double summerFMCHiProp;
        private int fallFMCLo;
        private int fallFMCHi;
        private double fallFMCHiProp;
        private FuelTypeCode openFuelType;
        private double ecoIgnitionProb;
        private List<Location> ecoregionSites;
        

        //---------------------------------------------------------------------


        /// <summary>
        /// Mean event size (hectares).
        /// </summary>
        public double MeanSize
        {
            get {
                return meanSize;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Standard deviation event size (hectares).
        /// </summary>
        public double StandardDeviation
        {
            get {
                return standardDeviation;
            }
        }
        
        public int SpringFMCLo
        {
            get {
                return springFMCLo;
            }
        }
        public int SpringFMCHi
        {
            get {
                return springFMCHi;
            }
        }
        public double SpringFMCHiProp
        {
            get {
                return springFMCHiProp;
            }
        }
        public int SummerFMCLo
        {
            get {
                return summerFMCLo;
            }
        }
        public int SummerFMCHi
        {
            get {
                return summerFMCHi;
            }
        }
        public double SummerFMCHiProp
        {
            get {
                return summerFMCHiProp;
            }
        }
        public int FallFMCLo
        {
            get {
                return fallFMCLo;
            }
        }
        public int FallFMCHi
        {
            get {
                return fallFMCHi;
            }
        }
        public double FallFMCHiProp
        {
            get {
                return fallFMCHiProp;
            }
        }
        public FuelTypeCode OpenFuelType
        {
            get {
                return openFuelType;
            }
        }
        public double EcoIgnitionProb
        {
            get {
                return ecoIgnitionProb;
            }
        }
        public List<Location> EcoregionSites
        {
            get
            {
                return ecoregionSites;
            }
        }
        //---------------------------------------------------------------------

        public MoreEcoregionParameters(
                                double meanSize,
                                double standardDeviation,
                                int springFMCLo,
                                int springFMCHi,
                                double springFMCHiProp,
                                int summerFMCLo,
                                int summerFMCHi,
                                double summerFMCHiProp,
                                int fallFMCLo,
                                int fallFMCHi,
                                double fallFMCHiProp,
                                FuelTypeCode openFuelType,
                                double ecoIgnitionProb
                                )
        {
            this.meanSize = meanSize;
            this.standardDeviation = standardDeviation;
            this.springFMCLo =      springFMCLo;
            this.springFMCHi =      springFMCHi; 
            this.springFMCHiProp =  springFMCHiProp;
            this.summerFMCLo =      summerFMCLo;
            this.summerFMCHi =      summerFMCHi; 
            this.summerFMCHiProp =  summerFMCHiProp;
            this.fallFMCLo =        fallFMCLo;
            this.fallFMCHi =        fallFMCHi; 
            this.fallFMCHiProp =    fallFMCHiProp;
            this.openFuelType =     openFuelType;
            this.ecoIgnitionProb =  ecoIgnitionProb;
            this.ecoregionSites = new List<Location>();
        }

        //---------------------------------------------------------------------

        public MoreEcoregionParameters()
        {
            this.meanSize = 0.0;
            this.standardDeviation = 0.0;
            this.springFMCLo =      0;
            this.springFMCHi =      0;
            this.springFMCHiProp =  0.0;
            this.summerFMCLo =      0;
            this.summerFMCHi =      0;
            this.summerFMCHiProp =  0.0;
            this.fallFMCLo =        0;
            this.fallFMCHi =        0;
            this.fallFMCHiProp =    0.0;
            this.openFuelType =     FuelTypeCode.O1a;
            this.ecoIgnitionProb =  1.0;
            this.ecoregionSites = new List<Location>();
        }
    }
}
