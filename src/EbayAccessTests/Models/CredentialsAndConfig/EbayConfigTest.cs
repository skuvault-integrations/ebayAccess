using EbayAccess.Models.CredentialsAndConfig;
using FluentAssertions;
using NUnit.Framework;

namespace EbayAccessTests.Models.CredentialsAndConfig
{
	[TestFixture]
	public class EbayConfigTest
	{
		[ Test ]
		public void EbayConfig_ConstructorCalledWithOutParameters_EndPointInitialized()
		{
			//------------ Arrange
			const string doesNotMetter = "does not metter";

			//------------ Act
			var ebayConfig = new EbayConfig(doesNotMetter, doesNotMetter, doesNotMetter);

			//------------ Assert
			ebayConfig.EndPoint.Should().NotBeNullOrWhiteSpace( "because constructor must initialize field from config" );
		}
	}
}