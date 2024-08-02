namespace CoreFtp.Infrastructure.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    public static class DefaultValueExtensions
    {
        public static DateTime ParseExactOrDefault( this string operand, params string[] formats )
        {
            DateTime parsedDate;

            return DateTime.TryParseExact( operand, formats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out parsedDate )
                ? parsedDate
                : default( DateTime );
        }

        public static long ParseOrDefault( this string operand )
        {
            long parsedLong;

            return long.TryParse( operand, out parsedLong )
                ? parsedLong
                : default( long );
        }

        public static TVal GetValueOrDefault< TKey, TVal >( this Dictionary<TKey, TVal> operand, TKey key )
        {
            TVal value;

            operand.TryGetValue( key, out value );
            return value;
        }
    }
}