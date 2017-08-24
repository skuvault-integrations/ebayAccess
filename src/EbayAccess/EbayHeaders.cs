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

	public enum ebaySites
	{
		AU = 15, //AUD
		AT = 16, //EUR
		BENL = 123, //EUR
		BEFR = 23, //EUR
		CA = 2, //CAD USD
		CAFR = 210, //CAD USD
		FR = 71, //EUR
		DE = 77, //EUR
		HK = 201, //HKD
		IN = 203, //INR
		IE = 205, //EUR
		IT = 101, //EUR
		MY = 207, //MYR
		NL = 146, //EUR
		PH = 211, //PHP
		PL = 212, //PLN
		SG = 216, //SGD
		ES = 186, //EUR
		CH = 193, //CHF
		UK = 3, //GBP
		US = 0, //USD
	}

	public enum ebayCurrency
	{
		Unknown = 0,
		AUD = 1,
		EUR = 2,
		CAD = 3,
		HKD = 4,
		INR = 5,
		MYR = 6,
		PHP = 7,
		PLN = 8,
		SGD = 9,
		CHF = 10,
		GBP = 11,
		USD = 12,
	}

	public static class EbayHeadersValues
	{
		static EbayHeadersValues()
		{
			XEbayApiSiteid = ( ( int )ebaySites.US ).ToString();
			XEbayApiCompatibilityLevel = "879";
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
			GetSellingManagerSoldListings = "GetSellingManagerSoldListings";
			ReviseFixedPriceItem = "ReviseFixedPriceItem";
		}

		public static string GetSessionID { get; private set; }
		public static string GetOrders { get; set; }
		public static string GetSellingManagerSoldListings { get; set; }
		public static string GetSellerList { get; set; }
		public static string GetItem { get; set; }
		public static string ReviseInventoryStatus { get; set; }
		public static string ReviseFixedPriceItem { get; set; }
		public static string FetchToken { get; set; }
	}
}