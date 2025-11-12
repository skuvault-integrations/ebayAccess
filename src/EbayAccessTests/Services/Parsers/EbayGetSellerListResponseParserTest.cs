using System;
using System.IO;
using EbayAccess.Services.Parsers;
using FluentAssertions;
using NUnit.Framework;

namespace EbayAccessTests.Services.Parsers
{
	[ TestFixture ]
	public class EbayGetSellerListResponseParserTest
	{
		[ Test ]
		public void FileStreamWithCorrectXml_ParseItemsResponse_HookupCorrectDeserializedObject()
		{
			//A
			var filePath = Path.Combine( AppContext.BaseDirectory, "Files", "GetSellerListResponse", "EbayServiceGetSellerListResponseWith3Items_DetailLevelAll.xml" );
			using( var fs = new FileStream( filePath, FileMode.Open, FileAccess.Read ) )
			{
				//A
				var orders = new EbayGetSellerListResponseParser().Parse( fs );

				//A
				orders.Items.Should().HaveCount( 3, "because in source file there is {0} items", 3 );
			}
		}
	}
}