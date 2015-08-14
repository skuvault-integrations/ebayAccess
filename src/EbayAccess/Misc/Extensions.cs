using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using EbayAccess.Models;
using EbayAccess.Models.BaseResponse;
using EbayAccess.Models.ReviseFixedPriceItemResponse;
using EbayAccess.Models.ReviseInventoryStatusResponse;
using Netco.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Item = EbayAccess.Models.ReviseInventoryStatusResponse.Item;

namespace EbayAccess.Misc
{
	public static class Extensions
	{
		#region StringTo
		public static Stream ToStream( this string source, Encoding encoding = null )
		{
			var ms = new MemoryStream();

			if( encoding == null )
				encoding = new UTF8Encoding();

			var buf = encoding.GetBytes( source );
			ms.Write( buf, 0, buf.Length );
			ms.Position = 0;
			return ms;
		}

		public static bool ToBool( this string source, bool throwException = false )
		{
			var parsedBool = default( bool );
			try
			{
				parsedBool = bool.Parse( source );
			}
			catch
			{
				if( throwException )
					throw;
			}

			return parsedBool;
		}

		public static decimal ToDecimalDotOrComaSeparated( this string source, bool throwException = false )
		{
			var parsedNumber = default ( decimal );
			try
			{
				parsedNumber = decimal.Parse( source, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture );
			}
			catch
			{
				try
				{
					parsedNumber = decimal.Parse( source, new NumberFormatInfo { NumberDecimalSeparator = "," } );
				}
				catch
				{
					if( throwException )
						throw;
				}
			}

			return parsedNumber;
		}

		public static int ToIntOrDefault( this string source, bool throwException = true )
		{
			try
			{
				return int.Parse( source, CultureInfo.InvariantCulture );
			}
			catch( Exception )
			{
				if( throwException )
					throw;
			}

			return default( int );
		}

		public static long ToLong( this string source )
		{
			var parsedNumber = long.Parse( source, CultureInfo.InvariantCulture );
			return parsedNumber;
		}

		public static DateTime ToDateTime( this string source, bool throwException = false )
		{
			DateTime dateTime;
			try
			{
				dateTime = XmlConvert.ToDateTime( source, XmlDateTimeSerializationMode.RoundtripKind | XmlDateTimeSerializationMode.Utc );
			}
			catch
			{
				dateTime = default( DateTime );
				if( throwException )
					throw;
			}

			return dateTime;
		}
		#endregion

		public static void SkipErrorsAndDo( this IEnumerable< EbayBaseResponse > source, Action< EbayBaseResponse > action, List< ResponseError > updateInventoryErrorsToSkip )
		{
			source.ForEach( x => x.SkipErrorsAndDo( action, updateInventoryErrorsToSkip ) );
		}

		public static IEnumerable< T > GetOnlyResponsesWithErrors< T >( this IEnumerable< T > source, Action< T > actionp ) where T : EbayBaseResponse
		{
			return source.Where( x => x.Errors != null && x.Errors.Any() );
		}

		public static IEnumerable< T > GetOnlyResponsesWithoutErrors< T >( this IEnumerable< T > source, Action< T > actionp ) where T : EbayBaseResponse
		{
			return source.Where( x => x.Errors == null || !x.Errors.Any() );
		}

		public static IEnumerable< ResponseError > CollectAllErros( this IEnumerable< EbayBaseResponse > source )
		{
			return source.Where( x => x.Errors != null && x.Errors.Any() ).SelectMany( x => x.Errors );
		}

		public static void ThrowOnError( this EbayBaseResponse response )
		{
			if( response.Errors != null && response.Errors.Any() )
				throw new Exception( response.Errors.ToJson() );
		}

		public static void ThrowOnError( this IEnumerable< EbayBaseResponse > responses, Func< IEnumerable< EbayBaseResponse >, string > exceptionMessage = null )
		{
			var ebayBaseResponses = responses as IList< EbayBaseResponse > ?? responses.ToList();
			var responsesWithErrors = ebayBaseResponses.Where( x => x != null && x.Errors != null && x.Errors.Any() ).ToList();

			if( responsesWithErrors.Any() )
			{
				var aggregatedErrors = responsesWithErrors.SelectMany( x => x.Errors ).ToList();
				var message = exceptionMessage == null ? aggregatedErrors.ToJson() : exceptionMessage( ebayBaseResponses );
				throw new Exception( message );
			}
		}

		public static string ToStringUtcIso8601( this DateTime dateTime )
		{
			var universalTime = dateTime.ToUniversalTime();
			var result = XmlConvert.ToString( universalTime, XmlDateTimeSerializationMode.RoundtripKind );
			return result;
		}

		public static bool IsZero( this decimal src )
		{
			const decimal epsolon = 0.000001m;
			return Math.Abs( src ) - epsolon == 0;
		}

		public static IEnumerable< UpdateInventoryResponse > ToUpdateInventoryResponses( this InventoryStatusResponse source )
		{
			return ( source.Items ?? new List< Item >() )
				.Where( x => x.ItemId.HasValue )
				.Select( x => new UpdateInventoryResponse() { ItemId = x.ItemId.Value } ).ToList();
		}

		public static IEnumerable< UpdateInventoryResponse > ToUpdateInventoryResponses( this IEnumerable< InventoryStatusResponse > source )
		{
			return source.SelectMany( x => x.ToUpdateInventoryResponses().ToList() );
		}

		public static UpdateInventoryResponse ToUpdateInventoryResponses( this ReviseFixedPriceItemResponse source )
		{
			return new UpdateInventoryResponse() { ItemId = source.Item != null ? source.Item.ItemId : long.MinValue };
		}

		public static IEnumerable< UpdateInventoryResponse > ToUpdateInventoryResponses( this IEnumerable< ReviseFixedPriceItemResponse > source )
		{
			return source.Select( x => x.ToUpdateInventoryResponses() ).Where( x => x.ItemId != long.MinValue );
		}

		public static string ToJson( this IEnumerable< string > source )
		{
			return ToJson( source, x => string.IsNullOrWhiteSpace( x ) ? PredefinedValues.NotAvailable : x );
		}

		public static string ToJson( this Dictionary< string, string > source )
		{
			if( source == null )
				source = new Dictionary< string, string >();

			var sourceList = source as IList< KeyValuePair< string, string > > ?? source.ToList();
			return ToJson( sourceList, x => string.Format( "{{{0}:{1}}}", string.IsNullOrWhiteSpace( x.Key ) ? PredefinedValues.NotAvailable : x.Key, string.IsNullOrWhiteSpace( x.Value ) ? PredefinedValues.NotAvailable : x.Value ) );
		}

		private static string ToJson< T >( this IEnumerable< T > source, Func< T, string > elementToString )
		{
			if( source == null )
				source = new List< T >();

			var sourceList = source as IList< T > ?? source.ToList();
			var values = sourceList.ToArray().Select( x => x != null ? elementToString( x ) : "null" ).ToList();
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


		public static string ToJson<T>(this IEnumerable<T> source)
		{
			try
			{
				if (source == null)
					return PredefinedValues.EmptyJsonObject;
				var objects = source as IList<T> ?? source.ToList();
				var items = string.Join(",", objects.Where(x => x != null).Select(x => x.ToJson()));
				var res = string.Format("{{Count:{0}, Items:[{1}]}}", objects.Count(), items);
				return res;
			}
			catch
			{
				return PredefinedValues.EmptyJsonList;
			}
		}

		public static string ToJson( this object source )
		{
			try
			{
				if( source == null )
					return PredefinedValues.EmptyJsonObject;
				else
				{
					var sourceConverted = source as ISerializableManual;
					if( sourceConverted != null )
						return sourceConverted.ToJsonManual();
					else
					{
						var serialized = JsonConvert.SerializeObject( source, new IsoDateTimeConverter() );
						return serialized;
					}
				}
			}
			catch( Exception )
			{
				return PredefinedValues.EmptyJsonObject;
			}
		}
	}
}