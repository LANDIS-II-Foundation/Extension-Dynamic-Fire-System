//  Copyright 2005 University of Wisconsin
//  Authors:  Robert M. Scheller, James B. Domingo
//  License:  Available at  
//  http://landis.forest.wisc.edu/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Edu.Wisc.Forest.Flel.Util;


namespace Landis.Fire
{
    /// <summary>
    /// Definition of a Fuel Type.
    /// </summary>
    public class SeasonTable
        : IEditable<ISeasonParameters[]>
    {

        private IEditableSeasonParameters[] parameters;

       //---------------------------------------------------------------------
        public int Count
        {
            get {
                return parameters.Length;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The event parameters for an ecoregion.
        /// </summary>
        public IEditableSeasonParameters this[int index]
        {
            get {
                return parameters[index];
            }

            set {
                parameters[index] = value;
            }
        }
        //---------------------------------------------------------------------

        public SeasonTable(int index)
        {
            parameters = new IEditableSeasonParameters[index];
        }

        //---------------------------------------------------------------------
        public bool IsComplete
        {
            get {
                foreach (IEditableSeasonParameters editableParms in parameters) {
                    if (editableParms != null && !editableParms.IsComplete)
                        return false;
                }
                return true;
            }
        }

        //---------------------------------------------------------------------

        public ISeasonParameters[] GetComplete()
        {
            if (IsComplete) {
                ISeasonParameters[] eventParms = new ISeasonParameters[parameters.Length];
                for (int i = 0; i < parameters.Length; i++) {
                    IEditableSeasonParameters editableParms = parameters[i];
                    if (editableParms != null)
                        eventParms[i] = editableParms.GetComplete();
                    else
                        eventParms[i] = new SeasonParameters();
                }
                return eventParms;
            }
            else
                return null;
        }

    }
}
