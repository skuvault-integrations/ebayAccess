using EbayAccess.Models.Credentials;
using EbayAccess.Models.CredentialsAndConfig;

namespace EbayAccessTests.TestEnvironment
{
	public class TestEmptyCredentials
	{
		public EbayUserCredentials GetEbayUserCredentials()
		{
			return new EbayUserCredentials( "AccountName: does not metter for test", "Token: does not metter for test" );
		}

		public EbayConfig GetEbayDevCredentials()
		{
			return new EbayConfig( "AppName: does not metter for test", "DevName: does not metter for test", "CertName: does not metter for test" );
		}

		public string GetEbayEndPoint()
		{
			return "EndPoint: does not metter for test";
		}
	}
}