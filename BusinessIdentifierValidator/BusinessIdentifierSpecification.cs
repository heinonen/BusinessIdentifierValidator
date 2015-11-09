using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessIdentifierValidator
{
    public class BusinessIdentifierSpecification : ISpecification<string>
    {
        private List<string> reasons = null;
        private List<int> identifierPartMultiplierList;
        private int identifierPartMultiplierListLength;

        private int identifierMaxLength;
        private int identifierMinLength;
        private char punctuationMark;

        private int totalMaxLength;
        private int totalMinLength;

        private int divider;

        public BusinessIdentifierSpecification()
        {
            //Alusta muuttujat
            reasons = new List<string>();
            //Kertoimet, HUOM! VASEMMALTA OIKEALLE!
            identifierPartMultiplierList = new List<int>() { 7, 9, 10, 5, 8, 4, 2 };
            identifierPartMultiplierListLength = identifierPartMultiplierList.Count;

            //Yksilöivän osan MAX ja MIN pituudet
            identifierMaxLength = identifierPartMultiplierListLength;
            identifierMinLength = identifierPartMultiplierListLength - 1;
            punctuationMark = '-';

            //Y-tunnuksen pituus: Lisää välimerkin ja tarkistusmerkin pituus yksilöivän osan pituuteen.
            totalMaxLength = identifierMaxLength + 2;
            totalMinLength = identifierMinLength + 2;

            //Jakaja
            divider = 11;
        }

        public IEnumerable<string> ReasonsForDissatisfaction
        {
            get
            {
                return reasons;
            }
        }

        public bool IsSatisfiedBy(string entity)
        {
            int identifierPart;
            int validationPart;

            string identifierString = null;
            string validationString = null;

            //Aseta oletusarvona y-tunnus validiksi
            bool retVal = true;

            //Käy säännöt läpi, ja lisää syy listaan miksi epäkelpo

            //https://www.vero.fi/download/Tarkistusmerkin_laskenta/%7BD5780347-547E-4C44-90C1-25F8AD9DA7F8%7D/6508
            //Y - tunnuksen muodostavat yksilöivä osa ja tarkistusmerkki mainitussa
            //järjestyksessä.Yksilöivän osan muodostaa seitsennumeroinen
            //järjestysnumero.
            //NOTE: http://tarkistusmerkit.teppovuori.fi/tarkmerk.htm#y-tunnus2
            //      --> yksilöivässä osassa numeroita oli aikaisemmin 6!
            if (entity.Length < totalMinLength)
            {
                reasons.Add("Tunnus (" + entity.Length + " merkkiä) on liian lyhyt (alle " + totalMinLength + " merkkiä).");
            }
            else if (entity.Length > totalMaxLength)
            {
                reasons.Add("Tunnus (" + entity.Length + " merkkiä) on liian pitkä (yli " + totalMaxLength + " merkkiä).");
            }
            else
            {
                //Lisää yksilöivän osan alkuun 0:ia, kunnes tunnus on max pituinen.
                while (entity.Length < totalMaxLength)
                {
                    entity = entity.Insert(0, "0");
                }

                //Tunnuksen muoto on NNNNNNN-T, jossa NNNNNNN on järjestysnumero ja T
                //on tarkistusmerkki.

                //Tarkista yksilöivä osa
                identifierString = entity.Substring(0, identifierMaxLength);
                bool isValid = Int32.TryParse(identifierString, NumberStyles.Integer, new CultureInfo("fi-FI"), out identifierPart);
                if(isValid == false)
                {
                    reasons.Add("Yksilöivä osa \"" + identifierString + "\" ei ole numero.");
                }

                //Tarkista välimerkki
                char tmpPunctuationMark;
                isValid = char.TryParse(entity.Substring(identifierMaxLength, 1), out tmpPunctuationMark);
                if (tmpPunctuationMark != punctuationMark)
                {
                    reasons.Add("Yksilöivän osan ja tarkistusmerkin välinen merkki (" + tmpPunctuationMark + ") ei ole oikea (" + punctuationMark + ").");
                }

                //Tarkista tarkistusnumero
                validationString = entity.Substring(totalMaxLength - 1, 1);
                isValid = Int32.TryParse(validationString, NumberStyles.Integer, new CultureInfo("fi-FI"), out validationPart);
                if (isValid == false)
                {
                    reasons.Add("Tarkistusnumero ei ole numero [0...9].");
                }

                //Laske ja validoi tarkistusmerkki, jos tunnuksen muoto on oikein
                if (reasons.Count == 0)
                {
                    //1. Kerrotaan VASEMMALTA lukien yksilöintiosan
                    //ensimmäinen   numero  luvulla 7
                    //toinen        "       "       9
                    //kolmas        "       "       10
                    //neljäs        "       "       5
                    //viides        "       "       8
                    //kuudes        "       "       4
                    //seitsemäs     "       "       2

                    //2. Lasketaan näin saadut tulot yhteen

                    //Laskettava validointiarvo
                    int validator = 0;
                    //Tunnuksen arvo N
                    int tmpN;

                    for (int i = 0; i < identifierMaxLength; i++)
                    {
                        isValid = Int32.TryParse(identifierString.Substring(i, 1), out tmpN);
                        if(isValid == false)
                        {
                            //Tämän pitäisi olla turha tarkistus! --> TO_BE_REMOVED
                            reasons.Add("Yksilöivä osa \"" + identifierString + "\" ei ole numero.");
                        }
                        validator += tmpN * identifierPartMultiplierList[i];
                    }

                    //3. Summa jaetaan luvulla 11
                    validator = validator % divider;

                    //4. Asetetaan tarkistusmerkiksi 0, 
                    //jos jakolaskun jakojäännös = 0
                    //TAI
                    //(11 - jakojäännös), jos jakolaskun jakojäännös > 1
                    //Jos jakolaskun jakojäännös on 1, niin kyseinen tunnus ei ole käytössä. 

                    if (validator > 1)
                    {
                        validator = 11 - validator;
                    }
                    else if(validator == 1)
                    {
                        reasons.Add("Tunnus ei ole käytössä.");
                    }
                    //else if (validator == 0) //Turha tarkistus & asetus --> TO_BE_REMOVED
                    //{
                    //    validator = 0;
                    //}

                    //Validoi tarkistusmerkki, jos tunnus on käytössä
                    if (reasons.Count == 0 && validator != validationPart)
                    {
                        reasons.Add("Tarkistusmerkki (" + validationPart + ") ei ole oikea (" + validator + ").");
                    }
                }
            }

            //Tarkista löytyikö syitä epäkelpoisuuteen.
            if (reasons.Count > 0)
            {
                retVal = false;
            }

            return retVal;
        }
    }

}
