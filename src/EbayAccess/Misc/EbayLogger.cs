using Netco.Logging;

namespace EbayAccess.Misc
{
	public static class EbayLogger
	{
		public static ILogger Log()
		{
			return NetcoLogger.GetLogger("EbayLogger");
		}
	}
}