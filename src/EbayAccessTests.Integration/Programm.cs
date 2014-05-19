using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EbayAccess;
using EbayAccess.Models.ReviseInventoryStatusRequest;
using EbayAccess.Services;
using EbayAccessTests.Integration.TestEnvironment;
using FluentAssertions;
using NUnit.Framework;

namespace EbayAccessTests.Integration
{
	[ TestFixture ]
	public class Programm : TestBase
	{
		[ Test ]
		public void GetOrders_EbayServiceWithProducts_HookupOrders()
		{
			//------------ Arrange
			var ebayFactory = new EbayFactory( this._credentials.GetEbayConfig() );
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
			var ebayFactory = new EbayFactory( this._credentials.GetEbayConfig() );
			var ebayService = ebayFactory.CreateService( this._credentials.GetEbayUserCredentials() );

			//------------ Act
			var ordersTask = ebayService.GetOrdersAsync( new DateTime( 2014, 5, 2, 18, 0, 0 ), new DateTime( 2014, 5, 3, 10, 0, 0 ) );
			ordersTask.Wait();
			var orders = ordersTask.Result;

			//------------ Assert
			orders.First().TransactionArray.TrueForAll( x => x.Variation != null && !string.IsNullOrWhiteSpace( x.Variation.Sku ) ).Should().BeTrue( "because on site there is an order contains 3 products, each is variation" );
		}

		[ Test ]
		public void GetOrders_EbayServiceWithOrdersSku_HookupProductsSku()
		{
			//------------ Arrange
			var ebayFactory = new EbayFactory( this._credentials.GetEbayConfig() );
			var ebayService = ebayFactory.CreateService( this._credentials.GetEbayUserCredentials() );

			//------------ Act
			var dateFrom = new DateTime( 2014, 4, 28, 19, 30, 0 );
			var dateTo = new DateTime( 2014, 4, 28, 19, 40, 0 );
			var ordersTask = ebayService.GetOrdersAsync( dateFrom, dateTo );
			ordersTask.Wait();
			var orders = ordersTask.Result;

			//------------ Assert
			orders.First().TransactionArray.TrueForAll( x => !string.IsNullOrWhiteSpace( x.Item.Sku ) ).Should().BeTrue( "because on site there is order with sku in specified time range {0}-{1}", dateFrom, dateTo );
		}

		[ Test ]
		public void GetProductsAsync_EbayServiceWithProductsVariationsSku_HookupProducts()
		{
			//------------ Arrange
			var ebayFactory = new EbayFactory( this._credentials.GetEbayConfig() );
			var ebayService = ebayFactory.CreateService( this._credentials.GetEbayUserCredentials() );

			//------------ Act
			var productsAsyncTask = ebayService.GetProductsAsync( new DateTime( 2014, 5, 2, 0, 0, 0 ), new DateTime( 2014, 5, 3, 10, 0, 0 ) );
			productsAsyncTask.Wait();
			var products = productsAsyncTask.Result;

			//------------ Assert
			products.Count().Should().Be( 1, "because there is 1 item on the site" );
		}

		[ Test ]
		public void UpdateProductsAsync_EbayServiceWithOrdersVariationsSku_ProductsUpdated()
		{
			//------------ Arrange
			var ebayFactory = new EbayFactory( this._credentials.GetEbayConfig() );
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
			var ebayFactory = new EbayFactory( this._credentials.GetEbayConfig() );
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
		public void GetProductsDetailsAsync_EbayServiceWithProductsVariationsSku_HookupProductsVariationsSku()
		{
			//------------ Arrange
			var ebayFactory = new EbayFactory( this._credentials.GetEbayConfig() );
			var ebayService = ebayFactory.CreateService( this._credentials.GetEbayUserCredentials() );

			//------------ Act
			var productsDetailsAsyncTask = ebayService.GetProductsDetailsAsync( new DateTime( 2014, 5, 2, 0, 0, 0 ), new DateTime( 2014, 5, 3, 10, 0, 0 ) );
			productsDetailsAsyncTask.Wait();
			var products = productsDetailsAsyncTask.Result;

			//------------ Assert
			products.Should().NotBeEmpty( "because in site there are items" );
			products.First().Variations.TrueForAll( x => !string.IsNullOrWhiteSpace( x.Sku ) ).Should().BeTrue( "because on site there is orders with Sku" );
		}

		[ Test ]
		public void GetProductsDetailsAsyncInTimeRange_EbayServiceWithProductsVariationsSku_HookupProductsThatEitherSingleVariationEitherNonVariation()
		{
			//------------ Arrange
			var ebayFactory = new EbayFactory( this._credentials.GetEbayConfig() );
			var ebayService = ebayFactory.CreateService( this._credentials.GetEbayUserCredentials() );

			//------------ Act
			var productsDetailsAsyncTask = ebayService.GetProductsDetailsAsync( new DateTime( 2014, 5, 2, 0, 0, 0 ), new DateTime( 2014, 5, 3, 10, 0, 0 ) );
			productsDetailsAsyncTask.Wait();
			var products = productsDetailsAsyncTask.Result;

			//------------ Assert
			products.Should().NotBeEmpty( "because in site there are items" );
			products.ToList().TrueForAll( x => !x.IsItemWithVariations() || !x.HaveMultiVariations() ).Should().BeTrue( "because before returned, items was devided by variations skus" );
		}

		[ Test ]
		public void GetProductsDetailsAsyncInTimeRange_EbayServiceWithProductsAndSkus_HookupProductsAndTheirSkus()
		{
			//------------ Arrange
			var ebayFactory = new EbayFactory( this._credentials.GetEbayConfig() );
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
			var ebayFactory = new EbayFactory( this._credentials.GetEbayConfig() );
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
		public async Task GetUserToken_EbayServiceWithCorrectRuName_HookupSessionId()
		{
			//A
			var ebayService = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfig() );

			//A
			var sessionId = await ebayService.GetSessionIdAsync().ConfigureAwait( false );
			ebayService.AutentificateUser( sessionId );
			var userToken = await ebayService.FetchTokenAsync( sessionId ).ConfigureAwait( false );

			//A
			sessionId.Should().NotBeNullOrWhiteSpace();
			userToken.Should().NotBeNullOrWhiteSpace();
		}
	}
}