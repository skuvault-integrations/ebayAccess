﻿using System.Collections.Generic;
using System.Linq;
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
			var service = new EbayService( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );

			//------------ Act
			var orders = service.GetOrders( ExistingOrdersCreatedInRange.DateFrom, ExistingOrdersCreatedInRange.DateTo );

			//------------ Assert
			orders.Count().Should().BeGreaterThan( 1, "because on site there are orders" );
		}

		[ Test ]
		public void GetOrdersAsync_ServiceWithExistingOrdersInSpecifiedTimeRange_HookupOrders()
		{
			//------------ Arrange
			var service = new EbayService( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );

			//------------ Act
			var ordersTask = service.GetOrdersAsync( ExistingOrdersCreatedInRange.DateFrom, ExistingOrdersCreatedInRange.DateTo );
			ordersTask.Wait();

			//------------ Assert
			ordersTask.Result.Count().Should().BeGreaterThan( 0, "because on site there are orders" );
		}
		#endregion

		#region UpdateProducts
		[ Test ]
		public void UpdateProductsAsync_EbayServiceWithFixedPriceProductsWithVariation_ProductsUpdated()
		{
			//------------ Arrange
			var ebayService = new EbayService( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );

			//------------ Act
			var updateProductsAsyncTask1 = ebayService.UpdateProductsAsync( new List< InventoryStatusRequest >
			{
				new InventoryStatusRequest { ItemId = ExistingProducts.FixedPrice1WithVariation1.ItemId, Sku = ExistingProducts.FixedPrice1WithVariation1.Sku, Quantity = ExistingProducts.FixedPrice1WithVariation1.Quantity + this.QtyUpdateFor },
				new InventoryStatusRequest { ItemId = ExistingProducts.FixedPrice1WithVariation2.ItemId, Sku = ExistingProducts.FixedPrice1WithVariation2.Sku, Quantity = ExistingProducts.FixedPrice1WithVariation2.Quantity + this.QtyUpdateFor },
			} );
			updateProductsAsyncTask1.Wait();

			var updateProductsAsyncTask2 = ebayService.UpdateProductsAsync( new List< InventoryStatusRequest >
			{
				ExistingProducts.FixedPrice1WithVariation1,
				ExistingProducts.FixedPrice1WithVariation2,
			} );
			updateProductsAsyncTask2.Wait();

			//------------ Assert
			( updateProductsAsyncTask1.Result.ToList().First( x => x.ItemId == ExistingProducts.FixedPrice1WithVariation1.ItemId && x.Sku == ExistingProducts.FixedPrice1WithVariation1.Sku ).Quantity - updateProductsAsyncTask2.Result.ToList().First( x => x.ItemId == ExistingProducts.FixedPrice1WithVariation1.ItemId && x.Sku == ExistingProducts.FixedPrice1WithVariation1.Sku ).Quantity ).Should().Be( this.QtyUpdateFor );
			( updateProductsAsyncTask1.Result.ToList().First( x => x.ItemId == ExistingProducts.FixedPrice1WithVariation2.ItemId && x.Sku == ExistingProducts.FixedPrice1WithVariation2.Sku ).Quantity - updateProductsAsyncTask2.Result.ToList().First( x => x.ItemId == ExistingProducts.FixedPrice1WithVariation2.ItemId && x.Sku == ExistingProducts.FixedPrice1WithVariation2.Sku ).Quantity ).Should().Be( this.QtyUpdateFor );
		}

		[ Test ]
		public void UpdateProductsAsync_EbayServiceWithFixedPriceProductsWithoutVariations_ProductsUpdated()
		{
			//------------ Arrange
			var ebayService = new EbayService( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );

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
		#endregion

		#region GetProductsDetails
		[ Test ]
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

		[Test]
		public void GetActiveProductsAsync_ServiceWithExistingProducts_HookupProducts()
		{
			//------------ Arrange
			var ebayService = new EbayService(this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox());

			//------------ Act
			var inventoryStat1Task = ebayService.GetActiveProductsAsync();
			inventoryStat1Task.Wait();
			var products = inventoryStat1Task.Result;

			//------------ Assert
			products.Count().Should().BeGreaterThan(0, "because on site there are items");
		}
		#endregion
	}
}