using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using EbayAccess.Models.GetOrdersResponse;
using EbayAccess.Models.GetSellerListCustomResponse;
using EbayAccess.Models.ReviseInventoryStatusRequest;
using Item = EbayAccess.Models.GetOrdersResponse.Item;

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

		public static string ToJson( this IEnumerable< Order > source )
		{
			var orders = source as IList< Order > ?? source.ToList();
			var items = string.Join( ",", orders.Select( x => string.Format( "{{id:{0},saleRecNum:{1},createdAt:{2}}}",
				string.IsNullOrWhiteSpace( x.GetOrderId( false ) ) ? PredefinedValues.NotAvailable : x.GetOrderId( false ),
				string.IsNullOrWhiteSpace( x.GetOrderId() ) ? PredefinedValues.NotAvailable : x.GetOrderId(),
				x.CreatedTime ) ) );
			var res = string.Format( "{{Count:{0}, Items:[{1}]}}", orders.Count(), items );
			return res;
		}

		public static string ToJson(this IEnumerable<string> source)
		{
			var orders = source as IList<string> ?? source.ToList();
			var items = string.Join( ",", source );
			var res = string.Format("{{Count:{0}, Items:[{1}]}}", orders.Count(), items);
			return res;
		}
	}
}