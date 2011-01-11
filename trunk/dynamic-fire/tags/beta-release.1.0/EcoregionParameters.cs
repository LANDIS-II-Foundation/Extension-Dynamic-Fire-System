//  Copyright 2006 University of Wisconsin
//  Author:  James B. Domingo
//  License:  Available at  
//  http://landis.forest.wisc.edu/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

namespace Landis.Fire
{
    /// <summary>
    /// The parameters for an ecoregion.
    /// </summary>
    public class EcoregionParameters
        : IEcoregionParameters
    {
        private string name;
        private string description;
        private ushort mapCode;
        private IMoreEcoregionParameters moreEcoregionParameters;

        //---------------------------------------------------------------------

        public string Name
        {
            get {
                return name;
            }
        }

        //---------------------------------------------------------------------

        public string Description
        {
            get {
                return description;
            }
        }

        //---------------------------------------------------------------------

        public ushort MapCode
        {
            get {
                return mapCode;
            }
        }

        //---------------------------------------------------------------------

        public IMoreEcoregionParameters MoreEcoregionParameters
        {
            get {
                return moreEcoregionParameters;
            }

            set {
                moreEcoregionParameters = value;
            }
        }

        //---------------------------------------------------------------------

        public EcoregionParameters(string name,
                                   string description,
                                   ushort mapCode)
        {
            this.name = name;
            this.description = description;
            this.mapCode = mapCode;

            this.moreEcoregionParameters = new MoreEcoregionParameters();
        }

        //---------------------------------------------------------------------

        public EcoregionParameters(IEcoregionParameters parameters)
        {
            name           = parameters.Name;
            description    = parameters.Description;
            mapCode        = parameters.MapCode;
            moreEcoregionParameters = parameters.MoreEcoregionParameters;
        }
    }
}
