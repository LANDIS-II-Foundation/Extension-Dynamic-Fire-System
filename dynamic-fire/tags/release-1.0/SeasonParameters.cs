//  Copyright 2006-2008 US Forest Service, Conservation Biology Institute
//  Authors:  Brian Miranda, Robert M. Scheller
//  License:  Available at  
//  http://www.landis-ii.org/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Edu.Wisc.Forest.Flel.Grids;
using Edu.Wisc.Forest.Flel.Util;
using System.Collections.Generic;

namespace Landis.Fire
{

    public enum SeasonName {Spring, Summer, Fall};
    public enum LeafOnOff  {LeafOn, LeafOff};

    public interface ISeasonParameters
    {
        SeasonName NameOfSeason{get;set;}
        LeafOnOff LeafStatus{get;set;}
        double FireProbability{get;set;}
        int PercentCuring{get;set;}
        double DayLengthProp{get;set;}
        int RecordCount {get; set;}
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
        private int percentCuring;
        private double dayLengthProp;
        private int recordCount;
        
        //---------------------------------------------------------------------
        public SeasonName NameOfSeason
        {
            get {
                return nameOfSeason;
            }
            set {
                nameOfSeason = value;
            }
        }
        //---------------------------------------------------------------------
        public double FireProbability
        {
            get {
                return fireProbability;
            }
            set {
                    if (value < 0.0 || value > 1.0)
                        throw new InputValueException(value.ToString(),
                            "Value must be between 0 and 1.0");
                fireProbability = value;
            }
        }
     
        //---------------------------------------------------------------------
        public int PercentCuring
        {
            get {
                return percentCuring;
            }
            set {
                    if (value < 0 || value > 100)
                        throw new InputValueException(value.ToString(),
                            "Value must be between 0 and 100");
                percentCuring = value;
            }
        }
        //---------------------------------------------------------------------
        public double DayLengthProp
        {
            get
            {
                return dayLengthProp;
            }
            set
            {
                    if (value < 0 || value > 1)
                        throw new InputValueException(value.ToString(),
                            "Value must be between 0.0 and 1.0");
                dayLengthProp = value;
            }
        }
        //---------------------------------------------------------------------
        public int RecordCount
        {
            get
            {
                return recordCount;
            }
            //New
            set
            {
                recordCount = value;
            }
        }
        //---------------------------------------------------------------------
        public LeafOnOff LeafStatus
        {
            get {
                return leafStatus;
            }
            set {
                leafStatus = value;
            }
        }
        //---------------------------------------------------------------------
        public SeasonParameters()
        {
        }
        //---------------------------------------------------------------------
/*
        public SeasonParameters(
            SeasonName nameOfSeason,
            LeafOnOff leafStatus,
            double fireProbability,
            int percentCuring,
            double dayLengthProp,
            int recordCount
            //System.Data.DataSet weatherDataSet
            )
        {
            this.nameOfSeason = nameOfSeason;
            this.leafStatus = leafStatus;
            this.fireProbability = fireProbability;
            this.percentCuring = percentCuring;
            this.dayLengthProp = dayLengthProp;
            this.recordCount = recordCount;
            //this.weatherDataSet = weatherDataSet;
        }

        //---------------------------------------------------------------------

        public SeasonParameters()
        {
            this.nameOfSeason = 0;  //Spring
            this.leafStatus = 0; //LeafOn
            this.fireProbability = 0.0;
            this.percentCuring = 0;
            this.dayLengthProp = 1.0;
            this.recordCount = 0;
            //this.weatherDataSet = null;
            
        }*/


    }
}
