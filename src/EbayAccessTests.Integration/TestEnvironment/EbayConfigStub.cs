using EbayAccess.Models.CredentialsAndConfig;

namespace EbayAccessTests.Integration.TestEnvironment
{
	public class EbayConfigStub : EbayConfig
	{
		public EbayConfigStub( string appName, string devName, string certName ) : base( appName, devName, certName )
		{
		}

		public EbayConfigStub( string appName, string devName, string certName, string ruName )
			: base( appName, devName, certName, ruName )
		{
		}

		public override string EndPoint
		{
			get { return "https://api.sandbox.ebay.com/ws/api.dll"; }
			//get { return "https://api.ebay.com/ws/api.dll"; }
		}

		public override string SignInUrl
		{
			get { return "https://signin.sandbox.ebay.com/ws/eBayISAPI.dll"; }
			//get { return "https://signin.ebay.com/ws/eBayISAPI.dll"; }
		}

		public override string EndPointBulkExhange
		{
			get { return "https://webservices.sandbox.ebay.com/BulkDataExchangeService"; }
			//get { return "https://storage.sandbox.ebay.com/FileTransferService"; }
		}
	}
}