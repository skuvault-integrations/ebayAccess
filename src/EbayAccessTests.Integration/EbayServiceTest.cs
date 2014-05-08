using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EbayAccess;
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
		public void GetOrders_ServiceWithExistingOrdersInSpecifiedTimeRange_HookupOrders()
		{
			//------------ Arrange
			var service = new EbayService( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayDevCredentials() );

			//------------ Act
			var orders = service.GetOrders( new DateTime( 2014, 1, 1, 0, 0, 0 ), new DateTime( 2014, 1, 21, 10, 0, 0 ) );

			//------------ Assert
			orders.Count().Should().Be( 2, "because on site there is 2 orders" );
		}

		[ Test ]
		public void GetOrdersAsync_ServiceWithExistingOrdersInSpecifiedTimeRange_HookupOrders()
		{
			//------------ Arrange
			var service = new EbayService( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayDevCredentials() );

			//------------ Act
			var ordersTask = service.GetOrdersAsync( new DateTime( 2014, 1, 1, 0, 0, 0 ), new DateTime( 2014, 1, 21, 10, 0, 0 ) );
			ordersTask.Wait();

			//------------ Assert
			ordersTask.Result.Count().Should().Be( 2, "because on site there is 2 orders" );
		}
		#endregion

		#region UpdateProducts
		[ Test ]
		public void UpdateItems()
		{
			//------------ Arrange
			var ebayService = new EbayService( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayDevCredentials() );
			const int qty1 = 100;
			const int qty2 = 200;
			var saleItemsIds = this._credentials.GetSaleItemsIds().ToArray();

			//------------ Act
			var inventoryStat1Task = ebayService.UpdateProductsAsync( new List< InventoryStatusRequest > { new InventoryStatusRequest { ItemId = saleItemsIds[ 1 ], Quantity = qty1 } } );
			var inventoryStat2Task = ebayService.UpdateProductsAsync( new List< InventoryStatusRequest > { new InventoryStatusRequest { ItemId = saleItemsIds[ 1 ], Quantity = qty2 } } );

			var commonTask = Task.WhenAll( inventoryStat1Task, inventoryStat2Task );
			commonTask.Wait();

			//------------ Assert
			( inventoryStat1Task.Result.First().Quantity - inventoryStat2Task.Result.First().Quantity ).Should()
				.Be( qty1 - qty2, String.Format( "because we set 1 qty {0}, then set 2 qty {1}", qty1, qty2 ) );
		}
		#endregion
	}
}