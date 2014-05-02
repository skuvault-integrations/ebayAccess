using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EbayAccess;
using EbayAccess.Models.ReviseInventoryStatusRequest;
using EbayAccessTests.Integration.TestEnvironment;
using FluentAssertions;
using NUnit.Framework;

namespace EbayAccessTests.Integration
{
	[ TestFixture ]
	public class EbayServiceLowLevelTest : TestBase
	{
		#region UpdateQuantity
		[ Test ]
		public async Task UpdateItemsQuantityAsync_EbayServiceWithExistingtems_QuantityUpdatedForAll()
		{
			//A
			var ebayService = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayDevCredentials(), this._credentials.GetEbayEndPoint() );

			const int itemsQty1 = 100;
			const int itemsQty2 = 200;
			var saleItemsIds = this._credentials.GetSaleItemsIds().ToArray();

			//A
			var inventoryStat1 = ( await ebayService.ReviseInventoriesStatusAsync( new List< InventoryStatusRequest >
			{
				new InventoryStatusRequest { ItemId = saleItemsIds[ 0 ], Quantity = itemsQty1 },
				new InventoryStatusRequest { ItemId = saleItemsIds[ 1 ], Quantity = itemsQty1 }
			} ).ConfigureAwait( false ) ).ToArray();
			var inventoryStat2 = ( await ebayService.ReviseInventoriesStatusAsync( new List< InventoryStatusRequest >
			{
				new InventoryStatusRequest { ItemId = saleItemsIds[ 0 ], Quantity = itemsQty2 },
				new InventoryStatusRequest { ItemId = saleItemsIds[ 1 ], Quantity = itemsQty2 }
			} ).ConfigureAwait( false ) ).ToArray();

			//A
			( inventoryStat1[ 0 ].Quantity - inventoryStat2[ 0 ].Quantity ).Should()
				.Be( itemsQty1 - itemsQty2, String.Format( "because we set 1 qty {0}, then set 2 qty {1}", itemsQty1, itemsQty2 ) );
			( inventoryStat1[ 1 ].Quantity - inventoryStat2[ 1 ].Quantity ).Should()
				.Be( itemsQty1 - itemsQty2, String.Format( "because we set 1 qty {0}, then set 2 qty {1}", itemsQty1, itemsQty2 ) );
		}

		[ Test ]
		public void UpdateItemQuantity_EbayServiceWithExistingItems_QuantityUpdated()
		{
			//A
			var ebayService = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayDevCredentials(), this._credentials.GetEbayEndPoint() );
			const int qty1 = 100;
			const int qty2 = 200;
			var saleItemsIds = this._credentials.GetSaleItemsIds().ToArray();

			//A
			var inventoryStat1 =
				ebayService.ReviseInventoryStatus( new InventoryStatusRequest { ItemId = saleItemsIds[ 1 ], Quantity = qty1 } );
			var inventoryStat2 =
				ebayService.ReviseInventoryStatus( new InventoryStatusRequest { ItemId = saleItemsIds[ 1 ], Quantity = qty2 } );

			//A
			( inventoryStat1.Quantity - inventoryStat2.Quantity ).Should()
				.Be( qty1 - qty2, String.Format( "because we set 1 qty {0}, then set 2 qty {1}", qty1, qty2 ) );
		}

		[ Test ]
		public void UpdateItemsQuantity_EbayServiceWithExistingItems_QuantityUpdatedForAll()
		{
			//A
			var ebayService = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayDevCredentials(), this._credentials.GetEbayEndPoint() );

			const int itemsQty1 = 100;
			const int itemsQty2 = 200;
			var saleItemsIds = this._credentials.GetSaleItemsIds().ToArray();

			//A
			var inventoryStat1 =
				ebayService.ReviseInventoriesStatus( new List< InventoryStatusRequest >
				{
					new InventoryStatusRequest { ItemId = saleItemsIds[ 0 ], Quantity = itemsQty1 },
					new InventoryStatusRequest { ItemId = saleItemsIds[ 1 ], Quantity = itemsQty1 }
				} ).ToArray();
			var inventoryStat2 = ebayService.ReviseInventoriesStatus( new List< InventoryStatusRequest >
			{
				new InventoryStatusRequest { ItemId = saleItemsIds[ 0 ], Quantity = itemsQty2 },
				new InventoryStatusRequest { ItemId = saleItemsIds[ 1 ], Quantity = itemsQty2 }
			} ).ToArray();

			//A
			( inventoryStat1[ 0 ].Quantity - inventoryStat2[ 0 ].Quantity ).Should()
				.Be( itemsQty1 - itemsQty2, String.Format( "because we set 1 qty {0}, then set 2 qty {1}", itemsQty1, itemsQty2 ) );
			( inventoryStat1[ 1 ].Quantity - inventoryStat2[ 1 ].Quantity ).Should()
				.Be( itemsQty1 - itemsQty2, String.Format( "because we set 1 qty {0}, then set 2 qty {1}", itemsQty1, itemsQty2 ) );
		}

		[ Test ]
		public async Task UpdateItemQuantityAsync_EbayServiceWithExistingItem_QuantityChanged()
		{
			//A
			var ebayService = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayDevCredentials(), this._credentials.GetEbayEndPoint() );
			const int qty1 = 100;
			const int qty2 = 200;
			var saleItemsIds = this._credentials.GetSaleItemsIds().ToArray();

			//A
			var inventoryStat1 =
				await ebayService.ReviseInventoryStatusAsync( new InventoryStatusRequest { ItemId = saleItemsIds[ 1 ], Quantity = qty1 } ).ConfigureAwait( false );
			var inventoryStat2 =
				await ebayService.ReviseInventoryStatusAsync( new InventoryStatusRequest { ItemId = saleItemsIds[ 1 ], Quantity = qty2 } ).ConfigureAwait( false );

			//A
			( inventoryStat1.Quantity - inventoryStat2.Quantity ).Should()
				.Be( qty1 - qty2, String.Format( "because we set 1 qty {0}, then set 2 qty {1}", qty1, qty2 ) );
		}
		#endregion

		#region GetItems
		[ Test ]
		public async Task GetItemsAsync_EbayServiceWithExistingItems_NotEmptyItemsCollection()
		{
			//A
			var ebayService = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayDevCredentials(), this._credentials.GetEbayEndPoint() );

			//A
			var orders =
				await ebayService.GetItemsAsync( new DateTime( 2014, 1, 1, 0, 0, 0 ), new DateTime( 2014, 1, 28, 10, 0, 0 ), TimeRangeEnum.StartTime ).ConfigureAwait( false );

			//A
			orders.Count().Should().BeGreaterThan( 0, "because on site there are items started in specified time" );
		}

		[ Test ]
		public void GetItems_EbayServiceWithExistingItems_NotEmptyItemsCollection()
		{
			//A
			var ebayService = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayDevCredentials(), this._credentials.GetEbayEndPoint() );

			//A
			var orders = ebayService.GetItems( new DateTime( 2014, 1, 1, 0, 0, 0 ),
				new DateTime( 2014, 1, 28, 10, 0, 0 ), TimeRangeEnum.StartTime );

			//A
			orders.Count().Should().BeGreaterThan( 0, "because on site there are items started in specified time" );
		}

		[ Test ]
		public void GetItem_EbayServiceWithExistingSaleItem_HookupItemWithDetails()
		{
			//A
			var ebayService = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayDevCredentials(), this._credentials.GetEbayEndPoint() );
			var saleItemsIds = this._credentials.GetSaleItemsIds().ToArray();

			//A
			var inventoryStat1 = ebayService.GetItem( saleItemsIds[ 1 ].ToString() );

			//A
			inventoryStat1.ItemId.Should().Be( saleItemsIds[ 1 ].ToString(), "Code requests item with id={0}.", saleItemsIds[ 1 ].ToString() );
		}

		[ Test ]
		public void GetItem_EbayServiceWithExistingSaleItemWithSku_HookupItemWithSku()
		{
			//A
			var ebayService = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayDevCredentials(), this._credentials.GetEbayEndPoint() );
			var saleItemsIds = this._credentials.GetSaleItemsIds().ToArray();

			//A
			var inventoryStat1 = ebayService.GetItem( saleItemsIds[ 2 ].ToString() );

			//A
			inventoryStat1.Variations.TrueForAll( x => !string.IsNullOrWhiteSpace( x.Sku ) ).Should().BeTrue( "because in ebay store sale item with ID={0} contains variations with sku", saleItemsIds[ 2 ].ToString() );
		}
		#endregion

		#region GetOrders
		[ Test ]
		public async Task GetOrdersAsync_EbayServiceWithExistingOrders_HookupOrders()
		{
			//A

			var ebayService = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayDevCredentials(), this._credentials.GetEbayEndPoint() );

			//A
			var orders =
				await ebayService.GetOrdersAsync( new DateTime( 2014, 1, 1, 0, 0, 0 ), new DateTime( 2014, 1, 21, 10, 0, 0 ) ).ConfigureAwait( false );

			//A
			orders.Count().Should().Be( 2, "because on site there is 2 orders" );
		}

		[ Test ]
		public void GetOrders_EbayServiceWithExistingOrders_HookupOrders()
		{
			//A
			var ebayService = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayDevCredentials(), this._credentials.GetEbayEndPoint() );

			//A
			var orders = ebayService.GetOrders( new DateTime( 2014, 1, 1, 0, 0, 0 ), new DateTime( 2014, 1, 21, 10, 0, 0 ) );

			//A
			orders.Count().Should().Be( 2, "because on site there is 2 orders" );
		}

		[ Test ]
		public void GetOrders_EbayServiceWithExistingOrders_HookupOrdersWithSku()
		{
			//A
			var ebayService = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayDevCredentials(), this._credentials.GetEbayEndPoint() );

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
			var ebayService = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayDevCredentials(), this._credentials.GetEbayEndPoint() );

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
			var ebayService = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayDevCredentials(), this._credentials.GetEbayEndPoint() );

			//A
			var orders = ebayService.GetOrders( new DateTime( 1999, 1, 1, 0, 0, 0 ),
				new DateTime( 1999, 1, 21, 10, 0, 0 ) );

			//A
			orders.Count().Should().Be( 0, "because on site there is no orders in specified time" );
		}
		#endregion
	}
}