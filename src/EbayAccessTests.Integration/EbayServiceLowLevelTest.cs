using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EbayAccess;
using EbayAccess.Models.ReviseInventoryStatusRequest;
using FluentAssertions;
using NUnit.Framework;

namespace EbayAccessTests.Integration
{
	[ TestFixture ]
	public class EbayServiceLowLevelTest
	{
		private TestCredentials _credentials;

		[ SetUp ]
		public void Init()
		{
			const string credentialsFilePath = @"..\..\Files\ebay_test_credentials.csv";
			const string devCredentialsFilePath = @"..\..\Files\ebay_test_devcredentials.csv";
			const string saleItemsIdsFilePath = @"..\..\Files\ebay_test_saleitemsids.csv";
			this._credentials = new TestCredentials( credentialsFilePath, devCredentialsFilePath, saleItemsIdsFilePath );
		}

		[ Test ]
		public async Task EbayServiceLowLevelWithExistingInventoryItesm_UpdateItemsQuantityAsync_QuantityUpdatedForAll()
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
		public void EbayServiceLowLevelWithExistingInventoryItesm_UpdateItemQuantity_QuantityUpdated()
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
		public void EbayServiceLowLevelWithExistingInventoryItesm_UpdateItemsQuantity_QuantityUpdatedForAll()
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
		public async Task EbayServiceLowLevelWithExistingItems_GetItemsAsync_NotEmptyItemsCollection()
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
		public void EbayServiceLowLevelWithExistingItems_GetItems_NotEmptyItemsCollection()
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
		public async Task EbayServiceLowLevelWithExistingOrders_GetOrdersAsync_HookInOrders()
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
		public void EbayServiceLowLevelWithExistingOrders_GetOrders_HookInOrders()
		{
			//A
			var ebayService = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayDevCredentials(), this._credentials.GetEbayEndPoint() );

			//A
			var orders = ebayService.GetOrders( new DateTime( 2014, 1, 1, 0, 0, 0 ), new DateTime( 2014, 1, 21, 10, 0, 0 ) );

			//A
			orders.Count().Should().Be( 2, "because on site there is 2 orders" );
		}

		[ Test ]
		public async Task EbayServiceLowLevelWithNotExistingOrders_GetOrdersAsync_EmptyOrdersCollection()
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
		public void EbayServiceLowLevelWithNotExistingOrders_GetOrders_EmptyOrdersCollection()
		{
			//A
			var ebayService = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayDevCredentials(), this._credentials.GetEbayEndPoint() );

			//A
			var orders = ebayService.GetOrders( new DateTime( 1999, 1, 1, 0, 0, 0 ),
				new DateTime( 1999, 1, 21, 10, 0, 0 ) );

			//A
			orders.Count().Should().Be( 0, "because on site there is no orders in specified time" );
		}

		[ Test ]
		public async Task EbayServiceLowLevelWithExistingInventoryItem_UpdateItemQuantityAsync_QuantityChanged()
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

		[ Test ]
		public void EbayServiceLowLevelWithExistingSaleItem_GetItem_HookapItemWithDetails()
		{
			//A
			var ebayService = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayDevCredentials(), this._credentials.GetEbayEndPoint() );
			const int qty1 = 100;
			const int qty2 = 200;
			var saleItemsIds = this._credentials.GetSaleItemsIds().ToArray();

			//A
			var inventoryStat1 = ebayService.GetItem( saleItemsIds[ 1 ].ToString() );

			//A
			inventoryStat1.ItemId.Should().Be( saleItemsIds[ 1 ].ToString(), "Code requests item with id={0}.", saleItemsIds[ 1 ].ToString() );
		}
	}
}