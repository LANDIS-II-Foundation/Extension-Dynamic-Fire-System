//  Authors:  Robert M. Scheller, Brian R. Miranda 

using Landis.Utilities;
using Landis.Core;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System;

namespace Landis.Extension.DynamicFire
{
    /// <summary>
    /// A parser that reads the plug-in's parameters from text input.
    /// </summary>
    public class InputParameterParser
        : TextParser<IInputParameters>
    {

        //---------------------------------------------------------------------

        public override string LandisDataValue
        {
            get
            {
                return "Dynamic Fire System";
            }
        }
        static InputParameterParser()
        {
            Landis.Utilities.Percentage p = new Landis.Utilities.Percentage();
            RegisterForInputValues();
        }

        //---------------------------------------------------------------------

        protected override IInputParameters Parse()
        {
            ReadLandisDataVar();

            InputParameters parameters = new InputParameters();

            InputVar<int> timestep = new InputVar<int>("Timestep");
            ReadVar(timestep);
            parameters.Timestep = timestep.Value;

            InputVar<SizeType> st = new InputVar<SizeType>("EventSizeType");
            ReadVar(st);
            parameters.FireSizeType = st.Value;
            
            InputVar<bool> bbui = new InputVar<bool>("BuildUpIndex");
            ReadVar(bbui);
            parameters.BUI = bbui.Value;

            InputVar<int> wrand = new InputVar<int>("WeatherRandomizer");
            if (ReadOptionalVar(wrand))
            {
                if (wrand.Value.Actual < 0 || wrand.Value.Actual > 4)
                    throw new InputValueException(wrand.Value.String,
                                                  "The weather randomizer code {0} must be 0 - 4",
                                                  wrand.Value.Actual);
                else
                    PlugIn.WeatherRandomizer = wrand.Value;
            
            }

            //----------------------------------------------------------
            // First, read table of additional parameters for ecoregions
            PlugIn.ModelCore.UI.WriteLine("   Loading FireRegion data...");
            //IEditableFireRegionDataset dataset = new EditableFireRegionDataset();
            List<IFireRegion> dataset = new List<IFireRegion>(0);
            
            Dictionary <string, int> nameLineNumbers = new Dictionary<string, int>();
            Dictionary <ushort, int> mapCodeLineNumbers = new Dictionary<ushort, int>();

            InputVar<string> name = new InputVar<string>("Name");
            InputVar<ushort> mapCode = new InputVar<ushort>("Map Code");
            InputVar<double> meanSize = new InputVar<double>("Mean Size");
            InputVar<double> standardDeviation = new InputVar<double>("Standard Deviation");
            InputVar<int> maxSize = new InputVar<int>("Max Size");
            InputVar<int> spLo = new InputVar<int>("Spring Low FMC");
            InputVar<int> spHi = new InputVar<int>("Spring High FMC");
            InputVar<double> spHiPro = new InputVar<double>("Spring High FMC proportion");
            InputVar<int> suLo = new InputVar<int>("Summer Low FMC");
            InputVar<int> suHi = new InputVar<int>("Summer High FMC");
            InputVar<double> suHiPro = new InputVar<double>("Summer High FMC proportion");
            InputVar<int> faLo = new InputVar<int>("Fall Low FMC");
            InputVar<int> faHi = new InputVar<int>("Fall High FMC");
            InputVar<double> faHiPro = new InputVar<double>("Fall High FMC proportion");
            InputVar<int> ftc = new InputVar<int>("Open Type Fuel");
            InputVar<double> ein = new InputVar<double>("FireRegion Ignition Number");
          
            Dictionary <string, int> lineNumbers = new Dictionary<string, int>();
            const string DynamicFireRegionTable = "DynamicEcoregionTable";
            const string InitialFireEcoregionsMap = "InitialFireEcoregionsMap";

            int fireRegionIndex = 0;            
            while (! AtEndOfInput && CurrentName != InitialFireEcoregionsMap) {
                //IEditableFireRegionParameters ecoparameters = new EditableFireRegionParameters();
                IFireRegion ecoparameters = new FireRegion(fireRegionIndex);

                dataset.Add(ecoparameters);

                StringReader currentLine = new StringReader(CurrentLine);

                int lineNumber;

                ReadValue(mapCode, currentLine);
                if (mapCodeLineNumbers.TryGetValue(mapCode.Value.Actual, out lineNumber))
                    throw new InputValueException(mapCode.Value.String,
                                                  "The map code {0} was previously used on line {1}",
                                                  mapCode.Value.Actual, lineNumber);
                else
                    mapCodeLineNumbers[mapCode.Value.Actual] = LineNumber;
                ecoparameters.MapCode = mapCode.Value;

                ReadValue(name, currentLine);
                if (nameLineNumbers.TryGetValue(name.Value.Actual, out lineNumber))
                    throw new InputValueException(name.Value.String,
                                                  "The name \"{0}\" was previously used on line {1}",
                                                  name.Value.Actual, lineNumber);
                else
                    nameLineNumbers[name.Value.Actual] = LineNumber;
                ecoparameters.Name = name.Value;

                ReadValue(meanSize, currentLine);
                ecoparameters.MeanSize = meanSize.Value;

                ReadValue(standardDeviation, currentLine);
                ecoparameters.StandardDeviation = standardDeviation.Value;
                
                ReadValue(maxSize, currentLine);
                ecoparameters.MaxSize = maxSize.Value;

                ReadValue(spLo, currentLine);
                ecoparameters.SpringFMCLo = spLo.Value;
                
                ReadValue(spHi, currentLine);
                ecoparameters.SpringFMCHi = spHi.Value;

                ReadValue(spHiPro, currentLine);
                ecoparameters.SpringFMCHiProp = spHiPro.Value;

                ReadValue(suLo, currentLine);
                ecoparameters.SummerFMCLo = suLo.Value;
                
                ReadValue(suHi, currentLine);
                ecoparameters.SummerFMCHi = suHi.Value;

                ReadValue(suHiPro, currentLine);
                ecoparameters.SummerFMCHiProp = suHiPro.Value;

                ReadValue(faLo, currentLine);
                ecoparameters.FallFMCLo = faLo.Value;
                
                ReadValue(faHi, currentLine);
                ecoparameters.FallFMCHi = faHi.Value;

                ReadValue(faHiPro, currentLine);
                ecoparameters.FallFMCHiProp = faHiPro.Value;
                
                ReadValue(ftc, currentLine);
                ecoparameters.OpenFuelType = ftc.Value;
                
                ReadValue(ein, currentLine);
                ecoparameters.EcoIgnitionNum = ein.Value;
                
                fireRegionIndex++;
               
                CheckNoDataAfter("the " + ein.Name + " column",
                                 currentLine);
                                 
                GetNextLine();
            }
            
            //Moved below since FireRegions now include seasonRecords
            FireRegions.Dataset = dataset; 
            
            InputVar<string> ecoregionsMap = new InputVar<string>("InitialFireEcoregionsMap");
            ReadVar(ecoregionsMap);
            FireRegions.ReadMap(ecoregionsMap.Value);

            //----------------------------------------------------------
            // Read in the table of dynamic ecoregions:

            ReadName(DynamicFireRegionTable);

            InputVar<string> mapName = new InputVar<string>("Dynamic Map Name");
            InputVar<int> year = new InputVar<int>("Year to read in new FireRegion Map");

            const string GroundSlopeFile = "GroundSlopeFile";
            const string Season = "SeasonTable";
            double previousYear = 0;

            while (! AtEndOfInput && CurrentName != GroundSlopeFile && CurrentName != Season) {
                StringReader currentLine = new StringReader(CurrentLine);

                //IEditableDynamicFireRegion dynEco = new EditableDynamicFireRegion();
                IDynamicFireRegion dynEco = new DynamicFireRegion();
                
                parameters.DynamicFireRegions.Add(dynEco);

                ReadValue(year, currentLine);
                dynEco.Year = year.Value;
                
                if (year.Value.Actual <= previousYear)
                {
                    throw new InputValueException(year.Value.String,
                        "Year must > the year ({0}) of the preceeding ecoregion map",
                        previousYear);
                }

                previousYear = year.Value.Actual;
                

                ReadValue(mapName, currentLine);
                dynEco.MapName = mapName.Value;

                CheckNoDataAfter("the " + mapName.Name + " column",
                                 currentLine);
                GetNextLine();
            }
            

            //----------------------------------------------------------
            // Optional topographic maps
            InputVar<string> groundSlopeFile = new InputVar<string>("GroundSlopeFile");
            if (ReadOptionalVar(groundSlopeFile)) {
            
                PlugIn.ModelCore.UI.WriteLine("   Loading Slope data...");
                
                Topography.ReadGroundSlopeMap(groundSlopeFile.Value);
                //ValidatePath(groundSlopeFile.Value);
                //FireRegions.Dataset = FireRegionDataset.ReadFile(ecoregionsFile.Value);

                InputVar<string> uphillSlopeMap = new InputVar<string>("UphillSlopeAzimuthMap");
                ReadVar(uphillSlopeMap);

                PlugIn.ModelCore.UI.WriteLine("   Loading Azimuth data...");

                Topography.ReadUphillSlopeAzimuthMap(uphillSlopeMap.Value);
            }

            //-------------------------------------------------------------------
            //  Read table of Seasons.
            //  Arranged in any order.
            PlugIn.ModelCore.UI.WriteLine("   Loading Seasons data...");

            ReadName(Season);

            InputVar<SeasonName> seasonName = new InputVar<SeasonName>("Season Name");
            InputVar<double> fp = new InputVar<double>("Season fire probability");
            InputVar<int> cc = new InputVar<int>("Curing Percent");
            InputVar<LeafOnOff> loo = new InputVar<LeafOnOff>("Leaves on or off?");
            InputVar<double> dlp = new InputVar<double>("Day length proportion");

            const string DynamicWeatherTable = "DynamicWeatherTable";
            const string WeatherPath = "InitialWeatherDatabase";
            
            int sn = 1;
            
            while (!AtEndOfInput && CurrentName != WeatherPath)
            {
                StringReader currentLine = new StringReader(CurrentLine);

                ReadValue(seasonName, currentLine);
                sn = (int) seasonName.Value.Actual;
                
                //PlugIn.ModelCore.UI.WriteLine("      Season index = {0}.", sn);

                ISeasonParameters seasonParms = new SeasonParameters();
                parameters.SeasonParameters[sn] = seasonParms;
                
                seasonParms.NameOfSeason = seasonName.Value;
                
                ReadValue(loo, currentLine);
                seasonParms.LeafStatus = loo.Value;

                ReadValue(fp, currentLine);
                seasonParms.FireProbability = fp.Value;          
                
                ReadValue(cc, currentLine);
                seasonParms.PercentCuring = cc.Value;

                ReadValue(dlp, currentLine);
                seasonParms.DayLengthProp = dlp.Value;
                
                CheckNoDataAfter("the " + cc.Name + " column",
                                 currentLine);

                GetNextLine();
            }
            //------------------------------------------------------------------
            //  Read path to weather table

            InputVar<string> weatherPath = new InputVar<string>("InitialWeatherDatabase");
            ReadVar(weatherPath);
            parameters.InitialWeatherPath = weatherPath.Value;

            //----------------------------------------------------------
            // Read in the table of dynamic weather:

            ReadName(DynamicWeatherTable);
            
            InputVar<string> fileName = new InputVar<string>("Dynamic Database Name");
            InputVar<int> yearW = new InputVar<int>("Year to read in new Weather data");

            const string FuelTypes = "FuelTypeTable";
            previousYear = 0;

            while (!AtEndOfInput && CurrentName != FuelTypes)
            {
                StringReader currentLine = new StringReader(CurrentLine);

                IDynamicWeather dynWeather = new DynamicWeather();
                parameters.DynamicWeather.Add(dynWeather);

                ReadValue(yearW, currentLine);
                dynWeather.Year = yearW.Value;

                if (yearW.Value.Actual <= previousYear)
                {
                    throw new InputValueException(yearW.Value.String,
                        "Year must > the year ({0}) of the preceeding weather database",
                        previousYear);
                }

                previousYear = yearW.Value.Actual;


                ReadValue(fileName, currentLine);
                dynWeather.FileName = fileName.Value;

                CheckNoDataAfter("the " + fileName.Name + " column",
                                 currentLine);
                GetNextLine();
            }

            //-------------------------------------------------------------------
            //  Read table of Fuel Types.
            //  Arranged in any order.
             PlugIn.ModelCore.UI.WriteLine("   Loading Fuels data...");

            ReadName(FuelTypes);

            InputVar<int> fuelIndex = new InputVar<int>("Fuel Type Index");
            InputVar<BaseFuelType> bft = new InputVar<BaseFuelType>("Base Fuel Type");
            InputVar<SurfaceFuelType> sft = new InputVar<SurfaceFuelType>("Surface Fuel Type");
            InputVar<double> ip = new InputVar<double>("Fuel type initiation probability");
            InputVar<int> a = new InputVar<int>("Fuel type coefficient - a");
            InputVar<double> b = new InputVar<double>("Fuel type coefficient - b");
            InputVar<double> c = new InputVar<double>("Fuel type coefficient - c");
            InputVar<double> q = new InputVar<double>("Fuel type coefficient - q");
            InputVar<int> bui = new InputVar<int>("Fuel type coefficient - bui");
            InputVar<double> maxbe = new InputVar<double>("Fuel type coefficient - Maximum BE");
            InputVar<int> cbh = new InputVar<int>("Crown Base Height");

            const string FireDamage = "FireDamageTable";
            const string SeverityCalibrate = "SeverityCalibrationFactor";
            while (! AtEndOfInput && CurrentName != SeverityCalibrate) {
                
                StringReader currentLine = new StringReader(CurrentLine);

                IFuelType fuelParms = new FuelType();

                ReadValue(fuelIndex, currentLine);
                fuelParms.FuelIndex = fuelIndex.Value;
              
                ReadValue(bft, currentLine);
                fuelParms.BaseFuel = bft.Value;

                ReadValue(sft, currentLine);
                fuelParms.SurfaceFuel = sft.Value;

                ReadValue(ip, currentLine);
                fuelParms.InitiationProbability = ip.Value;

                ReadValue(a, currentLine);
                fuelParms.A = a.Value;

                ReadValue(b, currentLine);
                fuelParms.B = b.Value;

                ReadValue(c, currentLine);
                fuelParms.C = c.Value;

                ReadValue(q, currentLine);
                fuelParms.Q = q.Value;

                ReadValue(bui, currentLine);
                fuelParms.BUI = bui.Value;
                
                ReadValue(maxbe, currentLine);
                fuelParms.MaxBE = maxbe.Value;

                ReadValue(cbh, currentLine);
                fuelParms.CBH = cbh.Value;

                parameters.FuelTypeParameters[fuelIndex.Value] = fuelParms;
                
                CheckNoDataAfter("the " + cbh.Name + " column",
                                 currentLine);

                GetNextLine();
            }

            //Read in Severity Calibration Factor
            InputVar<double> sevCalib = new InputVar<double>("SeverityCalibrationFactor");
            ReadVar(sevCalib);
            parameters.SeverityCalibrate = sevCalib.Value;
            
            
            //-------------------------------------------------------------------
            //  Read table of Fire Damage classes.
            //  Damages are in increasing order.
             PlugIn.ModelCore.UI.WriteLine("   Loading Fire Damage data...");
            
            ReadName(FireDamage);

            InputVar<Percentage> maxAge = new InputVar<Percentage>("Max Survival Age");
            InputVar<int> severTolerDifference = new InputVar<int>("Severity Tolerance Diff");

            const string MapNames = "MapNames";
            int previousNumber = -5;
            double previousMaxAge = 0.0;

            while (! AtEndOfInput && CurrentName != MapNames
                                  && previousNumber != 4) {
                StringReader currentLine = new StringReader(CurrentLine);

                IFireDamage damage = new FireDamage();
                parameters.FireDamages.Add(damage);

                ReadValue(maxAge, currentLine);
                damage.MaxAge = maxAge.Value;
                if (maxAge.Value.Actual <= 0)
                {
                //  Maximum age for damage must be > 0%
                    throw new InputValueException(maxAge.Value.String,
                                      "Must be > 0% for the all damage classes");
                }
                if (maxAge.Value.Actual > 1)
                {
                //  Maximum age for damage must be <= 100%
                    throw new InputValueException(maxAge.Value.String,
                                      "Must be <= 100% for the all damage classes");
                }
                //  Maximum age for every damage must be > 
                //  maximum age of previous damage.
                if (maxAge.Value.Actual <= previousMaxAge)
                {
                    throw new InputValueException(maxAge.Value.String,
                        "MaxAge must > the maximum age ({0}) of the preceeding damage class",
                        previousMaxAge);
                }

                previousMaxAge = (double) maxAge.Value.Actual;

                ReadValue(severTolerDifference, currentLine);
                damage.SeverTolerDifference = severTolerDifference.Value;

                //Check that the current damage number is > than
                //the previous number (numbers are must be in increasing
                //order).
                if (severTolerDifference.Value.Actual <= previousNumber)
                    throw new InputValueException(severTolerDifference.Value.String,
                                                  "Expected the damage number {0} to be greater than previous {1}",
                                                  damage.SeverTolerDifference, previousNumber);
                if (severTolerDifference.Value.Actual > 4)
                    throw new InputValueException(severTolerDifference.Value.String,
                                                  "Expected the damage number {0} to be less than 5",
                                                  damage.SeverTolerDifference);
                                                  
                previousNumber = severTolerDifference.Value.Actual;

                CheckNoDataAfter("the " + severTolerDifference.Name + " column",
                                 currentLine);
                GetNextLine();
            }
            
            if (parameters.FireDamages.Count == 0)
                throw NewParseException("No damage classes defined.");

            InputVar<string> mapNames = new InputVar<string>(MapNames);
            ReadVar(mapNames);
            parameters.MapNamesTemplate = mapNames.Value;

            InputVar<string> logFile = new InputVar<string>("LogFile");
            ReadVar(logFile);
            parameters.LogFileName = logFile.Value;

            InputVar<string> summaryLogFile = new InputVar<string>("SummaryLogFile");
            ReadVar(summaryLogFile);
            parameters.SummaryLogFileName = summaryLogFile.Value;

            CheckNoDataAfter(string.Format("the {0} parameter", summaryLogFile.Name));

            return parameters; //.GetComplete();
        }
        //---------------------------------------------------------------------

        public static Distribution DistParse(string word)
        {
            if (word == "gamma")
                return Distribution.gamma;
            else if (word == "lognormal")
                return Distribution.lognormal;
            else if (word == "normal")
                return Distribution.normal;
            else if (word == "Weibull")
                return Distribution.Weibull;
            throw new System.FormatException("Valid Distributions: gamma, lognormal, normal, Weibull");
        }

        public static LeafOnOff LooParse(string word)
        {
            if (word == "LeafOff")
                return LeafOnOff.LeafOff;
            else if (word == "LeafOn")
                return LeafOnOff.LeafOn;
            throw new System.FormatException("Valid Leaf Types: LeafOff, LeafOn");
        }

        public static SeasonName SNParse(string word)
        {
            if (word == "Spring")
                return SeasonName.Spring;
            else if (word == "Summer")
                return SeasonName.Summer;
            if (word == "Fall")
                return SeasonName.Fall;
            throw new System.FormatException("Valid Season Names: Spring, Summer, Fall");
        }

        
        public static SizeType STParse(string word)
        {
            if (word == "size_based")
                return SizeType.size_based;
            else if (word == "duration_based")
                return SizeType.duration_based;
            throw new System.FormatException("Valid Event Size Types: size_based and duration_based");
        }
            
        public static BaseFuelType BFParse(string word)
        {
            if (word == "Conifer")
                return BaseFuelType.Conifer;
            else if (word == "ConiferPlantation")
                return BaseFuelType.ConiferPlantation;
            else if (word == "Deciduous")
                return BaseFuelType.Deciduous;
            else if (word == "Open")
                return BaseFuelType.Open;
            else if (word == "NoFuel")
                return BaseFuelType.NoFuel;
            else if (word == "Slash")
                return BaseFuelType.Slash;
                
            throw new System.FormatException("Valid Fuel Types: Conifer, ConiferPlantation, Deciduous, Open, NoFuel, Slash.");
        }

        public static SurfaceFuelType SFParse(string word)
        {
            if (word == "C1")
                return SurfaceFuelType.C1;
            else if (word == "C2")
                return SurfaceFuelType.C2;
            else if (word == "C3")
                return SurfaceFuelType.C3;
            else if (word == "C4")
                return SurfaceFuelType.C4;
            else if (word == "C5")
                return SurfaceFuelType.C5;
            else if (word == "C6")
                return SurfaceFuelType.C6;
            else if (word == "C7")
                return SurfaceFuelType.C7;
            else if (word == "D1")
                return SurfaceFuelType.D1;
            else if (word == "S1")
                return SurfaceFuelType.S1;
            else if (word == "S2")
                return SurfaceFuelType.S2;
            else if (word == "S3")
                return SurfaceFuelType.S3;
            else if (word == "M1")
                return SurfaceFuelType.M1;
            else if (word == "M2")
                return SurfaceFuelType.M2;
            else if (word == "M3")
                return SurfaceFuelType.M3;
            else if (word == "M4")
                return SurfaceFuelType.M4;
            else if (word == "O1a")
                return SurfaceFuelType.O1a;
            else if (word == "O1b")
                return SurfaceFuelType.O1b;
                
            throw new System.FormatException("Valid Fuel Types: C1-C7, D1, S1-S3, M1-M4, O1a, O1b.");
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Registers the appropriate method for reading input values.
        /// </summary>
        public static void RegisterForInputValues()
        {
            Landis.Utilities.Type.SetDescription<BaseFuelType>("Base Fuel Type Code");
            InputValues.Register<BaseFuelType>(BFParse);

            Landis.Utilities.Type.SetDescription<SurfaceFuelType>("Surface Fuel Type Code");
            InputValues.Register<SurfaceFuelType>(SFParse);

            Landis.Utilities.Type.SetDescription<SizeType>("Size Type Indicator");
            InputValues.Register<SizeType>(STParse);

            Landis.Utilities.Type.SetDescription<SeasonName>("Season Name");
            InputValues.Register<SeasonName>(SNParse);

            Landis.Utilities.Type.SetDescription<LeafOnOff>("Leaf On or Off");
            InputValues.Register<LeafOnOff>(LooParse);

            Landis.Utilities.Type.SetDescription<Distribution>("Random Number Distribution");
            InputValues.Register<Distribution>(DistParse);
        }
        //---------------------------------------------------------------------

        private void ValidatePath(InputValue<string> path)
        {
            if (path.Actual.Trim(null) == "")
                throw new InputValueException(path.String,
                                              "Invalid file path: {0}",
                                              path.String);
        }

        //---------------------------------------------------------------------

        private IFireRegion GetFireRegion(InputValue<string>      ecoregionName,
                                        Dictionary<string, int> lineNumbers)
        {
            //IFireRegion ecoregion = FireRegions.Dataset[ecoregionName.Actual];
            IFireRegion ecoregion = FireRegions.FindName(ecoregionName.Actual);
            if (ecoregion == null)
                throw new InputValueException(ecoregionName.String,
                                              "{0} is not an ecoregion name.",
                                              ecoregionName.String);
            int lineNumber;
            if (lineNumbers.TryGetValue(ecoregion.Name, out lineNumber))
                throw new InputValueException(ecoregionName.String,
                                              "The ecoregion {0} was previously used on line {1}",
                                              ecoregionName.String, lineNumber);
            else
                lineNumbers[ecoregion.Name] = LineNumber;

            return ecoregion;
        }
        
    }
}
