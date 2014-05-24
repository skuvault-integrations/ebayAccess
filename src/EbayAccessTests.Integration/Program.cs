using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EbayAccess;
using EbayAccess.Models.ReviseInventoryStatusRequest;
using EbayAccess.Services;
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
			var orders = ebayService.GetOrders( ExistingOrdersCreatedInRange.DateFrom, ExistingOrdersCreatedInRange.DateTo );

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
			var productsAsyncTask = ebayService.GetProductsAsync( ExistsProductsCreatedInRange.DateFrom, ExistsProductsCreatedInRange.DateTo );
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

			( updateProductsAsyncTask1.Result.ToList().First( x => x.ItemId == ExistingProducts.FixedPrice1WithoutVariations.ItemId ).Quantity - updateProductsAsyncTask2.Result.ToList().First( x => x.ItemId == ExistingProducts.FixedPrice1WithoutVariations.ItemId ).Quantity ).Should().Be( this.QtyUpdateFor );
			( updateProductsAsyncTask1.Result.ToList().First( x => x.ItemId == ExistingProducts.FixedPrice2WithoutVariations.ItemId ).Quantity - updateProductsAsyncTask2.Result.ToList().First( x => x.ItemId == ExistingProducts.FixedPrice2WithoutVariations.ItemId ).Quantity ).Should().Be( this.QtyUpdateFor );
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
			( updateProductsAsyncTask1.Result.ToList().First( x => x.ItemId == ExistingProducts.FixedPrice1WithoutVariations.ItemId ).Quantity - updateProductsAsyncTask2.Result.ToList().First( x => x.ItemId == ExistingProducts.FixedPrice1WithoutVariations.ItemId ).Quantity ).Should().Be( this.QtyUpdateFor );
			( updateProductsAsyncTask1.Result.ToList().First( x => x.ItemId == ExistingProducts.FixedPrice2WithoutVariations.ItemId ).Quantity - updateProductsAsyncTask2.Result.ToList().First( x => x.ItemId == ExistingProducts.FixedPrice2WithoutVariations.ItemId ).Quantity ).Should().Be( this.QtyUpdateFor );
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
		public async Task GetUserTokenLowLevel_EbayServiceWithCorrectRuName_HookupToken()
		{
			////Attention!!! This code will regenerate youe credentials!!!

			//A
			var ebayService = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );

			//A
			var sessionId = await ebayService.GetSessionIdAsync().ConfigureAwait( false );
			ebayService.AuthenticateUser( sessionId );
			var userToken = await ebayService.FetchTokenAsync( sessionId ).ConfigureAwait( false );

			//A
			sessionId.Should().NotBeNullOrWhiteSpace();
			userToken.Should().NotBeNullOrWhiteSpace();
		}

		[ Test ]
		[ Ignore ]
		public void GetUserToken_EbaySandBoxServiceWithCorrectRuName_HookupToken()
		{
			////Attention!!! This code will regenerate youe credentials!!!

			//------------ Arrange
			var ebayFactory = new EbayFactory( this._credentials.GetEbayConfigSandbox() );
			var ebayService = ebayFactory.CreateService();

			//------------ Act
			var getUserTokenAsyncTask = ebayService.GetUserTokenAsync();
			getUserTokenAsyncTask.Wait();
			var token = getUserTokenAsyncTask.Result;

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
			var getUserTokenAsyncTask = ebayService.GetUserTokenAsync();
			getUserTokenAsyncTask.Wait();
			var token = getUserTokenAsyncTask.Result;

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
			var userSessionIdAsyncTask = ebayService.GetUserSessionIdAsync();
			userSessionIdAsyncTask.Wait();
			var sessionId = userSessionIdAsyncTask.Result;

			var authUri = ebayService.GetAuthUri( sessionId );

			var fetchUserTokenAsyncTask = ebayService.FetchUserTokenAsync( sessionId );
			fetchUserTokenAsyncTask.Wait();
			var token = fetchUserTokenAsyncTask.Result;

			//------------ Assert
			authUri.Should().NotBeNullOrEmpty();
			token.Should().NotBeNullOrEmpty();
			var cc = new CsvContext();
			cc.Write( new List< TestCredentials.FlatUserCredentialsCsvLine > { new TestCredentials.FlatUserCredentialsCsvLine { AccountName = "", Token = token } }, this.FilesEbayTestCredentialsCsv );
		}
	}
}