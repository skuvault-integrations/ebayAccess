using NUnit.Framework;

namespace EbayAccessTests.Integration.TestEnvironment
{
	[ TestFixture ]
	public abstract class TestBase
	{
		protected TestCredentials _credentials;

		[ SetUp ]
		public void Init()
		{
			const string credentialsFilePath = @"..\..\Files\ebay_test_credentials.csv";
			const string devCredentialsFilePath = @"..\..\Files\ebay_test_devcredentials.csv";
			const string saleItemsIdsFilePath = @"..\..\Files\ebay_test_saleitemsids.csv";
			const string runameFilePath = @"..\..\Files\ebay_test_runame.csv";
			this._credentials = new TestCredentials(credentialsFilePath, devCredentialsFilePath, saleItemsIdsFilePath, runameFilePath);
		}
	}
}