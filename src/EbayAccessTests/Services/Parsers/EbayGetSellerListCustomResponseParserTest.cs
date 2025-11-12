using System;
using System.IO;
using System.Linq;
using EbayAccess.Models.GetSellerListCustomResponse;
using EbayAccess.Services.Parsers;
using FluentAssertions;
using NUnit.Framework;

namespace EbayAccessTests.Services.Parsers
{
	[ TestFixture ]
	public class EbayGetSellerListCustomResponseParserTest
	{
		[ Test ]
		public void Parse_CorrectValuesListingStatus()
		{
			// Arrange
			var filePath = Path.Combine( AppContext.BaseDirectory, "Files", "GetSellerListCustomResponse", "EbayServiceGetSellerListCustomResponse.xml" );
			using( var fs = new FileStream( filePath, FileMode.Open, FileAccess.Read ) )
			{
				// Act
				var orders = new EbayGetSellerListCustomResponseParser().Parse( fs );

				// Assert
				orders.Items.Count( x => x.SellingStatus.ListingStatus == ListingStatusCodeTypeEnum.Active ).Should().Be( 1 );
				orders.Items.Count( x => x.SellingStatus.ListingStatus == ListingStatusCodeTypeEnum.Completed ).Should().Be( 1 );
				orders.Items.Count( x => x.SellingStatus.ListingStatus == ListingStatusCodeTypeEnum.Custom ).Should().Be( 1 );
				orders.Items.Count( x => x.SellingStatus.ListingStatus == ListingStatusCodeTypeEnum.CustomCode ).Should().Be( 1 );
				orders.Items.Count( x => x.SellingStatus.ListingStatus == ListingStatusCodeTypeEnum.Ended ).Should().Be( 1 );
			}
		}
		
		[ Test ]
		public void Parse_CorrectValuesListingStatusForVariations()
		{
			// Arrange
			var filePath = Path.Combine( AppContext.BaseDirectory, "Files", "GetSellerListCustomResponse", "EbayServiceGetSellerListCustomResponse.xml" );
			using( var fs = new FileStream( filePath, FileMode.Open, FileAccess.Read ) )
			{
				// Act
				var orders = new EbayGetSellerListCustomResponseParser().Parse( fs );

				// Assert
				orders.Items[0].Variations.Count( x => x.SellingStatus.ListingStatus == ListingStatusCodeTypeEnum.Active ).Should().Be( 1 );
				orders.Items[0].Variations.Count( x => x.SellingStatus.ListingStatus == ListingStatusCodeTypeEnum.Completed ).Should().Be( 1 );
				orders.Items[0].Variations.Count( x => x.SellingStatus.ListingStatus == ListingStatusCodeTypeEnum.Custom ).Should().Be( 1 );
				orders.Items[0].Variations.Count( x => x.SellingStatus.ListingStatus == ListingStatusCodeTypeEnum.CustomCode ).Should().Be( 1 );
				orders.Items[0].Variations.Count( x => x.SellingStatus.ListingStatus == ListingStatusCodeTypeEnum.Ended ).Should().Be( 1 );
			}
		}
	}
}