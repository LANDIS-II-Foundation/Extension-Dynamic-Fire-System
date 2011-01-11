//  Copyright 2006 University of Wisconsin
//  Author:  James B. Domingo
//  License:  Available at  
//  http://landis.forest.wisc.edu/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

namespace Landis.Fire
{
    public interface IEcoregion
        : IEcoregionParameters
    {
        /// <summary>
        /// Index of the ecoregion in the dataset of ecoregion parameters.
        /// </summary>
        int Index
        {
            get;
        }
    }

    /// <summary>
    /// The information for a fire ecoregion (its index and parameters).
    /// </summary>
    public class Ecoregion
        : EcoregionParameters, IEcoregion
    {
        private int index;

        //---------------------------------------------------------------------

        public int Index
        {
            get {
                return index;
            }
        }

        //---------------------------------------------------------------------

        public Ecoregion(int                  index,
                         IEcoregionParameters parameters)
            : base(parameters)
        {
            this.index = index;
        }
    }
}
