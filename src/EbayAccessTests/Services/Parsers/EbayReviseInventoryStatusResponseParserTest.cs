using System.IO;
using EbayAccess.Models.ReviseInventoryStatusResponse;
using EbayAccess.Services.Parsers;
using FluentAssertions;
using NUnit.Framework;

namespace EbayAccessTests.Services.Parsers
{
	[ TestFixture ]
	public class EbayReviseInventoryStatusResponseParserTest
	{
		[ Test ]
		public void CorrectReviseInventoryStatusResponseWithInventory_ParseInventoryStatusResponse_HookupCorrectDeserializedObject()
		{
			//A
			using( var fs = new FileStream( @".\Files\ReviseInventoryStatusResponse.xml", FileMode.Open, FileAccess.Read ) )
			{
				//A
				var inventoryStatus = new EbayReviseInventoryStatusResponseParser().Parse( fs );

				//A
				inventoryStatus.ShouldBeEquivalentTo( new InventoryStatusResponse
				{
					ItemId = 110136942332,
					StartPrice = 1.0,
					Quantity = 101,
					Sku = string.Empty
				}, "because in source file there this data" );
			}
		}
	}
}