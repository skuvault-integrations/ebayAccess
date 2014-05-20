using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EbayAccess.Misc;
using EbayAccess.Models.ReviseInventoryStatusRequest;
using EbayAccess.Services;
using EbayAccessTests.Integration.TestEnvironment;
using FluentAssertions;
using NUnit.Framework;

namespace EbayAccessTests.Integration.Services
{
	[ TestFixture ]
	public class EbayServiceLowLevelTest : TestBase
	{
		#region UpdateQuantity
		[ Test ]
		public async Task UpdateItemsQuantityAsync_EbayServiceWithNonVariationFixedPriceItems_QuantityUpdatedForAll()
		{
			//A
			var ebayService = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );

			const int itemsQty1 = 3;
			const int itemsQty2 = 4;
			var saleItemsIds = this._credentials.GetSaleItems();
			var item1 = saleItemsIds.First( x => String.Compare( x.Descr, TestItemsDescriptions.AnyExistingNonVariationItem, StringComparison.InvariantCultureIgnoreCase ) == 0 );
			var item2 = saleItemsIds.First( x => String.Compare( x.Descr, TestItemsDescriptions.AnyExistingNonVariationItem, StringComparison.InvariantCultureIgnoreCase ) == 0 && item1.Id != x.Id );

			//A
			var inventoryStat1 = ( await ebayService.ReviseInventoriesStatusAsync( new List< InventoryStatusRequest >
			{
				new InventoryStatusRequest { ItemId = item1.Id.ToLong(), Quantity = itemsQty1 },
				new InventoryStatusRequest { ItemId = item2.Id.ToLong(), Quantity = itemsQty1 }
			} ).ConfigureAwait( false ) ).ToArray();
			var inventoryStat2 = ( await ebayService.ReviseInventoriesStatusAsync( new List< InventoryStatusRequest >
			{
				new InventoryStatusRequest { ItemId = item1.Id.ToLong(), Quantity = itemsQty2 },
				new InventoryStatusRequest { ItemId = item2.Id.ToLong(), Quantity = itemsQty2 }
			} ).ConfigureAwait( false ) ).ToArray();

			//A
			( inventoryStat1[ 0 ].Quantity - inventoryStat2[ 0 ].Quantity ).Should()
				.Be( itemsQty1 - itemsQty2, String.Format( "because we set 1 qty {0}, then set 2 qty {1}", itemsQty1, itemsQty2 ) );
			( inventoryStat1[ 1 ].Quantity - inventoryStat2[ 1 ].Quantity ).Should()
				.Be( itemsQty1 - itemsQty2, String.Format( "because we set 1 qty {0}, then set 2 qty {1}", itemsQty1, itemsQty2 ) );
		}

		[ Test ]
		public void UpdateItemQuantity_EbayServiceWithExistingNonVariationFixedPriceItems_QuantityUpdated()
		{
			//A
			var ebayService = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );
			const int qty1 = 3;
			const int qty2 = 4;
			var saleItemsIds = this._credentials.GetSaleItems();
			var item1 = saleItemsIds.First( x => String.Compare( x.Descr, TestItemsDescriptions.AnyExistingNonVariationItem, StringComparison.InvariantCultureIgnoreCase ) == 0 );

			//A
			var inventoryStat1 =
				ebayService.ReviseInventoryStatus( new InventoryStatusRequest { ItemId = item1.Id.ToLong(), Quantity = qty1 } );
			var inventoryStat2 =
				ebayService.ReviseInventoryStatus( new InventoryStatusRequest { ItemId = item1.Id.ToLong(), Quantity = qty2 } );

			//A
			( inventoryStat1.Quantity - inventoryStat2.Quantity ).Should()
				.Be( qty1 - qty2, String.Format( "because we set 1 qty {0}, then set 2 qty {1}", qty1, qty2 ) );
		}

		[ Test ]
		public void UpdateItemsQuantity_EbayServiceWithNonVariationFixedPriceItems_QuantityUpdatedForAll()
		{
			//A
			var ebayService = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );

			const int itemsQty1 = 3;
			const int itemsQty2 = 4;
			var saleItemsIds = this._credentials.GetSaleItems();
			var item1 = saleItemsIds.First( x => String.Compare( x.Descr, TestItemsDescriptions.AnyExistingNonVariationItem, StringComparison.InvariantCultureIgnoreCase ) == 0 );
			var item2 = saleItemsIds.First( x => String.Compare( x.Descr, TestItemsDescriptions.AnyExistingNonVariationItem, StringComparison.InvariantCultureIgnoreCase ) == 0 && item1.Id != x.Id );

			//A
			var inventoryStat1 =
				ebayService.ReviseInventoriesStatus( new List< InventoryStatusRequest >
				{
					new InventoryStatusRequest { ItemId = item1.Id.ToLong(), Quantity = itemsQty1 },
					new InventoryStatusRequest { ItemId = item2.Id.ToLong(), Quantity = itemsQty1 }
				} ).ToArray();
			var inventoryStat2 = ebayService.ReviseInventoriesStatus( new List< InventoryStatusRequest >
			{
				new InventoryStatusRequest { ItemId = item1.Id.ToLong(), Quantity = itemsQty2 },
				new InventoryStatusRequest { ItemId = item2.Id.ToLong(), Quantity = itemsQty2 }
			} ).ToArray();

			//A
			( inventoryStat1[ 0 ].Quantity - inventoryStat2[ 0 ].Quantity ).Should()
				.Be( itemsQty1 - itemsQty2, String.Format( "because we set 1 qty {0}, then set 2 qty {1}", itemsQty1, itemsQty2 ) );
			( inventoryStat1[ 1 ].Quantity - inventoryStat2[ 1 ].Quantity ).Should()
				.Be( itemsQty1 - itemsQty2, String.Format( "because we set 1 qty {0}, then set 2 qty {1}", itemsQty1, itemsQty2 ) );
		}

		[ Test ]
		public async Task UpdateItemQuantityAsync_EbayServiceWithExistingNonVariationFixedPriceItem_QuantityChanged()
		{
			//A
			var ebayService = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );
			const int qty1 = 3;
			const int qty2 = 4;
			var saleItems = this._credentials.GetSaleItems();
			var item1 = saleItems.First( x => String.Compare( x.Descr, TestItemsDescriptions.AnyExistingNonVariationItem, StringComparison.InvariantCultureIgnoreCase ) == 0 );

			//A
			var inventoryStat1 =
				await ebayService.ReviseInventoryStatusAsync( new InventoryStatusRequest { ItemId = item1.Id.ToLong(), Quantity = qty1 } ).ConfigureAwait( false );
			var inventoryStat2 =
				await ebayService.ReviseInventoryStatusAsync( new InventoryStatusRequest { ItemId = item1.Id.ToLong(), Quantity = qty2 } ).ConfigureAwait( false );

			//A
			( inventoryStat1.Quantity - inventoryStat2.Quantity ).Should()
				.Be( qty1 - qty2, String.Format( "because we set 1 qty {0}, then set 2 qty {1}", qty1, qty2 ) );
		}
		#endregion

		#region GetItems
		[ Test ]
		public void GetItem_EbayServiceWithExistingSaleItem_HookupItem()
		{
			//A
			var ebayService = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );
			var saleItemsIds = this._credentials.GetSaleItems().First( x => String.Compare( x.Descr, TestItemsDescriptions.AnyExistingNonVariationItem, StringComparison.InvariantCultureIgnoreCase ) == 0 );

			//A
			var inventoryStat1 = ebayService.GetItem( saleItemsIds.Id );

			//A
			inventoryStat1.ItemId.Should().Be( saleItemsIds.Id, "Code requests item with id={0}.", saleItemsIds.Id );
		}

		[ Test ]
		public void GetItem_EbayServiceWithExistingSaleItemWithVariationsSku_HookupItemWithVariationsSku()
		{
			//A
			var ebayService = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );
			var saleItemsIds = this._credentials.GetSaleItems().First( x => String.Compare( x.Descr, TestItemsDescriptions.ExistingFixedPriceItemWithVariationsSku, StringComparison.InvariantCultureIgnoreCase ) == 0 );

			//A
			var inventoryStat1 = ebayService.GetItem( saleItemsIds.Id );

			//A
			inventoryStat1.Variations.TrueForAll( x => !string.IsNullOrWhiteSpace( x.Sku ) ).Should().BeTrue( "because in ebay store there is sale item with ID={0} contains variations with sku", saleItemsIds.Id );
		}
		#endregion

		#region GetOrders
		[ Test ]
		public async Task GetOrdersAsync_EbayServiceWithExistingOrders_HookupOrders()
		{
			//A
			var ebayService = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );
			var saleItems = this._credentials.GetSaleItems();
			var item1 = saleItems.First( x => String.Compare( x.Descr, TestItemsDescriptions.AnyExistingNonVariationItem, StringComparison.InvariantCultureIgnoreCase ) == 0 );
			//A
			var orders =
				await ebayService.GetOrdersAsync( item1.OrderedTime.ToDateTime().AddDays( -1 ), item1.OrderedTime.ToDateTime().AddDays( 1 ) ).ConfigureAwait( false );

			//A
			orders.Count().Should().Be( 1 );
		}

		[ Test ]
		public void GetOrders_EbayServiceWithExistingOrders_HookupOrders()
		{
			//A
			var ebayService = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );
			var saleItems = this._credentials.GetSaleItems();
			var item1 = saleItems.First( x => String.Compare( x.Descr, TestItemsDescriptions.AnyExistingNonVariationItem, StringComparison.InvariantCultureIgnoreCase ) == 0 );

			//A
			var orders = ebayService.GetOrders( item1.OrderedTime.ToDateTime().AddDays( -1 ), item1.OrderedTime.ToDateTime().AddDays( 1 ) );

			//A
			orders.Count().Should().Be( 1 );
		}

		[ Test ]
		public void GetOrders_EbayServiceWithExistingOrders_HookupOrdersWithSku()
		{
			//A
			var ebayService = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );

			//A
			var timeFrom = new DateTime( 2014, 5, 2, 18, 0, 45 );
			var timeTo = new DateTime( 2014, 5, 2, 18, 2, 45 );
			var orders = ebayService.GetOrders( timeFrom, timeTo );

			//A
			orders.ToList().First().TransactionArray.TrueForAll( x => x.Variation != null && !string.IsNullOrWhiteSpace( x.Variation.Sku ) ).Should().BeTrue( "because in ebay store order created in this time range [{0}{1}] contains items with sku", timeFrom, timeTo );
		}

		[ Test ]
		public async Task GetOrdersAsync_EbayServiceWithNotExistingOrders_EmptyOrdersCollection()
		{
			//A
			var ebayService = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );

			//A
			var orders =
				await ebayService.GetOrdersAsync( new DateTime( 1999, 1, 1, 0, 0, 0 ), new DateTime( 1999, 1, 21, 10, 0, 0 ) ).ConfigureAwait( false );

			//A
			orders.Count().Should().Be( 0, "because on site there is no orders in specified time" );
		}

		[ Test ]
		public void GetOrders_EbayServiceWithNotExistingOrders_EmptyOrdersCollection()
		{
			//A
			var ebayService = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );

			//A
			var orders = ebayService.GetOrders( new DateTime( 1999, 1, 1, 0, 0, 0 ),
				new DateTime( 1999, 1, 21, 10, 0, 0 ) );

			//A
			orders.Count().Should().Be( 0, "because on site there is no orders in specified time" );
		}
		#endregion

		#region GetSessionId
		[ Test ]
		public async Task GetSessionId_EbayServiceWithCorrectRuName_HookupSessionId()
		{
			//A
			var ebayService = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );

			//A
			var sessionId = await ebayService.GetSessionIdAsync().ConfigureAwait( false );

			//A
			sessionId.Should().NotBeNullOrWhiteSpace();
		}
		#endregion
	}
}