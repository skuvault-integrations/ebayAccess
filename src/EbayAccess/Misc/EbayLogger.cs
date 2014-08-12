using Netco.Logging;

namespace EbayAccess.Misc
{
	public static class EbayLogger
	{
		public static ILogger Log()
		{
			return NetcoLogger.GetLogger( "EbayLogger" );
		}

		public static void LogTraceStarted( string info )
		{
			Log().Trace( "[ebay] Start call:{0}.", info );
		}

		public static void LogTraceEnded( string info )
		{
			Log().Trace( "[ebay] End call:{0}.", info );
		}

		public static void LogTraceInnerStarted( string info )
		{
			Log().Trace( "[ebay] Internal Start call:{0}.", info );
		}

		public static void LogTraceInnerEnded( string info )
		{
			Log().Trace( "[ebay] Internal End call:{0}.", info );
		}

		public static void LogTraceInnerError( string info )
		{
			Log().Trace( "[ebay] Internal error:{0}.", info );
		}
	}
}