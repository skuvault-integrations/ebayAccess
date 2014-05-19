namespace EbayAccess
{
	public static class EbayHeaders
	{
		static EbayHeaders()
		{
			XEbayApiCallName = "X-EBAY-API-CALL-NAME";
			XEbayApiCompatibilityLevel = "X-EBAY-API-COMPATIBILITY-LEVEL";
			XEbayApiDevName = "X-EBAY-API-DEV-NAME";
			XEbayApiAppName = "X-EBAY-API-APP-NAME";
			XEbayApiSiteid = "X-EBAY-API-SITEID";
			XEbayApiCertName = "X-EBAY-API-CERT-NAME";
		}

		public static string XEbayApiCertName { get; private set; }

		public static string XEbayApiSiteid { get; private set; }

		public static string XEbayApiAppName { get; private set; }

		public static string XEbayApiDevName { get; private set; }

		public static string XEbayApiCompatibilityLevel { get; private set; }

		public static string XEbayApiCallName { get; private set; }
	}

	public static class EbayHeadersValues
	{
		static EbayHeadersValues()
		{
			XEbayApiSiteid = "0";
			XEbayApiCompatibilityLevel = "853";
		}

		public static string XEbayApiSiteid { get; private set; }

		public static string XEbayApiCompatibilityLevel { get; private set; }
	}

	public static class EbayHeadersMethodnames
	{
		static EbayHeadersMethodnames()
		{
			GetSessionID = "GetSessionID";
			GetOrders = "GetOrders";
			GetSellerList = "GetSellerList";
			GetItem = "GetItem";
			ReviseInventoryStatus = "ReviseInventoryStatus";
			FetchToken = "FetchToken";
		}

		public static string GetSessionID { get; private set; }
		public static string GetOrders { get; set; }
		public static string GetSellerList { get; set; }
		public static string GetItem { get; set; }
		public static string ReviseInventoryStatus { get; set; }
		public static string FetchToken { get; set; }
	}
}