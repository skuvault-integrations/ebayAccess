using Netco.Logging;

namespace EbayAccess.Misc
{
	public static class EbayLogger
	{
		private const string EbayIntegrationMarker = "ebay";
		public static int MaxLogLineSize = 0xA00000; //10mb

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
	}
}