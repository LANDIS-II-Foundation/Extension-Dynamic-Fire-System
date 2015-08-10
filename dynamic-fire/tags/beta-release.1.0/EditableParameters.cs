//  Copyright 2005 University of Wisconsin
//  Authors:  Robert M. Scheller, James B. Domingo
//  License:  Available at  
//  http://landis.forest.wisc.edu/developers/LANDIS-IISourceCodeLicenseAgreement.pdf


using Edu.Wisc.Forest.Flel.Util;
using System.Collections.Generic;

namespace Landis.Fire
{
    /// <summary>
    /// An editable set of parameters for the plug-in.
    /// </summary>
    public class EditableParameters
        : IEditable<IParameters>
    {
        private InputValue<int> timestep;
        private InputValue<SizeType> fireSizeType;
        private InputValue<bool> buildUpIndex;

        // The xTable classes have a fixed length equal to the number of ecoregions.
        // If an ecoregion is not included in the user input, the relevant values 
        // are given default values, typically zero (0).
        //private FireParameterTable eventParameters;
        private SeasonTable seasons;
        private WindDirectionTable windDirections;
        private FuelTypeTable fuelTypeParameters;

        // A ListOfEditable table can have variable length and there are no defaults:
        private ListOfEditable<IEditableDamageTable, IDamageTable> damages;
        private InputValue<string> mapNamesTemplate;
        private InputValue<string> logFileName;
        private InputValue<string> summaryLogFileName;

        //---------------------------------------------------------------------

        public InputValue<int> Timestep
        {
            get {
                return timestep;
            }

            set {
                if (value != null)
                    if (value.Actual < 0)
                        throw new InputValueException(value.String,
                                                      "Value must be = or > 0.");
                timestep = value;
            }
        }
        
        public InputValue<SizeType> FireSizeType
        {
            get {
                return fireSizeType;
            }
            set {
                fireSizeType = value;
            }
        }

        public InputValue<bool> BUI
        {
            get {
                return buildUpIndex;
            }
            set {
                buildUpIndex = value;
            }
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Values for each Season
        /// </summary>
        public SeasonTable SeasonParameters
        {
            get {
                return seasons;
            }
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Values for each Wind Direction Season
        /// </summary>
        public WindDirectionTable WindDirectionParameters
        {
            get {
                return windDirections;
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Definitions of Fuel types.
        /// </summary>
        public FuelTypeTable FuelTypeParameters
        {
            get {
                return fuelTypeParameters;
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Definitions of Fire damage classes.
        /// </summary>
        public IList<IEditableDamageTable> FireDamages
        {
            get {
                return damages;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Template for pathnames for output maps.
        /// </summary>
        public InputValue<string> MapNamesTemplate
        {
            get {
                return mapNamesTemplate;
            }

            set {
                if (value != null) {
                    MapNames.CheckTemplateVars(value.Actual);
                }
                mapNamesTemplate = value;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Name for the log file.
        /// </summary>
        public InputValue<string> LogFileName
        {
            get {
                return logFileName;
            }

            set {
                if (value != null) {
                    // FIXME: check for null or empty path (value.Actual);
                }
                logFileName = value;
            }
        }

        /// <summary>
        /// Name for the summary log file.
        /// </summary>
        public InputValue<string> SummaryLogFileName
        {
            get {
                return summaryLogFileName;
            }

            set {
                if (value != null) {
                    // FIXME: check for null or empty path (value.Actual);
                }
                summaryLogFileName = value;
            }
        }
        //---------------------------------------------------------------------

        public EditableParameters()
        {
            seasons = new SeasonTable(3);
            windDirections = new WindDirectionTable(3);
            fuelTypeParameters = new FuelTypeTable(50);
            damages = new ListOfEditable<IEditableDamageTable, IDamageTable>();
        }

        //---------------------------------------------------------------------

        public bool IsComplete
        {
            get {
                foreach (object parameter in new object[]{ timestep,
                                                            fireSizeType,
                                                            buildUpIndex,
                                                           mapNamesTemplate,
                                                           logFileName,
                                                           summaryLogFileName}) {
                    if (parameter == null)
                        return false;
                }
                return seasons.IsComplete && 
                        windDirections.IsComplete &&
                        fuelTypeParameters.IsComplete &&
                       damages.IsEachItemComplete &&
                       damages.Count >= 1;
            }
        }

        //---------------------------------------------------------------------

        public IParameters GetComplete()
        {
            if (IsComplete)
                return new Parameters(timestep.Actual,
                                    fireSizeType.Actual,
                                    buildUpIndex.Actual,
                                    seasons.GetComplete(),
                                    windDirections.GetComplete(),
                                      fuelTypeParameters.GetComplete(),
                                      damages.GetComplete(),
                                      mapNamesTemplate.Actual,
                                      logFileName.Actual,
                                      summaryLogFileName.Actual);
            else
                return null;
        }
    }
}
