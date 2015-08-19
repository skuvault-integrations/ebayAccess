using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
			}, new Guid().ToString() );
			var updateProductsAsyncTask2 = ebayService.ReviseInventoriesStatusAsync( new List< InventoryStatusRequest >
			{
				ExistingProducts.FixedPrice1WithoutVariations,
				ExistingProducts.FixedPrice2WithoutVariations,
			}, new Guid().ToString() );

			//A
			updateProductsAsyncTask1.Result.ToList().TrueForAll( x => x.Items.Count == 2 );
			updateProductsAsyncTask2.Result.ToList().TrueForAll( x => x.Items.Count == 2 );
			( updateProductsAsyncTask1.Result.ToList().First( x => x.Items[ 0 ].ItemId == ExistingProducts.FixedPrice1WithoutVariations.ItemId ).Items[ 0 ].Quantity - updateProductsAsyncTask2.Result.ToList().First( x => x.Items[ 0 ].ItemId == ExistingProducts.FixedPrice1WithoutVariations.ItemId ).Items[ 0 ].Quantity ).Should().Be( this.QtyUpdateFor );
			( updateProductsAsyncTask1.Result.ToList().First( x => x.Items[ 0 ].ItemId == ExistingProducts.FixedPrice2WithoutVariations.ItemId ).Items[ 0 ].Quantity - updateProductsAsyncTask2.Result.ToList().First( x => x.Items[ 0 ].ItemId == ExistingProducts.FixedPrice2WithoutVariations.ItemId ).Items[ 0 ].Quantity ).Should().Be( this.QtyUpdateFor );
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
			}, new Guid().ToString() );
			updateProductsAsyncTask1.Wait();
			var updateProductsAsyncTask2 = ebayService.ReviseInventoriesStatusAsync( new List< InventoryStatusRequest >
			{
				ExistingProducts.FixedPrice1WithVariation1,
				ExistingProducts.FixedPrice1WithVariation2,
			}, new Guid().ToString() );
			updateProductsAsyncTask2.Wait();

			//A
			( updateProductsAsyncTask1.Result.ToList().First( x => x.Items[ 0 ].ItemId == ExistingProducts.FixedPrice1WithVariation1.ItemId ).Items[ 0 ].Quantity - updateProductsAsyncTask2.Result.ToList().First( x => x.Items[ 0 ].ItemId == ExistingProducts.FixedPrice1WithVariation1.ItemId ).Items[ 0 ].Quantity ).Should().Be( this.QtyUpdateFor );
			( updateProductsAsyncTask1.Result.ToList().First( x => x.Items[ 0 ].ItemId == ExistingProducts.FixedPrice1WithVariation2.ItemId ).Items[ 0 ].Quantity - updateProductsAsyncTask2.Result.ToList().First( x => x.Items[ 0 ].ItemId == ExistingProducts.FixedPrice1WithVariation2.ItemId ).Items[ 0 ].Quantity ).Should().Be( this.QtyUpdateFor );
		}
		#endregion

		#region GetItems
		[ Test ]
		public void GetItemAsync_EbayServiceWithExistingFixedPriceVariationProduct_HookupItemId()
		{
			//A
			var ebayService = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );

			//A
			var inventoryStat1Task = ebayService.GetItemAsync( ExistingProducts.FixedPrice1WithVariation1.ItemId.ToString(), new Guid().ToString() );
			inventoryStat1Task.Wait();
			var inventoryStat1 = inventoryStat1Task.Result;
			//A
			inventoryStat1.ItemId.Should().Be( ExistingProducts.FixedPrice1WithVariation1.ItemId.ToString() );
		}

		[ Test ]
		public void GetItemAsync_EbayServiceWithExistingFixedPriceVariationProduct_HookupItemVariationsSku()
		{
			//A
			var ebayService = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );

			//A
			var inventoryStat1Task = ebayService.GetItemAsync( ExistingProducts.FixedPrice1WithVariation1.ItemId.ToString(), new Guid().ToString() );
			inventoryStat1Task.Wait();
			var inventoryStat1 = inventoryStat1Task.Result;

			//A
			inventoryStat1.Variations.TrueForAll( x => !string.IsNullOrWhiteSpace( x.Sku ) ).Should().BeTrue();
		}
		#endregion

		#region GetOrders
		[ Test ]
		public void GetOrdersAsync_GetByIdEbayServiceWithExistingOrders_HookupOrders()
		{
			//A
			var ebayServiceLowLevel = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );

			//A
			var ordersTask = ebayServiceLowLevel.GetOrdersAsync( "", ExistingOrdersIds.OrdersIds.ToArray() );
			ordersTask.Wait();
			//A
			ordersTask.Result.Orders.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public void GetOrdersAsync_EbayServiceWithExistingOrders_HookupOrders()
		{
			//A
			var ebayServiceLowLevel = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );

			//A
			var ordersTask = ebayServiceLowLevel.GetOrdersAsync( ExistingOrdersCreatedInRange.DateFrom, ExistingOrdersCreatedInRange.DateTo, GetOrdersTimeRangeEnum.CreateTime );
			ordersTask.Wait();
			//A
			ordersTask.Result.Orders.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public void GetOrdersAsync_EbayServiceWithNotExistingOrders_EmptyOrdersCollection()
		{
			//A
			var ebayServiceLowLevel = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );

			//A
			var ordersTask = ebayServiceLowLevel.GetOrdersAsync( NotExistingOrdersInRange.DateFrom, NotExistingOrdersInRange.DateTo, GetOrdersTimeRangeEnum.CreateTime );
			ordersTask.Wait();

			//A
			ordersTask.Result.Orders.Count().Should().Be( 0 );
		}
		#endregion

		#region GetSessionId
		[ Test ]
		public void GetSessionId_EbayServiceWithCorrectRuName_HookupSessionId()
		{
			//A
			var ebayService = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );

			//A
			var sessionId = ebayService.GetSessionId( new Guid().ToString() );

			//A
			sessionId.Should().NotBeNullOrWhiteSpace();
		}
		#endregion

		#region Jobs
		[ Test ]
		public async Task CreateUploadJob_EbayServiceWithCorrectRuName_HookupSessionId()
		{
			//A
			var ebayServiceLowLevel = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );

			//A
			var createuploadJobResponse = await ebayServiceLowLevel.CreateUploadJobAsync( Guid.NewGuid(), new Guid().ToString() ).ConfigureAwait( false );
			var abortJobResponse = await ebayServiceLowLevel.AbortJobAsync( createuploadJobResponse.JobId, new Guid().ToString() ).ConfigureAwait( false );

			//A
			createuploadJobResponse.Errors.Should().BeNull();
			abortJobResponse.Errors.Should().BeNull();
		}
		#endregion

		[ Test ]
		[ Ignore ]
		public void FetchToken_EbayServiceWithCorrectRuName_HookupToken()
		{
			////Attention!!! This code will regenerate youe credentials!!!

			//A
			var ebayService = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );

			//A
			var sessionId = ebayService.GetSessionId( new Guid().ToString() );
			ebayService.AuthenticateUser( sessionId );
			var userToken = ebayService.FetchToken( sessionId, new Guid().ToString() );

			//A
			sessionId.Should().NotBeNullOrWhiteSpace();
			userToken.Should().NotBeNullOrWhiteSpace();
		}
	}
}