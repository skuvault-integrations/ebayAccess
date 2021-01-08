using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using EbayAccess;
using EbayAccess.Misc;
using EbayAccess.Models;
using EbayAccess.Models.ReviseInventoryStatusRequest;
using EbayAccessTests.Integration.TestEnvironment;
using FluentAssertions;
using LINQtoCSV;
using NUnit.Framework;

namespace EbayAccessTests.Integration
{
	[ TestFixture ]
	public class Program : TestBase
	{
		[ Test ]
		public void GetOrders_EbayServiceWithMultipleOrders_HookupOrders()
		{
			//------------ Arrange
			var ebayFactory = new EbayFactory( this._credentials.GetEbayConfigSandbox() );
			var ebayService = ebayFactory.CreateService( this._credentials.GetEbayUserCredentials() );

			//------------ Act
			var ordersTask = ebayService.GetOrdersAsync( ExistingOrdersModifiedInRange.DateFrom, ExistingOrdersModifiedInRange.DateTo, CancellationToken.None );
			ordersTask.Wait();
			var orders = ordersTask.Result;

			//------------ Assert
			orders.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public void GetProductsAsyncInTimeRange_EbayServiceWithProductsVariationsSku_HookupProducts()
		{
			//------------ Arrange
			var ebayFactory = new EbayFactory( this._credentials.GetEbayConfigSandbox() );
			var ebayService = ebayFactory.CreateService( this._credentials.GetEbayUserCredentials() );

			//------------ Act
			var productsAsyncTask = ebayService.GetProductsByEndDateAsync( ExistsProductsCreatedInRange.DateFrom, ExistsProductsCreatedInRange.DateTo );
			productsAsyncTask.Wait();
			var products = productsAsyncTask.Result;

			//------------ Assert
			products.Count().Should().BeGreaterThan( 0 );
		}

		#region UpdateProductsAsync
		[ Test ]
		public void UpdateProductsAsync_EbayServiceWithFixedPriceProductVariations_ProductsUpdated()
		{
			//------------ Arrange
			var ebayFactory = new EbayFactory( this._credentials.GetEbayConfigSandbox() );
			var ebayService = ebayFactory.CreateService( this._credentials.GetEbayUserCredentials() );

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

		[ Test ]
		public void UpdateProductsAsync_EbayServiceWithFixedPriceProductNotVariations_ProductsUpdated()
		{
			//------------ Arrange
			var ebayFactory = new EbayFactory( this._credentials.GetEbayConfigSandbox() );
			var ebayService = ebayFactory.CreateService( this._credentials.GetEbayUserCredentials() );

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
		public void GetProductsDetailsAsyncInTimeRange_EbayServiceWithProducts_HookupProducts()
		{
			//------------ Arrange
			var ebayFactory = new EbayFactory( this._credentials.GetEbayConfigSandbox() );
			var ebayService = ebayFactory.CreateService( this._credentials.GetEbayUserCredentials() );

			//------------ Act
			var productsDetailsAsyncTask = ebayService.GetProductsDetailsAsync( ExistsProductsCreatedInRange.DateFrom, ExistsProductsCreatedInRange.DateTo );
			productsDetailsAsyncTask.Wait();
			var products = productsDetailsAsyncTask.Result;

			//------------ Assert
			products.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		[ Ignore ]
		public void GetProductsDetailsAsync_EbayServiceWithProducts_HookupProductsThatEitherSingleVariationEitherNonVariation()
		{
			//------------ Arrange
			var ebayFactory = new EbayFactory( this._credentials.GetEbayConfigSandbox() );
			var ebayService = ebayFactory.CreateService( this._credentials.GetEbayUserCredentials() );

			//------------ Act
			var productsDetailsAsyncTask = ebayService.GetProductsDetailsAsync();
			productsDetailsAsyncTask.Wait();
			var products = productsDetailsAsyncTask.Result;

			//------------ Assert
			products.Count().Should().BeGreaterThan( 0 );
			products.ToList().TrueForAll( x => !x.IsItemWithVariations() || !x.HaveMultiVariations() ).Should().BeTrue( "because before returned, items was devided by variations skus" );
		}
		#endregion

		[ Test ]
		[ Ignore ]
		public void GetUserToken_EbaySandBoxServiceWithCorrectRuName_HookupToken()
		{
			////Attention!!! This code will regenerate youe credentials!!!

			//------------ Arrange
			var ebayFactory = new EbayFactory( this._credentials.GetEbayConfigSandbox() );
			var ebayService = ebayFactory.CreateService();

			//------------ Act
			var token = ebayService.GetUserToken();

			//------------ Assert
			token.Should().NotBeNullOrEmpty();
			var cc = new CsvContext();
			cc.Write( new List< TestCredentials.FlatUserCredentialsCsvLine > { new TestCredentials.FlatUserCredentialsCsvLine { AccountName = "", Token = token } }, this.FilesEbayTestCredentialsCsv );
		}

		[ Test ]
		[ Ignore ]
		public void GetUserToken_EbayProductionServiceWithCorrectRuName_HookupToken()
		{
			////Attention!!! This code will regenerate youe credentials!!!
			//------------ Arrange
			var ebayFactory = new EbayFactory( this._credentials.GetEbayConfigSandbox() );
			var ebayService = ebayFactory.CreateService();

			//------------ Act
			var token = ebayService.GetUserToken();

			//------------ Assert
			token.Should().NotBeNullOrEmpty();
			var cc = new CsvContext();
			cc.Write( new List< TestCredentials.FlatUserCredentialsCsvLine > { new TestCredentials.FlatUserCredentialsCsvLine { AccountName = "", Token = token } }, this.FilesEbayTestCredentialsCsv );
		}

		[ Test ]
		[ Ignore ]
		public void GetUserTokenManually_EbaySandBoxServiceWithCorrectRuName_HookupToken()
		{
			////Attention!!! This code will regenerate youe credentials!!!
			//------------ Arrange
			var ebayFactory = new EbayFactory( this._credentials.GetEbayConfigSandbox() );
			var ebayService = ebayFactory.CreateService();

			//------------ Act
			var sessionId = ebayService.GetUserSessionId();
			var authUri = ebayService.GetAuthUri( sessionId );
			Process.Start( authUri );
			var token = ebayService.FetchUserToken( sessionId );

			//------------ Assert
			authUri.Should().NotBeNullOrEmpty();
			token.Should().NotBeNullOrEmpty();
			var cc = new CsvContext();
			cc.Write( new List< TestCredentials.FlatUserCredentialsCsvLine > { new TestCredentials.FlatUserCredentialsCsvLine { AccountName = "", Token = token } }, this.FilesEbayTestCredentialsCsv );
		}

		[ Test ]
		[ Ignore ]
		public void UpdateQty_UpdateForAllActiveProducts_AllActiveUpdated()
		{
			//------------ Arrange
			var ebayFactory = new EbayFactory( this._credentials.GetEbayConfigSandbox() );
			var ebayService = ebayFactory.CreateService( this._credentials.GetEbayUserCredentials() );

			//------------ Act
			var activeproductsTask = ebayService.GetActiveProductsAsync( CancellationToken.None, true );
			activeproductsTask.Wait();
			var activeProducts = activeproductsTask.Result;

			var ids = new List<string> { "110143658380",
"131430619451",
"271747038084a",
"271678623583a",
"271678911379a",
"281637200703a",
"281637201281a",
"271813925325a",
"271813926032a",
"281637202546a",
"271813926395a",
"271813926591a",
"271813927335a",
"281637203969a",
"271813928019a",
"281637204330a",
"281637204413a",
"271813928885a",
"271813929675a",
"281637205393a",
"271813930167a",
"281637205820a",
"281637206322a",
"271813931798a",
"281637206956a",
"271813935475a",
"271813938364a",
"281199745841a",
"271882280575a",
"271788708836a",
"281201163068a",
"281517254422a",
"281518276002a",
"271887995344a",
"281710345060a",
"281710352452a",
"271739351783a",
"271742387348a",
"271406349292a" };
			//activeProducts = activeProducts.Where( x => ids.Contains( x.ItemId ) );
			activeProducts = activeProducts.Take( 10 );

			var updateResultsaTask12 = ebayService.UpdateInventoryAsync( activeProducts.Where( x => x.GetSku( false ) != null ).Select( x => new UpdateInventoryRequest { ItemId = x.ItemId.ToLong(), Quantity = 1, Sku = x.GetSku( false ).Sku, ConditionID = x.ConditionId, IsVariation = x.IsItemWithVariations() } ) );
			updateResultsaTask12.Wait();
			var updateResultsa12 = updateResultsaTask12.Result;

			//var updateResultsaTask1 = ebayService.ReviseInventoriesStatusAsync( activeProducts.Select( x => new InventoryStatusRequest { ItemId = x.ItemId.ToLong(), Quantity = x.Quantity, Sku = x.GetSku().Sku } ) );
			//updateResultsaTask1.Wait();
			//var updateResultsa1 = updateResultsaTask1.Result;

			//var updateResultsaTask2 = ebayService.ReviseInventoriesStatusAsync( activeProducts.Select( x => new InventoryStatusRequest { ItemId = x.ItemId.ToLong(), Quantity = x.Quantity, Sku = x.GetSku().Sku } ) );
			//updateResultsaTask1.Wait();
			//var updateResultsa2 = updateResultsaTask2.Result;

			//------------ Assert
			//activeProducts.Count().Should().Be( 4999 );
			//updateResultsa1.Count().Should().Be( 1250 );

			System.Threading.Tasks.Task.Delay(150000);

			updateResultsa12.Count().Should().Be(1250);
		}
	}
}