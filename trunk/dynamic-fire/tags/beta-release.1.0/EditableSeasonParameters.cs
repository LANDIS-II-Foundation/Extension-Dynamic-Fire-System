//  Copyright 2005 University of Wisconsin
//  Authors:  Robert M. Scheller, James B. Domingo
//  License:  Available at  
//  http://landis.forest.wisc.edu/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Edu.Wisc.Forest.Flel.Util;

namespace Landis.Fire
{

    /// <summary>
    /// Editable definition of a Fire severity.
    /// </summary>
    public interface IEditableSeasonParameters
        : IEditable<ISeasonParameters>
    {
        InputValue<SeasonName> NameOfSeason {get; set;}
        InputValue<LeafOnOff> LeafStatus {get; set;}
        InputValue<double> FireProbability {get; set;}
        InputValue<Distribution> WSVDist{get; set;}
        InputValue<double> WSVP1 {get; set;}
        InputValue<double> WSVP2 {get; set;}
        InputValue<Distribution> FFMCDist{get; set;}
        InputValue<double> FFMCP1 {get; set;}
        InputValue<double> FFMCP2 {get; set;}
        InputValue<Distribution> BUIDist{get; set;}
        InputValue<double> BUIP1 {get; set;}
        InputValue<double> BUIP2 {get; set;}
        InputValue<int> PercentCuring {get; set;}
    }

    /// <summary>
    /// Editable definition of Fire Types.
    /// </summary>
    public class EditableSeasonParameters
        : IEditableSeasonParameters
    {
        private InputValue<SeasonName> nameOfSeason;
        private InputValue<LeafOnOff> leafStatus;
        private InputValue<double> fireProbability;
        private InputValue<Distribution> WSVdist;
        private InputValue<double> WSVp1;
        private InputValue<double> WSVp2;
        private InputValue<Distribution> FFMCdist;
        private InputValue<double> FFMCp1;
        private InputValue<double> FFMCp2;
        private InputValue<Distribution> BUIdist;
        private InputValue<double> BUIp1;
        private InputValue<double> BUIp2;
        private InputValue<int> percentCuring;

        //---------------------------------------------------------------------
        /// <summary>
        /// Indicate the name of the season.
        /// </summary>
        public InputValue<SeasonName> NameOfSeason
        {
            get {
                return nameOfSeason;
            }

            set {
                nameOfSeason = value;
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Indicate whether leaves are on or off trees for a given season.
        /// </summary>
        public InputValue<LeafOnOff> LeafStatus
        {
            get {
                return leafStatus;
            }

            set {
                leafStatus = value;
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Initiation Probability for Fuel type
        /// </summary>
        public InputValue<double> FireProbability
        {
            get {
                return fireProbability;
            }

            set {
                if (value != null) {
                    if (value.Actual < 0.0 || value.Actual > 1.0)
                        throw new InputValueException(value.String,
                            "Value must be between 0 and 1.0");
                }
                fireProbability = value;
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Indicate the Wind Speed Velocity (WSV) distribution.
        /// </summary>
        public InputValue<Distribution> WSVDist
        {
            get {
                return WSVdist;
            }

            set {
                WSVdist = value;
            }
        }
        //---------------------------------------------------------------------
        public InputValue<double> WSVP1
        {
            get {
                return WSVp1;
            }

            set {
                if (value != null) {
                    if (value.Actual < 0)
                        throw new InputValueException(value.String,
                            "Value must be great then 0");
                }
                WSVp1 = value;
            }
        }
        //---------------------------------------------------------------------
        public InputValue<double> WSVP2
        {
            get {
                return WSVp2;
            }

            set {
                if (value != null) {
                    if (value.Actual < 0)
                        throw new InputValueException(value.String,
                            "Value must be greater than 0");
                }
                WSVp2 = value;
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Indicate the Fine Fuel Moisture Code (FFMC) distribution.
        /// </summary>
        public InputValue<Distribution> FFMCDist
        {
            get {
                return FFMCdist;
            }

            set {
                FFMCdist = value;
            }
        }
        //---------------------------------------------------------------------
        public InputValue<double> FFMCP1
        {
            get {
                return FFMCp1;
            }

            set {
                if (value != null) {
                    if (value.Actual < 0 )
                        throw new InputValueException(value.String,
                            "Value must be greater than 0");
                }
                FFMCp1 = value;
            }
        }
        //---------------------------------------------------------------------
        public InputValue<double> FFMCP2
        {
            get {
                return FFMCp2;
            }

            set {
                if (value != null) {
                    if (value.Actual < 0 )
                        throw new InputValueException(value.String,
                            "Value must be greater than 0");
                }
                FFMCp2 = value;
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Indicate the Build Up Index (BUI) distribution.
        /// </summary>
        public InputValue<Distribution> BUIDist
        {
            get {
                return BUIdist;
            }

            set {
                BUIdist = value;
            }
        }
        //---------------------------------------------------------------------
        public InputValue<double> BUIP1
        {
            get {
                return BUIp1;
            }

            set {
                if (value != null) {
                    if (value.Actual < 0 )
                        throw new InputValueException(value.String,
                            "Value must be greater than 0");
                }
                BUIp1 = value;
            }
        }
        //---------------------------------------------------------------------
        public InputValue<double> BUIP2
        {
            get {
                return BUIp2;
            }

            set {
                if (value != null) {
                    if (value.Actual < 0 )
                        throw new InputValueException(value.String,
                            "Value must be greater than 0");
                }
                BUIp2 = value;
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Curing Coefficient for Fuel type
        /// </summary>
        public InputValue<int> PercentCuring
        {
            get {
                return percentCuring;
            }

            set {
                if (value != null) {
                    if (value.Actual < 0 || value.Actual > 100)
                        throw new InputValueException(value.String,
                            "Value must be between 0 and 100");
                }
                percentCuring = value;
            }
        }
        //---------------------------------------------------------------------

        public EditableSeasonParameters()
        {
        }

        //---------------------------------------------------------------------

        public bool IsComplete
        {
            get {
                foreach (object parameter in new object[]{ 
            nameOfSeason,
            leafStatus,
            fireProbability,
            WSVdist,
            WSVp1,
            WSVp2,
            FFMCdist,
            FFMCp1,
            FFMCp2,
            BUIdist,
            BUIp1,
            BUIp2,
            percentCuring
            }) {
                    if (parameter == null)
                        return false;
                }
                return true;
            }
        }

        //---------------------------------------------------------------------

        public ISeasonParameters GetComplete()
        {
            if (IsComplete)
                return new SeasonParameters(
            nameOfSeason.Actual,
            leafStatus.Actual,
            fireProbability.Actual,
            WSVdist.Actual,
            WSVp1.Actual,
            WSVp2.Actual,
            FFMCdist.Actual,
            FFMCp1.Actual,
            FFMCp2.Actual,
            BUIdist.Actual,
            BUIp1.Actual,
            BUIp2.Actual,
            percentCuring.Actual
            );
            else
                return null;
        }
    }
}
