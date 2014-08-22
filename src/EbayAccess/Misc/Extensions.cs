using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using EbayAccess.Models;
using EbayAccess.Models.BaseResponse;
using EbayAccess.Models.GetOrdersResponse;
using EbayAccess.Models.ReviseFixedPriceItemRequest;
using EbayAccess.Models.ReviseInventoryStatusRequest;
using Item = EbayAccess.Models.ReviseFixedPriceItemResponse.Item;

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

		public static string ToJson( this IEnumerable< string > source )
		{
			var orders = source as IList< string > ?? source.ToList();
			var items = string.Join( ",", source );
			var res = string.Format( "{{Count:{0}, Items:[{1}]}}", orders.Count(), items );
			return res;
		}

		public static string ToJson( this Dictionary< string, string > source )
		{
			var keysAndValues = source.ToArray().Select( x => string.Format( "{{{0}:{1}}}", x.Key, x.Value ) );
			var items = string.Join( ",", keysAndValues );
			var res = string.Format( "{{Count:{0}, Items:[{1}]}}", keysAndValues.Count(), items );
			return res;
		}

		private static string ToJson< T >( this IEnumerable< T > source, Func< T, string > elementToString )
		{
			var sourceList = source as IList< T > ?? source.ToList();
			var values = sourceList.ToArray().Select( elementToString ).ToList();
			var items = string.Join( ",", values );
			var res = string.Format( "{{Count:{0}, Items:[{1}]}}", values.Count(), items );
			return res;
		}

		public static string ToStringSafe( this Stream webResponseStream )
		{
			string responseStr;
			using( Stream streamCopy = new MemoryStream( ( int )webResponseStream.Length ) )
			{
				var sourcePos = webResponseStream.Position;
				webResponseStream.CopyTo( streamCopy );
				webResponseStream.Position = sourcePos;
				streamCopy.Position = 0;
				responseStr = new StreamReader( streamCopy ).ReadToEnd();
			}
			return responseStr;
		}

		public static string ToJson( this IEnumerable< ResponseError > sourceErrors )
		{
			var errors = sourceErrors.Select( x => string.Format( "{{ErrorCode:{0},ShortMessage:{1},LongMessage:{2},ErrorClassification:{3},ServerityCode:{4},ErrorParameters:{5}}}",
				x.ErrorCode,
				x.ShortMessage,
				x.LongMessage,
				x.ErrorClassification,
				x.ServerityCode,
				x.ErrorParameters )
				);
			return string.Format( "{{Count:{0}, Errors:[{1}]}}", errors.Count(), string.Join( ",", errors ) );
		}

		public static string ToJson( this IEnumerable< Item > source )
		{
			return ToJson( source, x => string.Format( "{{Id:{0},EndTime:{1}}}", x.ItemId, x.EndTime ) );
		}

		public static string ToJson( this IEnumerable< UpdateInventoryRequest > source )
		{
			return ToJson( source, x => string.Format( "{{Id:{0},Sku:{1},Qty{2}}}", x.ItemId, x.Sku, x.Quantity ) );
		}

		public static string ToJson( this IEnumerable< UpdateInventoryResponse > source )
		{
			return ToJson( source, x => string.Format( "{{Id:{0}}}", x.ItemId ) );
		}

		public static string ToJson( this IEnumerable< ReviseFixedPriceItemRequest > source )
		{
			return ToJson( source, x => string.Format( "{{Id:{0},Sku:{1},Quantity:{2}}}", x.ItemId, x.Sku, x.Quantity ) );
		}

		public static string ToJson( this IEnumerable< InventoryStatusRequest > source )
		{
			return ToJson( source, x => string.Format( "{{id:{0},sku:{1},qty:{2}}}",
				x.ItemId == null ? PredefinedValues.NotAvailable : x.ItemId.Value.ToString( CultureInfo.InvariantCulture ),
				string.IsNullOrWhiteSpace( x.Sku ) ? PredefinedValues.NotAvailable : x.Sku,
				x.Quantity == null ? PredefinedValues.NotAvailable : x.Quantity.ToString() ) );
		}

		public static string ToJson( this IEnumerable< Models.GetSellerListCustomResponse.Item > source )
		{
			return ToJson( source, x => string.Format( "{{id:{0},sku:{1},qty:{2}}}",
				string.IsNullOrWhiteSpace( x.ItemId ) ? PredefinedValues.NotAvailable : x.ItemId,
				string.IsNullOrWhiteSpace( x.Sku ) ? PredefinedValues.NotAvailable : x.Sku,
				x.Quantity.ToString( CultureInfo.InvariantCulture ) ) );
		}

		public static string ToJson( this IEnumerable< Models.ReviseInventoryStatusResponse.Item > source )
		{
			return ToJson( source, x => string.Format( "{{id:{0},sku:{1},qty:{2}}}",
				x.ItemId.HasValue ? x.ItemId.Value.ToString( CultureInfo.InvariantCulture ) : PredefinedValues.NotAvailable,
				string.IsNullOrWhiteSpace( x.Sku ) ? PredefinedValues.NotAvailable : x.Sku,
				x.Quantity.ToString() ) );
		}
	}
}