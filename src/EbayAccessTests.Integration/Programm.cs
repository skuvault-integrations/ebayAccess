using System;
using System.Collections.Generic;
using System.Linq;
using EbayAccess;
using EbayAccess.Models.ReviseInventoryStatusRequest;
using EbayAccessTests.Integration.TestEnvironment;
using FluentAssertions;
using NUnit.Framework;

namespace EbayAccessTests.Integration
{
	[ TestFixture ]
	public class Programm : TestBase
	{
		[ Test ]
		public void EbayServiceGetOrders_EbayServiceWithProducts_HookupProducts()
		{
			//------------ Arrange
			var ebayFactory = new EbayFactory( this._credentials.GetEbayDevCredentials() );
			var ebayService = ebayFactory.CreateService( this._credentials.GetEbayUserCredentials() );

			//------------ Act
			var orders = ebayService.GetOrders( new DateTime( 2014, 1, 1, 0, 0, 0 ), new DateTime( 2014, 1, 21, 10, 0, 0 ) );

			//------------ Assert
			orders.Count().Should().Be( 2, "because on site there is 2 orders" );
		}

		[ Test ]
		public void GetProductsDetailsAsync_EbayServiceWithProductsVariationsSku_HookupProductsVariationsSku()
		{
			//------------ Arrange
			var ebayFactory = new EbayFactory( this._credentials.GetEbayDevCredentials() );
			var ebayService = ebayFactory.CreateService( this._credentials.GetEbayUserCredentials() );

			//------------ Act
			var ordersTask = ebayService.GetProductsDetailsAsync( new DateTime( 2014, 5, 2, 0, 0, 0 ), new DateTime( 2014, 5, 3, 10, 0, 0 ) );
			ordersTask.Wait();
			var orders = ordersTask.Result;

			//------------ Assert
			orders.First().Variations.TrueForAll( x => !string.IsNullOrWhiteSpace( x.Sku ) ).Should().BeTrue( "because on site there is 2 orders" );
		}

		[ Test ]
		public void GetProductsDetailsAsync_EbayServiceWithProductsSku_HookupProductsSku()
		{
			//------------ Arrange
			var ebayFactory = new EbayFactory( this._credentials.GetEbayDevCredentials() );
			var ebayService = ebayFactory.CreateService( this._credentials.GetEbayUserCredentials() );

			//------------ Act
			var timeFromStart = new DateTime( 2014, 4, 18, 0, 0, 0 );
			var timeFromTo = new DateTime( 2014, 4, 19, 10, 0, 0 );
			var ordersTask = ebayService.GetProductsDetailsAsync( timeFromStart, timeFromTo );
			ordersTask.Wait();
			var orders = ordersTask.Result;

			//------------ Assert
			orders.ToList().TrueForAll( x => !string.IsNullOrWhiteSpace( x.Sku ) ).Should().BeTrue( "because on site there item with sku specified in time range {0}-{1}", timeFromStart, timeFromTo );
		}

		[ Test ]
		public void GetOrders_EbayServiceWithOrdersVariationsSku_HookupProductsVariationsSku()
		{
			//------------ Arrange
			var ebayFactory = new EbayFactory( this._credentials.GetEbayDevCredentials() );
			var ebayService = ebayFactory.CreateService( this._credentials.GetEbayUserCredentials() );

			//------------ Act
			var ordersTask = ebayService.GetOrdersAsync( new DateTime( 2014, 5, 2, 18, 0, 0 ), new DateTime( 2014, 5, 3, 10, 0, 0 ) );
			ordersTask.Wait();
			var orders = ordersTask.Result;

			//------------ Assert
			orders.First().TransactionArray.TrueForAll( x => x.Variation != null && !string.IsNullOrWhiteSpace( x.Variation.Sku ) ).Should().BeTrue( "because on site there is 2 orders" );
		}

		[ Test ]
		public void GetOrders_EbayServiceWithOrdersSku_HookupProductsSku()
		{
			//------------ Arrange
			var ebayFactory = new EbayFactory( this._credentials.GetEbayDevCredentials() );
			var ebayService = ebayFactory.CreateService( this._credentials.GetEbayUserCredentials() );

			//------------ Act
			var dateFrom = new DateTime( 2014, 4, 28, 19, 30, 0 );
			var dateTo = new DateTime( 2014, 4, 28, 19, 40, 0 );
			var ordersTask = ebayService.GetOrdersAsync( dateFrom, dateTo );
			ordersTask.Wait();
			var orders = ordersTask.Result;

			//------------ Assert
			orders.First().TransactionArray.TrueForAll( x => !string.IsNullOrWhiteSpace( x.Item.Sku ) ).Should().BeTrue( "because on site there is 1 order with sku in time range {0}-{1}", dateFrom, dateTo );
		}

		[ Test ]
		public void UpdateProductsAsync_EbayServiceWithOrdersVariationsSku_ProductsUpdated()
		{
			//------------ Arrange
			var ebayFactory = new EbayFactory( this._credentials.GetEbayDevCredentials() );
			var ebayService = ebayFactory.CreateService( this._credentials.GetEbayUserCredentials() );

			const int itemsQty1 = 499;
			const int itemsQty2 = 299;

			//------------ Act
			var updateProductsAsyncTask = ebayService.UpdateProductsAsync( new List< InventoryStatusRequest >
			{
				new InventoryStatusRequest { ItemId = 110142400319, Sku = "testSku501_B", Quantity = itemsQty1 },
			} );
			updateProductsAsyncTask.Wait();
			var inventoryStat1 = updateProductsAsyncTask.Result.ToArray();

			var updateProductsAsyncTask2 = ebayService.UpdateProductsAsync( new List< InventoryStatusRequest >
			{
				new InventoryStatusRequest { ItemId = 110142400319, Sku = "testSku501_B", Quantity = itemsQty2 },
			} );
			updateProductsAsyncTask2.Wait();
			var inventoryStat2 = updateProductsAsyncTask2.Result.ToArray();

			//------------ Assert
			( inventoryStat1[ 0 ].Quantity - inventoryStat2[ 0 ].Quantity ).Should()
				.Be( itemsQty1 - itemsQty2, String.Format( "because we set 1 qty {0}, then set 2 qty {1}", itemsQty1, itemsQty2 ) );
		}

		[ Test ]
		public void UpdateProductsAsync_EbayServiceWithOrdersSku_ProductsUpdated()
		{
			//------------ Arrange
			var ebayFactory = new EbayFactory( this._credentials.GetEbayDevCredentials() );
			var ebayService = ebayFactory.CreateService( this._credentials.GetEbayUserCredentials() );

			const int itemsQty1 = 405;
			const int itemsQty2 = 205;

			//------------ Act
			var updateProductsAsyncTask = ebayService.UpdateProductsAsync( new List< InventoryStatusRequest >
			{
				new InventoryStatusRequest { ItemId = 110141553531, Sku = "testSku11014", Quantity = itemsQty1 },
			} );
			updateProductsAsyncTask.Wait();
			var inventoryStat1 = updateProductsAsyncTask.Result.ToArray();

			var updateProductsAsyncTask2 = ebayService.UpdateProductsAsync( new List< InventoryStatusRequest >
			{
				new InventoryStatusRequest { ItemId = 110141553531, Sku = "testSku11014", Quantity = itemsQty2 },
			} );
			updateProductsAsyncTask2.Wait();
			var inventoryStat2 = updateProductsAsyncTask2.Result.ToArray();

			//------------ Assert
			( inventoryStat1[ 0 ].Quantity - inventoryStat2[ 0 ].Quantity ).Should()
				.Be( itemsQty1 - itemsQty2, String.Format( "because we set 1 qty {0}, then set 2 qty {1}", itemsQty1, itemsQty2 ) );
		}
	}
}