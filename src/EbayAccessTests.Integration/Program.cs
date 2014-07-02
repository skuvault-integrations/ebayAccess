using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EbayAccess;
using EbayAccess.Misc;
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
			var ordersTask = ebayService.GetOrdersAsync( ExistingOrdersModifiedInRange.DateFrom, ExistingOrdersModifiedInRange.DateTo );
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

		[ Test ]
		public void UpdateProductsAsync_EbayServiceWithFixedPriceProductVariations_ProductsUpdated()
		{
			//------------ Arrange
			var ebayFactory = new EbayFactory( this._credentials.GetEbayConfigSandbox() );
			var ebayService = ebayFactory.CreateService( this._credentials.GetEbayUserCredentials() );

			//------------ Act
			var updateProductsAsyncTask1 = ebayService.UpdateProductsAsync( new List< InventoryStatusRequest >
			{
				new InventoryStatusRequest { ItemId = ExistingProducts.FixedPrice1WithoutVariations.ItemId, Quantity = ExistingProducts.FixedPrice1WithoutVariations.Quantity + this.QtyUpdateFor },
				new InventoryStatusRequest { ItemId = ExistingProducts.FixedPrice2WithoutVariations.ItemId, Quantity = ExistingProducts.FixedPrice2WithoutVariations.Quantity + this.QtyUpdateFor },
			} );
			updateProductsAsyncTask1.Wait();

			var updateProductsAsyncTask2 = ebayService.UpdateProductsAsync( new List< InventoryStatusRequest >
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
			var updateProductsAsyncTask1 = ebayService.UpdateProductsAsync( new List< InventoryStatusRequest >
			{
				new InventoryStatusRequest { ItemId = ExistingProducts.FixedPrice1WithoutVariations.ItemId, Quantity = ExistingProducts.FixedPrice1WithoutVariations.Quantity + this.QtyUpdateFor },
				new InventoryStatusRequest { ItemId = ExistingProducts.FixedPrice2WithoutVariations.ItemId, Quantity = ExistingProducts.FixedPrice2WithoutVariations.Quantity + this.QtyUpdateFor },
			} );
			updateProductsAsyncTask1.Wait();

			var updateProductsAsyncTask2 = ebayService.UpdateProductsAsync( new List< InventoryStatusRequest >
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
			var ebayFactory = new EbayFactory( this._credentials.GetEbayConfigProduction() );
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
			var ebayFactory = new EbayFactory( this._credentials.GetEbayConfigProduction() );
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
			var activeproductsTask = ebayService.GetActiveProductsAsync();
			activeproductsTask.Wait();
			var activeProducts = activeproductsTask.Result;

			var updateResultsaTask1 = ebayService.UpdateProductsAsync( activeProducts.Select( x => new InventoryStatusRequest { ItemId = x.ItemId.ToLong(), Quantity = 10050, Sku = x.Sku } ) );
			updateResultsaTask1.Wait();
			var updateResultsa1 = updateResultsaTask1.Result;

			var updateResultsaTask2 = ebayService.UpdateProductsAsync( activeProducts.Select( x => new InventoryStatusRequest { ItemId = x.ItemId.ToLong(), Quantity = 100, Sku = x.Sku } ) );
			updateResultsaTask1.Wait();
			var updateResultsa2 = updateResultsaTask2.Result;

			//------------ Assert
			activeProducts.Count().Should().Be( 4999 );
			updateResultsa1.Count().Should().Be( 1250 );
			updateResultsa2.Count().Should().Be( 1250 );
		}
	}
}