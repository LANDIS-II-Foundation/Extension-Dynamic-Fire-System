//  Copyright 2005 University of Wisconsin
//  Authors:  Robert M. Scheller, James B. Domingo
//  License:  Available at  
//  http://landis.forest.wisc.edu/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Edu.Wisc.Forest.Flel.Util;

namespace Landis.Fire
{
    /// <summary>
    /// Editable parameters (size and frequency) for Fire events for a
    /// collection of ecoregions.
    /// </summary>
    public class MoreEcoregionParameterTable
        : IEditable<IMoreEcoregionParameters[]>
    {
        private IEditableMoreEcoregionParameters[] parameters;

        //---------------------------------------------------------------------

        /// <summary>
        /// The number of ecoregions in the dataset.
        /// </summary>
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
        public IEditableMoreEcoregionParameters this[int ecoregionIndex]
        {
            get {
                return parameters[ecoregionIndex];
            }

            set {
                parameters[ecoregionIndex] = value;
            }
        }

        //---------------------------------------------------------------------

        public MoreEcoregionParameterTable(int ecoregionCount)
        {
            parameters = new IEditableMoreEcoregionParameters[ecoregionCount];
        }

        //---------------------------------------------------------------------

        public bool IsComplete
        {
            get {
                foreach (IEditableMoreEcoregionParameters editableParms in parameters) {
                    if (editableParms != null && !editableParms.IsComplete)
                        return false;
                }
                return true;
            }
        }

        //---------------------------------------------------------------------

        public IMoreEcoregionParameters[] GetComplete()
        {
            if (IsComplete) {
                IMoreEcoregionParameters[] eventParms = new IMoreEcoregionParameters[parameters.Length];
                for (int i = 0; i < parameters.Length; i++) {
                    IEditableMoreEcoregionParameters editableParms = parameters[i];
                    if (editableParms != null)
                        eventParms[i] = editableParms.GetComplete();
                    else
                        eventParms[i] = new MoreEcoregionParameters();
                }
                return eventParms;
            }
            else
                return null;
        }
    }
}
