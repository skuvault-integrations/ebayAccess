using System.Collections.Generic;
using System.Text.RegularExpressions;
using Netco.Logging;

namespace EbayAccess.Misc
{
	public static class EbayLogger
	{
		private const string EbayIntegrationMarker = "ebay";
		public static int MaxLogLineSize = 100000;

		public static readonly List<string> PersonalFieldNames = new List< string >
		{
			//<Order>
			"SellerEmail", 
			//<ShippingAddress>
			"Name", "Street1", "Street2", "CityName", "StateOrProvince", "Country", "CountryName", "Phone",
			//<Buyer>
			"Email", "UserFirstName", "UserLastName"
		};

		public static ILogger Log()
		{
			return NetcoLogger.GetLogger( "EbayLogger" );
		}

		public static void LogTraceStarted( string info )
		{
			TraceLog( "Start call", info );
		}

		public static void LogTraceEnded( string info )
		{
			TraceLog( "End call", info );
		}
		public static void LogTrace( string info )
		{
			TraceLog( "Trace call", info );
		}

		public static void LogTraceInnerStarted( string info )
		{
			TraceLog( "Internal Start call", info );
		}

		public static void LogTraceInnerEnded( string info )
		{
			TraceLog( "Internal End call", info );
		}

		public static void LogTraceInnerError( string info )
		{
			TraceLog( "Internal error", info );
		}

		public static void LogTraceInnerErrorSkipped( string info )
		{
			TraceLog( "Internal error (skipped)", info );
		}

		private static void TraceLog( string type, string info )
		{
			if ( info != null 
				&& info.Length > MaxLogLineSize )
			{
				info = info.Substring( 0, MaxLogLineSize );
			}

			Log().Trace( "[{channel}] {type}:{info}", EbayIntegrationMarker, type, info );
		}

		/// <summary>This will remove personal info</summary>
		/// <param name="replaceWith">Text to replace personal information with</param>
		public static string RemovePersonalInfoFromXML( this string xmlString, string replaceWith = "***" )
		{
			return MaskFields( xmlString, PersonalFieldNames, replaceWith );
		}

		private static string MaskFields( string xmlString, List< string > fieldNames, string replaceWith )
		{
			var maskedString = xmlString;
			foreach( var fieldName in fieldNames )
			{
				var regexPattern = $"<{fieldName}>.+?</{fieldName}>";	//One or more characters inside the tags. "?" means non-greedy (will match minimal string)
				maskedString = Regex.Replace( maskedString, regexPattern, $"<{fieldName}>{replaceWith}</{fieldName}>" );
			}
			return maskedString;
		}
	}
}