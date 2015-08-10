//  Copyright 2005 University of Wisconsin
//  Authors:  Robert M. Scheller, James B. Domingo
//  License:  Available at  
//  http://landis.forest.wisc.edu/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Edu.Wisc.Forest.Flel.Util;

namespace Landis.Fire
{
    /// <summary>
    /// Definition of a Wind Direction Table.
    /// </summary>
    public class WindDirectionTable
        : IEditable<IWindDirectionParameters[]>
    {

        private IEditableWindDirectionParameters[] parameters;

       //---------------------------------------------------------------------
        public int Count
        {
            get {
                return parameters.Length;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The wind direction parameters for a season.
        /// </summary>
        public IEditableWindDirectionParameters this[int index]
        {
            get {
                return parameters[index];
            }

            set {
                parameters[index] = value;
            }
        }
        //---------------------------------------------------------------------

        public WindDirectionTable(int index)
        {
            parameters = new IEditableWindDirectionParameters[index];
        }

        //---------------------------------------------------------------------
        public bool IsComplete
        {
            get {
                foreach (IEditableWindDirectionParameters editableParms in parameters) {
                    if (editableParms != null && !editableParms.IsComplete)
                        return false;
                }
                return true;
            }
        }

        //---------------------------------------------------------------------

        public IWindDirectionParameters[] GetComplete()
        {
            if (IsComplete) {
                IWindDirectionParameters[] eventParms = new IWindDirectionParameters[parameters.Length];
                for (int i = 0; i < parameters.Length; i++) {
                    IEditableWindDirectionParameters editableParms = parameters[i];
                    if (editableParms != null)
                        eventParms[i] = editableParms.GetComplete();
                    else
                        eventParms[i] = new WindDirectionParameters();
                }
                return eventParms;
            }
            else
                return null;
        }

    }
}
