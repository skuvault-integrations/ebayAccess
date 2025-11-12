using System;
using System.Collections.Generic;
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
			var filePath = Path.Combine( AppContext.BaseDirectory, "Files", "ReviseInventoryStatusResponse.xml" );
			using( var fs = new FileStream( filePath, FileMode.Open, FileAccess.Read ) )
			{
				//A
				var inventoryStatus = new EbayReviseInventoryStatusResponseParser().Parse( fs );

				//A
				inventoryStatus.Should().BeEquivalentTo( new InventoryStatusResponse
				{
					Items = new List< Item >
					{
						new Item
						{
							ItemId = 110136942332,
							StartPrice = 1.0,
							Quantity = 101,
							Sku = string.Empty
						}
					}
				} );
			}
		}
	}
}