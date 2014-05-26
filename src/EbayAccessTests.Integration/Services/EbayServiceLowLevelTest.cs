using System;
using System.Collections.Generic;
using System.Linq;
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
		#region ReviseInventoriesStatusAsync
		[ Test ]
		public void ReviseInventoriesStatusAsync_EbayServiceWithNonVariationFixedPriceItems_QuantityUpdatedForAll()
		{
			//A
			var ebayService = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );

			//A
			var updateProductsAsyncTask1 = ebayService.ReviseInventoriesStatusAsync( new List< InventoryStatusRequest >
			{
				new InventoryStatusRequest { ItemId = ExistingProducts.FixedPrice1WithoutVariations.ItemId, Quantity = ExistingProducts.FixedPrice1WithoutVariations.Quantity + this.QtyUpdateFor },
				new InventoryStatusRequest { ItemId = ExistingProducts.FixedPrice2WithoutVariations.ItemId, Quantity = ExistingProducts.FixedPrice2WithoutVariations.Quantity + this.QtyUpdateFor },
			} );
			var updateProductsAsyncTask2 = ebayService.ReviseInventoriesStatusAsync( new List< InventoryStatusRequest >
			{
				ExistingProducts.FixedPrice1WithoutVariations,
				ExistingProducts.FixedPrice2WithoutVariations,
			} );

			//A
			( updateProductsAsyncTask1.Result.ToList().First( x => x.ItemId == ExistingProducts.FixedPrice1WithoutVariations.ItemId ).Quantity - updateProductsAsyncTask2.Result.ToList().First( x => x.ItemId == ExistingProducts.FixedPrice1WithoutVariations.ItemId ).Quantity ).Should().Be( this.QtyUpdateFor );
			( updateProductsAsyncTask1.Result.ToList().First( x => x.ItemId == ExistingProducts.FixedPrice2WithoutVariations.ItemId ).Quantity - updateProductsAsyncTask2.Result.ToList().First( x => x.ItemId == ExistingProducts.FixedPrice2WithoutVariations.ItemId ).Quantity ).Should().Be( this.QtyUpdateFor );
		}

		[ Test ]
		public void ReviseInventoriesStatus_EbayServiceWithNonVariationFixedPriceItems_QuantityUpdatedForAll()
		{
			//A
			var ebayService = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );

			//A
			var reviseInventoryStatusResponse1 = ebayService.ReviseInventoriesStatus( new List< InventoryStatusRequest >
			{
				new InventoryStatusRequest { ItemId = ExistingProducts.FixedPrice1WithoutVariations.ItemId, Quantity = ExistingProducts.FixedPrice1WithoutVariations.Quantity + this.QtyUpdateFor },
				new InventoryStatusRequest { ItemId = ExistingProducts.FixedPrice2WithoutVariations.ItemId, Quantity = ExistingProducts.FixedPrice2WithoutVariations.Quantity + this.QtyUpdateFor },
			} );
			var reviseInventoryStatusResponse2 = ebayService.ReviseInventoriesStatus( new List< InventoryStatusRequest >
			{
				ExistingProducts.FixedPrice1WithoutVariations,
				ExistingProducts.FixedPrice2WithoutVariations,
			} );

			//A
			( reviseInventoryStatusResponse1.ToList().First( x => x.ItemId == ExistingProducts.FixedPrice1WithoutVariations.ItemId ).Quantity - reviseInventoryStatusResponse2.ToList().First( x => x.ItemId == ExistingProducts.FixedPrice1WithoutVariations.ItemId ).Quantity ).Should().Be( this.QtyUpdateFor );
			( reviseInventoryStatusResponse1.ToList().First( x => x.ItemId == ExistingProducts.FixedPrice2WithoutVariations.ItemId ).Quantity - reviseInventoryStatusResponse2.ToList().First( x => x.ItemId == ExistingProducts.FixedPrice2WithoutVariations.ItemId ).Quantity ).Should().Be( this.QtyUpdateFor );
		}

		[ Test ]
		public void ReviseInventoriesStatusAsync_EbayServiceWithVariationFixedPriceItems_QuantityUpdatedForAll()
		{
			//A
			var ebayService = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );

			//A
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

			//A
			( updateProductsAsyncTask1.Result.ToList().First( x => x.ItemId == ExistingProducts.FixedPrice1WithVariation1.ItemId ).Quantity - updateProductsAsyncTask2.Result.ToList().First( x => x.ItemId == ExistingProducts.FixedPrice1WithVariation1.ItemId ).Quantity ).Should().Be( this.QtyUpdateFor );
			( updateProductsAsyncTask1.Result.ToList().First( x => x.ItemId == ExistingProducts.FixedPrice1WithVariation2.ItemId ).Quantity - updateProductsAsyncTask2.Result.ToList().First( x => x.ItemId == ExistingProducts.FixedPrice1WithVariation2.ItemId ).Quantity ).Should().Be( this.QtyUpdateFor );
		}

		[ Test ]
		public void ReviseInventoriesStatus_EbayServiceWithVariationFixedPriceItems_QuantityUpdatedForAll()
		{
			//A
			var ebayService = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );

			//A
			var reviseInventoryStatusResponse1 = ebayService.ReviseInventoriesStatus( new List< InventoryStatusRequest >
			{
				new InventoryStatusRequest { ItemId = ExistingProducts.FixedPrice1WithVariation1.ItemId, Sku = ExistingProducts.FixedPrice1WithVariation1.Sku, Quantity = ExistingProducts.FixedPrice1WithVariation1.Quantity + this.QtyUpdateFor },
				new InventoryStatusRequest { ItemId = ExistingProducts.FixedPrice1WithVariation2.ItemId, Sku = ExistingProducts.FixedPrice1WithVariation2.Sku, Quantity = ExistingProducts.FixedPrice1WithVariation2.Quantity + this.QtyUpdateFor },
			} );
			var reviseInventoryStatusResponse2 = ebayService.ReviseInventoriesStatus( new List< InventoryStatusRequest >
			{
				ExistingProducts.FixedPrice1WithVariation1,
				ExistingProducts.FixedPrice1WithVariation2,
			} );

			//A
			( reviseInventoryStatusResponse1.ToList().First( x => x.ItemId == ExistingProducts.FixedPrice1WithVariation1.ItemId ).Quantity - reviseInventoryStatusResponse2.ToList().First( x => x.ItemId == ExistingProducts.FixedPrice1WithVariation1.ItemId ).Quantity ).Should().Be( this.QtyUpdateFor );
			( reviseInventoryStatusResponse1.ToList().First( x => x.ItemId == ExistingProducts.FixedPrice1WithVariation2.ItemId ).Quantity - reviseInventoryStatusResponse2.ToList().First( x => x.ItemId == ExistingProducts.FixedPrice1WithVariation2.ItemId ).Quantity ).Should().Be( this.QtyUpdateFor );
		}
		#endregion

		#region GetItems
		[ Test ]
		public void GetItem_EbayServiceWithExistingFixedPriceVariationProduct_HookupItemId()
		{
			//A
			var ebayService = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );

			//A
			var inventoryStat1 = ebayService.GetItem( ExistingProducts.FixedPrice1WithVariation1.ItemId.ToString() );

			//A
			inventoryStat1.ItemId.Should().Be( ExistingProducts.FixedPrice1WithVariation1.ItemId.ToString() );
		}

		[ Test ]
		public void GetItem_EbayServiceWithExistingFixedPriceVariationProduct_HookupItemVariationsSku()
		{
			//A
			var ebayService = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );

			//A
			var inventoryStat1 = ebayService.GetItem( ExistingProducts.FixedPrice1WithVariation1.ItemId.ToString() );

			//A
			inventoryStat1.Variations.TrueForAll( x => !string.IsNullOrWhiteSpace( x.Sku ) ).Should().BeTrue();
		}
		#endregion

		#region GetOrders
		[ Test ]
		public void GetOrdersAsync_EbayServiceWithExistingOrders_HookupOrders()
		{
			//A
			var ebayServiceLowLevel = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );

			//A
			var ordersTask = ebayServiceLowLevel.GetOrdersAsync( ExistingOrdersCreatedInRange.DateFrom, ExistingOrdersCreatedInRange.DateTo );
			ordersTask.Wait();
			//A
			ordersTask.Result.Orders.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public void GetOrders_EbayServiceWithExistingOrders_HookupOrders()
		{
			//A
			var ebayServiceLowLevel = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );

			//A
			var orders = ebayServiceLowLevel.GetOrders( ExistingOrdersCreatedInRange.DateFrom, ExistingOrdersCreatedInRange.DateTo );

			//A
			orders.Orders.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public void GetOrdersAsync_EbayServiceWithNotExistingOrders_EmptyOrdersCollection()
		{
			//A
			var ebayServiceLowLevel = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );

			//A
			var ordersTask = ebayServiceLowLevel.GetOrdersAsync( NotExistingOrdersInRange.DateFrom, NotExistingOrdersInRange.DateTo );
			ordersTask.Wait();

			//A
			ordersTask.Result.Orders.Count().Should().Be( 0 );
		}

		[ Test ]
		public void GetOrders_EbayServiceWithNotExistingOrders_EmptyOrdersCollection()
		{
			//A
			var ebayServiceLowLevel = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );

			//A
			var orders = ebayServiceLowLevel.GetOrders( NotExistingOrdersInRange.DateFrom, NotExistingOrdersInRange.DateTo );

			//A
			orders.Orders.Count().Should().Be( 0, "because on site there is no orders in specified time" );
		}
		#endregion

		#region GetSessionId
		[ Test ]
		public void GetSessionId_EbayServiceWithCorrectRuName_HookupSessionId()
		{
			//A
			var ebayService = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );

			//A
			var sessionId = ebayService.GetSessionId();

			//A
			sessionId.Should().NotBeNullOrWhiteSpace();
		}
		#endregion

		#region Jobs
		[ Test ]
		public async Task CreateUploadJob_EbayServiceWithCorrectRuName_HookupSessionId()
		{
			//A
			var ebayService = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );

			//A
			var createuploadJobResponse = await ebayService.CreateUploadJobAsync( Guid.NewGuid() ).ConfigureAwait( false );
			var abortJobResponse = await ebayService.AbortJobAsync( createuploadJobResponse.JobId ).ConfigureAwait( false );

			//A
			createuploadJobResponse.Error.Should().BeNull();
			abortJobResponse.Error.Should().BeNull();
		}
		#endregion
	}
}