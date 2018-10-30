using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace OcorrenciasDP.Library.Globalization
{
    public class Globalization
    {
        public static DateTime HoraAtualBR()
        {
            return TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"));
        }

        public static string DataAtualExtensoBR() {
            DateTime data = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"));
            CultureInfo culture = new CultureInfo("pt-BR");
            return data.ToString("f", culture);
        }

        public static string DataHoraExtensoBR(DateTime data)
        {
             CultureInfo culture = new CultureInfo("pt-BR");
             return data.ToString("f", culture);
        }

        public static string DataExtensoBR(DateTime data)
        {
            CultureInfo culture = new CultureInfo("pt-BR");
            return data.ToString("D", culture);
        }
        
        public static DateTime ConverterData(string data)
        {
            CultureInfo culture = new CultureInfo("pt-BR");
            return DateTime.Parse(data, culture);
        }

    }
}
