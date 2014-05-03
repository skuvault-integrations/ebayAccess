using System.IO;
using EbayAccess.Services.Parsers;
using FluentAssertions;
using NUnit.Framework;

namespace EbayAccessTests.Services.Parsers
{
	[ TestFixture ]
	public class EbayGetItemResponseParserTest
	{
		[ Test ]
		public void Parse_GetItemResponseWithSku_HookupSku()
		{
			//A
			using (var fs = new FileStream(@".\Files\GetItemResponse\EbayServiceGetItemResponseWithItemSku.xml", FileMode.Open, FileAccess.Read))
			{
				//A
				var orders = new EbayGetItemResponseParser().Parse( fs );

				//A
				orders.Item.Sku.Should().NotBeNullOrWhiteSpace( "because in source file there is item with sku" );
			}
		}

		[Test]
		public void Parse_GetItemResponseWithVariationsSku_HookupVariationSkus()
		{
			//A
			using (var fs = new FileStream(@".\Files\GetItemResponse\EbayServiceGetItemResponseWithItemVariations.xml", FileMode.Open, FileAccess.Read))
			{
				//A
				var orders = new EbayGetItemResponseParser().Parse(fs);

				//A
				orders.Item.Variations.TrueForAll(x=>!string.IsNullOrWhiteSpace(x.Sku)).Should().BeTrue("because in source file there is item with variation skus");
			}
		}
	}
}