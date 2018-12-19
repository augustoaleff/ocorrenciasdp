using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OcorrenciasDP.Library
{
    public class Shared
    {

        public static string PegarPrimeiroNome(string nome)
        {
            string primeiroNome = "";

            for (int i = 0; i < nome.Length; i++)
            {
                string letras = nome.Substring(i, 1);

                if (letras != " ")
                {
                    primeiroNome += letras;
                }
                else
                {
                    break;
                }

            }

            return primeiroNome;

        }
    }
}
