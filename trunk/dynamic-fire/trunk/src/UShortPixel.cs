//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Srinivas S., Robert M. Scheller, James B. Domingo

using Landis.SpatialModeling;

namespace Landis.Extension.DynamicFire
{
    public class UShortPixel : Pixel
    {
        public Band<ushort> MapCode  = "The numeric code for each raster cell";

        public UShortPixel()
        {
            SetBands(MapCode);
        }
    }
}
