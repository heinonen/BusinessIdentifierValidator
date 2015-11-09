using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BusinessIdentifierValidator;

namespace BusinessIdentifierTest
{
    class BusinessIdentifierTest1
    {
        static void Main(string[] args)
        {
            BusinessIdentifierTest1 t1 = new BusinessIdentifierTest1();

            //1. Validi tunnus(7-numeroa), jakojäännös 0    => tarkistusmerkki 0
            t1.testBI(1, "1572860-0");

            //2. Validi tunnus (6-numeroa)
            t1.testBI(2, "737546-2");

            //3. Tunnuksen yksilöivä osa liian lyhyt
            t1.testBI(3, "12345-6");

            //4. Tunnuksen yksilöivä osa liian pitkä
            t1.testBI(4, "12345678-9");

            //5. Tunnuksen yksilöivä osa ei ole numero
            t1.testBI(5, "12345X7-8");

            //6. Tunnuksen tarkistusmerkki ei ole numero
            t1.testBI(6, "1234567-Z");

            //7. Tunnuksen välimerkki ei ole oikea
            t1.testBI(7, "1572860+0");

            //8. Jakojäännös 1, tunnus ei käytössä
            t1.testBI(8, "1181101-1"); //111 % 11

            //9. Validi tunnus, jakojäännös 2           => tarkistusmerkki 11-2=9
            t1.testBI(9, "1181000-9"); //101 % 11

            //10. Validi tunnus, jakojäännös 10         => tarkistusmerkki 11-10=1
            t1.testBI(10, "1181100-1"); //109 % 11

            //11. Väärä tarkistusmerkki
            t1.testBI(11, "1234567-8");

            //12. Tunnuksen yksilöivä osa eikä tarkistusmerkki ole numero, eikä välimerkki ole oikea
            t1.testBI(12, "ABCDEFG?H");
        }

        public void testBI(int testCase, string bi)
        {
            BusinessIdentifierSpecification bis = new BusinessIdentifierSpecification();

            Console.WriteLine("TEST " + testCase + "\nBI: " + bi + "\n\tisSatisfied:\t" + bis.IsSatisfiedBy(bi));
            Console.WriteLine("\tinvalidity reasons:");
            foreach (string reason in bis.ReasonsForDissatisfaction)
            {
                Console.WriteLine("\t\t\t" + reason);
            }
            Console.ReadLine();
        }
    }
}
