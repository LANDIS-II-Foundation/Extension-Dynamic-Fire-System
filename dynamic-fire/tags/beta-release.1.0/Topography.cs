//  Copyright 2006 University of Wisconsin
//  Author:  James B. Domingo
//  License:  Available at  
//  http://landis.forest.wisc.edu/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Landis.Landscape;
using Landis.RasterIO;

namespace Landis.Fire
{
    internal static class Topography
    {
        //---------------------------------------------------------------------

        internal static void ReadGroundSlopeMap(string path)
        {
            IInputRaster<TopoPixel> map = Model.Core.OpenRaster<TopoPixel>(path);
            using (map) {
                foreach (Site site in Model.Core.Landscape.AllSites) {
                    TopoPixel pixel = map.ReadPixel();
                    if (site.IsActive) {
                        ushort mapCode = pixel.Band0;
                        if (mapCode < 0 || mapCode > 100)
                            throw new PixelException(site.Location,
                                                     "invalid map code: {0}",
                                                     mapCode);
                        SiteVars.GroundSlope[site] = mapCode;
                    }
                }
            }
        }
        //---------------------------------------------------------------------

        internal static void ReadUphillSlopeAzimuthMap(string path)
        {
            IInputRaster<TopoPixel> map = Model.Core.OpenRaster<TopoPixel>(path);
            using (map) {
                foreach (Site site in Model.Core.Landscape.AllSites) {
                    TopoPixel pixel = map.ReadPixel();
                    if (site.IsActive) {
                        ushort mapCode = pixel.Band0;
                        if (mapCode < 0 || mapCode > 360)
                            throw new PixelException(site.Location,
                                                     "invalid map code: {0}",
                                                     mapCode);
                        SiteVars.UphillSlopeAzimuth[site] = mapCode;
                    }
                }
            }
        }

    }
}
