using System;
using System.IO;
using EbayAccess.Services.Parsers;
using FluentAssertions;
using NUnit.Framework;

namespace EbayAccessTests.Services.Parsers
{
	[ TestFixture ]
	public class EbayPaginationResultResponseParserTest
	{
		[ Test ]
		public void ParsePaginationResultResponse_ResultContainsMultiplePages_AllPagesHandled()
		{
			//A
			var filePath = Path.Combine( AppContext.BaseDirectory, "Files", "GetSellerListResponse", "EbayServiceGetSellerListResponseWith1PageOf4Contains1Item.xml" );
			using( var fs = new FileStream( filePath, FileMode.Open, FileAccess.Read ) )
			{
				const int itemCount = 4;
				const int pagesCount = 4;

				//A
				var orders = new EbayPaginationResultResponseParser().Parse( fs );

				//A
				orders.TotalNumberOfEntries.Should().Be( itemCount, "because source file contains record about {0} items", itemCount );
				orders.TotalNumberOfPages.Should().Be( pagesCount, "because source file contains record about {0} pages", pagesCount );
			}
		}
	}
}