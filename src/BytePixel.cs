//  Authors:    Robert M. Scheller, James B. Domingo

using Landis.SpatialModeling;

namespace Landis.Extension.DynamicFire
{
    public class BytePixel : Pixel
    {
        public Band<byte> MapCode  = "The numeric code for each raster cell";

        public BytePixel()
        {
            SetBands(MapCode);
        }
    }
}
