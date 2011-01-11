//  Copyright 2005 University of Wisconsin
//  Authors:  Robert M. Scheller, James B. Domingo
//  License:  Available at  
//  http://landis.forest.wisc.edu/developers/LANDIS-IISourceCodeLicenseAgreement.pdf
//  Edited by Brian R. Miranda (BRM)

using Edu.Wisc.Forest.Flel.Util;

// Validate Fire Parameter Table
namespace Landis.Fire
{
    /// <summary>
    /// Editable parameters (size and frequency) for Fire events in an
    /// ecoregion.
    /// </summary>
    public interface IEditableMoreEcoregionParameters
        : IEditable<IMoreEcoregionParameters>
    {

        //---------------------------------------------------------------------

        /// <summary>
        /// Mean event size (hectares).
        /// </summary>
        InputValue<double> MeanSize {get; set;}
        InputValue<double> StandardDeviation {get; set;}
        InputValue<int> SpringFMCLo{get;set;}
        InputValue<int> SpringFMCHi{get;set;}
        InputValue<double> SpringFMCHiProp{get;set;}
        InputValue<int> SummerFMCLo{get;set;}
        InputValue<int> SummerFMCHi{get;set;}
        InputValue<double> SummerFMCHiProp{get;set;}
        InputValue<int> FallFMCLo{get;set;}
        InputValue<int> FallFMCHi{get;set;}
        InputValue<double> FallFMCHiProp{get;set;}
        InputValue<FuelTypeCode> OpenFuelType{get;set;}
        InputValue<double> EcoIgnitionProb{get; set;}
    }
}


namespace Landis.Fire
{
    /// <summary>
    /// Editable parameters (size and frequency) for Fire events in an
    /// ecoregion.
    /// </summary>
    public class EditableMoreEcoregionParameters
        : IEditableMoreEcoregionParameters
    {
        private InputValue<double> meanSize;
        private InputValue<double> standardDeviation;
        private InputValue<int> springFMCLo;
        private InputValue<int> springFMCHi;
        private InputValue<double> springFMCHiProp;
        private InputValue<int> summerFMCLo;
        private InputValue<int> summerFMCHi;
        private InputValue<double> summerFMCHiProp;
        private InputValue<int> fallFMCLo;
        private InputValue<int> fallFMCHi;
        private InputValue<double> fallFMCHiProp;
        private InputValue<FuelTypeCode> openFuelType;
        private InputValue<double> ecoIgnitionProb;

        //---------------------------------------------------------------------

        /// <summary>
        /// Maximum event size (hectares).
        /// </summary>
        public InputValue<double> MeanSize
        {
            get {
                return meanSize;
            }

            set {
                if (value != null) {
                    if (value.Actual < 0)
                        throw new InputValueException(value.String,
                                                      "Value must be = or > 0.");
                }
                meanSize = value;
            }
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Mean event size (hectares).
        /// </summary>
        public InputValue<double> StandardDeviation
        {
            get {
                return standardDeviation;
            }

            set {
                if (value != null) {
                    if (value.Actual < 0)
                        throw new InputValueException(value.String,
                                                      "Value must be = or > 0.");
                }
                standardDeviation = value;
            }
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Low Value for Spring FMC.
        /// </summary>
        public InputValue<int> SpringFMCLo
        {
            get {
                return springFMCLo;
            }

            set {
                if (value != null) {
                    if (value.Actual < 0)
                        throw new InputValueException(value.String,
                                                      "Value must be = or > 0.");
                }
                springFMCLo = value;
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// High Value for Spring FMC.
        /// </summary>
        public InputValue<int> SpringFMCHi
        {
            get {
                return springFMCHi;
            }

            set {
                if (value != null) {
                    if (value.Actual < 0)
                        throw new InputValueException(value.String,
                                                      "Value must be = or > 0.");
                }
                springFMCHi = value;
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Proportion of High Values for Spring FMC.
        /// </summary>
        public InputValue<double> SpringFMCHiProp
        {
            get {
                return springFMCHiProp;
            }

            set {
                if (value != null) {
                    if (value.Actual < 0 || value.Actual > 1.0)
                        throw new InputValueException(value.String,
                                                      "Value must be = or > 0 and < or = 1.0.");
                }
                springFMCHiProp = value;
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Low Value for summer FMC.
        /// </summary>
        public InputValue<int> SummerFMCLo
        {
            get {
                return summerFMCLo;
            }

            set {
                if (value != null) {
                    if (value.Actual < 0)
                        throw new InputValueException(value.String,
                                                      "Value must be = or > 0.");
                }
                summerFMCLo = value;
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// High Value for summer FMC.
        /// </summary>
        public InputValue<int> SummerFMCHi
        {
            get {
                return summerFMCHi;
            }

            set {
                if (value != null) {
                    if (value.Actual < 0)
                        throw new InputValueException(value.String,
                                                      "Value must be = or > 0.");
                }
                summerFMCHi = value;
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Proportion of High Values for Summer FMC.
        /// </summary>
        public InputValue<double> SummerFMCHiProp
        {
            get {
                return summerFMCHiProp;
            }

            set {
                if (value != null) {
                    if (value.Actual < 0 || value.Actual > 1.0)
                        throw new InputValueException(value.String,
                                                      "Value must be = or > 0 and < or = 1.0.");
                }
                summerFMCHiProp = value;
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Low Value for summer FMC.
        /// </summary>
        public InputValue<int> FallFMCLo
        {
            get {
                return fallFMCLo;
            }

            set {
                if (value != null) {
                    if (value.Actual < 0)
                        throw new InputValueException(value.String,
                                                      "Value must be = or > 0.");
                }
                fallFMCLo = value;
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// High Value for Spring FMC.
        /// </summary>
        public InputValue<int> FallFMCHi
        {
            get {
                return fallFMCHi;
            }

            set {
                if (value != null) {
                    if (value.Actual < 0)
                        throw new InputValueException(value.String,
                                                      "Value must be = or > 0.");
                }
                fallFMCHi = value;
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Proportion of High Values for Fall FMC.
        /// </summary>
        public InputValue<double> FallFMCHiProp
        {
            get {
                return fallFMCHiProp;
            }

            set {
                if (value != null) {
                    if (value.Actual < 0 || value.Actual > 1.0)
                        throw new InputValueException(value.String,
                                                      "Value must be = or > 0 and < or = 1.0.");
                }
                fallFMCHiProp = value;
            }
        }
        
        //---------------------------------------------------------------------
        /// <summary>
        /// Convert any open fuel types to this type.
        /// </summary>
        public InputValue<FuelTypeCode> OpenFuelType
        {
            get {
                return openFuelType;
            }

            set {
                openFuelType = value;
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Ignition Probability for an Ecoregion.
        /// </summary>
        public InputValue<double> EcoIgnitionProb
        {
            get {
                return ecoIgnitionProb;
            }

            set {
                if (value != null) {
                    //-----Edited by BRM-----
                    if (value.Actual < 0)
                    //----------
                        throw new InputValueException(value.String,
                                                      "Value must be = or > 0");
                }
                ecoIgnitionProb = value;
            }
        }
        //---------------------------------------------------------------------

        public EditableMoreEcoregionParameters()
        {
        }

        //---------------------------------------------------------------------

        public bool IsComplete
        {
            get {
                foreach (object parameter in new object[]{ meanSize,
                                                           standardDeviation,
                                springFMCLo,
                                springFMCHi,
                                springFMCHiProp,
                                summerFMCLo,
                                summerFMCHi,
                                summerFMCHiProp,
                                fallFMCLo,
                                fallFMCHi,
                                fallFMCHiProp,
                                openFuelType,
                                ecoIgnitionProb
                                                           }) {
                    if (parameter == null)
                        return false;
                }
                return true;
            }
        }

        //---------------------------------------------------------------------

        public IMoreEcoregionParameters GetComplete()
        {
            if (IsComplete)
                return new MoreEcoregionParameters(meanSize.Actual,
                                          standardDeviation.Actual,
                                springFMCLo.Actual,
                                springFMCHi.Actual,
                                springFMCHiProp.Actual,
                                summerFMCLo.Actual,
                                summerFMCHi.Actual,
                                summerFMCHiProp.Actual,
                                fallFMCLo.Actual,
                                fallFMCHi.Actual,
                                fallFMCHiProp.Actual,
                                openFuelType.Actual,
                                ecoIgnitionProb.Actual
                                          );
            else
                return null;
        }
    }
}


