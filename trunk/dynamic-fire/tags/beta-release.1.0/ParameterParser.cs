//  Copyright 2005 University of Wisconsin
//  Authors:  Robert M. Scheller, James B. Domingo
//  License:  Available at  
//  http://landis.forest.wisc.edu/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Edu.Wisc.Forest.Flel.Util;
using Landis.Ecoregions;
using Landis.Util;
using System.Collections.Generic;
using System.Text;

namespace Landis.Fire
{
    /// <summary>
    /// A parser that reads the plug-in's parameters from text input.
    /// </summary>
    public class ParameterParser
        : Landis.TextParser<IParameters>
    {
        public static IDataset EcoregionsDataset = null;

        //---------------------------------------------------------------------

        public override string LandisDataValue
        {
            get {
                return "Fire 2006";
            }
        }

        //---------------------------------------------------------------------

        public ParameterParser()
        {
            Edu.Wisc.Forest.Flel.Util.Percentage p = new Edu.Wisc.Forest.Flel.Util.Percentage();
            RegisterForInputValues();
        }

        //---------------------------------------------------------------------

        protected override IParameters Parse()
        {
            ReadLandisDataVar();

            EditableParameters parameters = new EditableParameters();

            InputVar<int> timestep = new InputVar<int>("Timestep");
            ReadVar(timestep);
            parameters.Timestep = timestep.Value;

            InputVar<SizeType> st = new InputVar<SizeType>("EventSizeType");
            ReadVar(st);
            parameters.FireSizeType = st.Value;
            
            InputVar<bool> bbui = new InputVar<bool>("BuildUpIndex");
            ReadVar(bbui);
            parameters.BUI = bbui.Value;


            //  Read optional variable FireEcoregions : text file with fire ecoregions
            //  Format much like main ecoregion file:
            //  
            //     >> Map
            //     >> Code  Name  Description
            //     >> ----  ----  -----------
            
            //  If FireEcoregions variable is present, then the variable FireEcoregionMap
            //  must be present.
            InputVar<string> ecoregionsFile = new InputVar<string>("FireEcoregions");
            if (ReadOptionalVar(ecoregionsFile)) {
                ValidatePath(ecoregionsFile.Value);
                Ecoregions.Dataset = EcoregionDataset.ReadFile(ecoregionsFile.Value);

                InputVar<string> ecoregionsMap = new InputVar<string>("FireEcoregionsMap");
                ReadVar(ecoregionsMap);
                Ecoregions.ReadMap(ecoregionsMap.Value);
            }
            else {
                //  Use the main ecoregions as the default fire ecoregions
                Ecoregions.Dataset = new EcoregionDataset();
            }

            //----------------------------------------------------------
            // Optional topographic maps
            InputVar<string> groundSlopeFile = new InputVar<string>("GroundSlopeFile");
            if (ReadOptionalVar(groundSlopeFile)) {
                
                Topography.ReadGroundSlopeMap(groundSlopeFile.Value);
                //ValidatePath(groundSlopeFile.Value);
                //Ecoregions.Dataset = EcoregionDataset.ReadFile(ecoregionsFile.Value);

                InputVar<string> uphillSlopeMap = new InputVar<string>("UphillSlopeAzimuthMap");
                ReadVar(uphillSlopeMap);
                Topography.ReadUphillSlopeAzimuthMap(uphillSlopeMap.Value);
            }

            //----------------------------------------------------------
            // First, read table of additional parameters for ecoregions
            UI.WriteLine("   Loading additional ecoregion data...");
            
            InputVar<string> ecoregionName = new InputVar<string>("Ecoregion");
            InputVar<double> meanSize = new InputVar<double>("Mean Size");
            InputVar<double> standardDeviation = new InputVar<double>("Standard Deviation");
            InputVar<int> spLo = new InputVar<int>("Spring Low FMC");
            InputVar<int> spHi = new InputVar<int>("Spring High FMC");
            InputVar<double> spHiPro = new InputVar<double>("Spring High FMC proportion");
            InputVar<int> suLo = new InputVar<int>("Summer Low FMC");
            InputVar<int> suHi = new InputVar<int>("Summer High FMC");
            InputVar<double> suHiPro = new InputVar<double>("Summer High FMC proportion");
            InputVar<int> faLo = new InputVar<int>("Fall Low FMC");
            InputVar<int> faHi = new InputVar<int>("Fall High FMC");
            InputVar<double> faHiPro = new InputVar<double>("Fall High FMC proportion");
            InputVar<FuelTypeCode> ftc = new InputVar<FuelTypeCode>("Open Type Fuel");
            InputVar<double> eip = new InputVar<double>("Ecoregion Ignition Probability");

            Dictionary <string, int> lineNumbers = new Dictionary<string, int>();
            const string Season = "SeasonTable";

            while (! AtEndOfInput && CurrentName != Season) {
                StringReader currentLine = new StringReader(CurrentLine);

                ReadValue(ecoregionName, currentLine);

                IEcoregion ecoregion = GetEcoregion(ecoregionName.Value,
                                                    lineNumbers);

                IEditableMoreEcoregionParameters eventParms = new EditableMoreEcoregionParameters();

                ReadValue(meanSize, currentLine);
                eventParms.MeanSize = meanSize.Value;

                ReadValue(standardDeviation, currentLine);
                eventParms.StandardDeviation = standardDeviation.Value;
                
                ReadValue(spLo, currentLine);
                eventParms.SpringFMCLo = spLo.Value;
                
                ReadValue(spHi, currentLine);
                eventParms.SpringFMCHi = spHi.Value;

                ReadValue(spHiPro, currentLine);
                eventParms.SpringFMCHiProp = spHiPro.Value;

                ReadValue(suLo, currentLine);
                eventParms.SummerFMCLo = suLo.Value;
                
                ReadValue(suHi, currentLine);
                eventParms.SummerFMCHi = suHi.Value;

                ReadValue(suHiPro, currentLine);
                eventParms.SummerFMCHiProp = suHiPro.Value;

                ReadValue(faLo, currentLine);
                eventParms.FallFMCLo = faLo.Value;
                
                ReadValue(faHi, currentLine);
                eventParms.FallFMCHi = faHi.Value;

                ReadValue(faHiPro, currentLine);
                eventParms.FallFMCHiProp = faHiPro.Value;
                
                ReadValue(ftc, currentLine);
                eventParms.OpenFuelType = ftc.Value;
                
                ReadValue(eip, currentLine);
                eventParms.EcoIgnitionProb = eip.Value;

                ecoregion.MoreEcoregionParameters = eventParms.GetComplete();

                CheckNoDataAfter("the " + eip.Name + " column",
                                 currentLine);
                GetNextLine();
            }
            
            //-------------------------------------------------------------------
            //  Read table of Fuel Types.
            //  Arranged in any order.
            UI.WriteLine("   Loading seasons data...");

            ReadName(Season);

            InputVar<SeasonName> seasonName = new InputVar<SeasonName>("Season Name");
            InputVar<double> fp = new InputVar<double>("Season fire probability");
            //Remove this section
            InputVar<Distribution> wsvd = new InputVar<Distribution>("Wind Speed Velocity Distribution");
            InputVar<double> wsv1 = new InputVar<double>("Wind Speed Velocity - Parameter 1");
            InputVar<double> wsv2 = new InputVar<double>("Wind Speed Velocity - Parameter 2");
            InputVar<Distribution> ffmcd = new InputVar<Distribution>("FFMC Distribution");
            InputVar<double> ffmc1 = new InputVar<double>("FFMC - Parameter 1");
            InputVar<double> ffmc2 = new InputVar<double>("FFMC - Parameter 2");
            InputVar<Distribution> buid = new InputVar<Distribution>("Build Up Index Distribution");
            InputVar<double> bui1 = new InputVar<double>("BUI - Parameter 1");
            InputVar<double> bui2 = new InputVar<double>("BUI - Parameter 2");
            //
           
            InputVar<int> cc = new InputVar<int>("Curing Percent");
            InputVar<LeafOnOff> loo = new InputVar<LeafOnOff>("Leaves on or off?");

            //Move to below
            const string WindDirs = "WindDirectionTable";
            //Replace with
            // const string WeatherPath = "WeatherTablePath";

            //Move to below
            while (! AtEndOfInput && CurrentName != WindDirs) {
            //Replace with
            // while (! AtEndOfInput && CurrentName != WeatherPath) {

                StringReader currentLine = new StringReader(CurrentLine);

                ReadValue(seasonName, currentLine);
                int sn = (int) seasonName.Value.Actual;
                
                UI.WriteLine("      Season index = {0}.", sn);

                IEditableSeasonParameters seasonParms = new EditableSeasonParameters();
                parameters.SeasonParameters[sn] = seasonParms;
                
                seasonParms.NameOfSeason = seasonName.Value;
                
                ReadValue(loo, currentLine);
                seasonParms.LeafStatus = loo.Value;

                ReadValue(fp, currentLine);
                seasonParms.FireProbability = fp.Value;
                
                //Remove this section
                ReadValue(wsvd, currentLine);
                seasonParms.WSVDist = wsvd.Value;

                ReadValue(wsv1, currentLine);
                seasonParms.WSVP1 = wsv1.Value;

                ReadValue(wsv2, currentLine);
                seasonParms.WSVP2 = wsv2.Value;

                ReadValue(ffmcd, currentLine);
                seasonParms.FFMCDist = ffmcd.Value;

                ReadValue(ffmc1, currentLine);
                seasonParms.FFMCP1 = ffmc1.Value;

                ReadValue(ffmc2, currentLine);
                seasonParms.FFMCP2 = ffmc2.Value;

                ReadValue(buid, currentLine);
                seasonParms.BUIDist = buid.Value;

                ReadValue(bui1, currentLine);
                seasonParms.BUIP1 = bui1.Value;

                ReadValue(bui2, currentLine);
                seasonParms.BUIP2 = bui2.Value;
                //
                ReadValue(cc, currentLine);
                seasonParms.PercentCuring = cc.Value;
                
                CheckNoDataAfter("the " + cc.Name + " column",
                                 currentLine);

                GetNextLine();
            }
            //------------------------------------------------------------------
            //  Read path to weather table
            //  ReadName (WeatherPath);
            //  UI.WriteLine("   Loading weather table...");
            //  InputVar<string> weatherPath = new InputVar<string>("Weather Table Path");
            //  while (! AtEndOfInput && CurrentName != WindDirs) {
            //  StringReader currentLine = new StringReader(CurrentLine);
            //  ReadValue(weatherPath, currentLine);
            //  string path = weatherPath.Value.Actual;
            //  UI.WriteLine("    Weather Table Path = {0}", path);
            //  string cString = "Provider=Microsoft.Jet.OLEDB.4.0; Data Source="+path;
            //  OleDbConnection weatherConn = new OleDbConnection(cString);
            //  weatherConn.Open();
            //  OleDbDataAdapter adapter = new OleDbDataAdapter();
            //  OleDbCommand selectCmd = new OleDbCommand("SELECT ID FROM ?", myConnection);
            //  selectCmd.Parameters.Add("Season", OleDbType.VarChar, 15);
            //  adapter.SelectCmd = selectCmd;
            //  DataSet weatherDataSet = new DataSet();
            //  int recordCount = 0;
            //  for each (int i in 1 to sn)
            //    parameters.SeasonParameters[sn] = seasonParms;
            //    string seasonName = seasonParms.NameOfSeason;
            //    adapter.SelectCommand.Parameters["Season"].Value = seasonName;
            //    adapter.Fill (weatherDataSet);
            //    int sprRecordCount = ds.Tables["Table"].Rows.Count;
            //    
            //

            //-------------------------------------------------------------------
            //  Read table of wind directions by Season.
            //  Arranged in any order.
            ReadName(WindDirs);
            UI.WriteLine("   Loading wind direction season data...");

            InputVar<SeasonName> windSeasonName = new InputVar<SeasonName>("Wind Season Name");
            InputVar<double> n = new InputVar<double>("Wind Direction probability");

            const string FuelTypes = "FuelTypeTable";

            while (! AtEndOfInput && CurrentName != FuelTypes) {
                StringReader currentLine = new StringReader(CurrentLine);

                ReadValue(windSeasonName, currentLine);
                int wsn = (int) windSeasonName.Value.Actual;
                
                UI.WriteLine("      Wind Season index = {0}.", wsn);

                IEditableWindDirectionParameters windSeasonParms = new EditableWindDirectionParameters();
                parameters.WindDirectionParameters[wsn] = windSeasonParms;
                
                windSeasonParms.NameOfSeason = windSeasonName.Value;
                
                float sumWindDirs = 0;

                for (int i = 0; i < 8; i++)
                {
                    ReadValue(n, currentLine);
                    if(n.Value < 0.0 || n.Value > 1.0)
                        throw NewParseException("Season: {0}.  Wind Direction Probabilites must be between 0 and 1.0", windSeasonName.Value);
                    sumWindDirs += (float) n.Value;
                    windSeasonParms.WindDirections[i] = n.Value;
                    UI.WriteLine("prob = {0}.  sum = {1}.", n.Value, sumWindDirs);
                }
                
                if(System.Math.Round(sumWindDirs,3) != 1.000)
                    throw NewParseException("Season: {0}.  The sum of wind direction probabilites must = 1.0.  Sum = {1}.", windSeasonName.Value, sumWindDirs);
                    

                CheckNoDataAfter("the " + n.Name + " column",
                                 currentLine);

                GetNextLine();
            }

            //-------------------------------------------------------------------
            //  Read table of Fuel Types.
            //  Arranged in any order.
            UI.WriteLine("   Loading fuels data...");

            ReadName(FuelTypes);

            InputVar<FuelTypeCode> fuelType = new InputVar<FuelTypeCode>("Fuel Type Code");
            InputVar<double> ip = new InputVar<double>("Fuel type initiation probability");
            InputVar<int> a = new InputVar<int>("Fuel type coefficient - a");
            InputVar<double> b = new InputVar<double>("Fuel type coefficient - b");
            InputVar<double> c = new InputVar<double>("Fuel type coefficient - c");
            InputVar<double> q = new InputVar<double>("Fuel type coefficient - q");
            InputVar<int> bui = new InputVar<int>("Fuel type coefficient - bui");
            InputVar<double> maxbe = new InputVar<double>("Fuel type coefficient - Maximum BE");
            InputVar<int> cbh = new InputVar<int>("Crown Base Height");

            const string FireDamage = "FireDamageTable";

            while (! AtEndOfInput && CurrentName != FireDamage) {
                StringReader currentLine = new StringReader(CurrentLine);

                ReadValue(fuelType, currentLine);
                int ft = (int) fuelType.Value.Actual;
                
                UI.WriteLine("      Fuel type index = {0}.", ft);

                IEditableFuelTypes fuelParms = new EditableFuelTypes();
                parameters.FuelTypeParameters[ft] = fuelParms;

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

                CheckNoDataAfter("the " + cbh.Name + " column",
                                 currentLine);

                GetNextLine();
            }


            //-------------------------------------------------------------------
            //  Read table of Fire Damage classes.
            //  Damages are in increasing order.
            ReadName(FireDamage);

            InputVar<Percentage> maxAge = new InputVar<Percentage>("Max Survival Age");
            InputVar<int> severTolerDifference = new InputVar<int>("Severity Tolerance Diff");

            const string MapNames = "MapNames";
            int previousNumber = -4;
            double previousMaxAge = 0.0;

            while (! AtEndOfInput && CurrentName != MapNames
                                  && previousNumber != 4) {
                StringReader currentLine = new StringReader(CurrentLine);

                IEditableDamageTable damage = new EditableDamageTable();
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

            return parameters.GetComplete();
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
            

        public static FuelTypeCode FTParse(string word)
        {
            if (word == "C1")
                return FuelTypeCode.C1;
            else if (word == "C2")
                return FuelTypeCode.C2;
            else if (word == "C3")
                return FuelTypeCode.C3;
            else if (word == "C4")
                return FuelTypeCode.C4;
            else if (word == "C5")
                return FuelTypeCode.C5;
            else if (word == "C6")
                return FuelTypeCode.C6;
            else if (word == "C7")
                return FuelTypeCode.C7;
            else if (word == "D1")
                return FuelTypeCode.D1;
            else if (word == "S1")
                return FuelTypeCode.S1;
            else if (word == "S2")
                return FuelTypeCode.S2;
            else if (word == "S3")
                return FuelTypeCode.S3;
            else if (word == "M1")
                return FuelTypeCode.M1;
            else if (word == "M2")
                return FuelTypeCode.M2;
            else if (word == "M3")
                return FuelTypeCode.M3;
            else if (word == "M4")
                return FuelTypeCode.M4;
            else if (word == "O1a")
                return FuelTypeCode.O1a;
            else if (word == "O1b")
                return FuelTypeCode.O1b;
            else if (word == "NoFuel")
                return FuelTypeCode.NoFuel;
                
            throw new System.FormatException("Valid Fuel Types: C1-C7, D1, S1-S3, M1-M4, O1a, O1b, NoFuel.");
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Registers the appropriate method for reading input values.
        /// </summary>
        public static void RegisterForInputValues()
        {
            Type.SetDescription<FuelTypeCode>("Fuel Type Code");
            InputValues.Register<FuelTypeCode>(FTParse);

            Type.SetDescription<SizeType>("Size Type Indicator");
            InputValues.Register<SizeType>(STParse);

            Type.SetDescription<SeasonName>("Season Name");
            InputValues.Register<SeasonName>(SNParse);

            Type.SetDescription<LeafOnOff>("Leaf On or Off");
            InputValues.Register<LeafOnOff>(LooParse);

            Type.SetDescription<Distribution>("Random Number Distribution");
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

        private IEcoregion GetEcoregion(InputValue<string>      ecoregionName,
                                        Dictionary<string, int> lineNumbers)
        {
            IEcoregion ecoregion = Ecoregions.Dataset[ecoregionName.Actual];
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
