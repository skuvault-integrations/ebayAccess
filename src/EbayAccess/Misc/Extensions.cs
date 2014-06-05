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

		public static decimal ToDecimalDotOrComaSeparated( this string srcString, bool throwException = false )
		{
			var parsedNumber = default ( decimal );
			try
			{
				parsedNumber = decimal.Parse( srcString, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture );
			}
			catch
			{
				try
				{
					parsedNumber = decimal.Parse( srcString, new NumberFormatInfo { NumberDecimalSeparator = "," } );
				}
				catch
				{
					if( throwException )
						throw;
				}
			}

			return parsedNumber;
		}

		public static bool ToBool( this string srcString, bool throwException = false )
		{
			var parsedBool = default( bool );
			try
			{
				parsedBool = bool.Parse( srcString );
			}
			catch
			{
				if( throwException )
					throw;
			}

			return parsedBool;
		}

		public static bool IsZero( this decimal src )
		{
			const decimal epsolon = 0.000001m;
			return Math.Abs( src ) - epsolon == 0;
		}

		public static int ToIntOrDefault( this string srcString, bool throwException = true )
		{
			try
			{
				return int.Parse( srcString, CultureInfo.InvariantCulture );
			}
			catch( Exception )
			{
				if( throwException )
					throw;
			}

			return default( int );
		}

		public static long ToLong( this string srcString )
		{
			var parsedNumber = long.Parse( srcString, CultureInfo.InvariantCulture );
			return parsedNumber;
		}

		public static DateTime ToDateTime( this string srcString, bool throwException = false )
		{
			DateTime dateTime;
			try
			{
				dateTime = XmlConvert.ToDateTime( srcString, XmlDateTimeSerializationMode.RoundtripKind | XmlDateTimeSerializationMode.Utc );
			}
			catch
			{
				dateTime = default( DateTime );
				if( throwException )
					throw;
			}

			return dateTime;
		}
	}
}