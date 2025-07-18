using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EbayAccess;
using EbayAccess.Models.Credentials;
using EbayAccess.Models.CredentialsAndConfig;
using EbayAccess.Services;
using FluentAssertions;
using Netco.Logging;
using NSubstitute;
using NUnit.Framework;

namespace EbayAccessTests.Integration.Services
{
	[TestFixture]
	public class GetSellerListTests
	{
		[Test]
		public async Task GetActiveProductsAsync_ShouldExcludeItem_WhenVariationSkuEqualsParentSku()
		{
			// Arrange
			var testXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<GetSellerListResponse xmlns=""urn:ebay:apis:eBLBaseComponents"">
  <Timestamp>2024-01-01T00:00:00.000Z</Timestamp>
  <Ack>Success</Ack>
  <Version>1234</Version>
  <Build>TestBuild</Build>
  <PaginationResult>
    <TotalNumberOfPages>1</TotalNumberOfPages>
    <TotalNumberOfEntries>1</TotalNumberOfEntries>
  </PaginationResult>
  <HasMoreItems>false</HasMoreItems>
  <ItemArray>
    <Item>
      <ItemID>255816610579</ItemID>
      <SKU>AN4034-Black-OS</SKU>
      <Title>Sheer and Floral Applique Thigh High Stockings 2-Pack Black Womens OS Hosiery</Title>
      <Quantity>3</Quantity>
      <ListingDuration>GTC</ListingDuration>
      <SellingStatus>
        <CurrentPrice currencyID=""USD"">35.95</CurrentPrice>
        <QuantitySold>0</QuantitySold>
        <ListingStatus>Active</ListingStatus>
      </SellingStatus>
      <Variations>
        <Variation>
          <SKU>AN4034-Black-OS</SKU>
          <StartPrice currencyID=""USD"">35.95</StartPrice>
          <Quantity>3</Quantity>
          <SellingStatus>
            <QuantitySold>0</QuantitySold>
          </SellingStatus>
          <VariationSpecifics>
            <NameValueList>
              <Name>Color</Name>
              <Value>Black</Value>
            </NameValueList>
            <NameValueList>
              <Name>Hosiery Size</Name>
              <Value>OS</Value>
            </NameValueList>
          </VariationSpecifics>
          <VariationProductListingDetails>
            <UPC>619843046894</UPC>
          </VariationProductListingDetails>
        </Variation>
      </Variations>
      <ConditionID>1000</ConditionID>
    </Item>
  </ItemArray>
  <ItemsPerPage>50</ItemsPerPage>
  <PageNumber>1</PageNumber>
  <ReturnedItemCountActual>1</ReturnedItemCountActual>
</GetSellerListResponse>";

			var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(testXml));

			var mockWebRequestServices = Substitute.For<IWebRequestServices>();
			mockWebRequestServices
				.GetResponseStreamAsync( 
					Arg.Any<System.Net.HttpWebRequest>(),
					Arg.Any<Mark>(),
					Arg.Any<CancellationToken>(),
					Arg.Any<bool>()
				)
				.Returns(_ => Task.FromResult<Stream>(xmlStream));
			
			var config = new EbayConfig(
				appName: "testApp",
				devName: "testDev",
				certName: "testCert"
			);

			var ebayServiceLowLevel = new EbayServiceLowLevel(
				credentials : new EbayUserCredentials("token", "userToken", 0),
				ebayConfig: config,
				webRequestServices: mockWebRequestServices
			);

			var ebayService = new EbayService(ebayServiceLowLevel);

			// Act
			var result = (await ebayService.GetActiveProductsAsync(CancellationToken.None)).ToList();

			// Assert
			result.Should().NotBeNull();
			result.Should().NotContain(i => i.GetSku(false).Sku == "AN4034-Black-OS");
		}
	}
}