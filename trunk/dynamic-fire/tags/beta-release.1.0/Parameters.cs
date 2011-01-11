//  Copyright 2005 University of Wisconsin
//  Authors:  Robert M. Scheller, James B. Domingo
//  License:  Available at  
//  http://landis.forest.wisc.edu/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

namespace Landis.Fire
{

    public enum SizeType {size_based, duration_based};

    /// <summary>
    /// Parameters for the plug-in.
    /// </summary>
    public interface IParameters
    {
        int Timestep{get;}
        SizeType FireSizeType{get;}
        bool BUI{get;}
        ISeasonParameters[] SeasonParameters{get;}
        IWindDirectionParameters[] WindDirectionParameters{get;}
        IFuelTypeParameters[] FuelTypeParameters{get;}
        IDamageTable[] FireDamages{get;}
        string MapNamesTemplate{get;}
        string LogFileName{get;}
        string SummaryLogFileName{get;}
    }
}

namespace Landis.Fire
{
    /// <summary>
    /// Parameters for the plug-in.
    /// </summary>
    public class Parameters
        : IParameters
    {
        private int timestep;
        private SizeType fireSizeType;
        private bool buildUpIndex;
        private ISeasonParameters[] seasonParameters;
        private IWindDirectionParameters[] windDirectionParameters;
        private IFuelTypeParameters[] fuelTypeParameters;
        private IDamageTable[] damages;
        private string mapNamesTemplate;
        private string logFileName;
        private string summaryLogFileName;


        //---------------------------------------------------------------------

        /// <summary>
        /// Timestep (years)
        /// </summary>
        public int Timestep
        {
            get {
                return timestep;
            }
        }
        //---------------------------------------------------------------------
        public SizeType FireSizeType
        {
            get {
                return fireSizeType;
            }
        }
        
        public bool BUI
        {
            get {
                return buildUpIndex;
            }
        }

        //---------------------------------------------------------------------
        public ISeasonParameters[] SeasonParameters
        {
            get {
                return seasonParameters;
            }
        }
        //---------------------------------------------------------------------
        public IWindDirectionParameters[] WindDirectionParameters
        {
            get {
                return windDirectionParameters;
            }
        }
        //---------------------------------------------------------------------
        public IFuelTypeParameters[] FuelTypeParameters
        {
            get {
                return fuelTypeParameters;
            }
        }

        //---------------------------------------------------------------------
        public IDamageTable[] FireDamages
        {
            get {
                return damages;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Template for the filenames for output maps.
        /// </summary>
        public string MapNamesTemplate
        {
            get {
                return mapNamesTemplate;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Name of log file.
        /// </summary>
        public string LogFileName
        {
            get {
                return logFileName;
            }
        }

        /// <summary>
        /// Name of log file.
        /// </summary>
        public string SummaryLogFileName
        {
            get {
                return summaryLogFileName;
            }
        }
        //---------------------------------------------------------------------

        public Parameters(int               timestep,
                          SizeType          fireSizeType,
                          bool              buildUpIndex,
                          ISeasonParameters[]  seasonParameters,
                          IWindDirectionParameters[]  windDirectionParameters,
                          IFuelTypeParameters[]  fuelTypeParameters,
                          IDamageTable[]    damages,
                          string            mapNameTemplate,
                          string            logFileName,
                          string            summaryLogFileName)
        {
            this.timestep = timestep;
            this.fireSizeType = fireSizeType;
            this.buildUpIndex = buildUpIndex;
            this.seasonParameters = seasonParameters;
            this.windDirectionParameters = windDirectionParameters;
            this.fuelTypeParameters = fuelTypeParameters;
            this.damages = damages;
            this.mapNamesTemplate = mapNameTemplate;
            this.logFileName = logFileName;
            this.summaryLogFileName = summaryLogFileName;
        }
    }
}
