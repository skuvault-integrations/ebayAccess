using System;
using System.Collections.Generic;
using System.Linq;
using EbayAccess;
using EbayAccess.Misc;
using EbayAccess.Models.ReviseInventoryStatusRequest;
using EbayAccessTests.Integration.TestEnvironment;
using FluentAssertions;
using NUnit.Framework;

namespace EbayAccessTests.Integration
{
	[ TestFixture ]
	public class EbayServiceTest : TestBase
	{
		#region GetOrders
		[ Test ]
		public void GetOrders_ServiceWithExistingOrdersInSpecifiedTimeRange_HookupOrders()
		{
			//------------ Arrange
			var service = new EbayService( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );
			var saleItems = this._credentials.GetSaleItems();
			var item1 = saleItems.First( x => String.Compare( x.Descr, TestItemsDescriptions.AnyExistingNonVariationItem, StringComparison.InvariantCultureIgnoreCase ) == 0 );

			//------------ Act
			var orders = service.GetOrders( item1.OrderedTime.ToDateTime().AddDays( -1 ), item1.OrderedTime.ToDateTime().AddDays( 1 ) );

			//------------ Assert
			orders.Count().Should().Be( 1, "because on site there is 1 orders" );
		}

		[ Test ]
		public void GetOrdersAsync_ServiceWithExistingOrdersInSpecifiedTimeRange_HookupOrders()
		{
			//------------ Arrange
			var service = new EbayService( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );
			var saleItems = this._credentials.GetSaleItems();
			var item1 = saleItems.First( x => String.Compare( x.Descr, TestItemsDescriptions.AnyExistingNonVariationItem, StringComparison.InvariantCultureIgnoreCase ) == 0 );

			//------------ Act
			var ordersTask = service.GetOrdersAsync( item1.OrderedTime.ToDateTime().AddDays( -1 ), item1.OrderedTime.ToDateTime().AddDays( 1 ) );
			ordersTask.Wait();

			//------------ Assert
			ordersTask.Result.Count().Should().Be( 1, "because on site there is 1 orders" );
		}
		#endregion

		#region UpdateProducts
		[ Test ]
		public void UpdateProductsAsync_EbayServiceWithFixedPriceProductsWithVariation_ProductsUpdated()
		{
			//------------ Arrange
			var ebayService = new EbayService( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );
			const int qty1 = 3;
			const int qty2 = 4;
			var saleItemsIds = this._credentials.GetSaleItems();
			var item1 = saleItemsIds.First( x => String.Compare( x.Descr, TestItemsDescriptions.ExistingFixedPriceItemWithVariationsSku, StringComparison.InvariantCultureIgnoreCase ) == 0 );
			var item2 = saleItemsIds.First( x => String.Compare( x.Descr, TestItemsDescriptions.ExistingFixedPriceItemWithVariationsSku, StringComparison.InvariantCultureIgnoreCase ) == 0 && item1.Sku != x.Sku );

			//------------ Act
			var inventoryStat1Task = ebayService.UpdateProductsAsync( new List< InventoryStatusRequest >
			{
				new InventoryStatusRequest { ItemId = item1.Id.ToLong(), Sku = item1.Sku, Quantity = qty1 },
				new InventoryStatusRequest { ItemId = item2.Id.ToLong(), Sku = item2.Sku, Quantity = qty1 }
			} );
			inventoryStat1Task.Wait();

			var inventoryStat2Task = ebayService.UpdateProductsAsync( new List< InventoryStatusRequest >
			{
				new InventoryStatusRequest { ItemId = item1.Id.ToLong(), Sku = item1.Sku, Quantity = qty2 },
				new InventoryStatusRequest { ItemId = item2.Id.ToLong(), Sku = item2.Sku, Quantity = qty2 }
			} );
			inventoryStat2Task.Wait();

			//------------ Assert
			( inventoryStat2Task.Result.ToList()[ 0 ].Quantity - inventoryStat1Task.Result.ToList()[ 0 ].Quantity ).Should()
				.Be( qty2 - qty1, String.Format( "because we set 1 qty {0}, then set 2 qty {1}", qty1, qty2 ) );
			( inventoryStat2Task.Result.ToList()[ 1 ].Quantity - inventoryStat1Task.Result.ToList()[ 1 ].Quantity ).Should()
				.Be( qty2 - qty1, String.Format( "because we set 1 qty {0}, then set 2 qty {1}", qty1, qty2 ) );
		}
		#endregion

		#region GetProductsDetails
		[ Test ]
		public void GetProductsDetails()
		{
			//------------ Arrange
			var ebayService = new EbayService( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );

			//------------ Act
			var inventoryStat1Task = ebayService.GetProductsDetailsAsync();
			inventoryStat1Task.Wait();
			var products = inventoryStat1Task.Result;

			//------------ Assert
			products.Count().Should().BeGreaterThan( 0, "because on site there are items" );
		}
		#endregion
	}
}