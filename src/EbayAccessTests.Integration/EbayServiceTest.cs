using System;
using System.Collections.Generic;
using System.Linq;
using EbayAccess;
using EbayAccess.Misc;
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
		public void GetSaleRecordsNumbers_ResponseTooksTooLongTime_Exception()
		{
			//------------ Arrange
			var service = new EbayService( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );
			service.DelayForMethod[ "GetSaleRecordsNumbersAsync" ] = 25500;

			var saleNumbers = new List< string >();
			var existingSaleNumbersArray = ExistingOrdersIds.SaleNumers.ToArray();
			for( var i = 0; i < 1000; i++ )
			{
				saleNumbers.Add( existingSaleNumbersArray[ i % existingSaleNumbersArray.Length ] );
			}

			//------------ Act
			Action act = () =>
			{
				var ordersIdsAsync = service.GetSaleRecordsNumbersAsync( saleNumbers.ToArray() );
				ordersIdsAsync.Wait();
			};

			//------------ Assert
			act.ShouldThrow< Exception >();
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
		public void GetOrdersAsync_ServiceWithExistingOrdersInSpecifiedTimeRange_HookupOrders()
		{
			//------------ Arrange
			var service = new EbayService( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );

			//------------ Act
			var ordersTask = service.GetOrdersAsync( DateTime.Now.AddMonths(0).AddDays(-29), DateTime.Now.AddMonths(0) );
			ordersTask.Wait();

			//------------ Assert
			ordersTask.Result.Count().Should().BeGreaterThan( 0, "because on site there are orders" );
		}
		#endregion

		#region UpdateProducts
		[ Test ]
		public void ReviseInventoriesStatusAsync_EbayServiceWithFixedPriceProductsWithVariation_ProductsUpdated()
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
		}

		[ Test ]
		public void UpdateInventoryAsyncOld_UpdateFixedPriceItemWithVariationsAndNonvariation_NoExceptionOccuredAndResponseNotEmpty()
		{
			//------------ Arrange
			var ebayService = new EbayService( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );

			var temp1 = ebayService.GetActiveProductsAsync( true );
			temp1.Wait();
			var activeProducts = temp1.Result.ToList();

			var activeProductWithVariations1 = activeProducts.First( x => x.IsItemWithVariations() );
			var activeProductWithoutVariations1 = activeProducts.First( x => !x.IsItemWithVariations() );

			//------------ Act
			//1
			var updayeInventoryTask1 = ebayService.UpdateInventoryAsync( new List< UpdateInventoryRequest >
			{
				new UpdateInventoryRequest { ItemId = activeProductWithVariations1.ItemId.ToLongOrDefault( false ), Sku = activeProductWithVariations1.GetSku().Sku, Quantity = 100500, ConditionID = activeProductWithVariations1.ConditionId, IsVariation = activeProductWithVariations1.IsItemWithVariations() },
				new UpdateInventoryRequest { ItemId = activeProductWithoutVariations1.ItemId.ToLongOrDefault( false ), Sku = activeProductWithoutVariations1.GetSku().Sku, Quantity = 100500, ConditionID = activeProductWithoutVariations1.ConditionId, IsVariation = activeProductWithoutVariations1.IsItemWithVariations() },
			} );
			updayeInventoryTask1.Wait();
			var updateInventoryResponse1 = updayeInventoryTask1.Result.ToList();

			//2
			var temp2 = ebayService.GetActiveProductsAsync( true );
			temp2.Wait();
			activeProducts = temp2.Result.ToList();
			var activeProductWithVariations2 = activeProducts.First( x => x.ItemId == activeProductWithVariations1.ItemId && x.Sku == activeProductWithVariations1.Sku );
			var activeProductWithoutVariations2 = activeProducts.First( x => x.ItemId == activeProductWithoutVariations1.ItemId && x.Sku == activeProductWithoutVariations1.Sku );

			var updateInventoryTask2 = ebayService.UpdateInventoryAsync( new List< UpdateInventoryRequest >
			{
				new UpdateInventoryRequest { ItemId = activeProductWithVariations2.ItemId.ToLongOrDefault( false ), Sku = activeProductWithVariations2.Sku, Quantity = 0, ConditionID = activeProductWithVariations2.ConditionId, IsVariation = activeProductWithVariations2.IsItemWithVariations() },
				new UpdateInventoryRequest { ItemId = activeProductWithoutVariations2.ItemId.ToLongOrDefault( false ), Sku = activeProductWithoutVariations2.Sku, Quantity = 0, ConditionID = activeProductWithoutVariations2.ConditionId, IsVariation = activeProductWithoutVariations2.IsItemWithVariations() },
			} );
			updateInventoryTask2.Wait();
			var updateInventoryResponse2 = updateInventoryTask2.Result.ToList();

			//3
			var temp3 = ebayService.GetActiveProductsAsync( true );
			temp3.Wait();
			activeProducts = temp3.Result.ToList();
			var activeProductWithVariations3 = activeProducts.First( x => x.ItemId == activeProductWithVariations1.ItemId && x.Sku == activeProductWithVariations1.Sku );
			var activeProductWithoutVariations3 = activeProducts.First( x => x.ItemId == activeProductWithoutVariations1.ItemId && x.Sku == activeProductWithoutVariations1.Sku );

			//------------ Assert
			activeProductWithVariations3.Quantity.Should().Be( 0 );
			activeProductWithoutVariations3.Quantity.Should().Be( 0 );

			activeProductWithVariations2.Quantity.Should().Be( 100500 );
			activeProductWithoutVariations2.Quantity.Should().Be( 100500 );
		}

		[ Test ]
		[ TestCase( UpdateInventoryAlgorithm.Econom ) ]
		[ TestCase( UpdateInventoryAlgorithm.Old ) ]
		public void UpdateInventoryAsync_UpdateFixedPriceItemWithVariationsAndNonvariation_NoExceptionOccuredAndResponseNotEmpty( UpdateInventoryAlgorithm algorithm )
		{
			//------------ Arrange
			var ebayService = new EbayService( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );

			var temp1 = ebayService.GetActiveProductsAsync( true );
			temp1.Wait();
			var activeProducts = temp1.Result.ToList();

			var activeProductWithVariations1 = activeProducts.First( x => x.IsItemWithVariations() );
			var activeProductWithoutVariations1 = activeProducts.First( x => !x.IsItemWithVariations() );

			//------------ Act
			//1
			var updayeInventoryTask1 = ebayService.UpdateInventoryAsync( new List< UpdateInventoryRequest >
			{
				new UpdateInventoryRequest { ItemId = activeProductWithVariations1.ItemId.ToLongOrDefault( false ), Sku = activeProductWithVariations1.GetSku().Sku, Quantity = 100500, ConditionID = activeProductWithVariations1.ConditionId, IsVariation = activeProductWithVariations1.IsItemWithVariations() },
				new UpdateInventoryRequest { ItemId = activeProductWithoutVariations1.ItemId.ToLongOrDefault( false ), Sku = activeProductWithoutVariations1.GetSku().Sku, Quantity = 100500, ConditionID = activeProductWithoutVariations1.ConditionId, IsVariation = activeProductWithoutVariations1.IsItemWithVariations() },
			}, algorithm );
			updayeInventoryTask1.Wait();
			var updateInventoryResponse1 = updayeInventoryTask1.Result.ToList();

			//2
			var temp2 = ebayService.GetActiveProductsAsync( true );
			temp2.Wait();
			activeProducts = temp2.Result.ToList();
			var activeProductWithVariations2 = activeProducts.First( x => x.ItemId == activeProductWithVariations1.ItemId && x.GetSku().Sku == activeProductWithVariations1.GetSku().Sku );
			var activeProductWithoutVariations2 = activeProducts.First( x => x.ItemId == activeProductWithoutVariations1.ItemId && x.GetSku().Sku == activeProductWithoutVariations1.GetSku().Sku );

			var updateInventoryTask2 = ebayService.UpdateInventoryAsync( new List< UpdateInventoryRequest >
			{
				new UpdateInventoryRequest { ItemId = activeProductWithVariations2.ItemId.ToLongOrDefault( false ), Sku = activeProductWithVariations2.GetSku().Sku, Quantity = 0, ConditionID = activeProductWithVariations2.ConditionId, IsVariation = activeProductWithVariations2.IsItemWithVariations() },
				new UpdateInventoryRequest { ItemId = activeProductWithoutVariations2.ItemId.ToLongOrDefault( false ), Sku = activeProductWithoutVariations2.GetSku().Sku, Quantity = 0, ConditionID = activeProductWithoutVariations2.ConditionId, IsVariation = activeProductWithoutVariations2.IsItemWithVariations() },
			}, algorithm );
			updateInventoryTask2.Wait();
			var updateInventoryResponse2 = updateInventoryTask2.Result.ToList();

			//3
			var temp3 = ebayService.GetActiveProductsAsync( true );
			temp3.Wait();
			activeProducts = temp3.Result.ToList();
			var activeProductWithVariations3 = activeProducts.First( x => x.ItemId == activeProductWithVariations1.ItemId && x.GetSku().Sku == activeProductWithVariations1.GetSku().Sku );
			var activeProductWithoutVariations3 = activeProducts.First( x => x.ItemId == activeProductWithoutVariations1.ItemId && x.GetSku().Sku == activeProductWithoutVariations1.GetSku().Sku );

			//------------ Assert
			activeProductWithVariations3.GetQuantity().Quantity.Should().Be( 0 );
			activeProductWithoutVariations3.GetQuantity().Quantity.Should().Be( 0 );

			activeProductWithVariations2.GetQuantity().Quantity.Should().Be( 100500 );
			activeProductWithoutVariations2.GetQuantity().Quantity.Should().Be( 100500 );
		}


		[ Test ]
		[ TestCase( UpdateInventoryAlgorithm.Econom ) ]
		[ TestCase( UpdateInventoryAlgorithm.Old ) ]
		public void UpdateInventoryAsync_Update2itemsOneOfThemDoesNotExist_ExceptionOccured( UpdateInventoryAlgorithm updateInventoryAlgorithm )
		{
			//------------ Arrange
			var ebayService = new EbayService( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );

			//------------ Act
			var updateInventoryRequestExistingItem = new UpdateInventoryRequest { ItemId = ExistingProducts.FixedPrice1WithoutVariations.ItemId, Sku = ExistingProducts.FixedPrice1WithoutVariations.Sku, Quantity = 0 };
			var updateInventoryRequestNotExistingItem = new UpdateInventoryRequest { ItemId = 0000, Sku = ExistingProducts.FixedPrice1WithVariation1.Sku + "qwe", Quantity = 0 };
			var updateProductsAsyncTask1 = ebayService.UpdateInventoryAsync( new List< UpdateInventoryRequest > { updateInventoryRequestNotExistingItem, updateInventoryRequestExistingItem, }, updateInventoryAlgorithm );

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
		public void ReviseInventoriesStatusAsync_EbayServiceWithFixedPriceProductsWithoutVariations_ProductsUpdated()
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