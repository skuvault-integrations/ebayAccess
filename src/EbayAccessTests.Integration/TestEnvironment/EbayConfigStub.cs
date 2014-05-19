using EbayAccess.Models.CredentialsAndConfig;

namespace EbayAccessTests.Integration.TestEnvironment
{
	public class EbayConfigStub : EbayConfig
	{
		public EbayConfigStub( string appName, string devName, string certName ) : base( appName, devName, certName )
		{
		}
		public EbayConfigStub(string appName, string devName, string certName, string ruName)
			: base(appName, devName, certName, ruName)
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
	}
}