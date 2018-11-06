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
            return data.ToString("f", culture); //19 de setembro de 2018 14:30
        }

        public static string DataHoraExtensoBR(DateTime data)
        {
             CultureInfo culture = new CultureInfo("pt-BR");
             return data.ToString("f", culture); //19 de setembro de 2018 14:30
        }

        public static string DataExtensoBR(DateTime data)
        {
            CultureInfo culture = new CultureInfo("pt-BR");
            return data.ToString("D", culture); //19 de setembro de 2018
        }

        public static string DataCurtaBR(DateTime data)
        {
            CultureInfo culture = new CultureInfo("pt-BR");
            return data.ToString("d", culture); // 19/09/2018
        }

        public static DateTime ConverterData(string data)
        {
            CultureInfo culture = new CultureInfo("pt-BR");
            return DateTime.Parse(data, culture);
        }

        public static string DataRelatorioPdfBR()
        {
            CultureInfo culture = new CultureInfo("pt-BR");
            string data = string.Concat(HoraAtualBR().ToString("d", culture),"_",HoraAtualBR().ToString("t",culture));
            return data; // 19/09/2018_14:30
        }
    }
}
