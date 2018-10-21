using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace cityLife4
{
    public partial class Apartment
    {
        /// <summary>
        /// The method gives the nightly price for the apartment, without discounts
        /// Note that discounts can be calculated only if you know the exact dates of 
        /// staying in the apartment
        /// </summary>
        /// <param name="adults">number of adults</param>
        /// <param name="children">number of fhildren</param>
        /// <param name="weekend">true for weekend, false for weekday</param>
        /// <param name="currencyCode">the currency code in which the price should be quoted.</param>
        /// <param name="atDate">the date for which you want to get the exchange rate</param>
        /// <returns>nightly price</returns>
        public int PricePerNight(int adults, int children, bool weekend, string currencyCode, DateTime atDate)
        {
            
            var thePrice = from price in this.Pricings
                           where price.adults == adults && price.children == children
                           select price;
            if (thePrice.Count()==0)
            {
                //We did not find a pricing record for that number of adults and children
                return 0;
            }
            else
            {
                if (weekend)
                {
                    return thePrice.First().WeekendPriceBy(currencyCode, atDate);
                }
                else
                {
                    return thePrice.First().WeekdayPriceBy(currencyCode, atDate);
                }
            }
        }

        /// <summary>
        /// The method returns the main landscape photo of the apartment, if such exists, and null if none. If more than one photo is tagged
        /// as "main"and "landscape" - return the first one
        /// </summary>
        /// <returns>apartment main photo or null if nont exists</returns>
        public ApartmentPhoto MainPhotoLandscape()
        {
                ApartmentPhoto theMainPhoto = this.ApartmentPhotoes.FirstOrDefault(aPhoto => aPhoto.type == PhotoType.Main && 
                                                                                   aPhoto.orientation == OrientationType.Landscape);
                return theMainPhoto;
        }
        /// <summary>
        /// The method returns the main portrait photo of the apartment, if such exists, and null if none. If more than one photo is tagged
        /// as "main" and "portrait" - return the first one
        /// </summary>
        /// <returns>apartment main photo or null if nont exists</returns>
        public ApartmentPhoto MainPhotoPortrait()
        {
            ApartmentPhoto theMainPhoto = this.ApartmentPhotoes.FirstOrDefault(aPhoto => aPhoto.type == PhotoType.Main &&
                                                                               aPhoto.orientation == OrientationType.Portrait);
            return theMainPhoto;
        }
    }
}