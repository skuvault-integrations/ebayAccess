using System;
using System.Collections.Generic;
using System.Linq;
using EbayAccess;
using EbayAccess.Models;
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
		public void GetSaleRecordsNumbers_ServiceWithExistingOrders_HookupOrdersIds()
		{
			//------------ Arrange
			var service = new EbayService( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );

			//------------ Act
			var ordersIdsAsync = service.GetSaleRecordsNumbersAsync( ExistingOrdersIds.SaleNumers.ToArray() );
			ordersIdsAsync.Wait();

			//------------ Assert
			ordersIdsAsync.Result.Should().BeEquivalentTo( ExistingOrdersIds.SaleNumers.ToArray() );
		}

		[ Test ]
		public void GetOrdersIds_ServiceWithExistingOrders_HookupOrdersIds()
		{
			//------------ Arrange
			var service = new EbayService( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );

			//------------ Act
			var ordersIdsAsync = service.GetOrdersIdsAsync( ExistingOrdersIds.OrdersIds.ToArray() );
			ordersIdsAsync.Wait();

			//------------ Assert
			ordersIdsAsync.Result.Count().Should().Be( ExistingOrdersIds.OrdersIds.Count );
		}

		[ Test ]
		public void GetOrdersIds_ServiceWithNotExistingOrders_HookupZeroOrdersIds()
		{
			//------------ Arrange
			var service = new EbayService( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );

			//------------ Act
			var ordersIdsAsync = service.GetOrdersIdsAsync( NotExistingBecauseOfCombinedOrdersIds.OrdersIds.ToArray() );
			ordersIdsAsync.Wait();

			//------------ Assert
			ordersIdsAsync.Result.Count().Should().Be( 0 );
		}

		[ Test ]
		public void GetOrders_ServiceWithExistingOrdersInSpecifiedTimeRange_HookupOrders()
		{
			//------------ Arrange
			var service = new EbayService( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );

			//------------ Act
			var orders = service.GetOrders( ExistingOrdersModifiedInRange.DateFrom, ExistingOrdersModifiedInRange.DateTo );

			//------------ Assert
			orders.Count().Should().BeGreaterThan( 1, "because on site there are orders" );
		}

		[ Test ]
		public void GetOrdersAsync_ServiceWithExistingOrdersInSpecifiedTimeRange_HookupOrders()
		{
			//------------ Arrange
			var service = new EbayService( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );

			//------------ Act
			var dateTime = new DateTime( 2014, 7, 30, 19, 57, 00, DateTimeKind.Local );
			var dateFrom = new DateTime( 2014, 7, 5, 19, 56, 00, DateTimeKind.Local );
			var ordersTask = service.GetOrdersAsync( dateFrom, dateTime );
			ordersTask.Wait();

			//------------ Assert
			ordersTask.Result.Count().Should().BeGreaterThan( 0, "because on site there are orders" );
		}
		#endregion

		#region UpdateProducts
		[ Test ]
		public void UpdateProductsAsync_EbayServiceWithFixedPriceProductsWithVariation_ProductsUpdated()
		{
			//------------ Arrange
			var ebayService = new EbayService( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );

			//------------ Act
			var updateProductsAsyncTask1 = ebayService.ReviseInventoriesStatusAsync( new List< InventoryStatusRequest >
			{
				new InventoryStatusRequest { ItemId = ExistingProducts.FixedPrice1WithVariation1.ItemId, Sku = ExistingProducts.FixedPrice1WithVariation1.Sku, Quantity = ExistingProducts.FixedPrice1WithVariation1.Quantity + this.QtyUpdateFor },
				new InventoryStatusRequest { ItemId = ExistingProducts.FixedPrice1WithVariation2.ItemId, Sku = ExistingProducts.FixedPrice1WithVariation2.Sku, Quantity = ExistingProducts.FixedPrice1WithVariation2.Quantity + this.QtyUpdateFor },
			} );
			updateProductsAsyncTask1.Wait();

			var updateProductsAsyncTask2 = ebayService.ReviseInventoriesStatusAsync( new List< InventoryStatusRequest >
			{
				ExistingProducts.FixedPrice1WithVariation1,
				ExistingProducts.FixedPrice1WithVariation2,
			} );
			updateProductsAsyncTask2.Wait();

			//------------ Assert
			updateProductsAsyncTask1.Result.ToList().TrueForAll( x => x.Items.Count == 2 );
			updateProductsAsyncTask2.Result.ToList().TrueForAll( x => x.Items.Count == 2 );
			//(updateProductsAsyncTask1.Result.ToList().First(x => x.Items[0].ItemId == ExistingProducts.FixedPrice1WithVariation1.ItemId && x.Items[0].Sku == ExistingProducts.FixedPrice1WithVariation1.Sku).Items[0].Quantity - updateProductsAsyncTask2.Result.ToList().First(x => x.Items[0].ItemId == ExistingProducts.FixedPrice1WithVariation1.ItemId && x.Items[0].Sku == ExistingProducts.FixedPrice1WithVariation1.Sku).Items[0].Quantity).Should().Be(this.QtyUpdateFor);
			//(updateProductsAsyncTask1.Result.ToList().First(x => x.Items[0].ItemId == ExistingProducts.FixedPrice1WithVariation2.ItemId && x.Items[0].Sku == ExistingProducts.FixedPrice1WithVariation2.Sku).Items[0].Quantity - updateProductsAsyncTask2.Result.ToList().First(x => x.Items[0].ItemId == ExistingProducts.FixedPrice1WithVariation2.ItemId && x.Items[0].Sku == ExistingProducts.FixedPrice1WithVariation2.Sku).Items[0].Quantity).Should().Be(this.QtyUpdateFor);
		}

		[ Test ]
		public void UpdateInventoryAsync_UpdateFixedPriceItemWithVariationsAndNonvariation_NoExceptionOccuredAndResponseNotEmpty()
		{
			//------------ Arrange
			var ebayService = new EbayService( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );

			//------------ Act

			var resp = Enumerable.Empty< UpdateInventoryResponse >();
			Action act = () =>
			{
				var updayeInv = ebayService.UpdateInventoryAsync( new List< UpdateInventoryRequest >
				{
					new UpdateInventoryRequest { ItemId = ExistingProducts.FixedPrice1WithVariation1.ItemId, Sku = ExistingProducts.FixedPrice1WithVariation1.Sku, Quantity = 0 },
					new UpdateInventoryRequest { ItemId = ExistingProducts.FixedPrice1WithoutVariations.ItemId, Sku = ExistingProducts.FixedPrice1WithoutVariations.Sku, Quantity = 0 },
				} );
				updayeInv.Wait();
				resp = updayeInv.Result.ToList();
			};

			//------------ Assert
			act.ShouldNotThrow< Exception >();
			resp.Should().NotBeEmpty();
		}

		[ Test ]
		public void UpdateInventoryAsync_Update2itemsOneOfThemDoesNotExist_NoExceptionOccuredAndResponseContainsOnlyUpdatedItemId()
		{
			//------------ Arrange
			var ebayService = new EbayService( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );

			//------------ Act
			var updateInventoryRequestExistingItem = new UpdateInventoryRequest { ItemId = ExistingProducts.FixedPrice1WithoutVariations.ItemId, Sku = ExistingProducts.FixedPrice1WithoutVariations.Sku, Quantity = 0 };
			var updateInventoryRequestNotExistingItem = new UpdateInventoryRequest { ItemId = ExistingProducts.FixedPrice1WithVariation1.ItemId + 50000, Sku = ExistingProducts.FixedPrice1WithVariation1.Sku + "qwe", Quantity = 0 };
			var updateProductsAsyncTask1 = ebayService.UpdateInventoryAsync( new List< UpdateInventoryRequest > { updateInventoryRequestNotExistingItem, updateInventoryRequestExistingItem, } );

			var resp = Enumerable.Empty< UpdateInventoryResponse >();
			Action act = () =>
			{
				updateProductsAsyncTask1.Wait();
				resp = updateProductsAsyncTask1.Result.ToList();
			};

			//------------ Assert
			act.ShouldThrow< Exception >();
		}

		[ Test ]
		public void UpdateProductsAsync_EbayServiceWithFixedPriceProductsWithoutVariations_ProductsUpdated()
		{
			//------------ Arrange
			var ebayService = new EbayService( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );

			//------------ Act
			var updateProductsAsyncTask1 = ebayService.ReviseInventoriesStatusAsync( new List< InventoryStatusRequest >
			{
				new InventoryStatusRequest { ItemId = ExistingProducts.FixedPrice1WithoutVariations.ItemId, Quantity = ExistingProducts.FixedPrice1WithoutVariations.Quantity + this.QtyUpdateFor },
				new InventoryStatusRequest { ItemId = ExistingProducts.FixedPrice2WithoutVariations.ItemId, Quantity = ExistingProducts.FixedPrice2WithoutVariations.Quantity + this.QtyUpdateFor },
			} );
			updateProductsAsyncTask1.Wait();

			var updateProductsAsyncTask2 = ebayService.ReviseInventoriesStatusAsync( new List< InventoryStatusRequest >
			{
				ExistingProducts.FixedPrice1WithoutVariations,
				ExistingProducts.FixedPrice2WithoutVariations,
			} );
			updateProductsAsyncTask2.Wait();

			//------------ Assert
			updateProductsAsyncTask1.Result.ToList().TrueForAll( x => x.Items.Count == 2 );
			updateProductsAsyncTask2.Result.ToList().TrueForAll( x => x.Items.Count == 2 );
			//(updateProductsAsyncTask1.Result.ToList().First(x => x.Items[0].ItemId == ExistingProducts.FixedPrice1WithoutVariations.ItemId).Items[0].Quantity - updateProductsAsyncTask2.Result.ToList().First(x => x.Items[0].ItemId == ExistingProducts.FixedPrice1WithoutVariations.ItemId).Items[0].Quantity).Should().Be(this.QtyUpdateFor);
			//(updateProductsAsyncTask1.Result.ToList().First(x => x.Items[0].ItemId == ExistingProducts.FixedPrice2WithoutVariations.ItemId).Items[0].Quantity - updateProductsAsyncTask2.Result.ToList().First(x => x.Items[0].ItemId == ExistingProducts.FixedPrice2WithoutVariations.ItemId).Items[0].Quantity).Should().Be(this.QtyUpdateFor);
		}
		#endregion

		#region GetProductsDetails
		[ Test ]
		[ Ignore ]
		public void GetProductsDetailsAsync_ServiceWithExistingProducts_HookupProducts()
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

		[ Test ]
		public void GetActiveProductsAsync_ServiceWithExistingProducts_HookupProducts()
		{
			//------------ Arrange
			var ebayService = new EbayService( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );

			//------------ Act
			var inventoryStat1Task = ebayService.GetActiveProductsAsync();
			inventoryStat1Task.Wait();
			var products = inventoryStat1Task.Result;

			//------------ Assert
			products.Count().Should().BeGreaterThan( 0, "because on site there are items" );
		}
		#endregion
	}
}