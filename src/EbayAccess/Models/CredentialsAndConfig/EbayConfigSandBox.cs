namespace EbayAccess.Models.CredentialsAndConfig
{
	public class EbayConfigSandBox : EbayConfig
	{
		public EbayConfigSandBox( string appName, string devName, string certName )
			: base( appName, devName, certName )
		{
		}

		public EbayConfigSandBox( string appName, string devName, string certName, string ruName )
			: base( appName, devName, certName, ruName )
		{
		}

		public override string EndPoint
		{
			get { return "https://api.sandbox.ebay.com/ws/api.dll"; }
		}

		public override string SignInUrl
		{
			get { return "https://signin.sandbox.ebay.com/ws/eBayISAPI.dll"; }
		}

		public override string EndPointBulkExhange
		{
			get { return "https://webservices.sandbox.ebay.com/BulkDataExchangeService"; }
		}
	}
}