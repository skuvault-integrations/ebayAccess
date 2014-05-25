using System;
using System.Globalization;
using System.Xml;

namespace EbayAccess.Misc
{
	public static class Extensions
	{
		public static string ToStringUtcIso8601( this DateTime dateTime )
		{
			var universalTime = dateTime.ToUniversalTime();
			var result = XmlConvert.ToString( universalTime, XmlDateTimeSerializationMode.RoundtripKind );
			return result;
		}

		public static decimal ToDecimalDotOrComaSeparated( this string srcString )
		{
			decimal parsedNumber;
			try
			{
				parsedNumber = decimal.Parse( srcString, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture );
			}
			catch( Exception )
			{
				parsedNumber = decimal.Parse( srcString, new NumberFormatInfo { NumberDecimalSeparator = "," } );
			}

			return parsedNumber;
		}

		public static bool IsZero( this decimal src )
		{
			const decimal epsolon = 0.000001m;
			;
			return Math.Abs( src ) - epsolon == 0;
		}

		public static int ToIntOrDefault( this string srcString )
		{
			try
			{
				return int.Parse( srcString, CultureInfo.InvariantCulture );
			}
			catch( Exception )
			{
				return default( int );
			}
		}

		public static long ToLong( this string srcString )
		{
			var parsedNumber = long.Parse( srcString, CultureInfo.InvariantCulture );
			return parsedNumber;
		}

		public static DateTime ToDateTime( this string srcString )
		{
			try
			{
				var dateTime = DateTime.Parse( srcString, CultureInfo.InvariantCulture );
				return dateTime;
			}
			catch
			{
				try
				{
					var result = XmlConvert.ToDateTime( srcString, XmlDateTimeSerializationMode.RoundtripKind | XmlDateTimeSerializationMode.Utc );
					return result;
				}
				catch
				{
					return default( DateTime );
				}
			}
		}
	}
}