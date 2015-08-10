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
    public class FuelTypeTable
        : IEditable<IFuelTypeParameters[]>
    {

        private IEditableFuelTypes[] parameters;

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
        public IEditableFuelTypes this[int index]
        {
            get {
                return parameters[index];
            }

            set {
                parameters[index] = value;
            }
        }
        //---------------------------------------------------------------------

        public FuelTypeTable(int index)
        {
            parameters = new IEditableFuelTypes[index];
        }

        //---------------------------------------------------------------------
        public bool IsComplete
        {
            get {
                foreach (IEditableFuelTypes editableParms in parameters) {
                    if (editableParms != null && !editableParms.IsComplete)
                        return false;
                }
                return true;
            }
        }

        //---------------------------------------------------------------------

        public IFuelTypeParameters[] GetComplete()
        {
            if (IsComplete) {
                IFuelTypeParameters[] eventParms = new IFuelTypeParameters[parameters.Length];
                for (int i = 0; i < parameters.Length; i++) {
                    IEditableFuelTypes editableParms = parameters[i];
                    if (editableParms != null)
                        eventParms[i] = editableParms.GetComplete();
                    else
                        eventParms[i] = new FuelTypeParameters();
                }
                return eventParms;
            }
            else
                return null;
        }

    }
}
