using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using EbayAccess;
using EbayAccess.Misc;
using EbayAccess.Models;
using EbayAccess.Models.GetSellerListCustomResponse;
using EbayAccess.Models.ReviseFixedPriceItemRequest;
using EbayAccess.Models.ReviseInventoryStatusRequest;
using EbayAccess.Services;
using EbayAccessTests.Integration.TestEnvironment;
using FluentAssertions;
using Netco.Logging;
using NUnit.Framework;

namespace EbayAccessTests.Integration
{
	// 10/19/2023: Integration tests fail since it appears that we no longer have a sandbox eBay account.
	//	We should be able to make calls to a production test seller: just do this._credentials.GetEbayConfigProduction(),
	//	and copy the ApiToken from a working channel account to ebay_test_credentials.csv.
	//TODO GUARD-3137 Find out if we're allowed to use the LinnWorks production test seller for this, then update the tests accordingly
	[ TestFixture ]
	public class EbayServiceTest: TestBase
	{
		#region GetOrders
		[ Test ]
		public void GetOrdersIds_ServiceWithExistingOrders_HookupOrdersIds()
		{
			//------------ Arrange
			var service = new EbayService( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );

			//------------ Act
			var ordersIdsAsync = service.GetOrdersIdsAsync( CancellationToken.None, Mark.Blank(), ExistingOrdersIds.OrdersIds.ToArray() );
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
			var ordersIdsAsync = service.GetOrdersIdsAsync( CancellationToken.None, Mark.Blank(), NotExistingBecauseOfCombinedOrdersIds.OrdersIds.ToArray() );
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
			var ordersTask = service.GetOrdersAsync( DateTime.Now.AddMonths( 0 ).AddDays( -29 ), DateTime.Now.AddMonths( 0 ), CancellationToken.None, Mark.Blank() );
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
			}, CancellationToken.None );
			updateProductsAsyncTask1.Wait();

			var updateProductsAsyncTask2 = ebayService.ReviseInventoriesStatusAsync( new List< InventoryStatusRequest >
			{
				ExistingProducts.FixedPrice1WithVariation1,
				ExistingProducts.FixedPrice1WithVariation2,
			}, CancellationToken.None );
			updateProductsAsyncTask2.Wait();

			//------------ Assert
			updateProductsAsyncTask1.Result.ToList().TrueForAll( x => x.Items.Count == 2 );
			updateProductsAsyncTask2.Result.ToList().TrueForAll( x => x.Items.Count == 2 );
		}

		[ Test ]
		public void UpdateInventoryAsyncEconom_UpdateMoreThanFourItemsWithoutVariationsAndNotAndRandomOrder()
		{
			//------------ Arrange
			var ebayService = new EbayService( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );
			var rand = new Random();

			//------------ Act
			var requests = new List< UpdateInventoryRequest >
			{
				new UpdateInventoryRequest { ItemId = ExistingProducts.FixedPrice1WithVariation1.ItemId, Sku = ExistingProducts.FixedPrice1WithVariation1.Sku, Quantity = rand.Next( 1, 100), IsVariation = false },
				new UpdateInventoryRequest { ItemId = ExistingProducts.FixedPrice1WithVariation2.ItemId, Sku = ExistingProducts.FixedPrice1WithVariation2.Sku, Quantity = rand.Next( 1, 100), IsVariation = false },
				new UpdateInventoryRequest { ItemId = ExistingProducts.FixedPrice1WithVariation3.ItemId, Sku = ExistingProducts.FixedPrice1WithVariation3.Sku, Quantity = rand.Next( 1, 100), IsVariation = false },
				new UpdateInventoryRequest { ItemId = ExistingProducts.FixedPrice1WithoutVariations.ItemId, Sku = ExistingProducts.FixedPrice1WithoutVariations.Sku, Quantity = rand.Next( 1, 100), IsVariation = false },
				new UpdateInventoryRequest { ItemId = ExistingProducts.FixedPrice1WithVariation4.ItemId, Sku = ExistingProducts.FixedPrice1WithVariation4.Sku, Quantity = rand.Next( 1, 100), IsVariation = false },
				new UpdateInventoryRequest { ItemId = ExistingProducts.FixedPrice1WithVariation5.ItemId, Sku = ExistingProducts.FixedPrice1WithVariation5.Sku, Quantity = rand.Next( 1, 100), IsVariation = false },
				new UpdateInventoryRequest { ItemId = ExistingProducts.FixedPrice2WithoutVariations.ItemId, Sku = ExistingProducts.FixedPrice2WithoutVariations.Sku, Quantity = rand.Next( 1, 100), IsVariation = false },
			};

			var updateProductsAsyncTask1 = ebayService.UpdateInventoryAsync( requests, CancellationToken.None, UpdateInventoryAlgorithm.Econom );
			updateProductsAsyncTask1.Wait();

			//------------ Assert
			updateProductsAsyncTask1.Result.Count().Should().Be( 3 );
		}

		[ Test ]
		[ TestCase( UpdateInventoryAlgorithm.Econom ) ]
		[ TestCase( UpdateInventoryAlgorithm.Old ) ]
		public void UpdateInventoryAsync_UpdateFixedPriceItemWithVariationsAndNonvariation_NoExceptionOccuredAndResponseNotEmpty( UpdateInventoryAlgorithm algorithm )
		{
			//------------ Arrange
			var ebayService = new EbayService( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );

			var temp1 = ebayService.GetActiveProductsAsync( CancellationToken.None, true, true );
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
			}, CancellationToken.None, algorithm );
			updayeInventoryTask1.Wait();
			var updateInventoryResponse1 = updayeInventoryTask1.Result.ToList();

			//2
			var temp2 = ebayService.GetActiveProductsAsync( CancellationToken.None, true, true );
			temp2.Wait();
			activeProducts = temp2.Result.ToList();
			var activeProductWithVariations2 = activeProducts.First( x => x.ItemId == activeProductWithVariations1.ItemId && x.GetSku().Sku == activeProductWithVariations1.GetSku().Sku );
			var activeProductWithoutVariations2 = activeProducts.First( x => x.ItemId == activeProductWithoutVariations1.ItemId && x.GetSku().Sku == activeProductWithoutVariations1.GetSku().Sku );

			var updateInventoryTask2 = ebayService.UpdateInventoryAsync( new List< UpdateInventoryRequest >
			{
				new UpdateInventoryRequest { ItemId = activeProductWithVariations2.ItemId.ToLongOrDefault( false ), Sku = activeProductWithVariations2.GetSku().Sku, Quantity = 0, ConditionID = activeProductWithVariations2.ConditionId, IsVariation = activeProductWithVariations2.IsItemWithVariations() },
				new UpdateInventoryRequest { ItemId = activeProductWithoutVariations2.ItemId.ToLongOrDefault( false ), Sku = activeProductWithoutVariations2.GetSku().Sku, Quantity = 0, ConditionID = activeProductWithoutVariations2.ConditionId, IsVariation = activeProductWithoutVariations2.IsItemWithVariations() },
			}, CancellationToken.None, algorithm );
			updateInventoryTask2.Wait();
			var updateInventoryResponse2 = updateInventoryTask2.Result.ToList();

			//3
			var temp3 = ebayService.GetActiveProductsAsync( CancellationToken.None, true, true );
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
		public void UpdateInventoryAsync_UpdateFixedPriceItemWithVariationsThroughReviseFixedPriceItemCall()
		{
			var ebayService = new EbayService( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );
			int quantity = new Random().Next( 1, 100 );
			var request = new List< ReviseFixedPriceItemRequest >() { 
				new ReviseFixedPriceItemRequest { ItemId = ExistingProducts.FixedPrice1WithVariation1.ItemId, Sku = ExistingProducts.FixedPrice1WithVariation1.Sku, Quantity = ExistingProducts.FixedPrice1WithVariation1.Quantity + quantity },
				new ReviseFixedPriceItemRequest { ItemId = ExistingProducts.FixedPrice1WithVariation2.ItemId, Sku = ExistingProducts.FixedPrice1WithVariation2.Sku, Quantity = ExistingProducts.FixedPrice1WithVariation2.Quantity + quantity },
				new ReviseFixedPriceItemRequest { ItemId = ExistingProducts.FixedPrice1WithVariation3.ItemId, Sku = ExistingProducts.FixedPrice1WithVariation3.Sku, Quantity = ExistingProducts.FixedPrice1WithVariation3.Quantity + quantity },
			};

			var reviseInventoryTask = ebayService.ReviseFixedPriceItemsAsync( request, CancellationToken.None );
			reviseInventoryTask.Wait();

			var products = ebayService.GetActiveProductsAsync( CancellationToken.None, true, true ).Result;
			
			var productWithVariation1 = products.FirstOrDefault( pr => pr.GetSku().Sku == ExistingProducts.FixedPrice1WithVariation1.Sku );
			productWithVariation1.GetQuantity().Quantity.Should().Be( request[0].Quantity );

			var productWithVariation2 = products.FirstOrDefault( pr => pr.GetSku().Sku == ExistingProducts.FixedPrice1WithVariation2.Sku );
			productWithVariation2.GetQuantity().Quantity.Should().Be( request[1].Quantity );

			var productWithVariation3 = products.FirstOrDefault( pr => pr.GetSku().Sku == ExistingProducts.FixedPrice1WithVariation3.Sku );
			productWithVariation3.GetQuantity().Quantity.Should().Be( request[2].Quantity );
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
			var updateProductsAsyncTask1 = ebayService.UpdateInventoryAsync( new List< UpdateInventoryRequest > { updateInventoryRequestNotExistingItem, updateInventoryRequestExistingItem, }, CancellationToken.None, updateInventoryAlgorithm );

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
			}, CancellationToken.None );
			updateProductsAsyncTask1.Wait();

			var updateProductsAsyncTask2 = ebayService.ReviseInventoriesStatusAsync( new List< InventoryStatusRequest >
			{
				ExistingProducts.FixedPrice1WithoutVariations,
				ExistingProducts.FixedPrice2WithoutVariations,
			}, CancellationToken.None );
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
			var inventoryStat1Task = ebayService.GetActiveProductsAsync( CancellationToken.None );
			inventoryStat1Task.Wait();
			var products = inventoryStat1Task.Result;

			//------------ Assert
			products.Count().Should().BeGreaterThan( 0, "because on site there are items" );
		}

		[ Test ]
		public void GetActiveProductPullItemsAsync_ServiceWithExistingProducts_HookupProducts()
		{
			//------------ Arrange
			var ebayService = new EbayService( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );

			//------------ Act
			var task = ebayService.GetActiveProductPullItemsAsync( CancellationToken.None );
			task.Wait();
			var products = task.Result;

			//------------ Assert
			products.Count().Should().BeGreaterThan( 0, "because on site there are items" );
		}

		[ Test ]
		public void GetActiveProductsAsync_ServiceWithExistingProductsCancellationTokenCancelled_ReturnPartOfProductsOrEmptyCollectionImmediatelly()
		{
			//------------ Arrange
			var ebayService = new EbayService( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );

			var sw1 = Stopwatch.StartNew();
			var activeProductsTask1 = ebayService.GetActiveProductsAsync( CancellationToken.None, true, false, null, Mark.Blank() );
			activeProductsTask1.Wait();
			var products1 = activeProductsTask1.Result;
			sw1.Stop();

			var cts = new CancellationTokenSource( ( int )( sw1.ElapsedMilliseconds * 0.5 ) );
			var ct = cts.Token;

			//------------ Act
			var sw2 = Stopwatch.StartNew();
			var activeProductsTask2 = ebayService.GetActiveProductsAsync( ct );
			activeProductsTask2.Wait();
			var products2 = activeProductsTask2.Result;
			sw2.Stop();

			//------------ Assert
			var products2List = products2 as IList< Item > ?? products2.ToList();
			var products1List = products1 as IList< Item > ?? products1.ToList();
			products2List.Count().Should().BeLessOrEqualTo( products1List.Count(), "because on site there are items" );
			products2List.Count().Should().BeGreaterThan( 0, "because on site there are items" );
			sw2.ElapsedMilliseconds.Should().BeLessThan( sw1.ElapsedMilliseconds, "because on site there are items" );
			Debug.WriteLine( "products1 {0}, t:{1}", products1List.Count(), sw1.Elapsed.ToString() );
			Debug.WriteLine( "products2 {0}, t:{1}", products2List.Count(), sw2.Elapsed.ToString() );
		}
		#endregion

		[ Test ]
		public void GetSaleRecordsNumbers_WhenLowTimeOutSet_ThenTimesOut()
		{
			const int reallyShortTimeout = 100;
			var service = new EbayService( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox(), 
				new WebRequestServices(), reallyShortTimeout );

			Action act = () =>
			{
				var ordersIdsAsync = service.GetOrdersIdsAsync( new CancellationToken(), Mark.CreateNew(), new [] { "123 " }.ToArray() );
				ordersIdsAsync.Wait();
			};

			act.ShouldThrow< EbayCommonException >();
		}
	}
}