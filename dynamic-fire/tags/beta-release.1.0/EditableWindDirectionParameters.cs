//  Copyright 2005 University of Wisconsin
//  Authors:  Robert M. Scheller, James B. Domingo
//  License:  Available at  
//  http://landis.forest.wisc.edu/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Edu.Wisc.Forest.Flel.Util;

namespace Landis.Fire
{

    /// <summary>
    /// Editable definition of a Wind directions.
    /// </summary>
    public interface IEditableWindDirectionParameters
        : IEditable<IWindDirectionParameters>
    {
        InputValue<SeasonName> NameOfSeason {get; set;}
        InputValue<double>[] WindDirections {get;}
    }

    public class EditableWindDirectionParameters
        : IEditableWindDirectionParameters
    {
        private InputValue<SeasonName> nameOfSeason;
        private InputValue<double>[] windDirections; 
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
        /// Probability for Eight Cardinal Directions
        /// </summary>
         public InputValue<double>[] WindDirections
        {
            get {
                return windDirections;
            }
            
            set {
                windDirections = value;
            }
        }
        //---------------------------------------------------------------------

        public EditableWindDirectionParameters()
        {
            windDirections = new InputValue<double>[8];
        }

        //---------------------------------------------------------------------

        public bool IsComplete
        {
            get {
                foreach (object parameter in new object[]{ 
            nameOfSeason,
            }) {
                    if (parameter == null)
                        return false;
                }
                foreach (InputValue<double> windDir in windDirections)
                    if (windDir == null)
                        return false;
                return true;
            }
        }

        //---------------------------------------------------------------------

        public IWindDirectionParameters GetComplete()
        {
            if (IsComplete) {
                double[] windDirs = new double[8];
                for (int i = 0; i < 8; i++)
                    windDirs[i] = windDirections[i].Actual;
                return new WindDirectionParameters(
                                                    nameOfSeason.Actual,
                                                    windDirs
                                                    );
            }
            else
                return null;
        }
    }
}
