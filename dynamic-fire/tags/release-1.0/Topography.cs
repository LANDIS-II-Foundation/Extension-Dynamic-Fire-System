//  Copyright 2006 University of Wisconsin
//  Author:  James B. Domingo
//  License:  Available at  
//  http://landis.forest.wisc.edu/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Landis.Landscape;
using Landis.RasterIO;
using System.IO;


namespace Landis.Fire
{
    internal static class Topography
    {
        //---------------------------------------------------------------------

        internal static void ReadGroundSlopeMap(string path)
        {
            IInputRaster<TopoPixel> map; // = Model.Core.OpenRaster<TopoPixel>(path);

            try {
                map = Model.Core.OpenRaster<TopoPixel>(path);
            }
            catch (FileNotFoundException) {
                string mesg = string.Format("Error: The file {0} does not exist", path);
                throw new System.ApplicationException(mesg);
            }

            if (map.Dimensions != Model.Core.Landscape.Dimensions)
            {
                string mesg = string.Format("Error: The input map {0} does not have the same dimension (row, column) as the ecoregions map", path);
                throw new System.ApplicationException(mesg);
            }

            using (map) {
                foreach (Site site in Model.Core.Landscape.AllSites) {
                    TopoPixel pixel = map.ReadPixel();
                    if (site.IsActive) {
                        ushort mapCode = pixel.Band0;
                        if (mapCode < 0)
                            throw new PixelException(site.Location,
                                                     "Ground Slope invalid map code: {0}",
                                                     mapCode);
                        SiteVars.GroundSlope[site] = mapCode;
                    }
                }
            }
        }
        //---------------------------------------------------------------------

        internal static void ReadUphillSlopeAzimuthMap(string path)
        {
            IInputRaster<TopoPixel> map; // = Model.Core.OpenRaster<TopoPixel>(path);

            try {
                map = Model.Core.OpenRaster<TopoPixel>(path);
            }
            catch (FileNotFoundException) {
                string mesg = string.Format("Error: The file {0} does not exist", path);
                throw new System.ApplicationException(mesg);
            }

            if (map.Dimensions != Model.Core.Landscape.Dimensions)
            {
                string mesg = string.Format("Error: The input map {0} does not have the same dimension (row, column) as the ecoregions map", path);
                throw new System.ApplicationException(mesg);
            }

            using (map) {
                foreach (Site site in Model.Core.Landscape.AllSites) {
                    TopoPixel pixel = map.ReadPixel();
                    if (site.IsActive) {
                        ushort mapCode = pixel.Band0;
                        if (mapCode < 0 || mapCode > 360)
                            throw new PixelException(site.Location,
                                                     "Uphill slope azimuth invalid map code: {0}",
                                                     mapCode);
                        SiteVars.UphillSlopeAzimuth[site] = mapCode;
                    }
                }
            }
        }

    }
}
