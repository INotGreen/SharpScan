namespace CoreFtp.Infrastructure.Extensions
{
    using System;
#if NETSTANDARD
    using System.Reflection;
#endif

    public static class EnumExtensions
    {
        public static TEnum? ToNullableEnum< TEnum >( this string operand ) where TEnum : struct, IComparable, IFormattable, IConvertible
        {
            TEnum enumOut;
            if ( Enum.TryParse( operand, true, out enumOut ) )
            {
                return enumOut;
            }

            return null;
        }

        public static TEnum? ToNullableEnum< TEnum >( this int operand ) where TEnum : struct, IComparable, IFormattable, IConvertible
        {
            if ( Enum.IsDefined( typeof( TEnum ), operand ) )
            {
                return (TEnum) (object) operand;
            }

            return null;
        }
    }
}