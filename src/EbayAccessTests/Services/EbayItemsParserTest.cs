using System.IO;
using EbayAccess.Services;
using EbayAccess.Services.Parsers;
using FluentAssertions;
using NUnit.Framework;

namespace EbayAccessTests.Services
{
	[ TestFixture ]
	public class EbayItemsParserTest
	{
		[ Test ]
		public void FileStreamWithCorrectXml_ParseItemsResponse_HookupCorrectDeserializedObject()
		{
			//A
			using(
				var fs = new FileStream( @".\Files\EbayServiceGetSellerListResponseWith3Items_DetailLevelAll.xml", FileMode.Open,
					FileAccess.Read ) )
			{
				//A
				var orders = EbayItemsParser.ParseGetSallerListResponse( fs );

				//A
				orders.Should().HaveCount( 3, "because in source file there is {0} items", 3 );
			}
		}
	}
}