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

namespace Landis.Fire
{

    public enum SeasonName {Spring, Summer, Fall};
    public enum LeafOnOff  {LeafOn, LeafOff};
    public enum Distribution {gamma, lognormal, normal, Weibull};

    public interface ISeasonParameters
    {
        SeasonName NameOfSeason{get;}
        LeafOnOff LeafStatus{get;}
        double FireProbability{get;}
        Distribution WSVDist{get;}
        double WSVP1{get;}
        double WSVP2{get;}
        Distribution FFMCDist{get;}
        double FFMCP1{get;}
        double FFMCP2{get;}
        Distribution BUIDist{get;}
        double BUIP1{get;}
        double BUIP2{get;}
        int PercentCuring{get;}
    }
}

namespace Landis.Fire
{
    public class SeasonParameters
    : ISeasonParameters
    {
        private SeasonName nameOfSeason;
        private LeafOnOff leafStatus;
        private double fireProbability;
        private Distribution WSVdist;
        private double WSVp1;
        private double WSVp2;
        private Distribution FFMCdist;
        private double FFMCp1;
        private double FFMCp2;
        private Distribution BUIdist;
        private double BUIp1;
        private double BUIp2;
        private int percentCuring;
        
        //---------------------------------------------------------------------
        public SeasonName NameOfSeason
        {
            get {
                return nameOfSeason;
            }
        }
        //---------------------------------------------------------------------
        public double FireProbability
        {
            get {
                return fireProbability;
            }
        }

        //---------------------------------------------------------------------
        public Distribution WSVDist
        {
            get {
                return WSVdist;
            }
        }
        //---------------------------------------------------------------------
        public double WSVP1
        {
            get {
                return WSVp1;
            }
        }
        //---------------------------------------------------------------------
        public double WSVP2
        {
            get {
                return WSVp2;
            }
        }
        //---------------------------------------------------------------------
        public Distribution FFMCDist
        {
            get {
                return FFMCdist;
            }
        }
        //---------------------------------------------------------------------
        public double FFMCP1
        {
            get {
                return FFMCp1;
            }
        }
        //---------------------------------------------------------------------
        public double FFMCP2
        {
            get {
                return FFMCp2;
            }
        }
        //---------------------------------------------------------------------
        public Distribution BUIDist
        {
            get {
                return BUIdist;
            }
        }
        //---------------------------------------------------------------------
        public double BUIP1
        {
            get {
                return BUIp1;
            }
        }
        //---------------------------------------------------------------------
        public double BUIP2
        {
            get {
                return BUIp2;
            }
        }
        //---------------------------------------------------------------------
        public int PercentCuring
        {
            get {
                return percentCuring;
            }
        }
        //---------------------------------------------------------------------
        public LeafOnOff LeafStatus
        {
            get {
                return leafStatus;
            }
        }
        //---------------------------------------------------------------------

        public SeasonParameters(
            SeasonName nameOfSeason,
            LeafOnOff leafStatus,
            double fireProbability,
            Distribution WSVdist,
            double WSVp1,
            double WSVp2,
            Distribution FFMCdist,
            double FFMCp1,
            double FFMCp2,
            Distribution BUIdist,
            double BUIp1,
            double BUIp2,
            int percentCuring
            )
        {
            this.nameOfSeason = nameOfSeason;
            this.leafStatus = leafStatus;
            this.fireProbability = fireProbability;
            this.WSVdist = WSVdist;
            this.WSVp1 = WSVp1;
            this.WSVp2 = WSVp2;
            this.FFMCdist = FFMCdist;
            this.FFMCp1 = FFMCp1;
            this.FFMCp2 = FFMCp2;
            this.BUIdist = BUIdist;
            this.BUIp1 = BUIp1;
            this.BUIp2 = BUIp2;
            this.percentCuring = percentCuring;
        }

        //---------------------------------------------------------------------

        public SeasonParameters()
        {
            this.nameOfSeason = 0;  //Spring
            this.leafStatus = 0; //LeafOn
            this.fireProbability = 0.0;
            this.WSVdist = 0;
            this.WSVp1 = 0;
            this.WSVp2 = 0;
            this.FFMCdist = 0;
            this.FFMCp1 = 0;
            this.FFMCp2 = 0;
            this.BUIdist = 0;
            this.BUIp1 = 0;
            this.BUIp2 = 0;
            this.percentCuring = 0;
        }


    }
}
