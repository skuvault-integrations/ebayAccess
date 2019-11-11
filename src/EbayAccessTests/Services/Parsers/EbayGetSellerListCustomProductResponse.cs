using System.IO;
using System.Linq;
using EbayAccess.Services.Parsers;
using FluentAssertions;
using NUnit.Framework;

namespace EbayAccessTests.Services.Parsers
{
	[ TestFixture ]
	public class GetSellerListCustomProductResponse
	{
		[ Test ]
		public void ParseProduct()
		{
			const string filePath = @".\Files\GetSellerListCustomProductResponse\EbayServiceGetSellerListCustomProductResponse.xml";
			using( var fs = new FileStream( filePath, FileMode.Open, FileAccess.Read ) )
			{
				var products = new EbayGetSellerListCustomProductResponseParser().Parse( fs ).Items;

				var product = products.First();
				products.Should().HaveCount( 1, "because in source file there is {0} items", 1 );
				product.Title.Should().Be( "New Ralph Lauren Polo shirt Pink Black Blue Yellow" );
				product.Sku.Should().Be( "testsku2" );
				product.ClassificationName.Should().Be( "Everything Else:Test Category For Internal Use Only Parent Level 2:Test Category For Internal Use Only Parent Level 3:Attributes7" );
				product.ImageUrl.Should().Be( "https://i12.ebayimg.com/03/i/04/8a/5f/a1_1_sbl.JPG" );
				product.LongDescription.Should().Be( "<font rwr='1' size='4' style='font-family:Arial'><font rwr=\"1\" size=\"4\" style=\"font-family:Arial\">Shirt</font></font>" );
				product.Weight.Units.Should().Be( "lbs" );
				product.Weight.GetValue().Should().Be( 2 + (decimal) 8 / 16 );
				product.Duration.Should().Be( "GTC" );
				product.Variations.Should().HaveCount( 6 );
			}
		}

		[ Test ]
		public void ParseVariations()
		{
			const string filePath = @".\Files\GetSellerListCustomProductResponse\EbayServiceGetSellerListCustomProductResponse.xml";
			using( var fs = new FileStream( filePath, FileMode.Open, FileAccess.Read ) )
			{
				var products = new EbayGetSellerListCustomProductResponseParser().Parse( fs );

				var variations = products.Items.First().Variations;
				var firstVariation = variations.First();
				variations.Should().HaveCount( 6, "because in source file there is {0} items", 6 );
				firstVariation.Title.Should().Be( "New Ralph Lauren Polo shirt Pink Black Blue Yellow: Color - Pink, Size - S" );
				firstVariation.Sku.Should().Be( "RLauren_Wom_TShirt_Pnk_S" );
			}
		}
	}
}
