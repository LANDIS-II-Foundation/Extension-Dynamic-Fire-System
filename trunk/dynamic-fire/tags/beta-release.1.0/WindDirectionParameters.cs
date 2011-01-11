//  Copyright 2005 University of Wisconsin
//  Authors:  Robert M. Scheller, James B. Domingo
//  License:  Available at  
//  http://landis.forest.wisc.edu/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Landis.Util;
using System.Collections.Generic;

namespace Landis.Fire
{

    public interface IWindDirectionParameters
    {
        SeasonName NameOfSeason{get;}
        double[] WindDirections {get;}
    }
}

namespace Landis.Fire
{
    public class WindDirectionParameters
    : IWindDirectionParameters
    {
        private SeasonName nameOfSeason;
        private double[] windDirections;
        
        //---------------------------------------------------------------------
        public SeasonName NameOfSeason
        {
            get {
                return nameOfSeason;
            }
        }
        //---------------------------------------------------------------------
        public double[] WindDirections
        {
            get {
                return windDirections;
            }
        }
        //---------------------------------------------------------------------

        public WindDirectionParameters(
            SeasonName nameOfSeason,
            double[] windDirections
            )
        {
            this.nameOfSeason = nameOfSeason;
            this.windDirections = windDirections;
        }

        //---------------------------------------------------------------------

        public WindDirectionParameters()
        {
        }


    }
}
