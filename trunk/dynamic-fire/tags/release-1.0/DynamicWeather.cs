//  Copyright 2005 University of Wisconsin
//  Authors:  Robert M. Scheller, James B. Domingo
//  License:  Available at  
//  http://landis.forest.wisc.edu/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Edu.Wisc.Forest.Flel.Util;

namespace Landis.Fire
{

    public interface IDynamicWeather
    {
        int Year {get;set;}
        string FileName{get;set;}
    }
}

namespace Landis.Fire
{
    public class DynamicWeather
    : IDynamicWeather
    {
        private string fileName;
        private int year;
        
        //---------------------------------------------------------------------
        public string FileName
        {
            get {
                return fileName;
            }
            set {
                if (value.Trim(null) == "")
                throw new InputValueException(value,
                                              "Invalid file path: {0}",
                                              value);
                fileName = value;
            }
        }

        //---------------------------------------------------------------------
        public int Year
        {
            get {
                return year;
            }
            set {
                    if (value < 0 )
                        throw new InputValueException(value.ToString(),
                            "Value must be > 0 ");
                year = value;
            }
        }
        //---------------------------------------------------------------------

        public DynamicWeather()
        {
        }

/*        //---------------------------------------------------------------------
        public DynamicWeatherTable(
            string fileName,
            int year
            )
        {
            this.fileName = fileName;
            this.year = year;
        }

        //---------------------------------------------------------------------

        public DynamicWeatherTable()
        {
            this.fileName = "";
            this.year = 0;
        }
*/

    }
}
