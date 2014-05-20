using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EbayAccess;
using EbayAccess.Misc;
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
			var orders = ebayService.GetOrders( new DateTime( 2014, 1, 1, 0, 0, 0 ), new DateTime( 2014, 1, 21, 10, 0, 0 ) );

			//------------ Assert
			orders.Count().Should().Be( 2, "because on site there is 2 orders" );
		}

		[ Test ]
		public void GetOrders_EbayServiceWithOrdersVariationsSku_HookupProductsVariationsSku()
		{
			//------------ Arrange
			var ebayFactory = new EbayFactory( this._credentials.GetEbayConfigSandbox() );
			var ebayService = ebayFactory.CreateService( this._credentials.GetEbayUserCredentials() );
			var saleItems = this._credentials.GetSaleItems();
			var item1 = saleItems.First( x => String.Compare( x.Descr, TestItemsDescriptions.AnyExistingNonVariationItem, StringComparison.InvariantCultureIgnoreCase ) == 0 );

			//------------ Act
			var ordersTask = ebayService.GetOrdersAsync( item1.OrderedTime.ToDateTime().AddHours( -1 ), item1.OrderedTime.ToDateTime().AddHours( 1 ) );
			ordersTask.Wait();
			var orders = ordersTask.Result;

			//------------ Assert
			orders.First().TransactionArray.Where( x => x.Item.ItemDetails.IsItemWithVariations() ).Count().Should().BeGreaterThan( 0 );
			orders.First().TransactionArray.Where( x => x.Item.ItemDetails.IsItemWithVariations() ).ToList().TrueForAll( x => !string.IsNullOrWhiteSpace( x.Variation.Sku ) ).Should().BeTrue( "because on site there is an order contains 3 products, each is variation" );
		}

		[ Test ]
		public void GetOrders_EbayServiceWithOrdersSku_HookupProductsSku()
		{
			//------------ Arrange
			var ebayFactory = new EbayFactory( this._credentials.GetEbayConfigSandbox() );
			var ebayService = ebayFactory.CreateService( this._credentials.GetEbayUserCredentials() );

			//------------ Act
			var dateFrom = new DateTime( 2014, 4, 28, 19, 30, 0 );
			var dateTo = new DateTime( 2014, 5, 28, 19, 40, 0 );
			var ordersTask = ebayService.GetOrdersAsync( dateFrom, dateTo );
			ordersTask.Wait();
			var orders = ordersTask.Result;

			//------------ Assert
			orders.First().TransactionArray.Where( x => !x.Item.ItemDetails.IsItemWithVariations() ).Count().Should().BeGreaterThan( 0 );
			orders.First().TransactionArray.Where( x => !x.Item.ItemDetails.IsItemWithVariations() ).ToList().TrueForAll( x => !string.IsNullOrWhiteSpace( x.Item.Sku ) );
		}

		[ Test ]
		public void GetProductsAsyncInTimeRange_EbayServiceWithProductsVariationsSku_HookupProducts()
		{
			//------------ Arrange
			var ebayFactory = new EbayFactory( this._credentials.GetEbayConfigSandbox() );
			var ebayService = ebayFactory.CreateService( this._credentials.GetEbayUserCredentials() );
			var saleItems = this._credentials.GetSaleItems().First( x => String.Compare( x.Descr, TestItemsDescriptions.ExistingFixedPriceItemWithVariationsSku, StringComparison.InvariantCultureIgnoreCase ) == 0 );

			//------------ Act
			var productsAsyncTask = ebayService.GetProductsAsync( saleItems.CreationTime.ToDateTime().AddMinutes( -1 ), saleItems.CreationTime.ToDateTime().AddMinutes( 1 ) );
			productsAsyncTask.Wait();
			var products = productsAsyncTask.Result;

			//------------ Assert
			products.Count().Should().Be( 1, "because there is 1 item on the site" );
		}

		[ Test ]
		public void UpdateProductsAsync_EbayServiceWithProductVariationsSku_ProductsUpdated()
		{
			//------------ Arrange
			var ebayFactory = new EbayFactory( this._credentials.GetEbayConfigSandbox() );
			var ebayService = ebayFactory.CreateService( this._credentials.GetEbayUserCredentials() );
			var saleItems = this._credentials.GetSaleItems();
			var item1 = saleItems.First( x => String.Compare( x.Descr, TestItemsDescriptions.ExistingFixedPriceItemWithVariationsSku, StringComparison.InvariantCultureIgnoreCase ) == 0 );
			var item2 = saleItems.First( x => String.Compare( x.Descr, TestItemsDescriptions.ExistingFixedPriceItemWithVariationsSku, StringComparison.InvariantCultureIgnoreCase ) == 0 && item1.Sku != x.Sku );

			const int itemsQty1 = 3;
			const int itemsQty2 = 4;

			//------------ Act
			var updateProductsAsyncTask = ebayService.UpdateProductsAsync( new List< InventoryStatusRequest >
			{
				new InventoryStatusRequest { ItemId = item1.Id.ToLong(), Sku = item1.Sku, Quantity = itemsQty1 },
				new InventoryStatusRequest { ItemId = item2.Id.ToLong(), Sku = item2.Sku, Quantity = itemsQty1 },
			} );
			updateProductsAsyncTask.Wait();
			var inventoryStat1 = updateProductsAsyncTask.Result.ToArray();

			var updateProductsAsyncTask2 = ebayService.UpdateProductsAsync( new List< InventoryStatusRequest >
			{
				new InventoryStatusRequest { ItemId = item1.Id.ToLong(), Sku = item1.Sku, Quantity = itemsQty2 },
				new InventoryStatusRequest { ItemId = item2.Id.ToLong(), Sku = item2.Sku, Quantity = itemsQty2 },
			} );
			updateProductsAsyncTask2.Wait();
			var inventoryStat2 = updateProductsAsyncTask2.Result.ToArray();

			//------------ Assert
			( inventoryStat1[ 0 ].Quantity - inventoryStat2[ 0 ].Quantity ).Should()
				.Be( itemsQty1 - itemsQty2, String.Format( "because we set 1 qty {0}, then set 2 qty {1}", itemsQty1, itemsQty2 ) );
		}

		[ Test ]
		public void UpdateProductsAsync_EbayServiceWithNonVariationFixedPriceProducts_ProductsUpdated()
		{
			//------------ Arrange
			var ebayFactory = new EbayFactory( this._credentials.GetEbayConfigSandbox() );
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

		#region GetProductsDetails
		[ Test ]
		public void GetProductsDetailsAsyncInTimeRange_EbayServiceWithProductsVariationsSku_HookupProductsVariationsSku()
		{
			//------------ Arrange
			var ebayFactory = new EbayFactory( this._credentials.GetEbayConfigSandbox() );
			var ebayService = ebayFactory.CreateService( this._credentials.GetEbayUserCredentials() );
			var saleItems = this._credentials.GetSaleItems().First( x => String.Compare( x.Descr, TestItemsDescriptions.ExistingFixedPriceItemWithVariationsSku, StringComparison.InvariantCultureIgnoreCase ) == 0 );

			//------------ Act
			var productsDetailsAsyncTask = ebayService.GetProductsDetailsAsync( saleItems.CreationTime.ToDateTime().AddMinutes( -1 ), saleItems.CreationTime.ToDateTime().AddMinutes( 1 ) );
			productsDetailsAsyncTask.Wait();
			var products = productsDetailsAsyncTask.Result;

			//------------ Assert
			products.Should().NotBeEmpty( "because in site there is item with variation" );
			products.First().Variations.TrueForAll( x => !string.IsNullOrWhiteSpace( x.Sku ) ).Should().BeTrue( "because on site there is products with variation Sku" );
		}

		[ Test ]
		public void GetProductsDetailsAsync_EbayServiceWithProductsVariationsSku_HookupProductsThatEitherSingleVariationEitherNonVariation()
		{
			//------------ Arrange
			var ebayFactory = new EbayFactory( this._credentials.GetEbayConfigSandbox() );
			var ebayService = ebayFactory.CreateService( this._credentials.GetEbayUserCredentials() );

			//------------ Act
			var productsDetailsAsyncTask = ebayService.GetProductsDetailsAsync();
			productsDetailsAsyncTask.Wait();
			var products = productsDetailsAsyncTask.Result;

			//------------ Assert
			products.Should().NotBeEmpty( "because in site there are items" );
			products.ToList().TrueForAll( x => !x.IsItemWithVariations() || !x.HaveMultiVariations() ).Should().BeTrue( "because before returned, items was devided by variations skus" );
		}

		[ Test ]
		public void GetProductsDetailsAsyncInTimeRange_EbayServiceWithProductsSkus_HookupProductsAndTheirSkus()
		{
			//------------ Arrange
			var ebayFactory = new EbayFactory( this._credentials.GetEbayConfigSandbox() );
			var ebayService = ebayFactory.CreateService( this._credentials.GetEbayUserCredentials() );

			//------------ Act
			var timeFromStart = new DateTime( 2014, 4, 18, 0, 0, 0 );
			var timeFromTo = new DateTime( 2014, 4, 19, 10, 0, 0 );
			var productsDetailsAsyncTask = ebayService.GetProductsDetailsAsync( timeFromStart, timeFromTo );
			productsDetailsAsyncTask.Wait();
			var products = productsDetailsAsyncTask.Result;

			//------------ Assert
			products.Should().NotBeEmpty( "because in site there are items" );
			products.ToList().TrueForAll( x => !string.IsNullOrWhiteSpace( x.Sku ) ).Should().BeTrue( "because on site there is item with sku specified in time range {0}-{1}", timeFromStart, timeFromTo );
		}

		[ Test ]
		public void GetProductsDetailsAsync_EbayServiceWithProductsWithVariationSku_HookupProductsWithTheirVariationSkus()
		{
			//------------ Arrange
			var ebayFactory = new EbayFactory( this._credentials.GetEbayConfigSandbox() );
			var ebayService = ebayFactory.CreateService( this._credentials.GetEbayUserCredentials() );

			//------------ Act
			var productsDetailsAsyncTask = ebayService.GetProductsDetailsAsync();
			productsDetailsAsyncTask.Wait();
			var products = productsDetailsAsyncTask.Result;

			//------------ Assert
			products.Count().Should().BeGreaterThan( 0, "because on site there are items" );
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
			var getUserTokenAsyncTask = ebayService.GetUserToken();
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
			//------------ Arrange
			var ebayFactory = new EbayFactory( this._credentials.GetEbayConfigProduction() );
			var ebayService = ebayFactory.CreateService();

			//------------ Act
			var getUserTokenAsyncTask = ebayService.GetUserToken();
			getUserTokenAsyncTask.Wait();
			var token = getUserTokenAsyncTask.Result;

			//------------ Assert
			token.Should().NotBeNullOrEmpty();
			var cc = new CsvContext();
			cc.Write( new List< TestCredentials.FlatUserCredentialsCsvLine > { new TestCredentials.FlatUserCredentialsCsvLine { AccountName = "", Token = token } }, this.FilesEbayTestCredentialsCsv );
		}
	}
}