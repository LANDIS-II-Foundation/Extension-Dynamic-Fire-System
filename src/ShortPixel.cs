//  Authors:  Srinivas S., Robert M. Scheller, James B. Domingo

using Landis.SpatialModeling;

namespace Landis.Extension.DynamicFire
{
    public class ShortPixel : Pixel
    {
        public Band<short> MapCode  = "The numeric code for each raster cell";

        public ShortPixel()
        {
            SetBands(MapCode);
        }
    }
}
