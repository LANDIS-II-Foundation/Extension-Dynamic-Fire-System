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

    //This enumerated list must EXACTLY match the FuelTypeCode list found in the fire 2006 extension.
    public enum FuelTypeCode {C1, C2, C3, C4, C5, C6, C7, D1, S1, S2, S3, M1, M2, M3, M4, O1a, O1b, NoFuel};

    public interface IFuelTypeParameters
    {
        double InitiationProbability{get;}
        int A{get;}
        double B{get;}
        double C{get;}
        double Q{get;}
        int BUI{get;}
        double MaxBE{get;}
        int CBH{get;}  //Crown base height
    }
}

namespace Landis.Fire
{
    public class FuelTypeParameters
    : IFuelTypeParameters
    {
        private double initiationProbability;
        private int a;
        private double b;
        private double c;
        private double q;
        private int bui;
        private double maxBE;
        private int cbh;
        
        //---------------------------------------------------------------------
        public double InitiationProbability
        {
            get {
                return initiationProbability;
            }
        }

        //---------------------------------------------------------------------
        public int A
        {
            get {
                return a;
            }
        }
        //---------------------------------------------------------------------
        public double B
        {
            get {
                return b;
            }
        }
        //---------------------------------------------------------------------
        public double C
        {
            get {
                return c;
            }
        }
        //---------------------------------------------------------------------
        public double Q
        {
            get {
                return q;
            }
        }
        //---------------------------------------------------------------------
        public int BUI
        {
            get {
                return bui;
            }
        }
        //---------------------------------------------------------------------
        public double MaxBE
        {
            get {
                return maxBE;
            }
        }
        //---------------------------------------------------------------------
        public int CBH
        {
            get {
                return cbh;
            }
        }
        //---------------------------------------------------------------------

        public FuelTypeParameters(
            double initiationProbability,
            int a,
            double b,
            double c,
            double q,
            int bui,
            double maxBE,
            int cbh)
        {
            this.initiationProbability = initiationProbability;
            this.a = a;
            this.b = b;
            this.c = c;
            this.q = q;
            this.bui = bui;
            this.maxBE = maxBE;
            this.cbh = cbh;
        }

        //---------------------------------------------------------------------

        public FuelTypeParameters()
        {
            this.initiationProbability = 0.0;
            this.a = 0;
            this.b = 0.0;
            this.c = 0.0;
            this.q = 0.0;
            this.bui = 0;
            this.maxBE = 0.0;
            this.cbh = 0;
        }


    }
}
