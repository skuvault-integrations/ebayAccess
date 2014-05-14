using EbayAccess.Models.CredentialsAndConfig;

namespace EbayAccessTests.Integration.TestEnvironment
{
	public class EbayConfigStub : EbayConfig
	{
		public EbayConfigStub( string appName, string devName, string certName ) : base( appName, devName, certName )
		{
		}

		public override string EndPoint
		{
			get { return "https://api.sandbox.ebay.com/ws/api.dll"; }
		}
	}
}