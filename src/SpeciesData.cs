//  Author: Robert Scheller, Melissa Lucash


namespace Landis.Extension.DynamicFire
{
    public class SpeciesData 
    {
        public static Landis.Library.Parameters.Species.AuxParm<int> FireTolerance;

        //---------------------------------------------------------------------
        public static void Initialize(IInputParameters parameters)
        {
            FireTolerance          = parameters.FireTolerance;
        }
    }
}
