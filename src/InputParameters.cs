//  Authors:  Robert M. Scheller, Brian R. Miranda 

using Landis.Utilities;
using System.Collections.Generic;
using Landis.Core;


namespace Landis.Extension.DynamicFire
{

    public enum SizeType {size_based, duration_based};
    public enum Distribution {gamma, lognormal, normal, Weibull};

    /// <summary>
    /// Parameters for the plug-in.
    /// </summary>
    public interface IInputParameters
    {
        int Timestep{get;set;}
        Landis.Library.Parameters.Species.AuxParm<int> FireTolerance { get; }
        SizeType FireSizeType{get;set;}
        bool BUI{get;set;}
        double SeverityCalibrate { get;set;}
        List<IDynamicFireRegion> DynamicFireRegions {get;}
        List<IDynamicWeather> DynamicWeather { get;}
        ISeasonParameters[] SeasonParameters{get;}
        IFuelType[] FuelTypeParameters{get;}
        List<IFireDamage> FireDamages{get;}
        string MapNamesTemplate{get;set;}
        string LogFileName{get;set;}
        string SummaryLogFileName{get;set;}
        string InitialWeatherPath{get;set;}
    }
}

namespace Landis.Extension.DynamicFire
{
    /// <summary>
    /// Parameters for the plug-in.
    /// </summary>
    public class InputParameters
        : IInputParameters
    {
        private int timestep;
        private string mapNamesTemplate;

        public Landis.Library.Parameters.Species.AuxParm<int> FireTolerance { get; set; }


        //---------------------------------------------------------------------

        /// <summary>
        /// Timestep (years)
        /// </summary>
        public int Timestep
        {
            get {
                return timestep;
            }
            set {
                    if (value < 0)
                        throw new InputValueException(value.ToString(),
                                                      "Value must be = or > 0.");
                timestep = value;
            }
        }
        //---------------------------------------------------------------------
        public SizeType FireSizeType { get; set; }
        //---------------------------------------------------------------------

        public bool BUI { get; set; }

        //---------------------------------------------------------------------
        /*
        public int WeatherRandomizer
        {
            get {
                return weatherRandomizer;
            }
        }*/
        //---------------------------------------------------------------------

        public double SeverityCalibrate { get; set; }

        //---------------------------------------------------------------------

        public List<IDynamicFireRegion> DynamicFireRegions { get; }
        //---------------------------------------------------------------------
        public List<IDynamicWeather> DynamicWeather { get; }
        //---------------------------------------------------------------------

        public ISeasonParameters[] SeasonParameters { get; set; }
        //---------------------------------------------------------------------
        public IFuelType[] FuelTypeParameters { get; }

        //---------------------------------------------------------------------
        public List<IFireDamage> FireDamages { get; }

        //---------------------------------------------------------------------

        /// <summary>
        /// Template for the filenames for output maps.
        /// </summary>
        public string MapNamesTemplate
        {
            get {
                return mapNamesTemplate;
            }
            set {
                    MapNames.CheckTemplateVars(value);
                mapNamesTemplate = value;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Name of log file.
        /// </summary>
        public string LogFileName { get; set; }

        //---------------------------------------------------------------------

        /// <summary>
        /// Name of log file.
        /// </summary>
        public string InitialWeatherPath { get; set; }
        /// <summary>
        /// Name of log file.
        /// </summary>
        public string SummaryLogFileName { get; set; }
        //---------------------------------------------------------------------

        public InputParameters(ISpeciesDataset speciesDataset)
        {
            SeasonParameters = new SeasonParameters[3];
            FireDamages = new List<IFireDamage>();
            DynamicFireRegions = new List<IDynamicFireRegion>();
            DynamicWeather = new List<IDynamicWeather>();
            
            FuelTypeParameters = new FuelType[100];
            for(int i=0; i<100; i++)
                FuelTypeParameters[i] = new FuelType();
            FireTolerance = new Landis.Library.Parameters.Species.AuxParm<int>(speciesDataset);
        }
       
    }
}
