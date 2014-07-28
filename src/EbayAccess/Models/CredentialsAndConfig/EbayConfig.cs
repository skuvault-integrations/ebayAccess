using CuttingEdge.Conditions;

namespace EbayAccess.Models.CredentialsAndConfig
{
	public class EbayConfig
	{
		public string AppName { get; private set; }
		public string DevName { get; private set; }
		public string CertName { get; private set; }
		public string RuName { get; private set; }
		public int SiteId { get; private set; }

		public virtual string EndPoint { get; private set; }
		public virtual string SignInUrl { get; set; }
		public virtual string EndPointBulkExhange { get; private set; }

		public EbayConfig( string appName, string devName, string certName, int siteId = ( int )ebaySites.US )
		{
			Condition.Requires( appName, "appName" ).IsNotNullOrWhiteSpace();
			Condition.Requires( devName, "devName" ).IsNotNullOrWhiteSpace();
			Condition.Requires( certName, "certName" ).IsNotNullOrWhiteSpace();

			this.AppName = appName;
			this.DevName = devName;
			this.CertName = certName;
			this.EndPoint = "https://api.ebay.com/ws/api.dll";
			this.EndPointBulkExhange = "https://webservices.ebay.com/BulkDataExchangeService";
			this.SignInUrl = "https://signin.ebay.com/ws/eBayISAPI.dll";
			this.SiteId = siteId;
		}

		public EbayConfig( string appName, string devName, string certName, string ruName, int siteId = ( int )ebaySites.US )
			: this( appName, devName, certName, siteId )
		{
			Condition.Requires( ruName, "ruName" ).IsNotNullOrWhiteSpace();
			this.RuName = ruName;
		}
	}
}