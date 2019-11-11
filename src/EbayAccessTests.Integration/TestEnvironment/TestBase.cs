using System;
using System.Collections.Generic;
using EbayAccess.Models.ReviseInventoryStatusRequest;
using EbayAccess.Services;
using Netco.Logging;
using Netco.Logging.NLogIntegration;
using NUnit.Framework;

namespace EbayAccessTests.Integration.TestEnvironment
{
	[ TestFixture ]
	public abstract class TestBase
	{
		protected TestCredentials _credentials;
		internal IEbayServiceLowLevel _ebayService;

		protected string FilesEbayTestCredentialsCsv = @"..\..\Files\ebay_test_credentials.csv";
		protected string FilesEbayTestDevcredentialsCsv = @"..\..\Files\ebay_test_devcredentials.csv";
		protected string FilesEbayTestSaleitemsidsCsv = @"..\..\Files\ebay_test_saleitemsids.csv";
		protected string FilesEbayTestRunameCsv = @"..\..\Files\ebay_test_runame.csv";

		protected uint QtyUpdateFor = 500;

		[ SetUp ]
		public void Init()
		{
			this._credentials = new TestCredentials( this.FilesEbayTestCredentialsCsv, this.FilesEbayTestDevcredentialsCsv, this.FilesEbayTestSaleitemsidsCsv, this.FilesEbayTestRunameCsv );
			NetcoLogger.LoggerFactory = ( ILoggerFactory )new NLogLoggerFactory();
			this._ebayService = new EbayServiceLowLevel( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayConfigSandbox() );
		}
	}

	internal static class ExistsProductsCreatedInRange
	{
		public static readonly DateTime DateFrom = new DateTime( 2019, 10, 1, 15, 0, 0, DateTimeKind.Local );
		public static readonly DateTime DateTo = new DateTime( 2019, 12, 1, 15, 0, 0, DateTimeKind.Local );
	}

	internal static class ExistingOrdersCreatedInRange
	{
		public static readonly DateTime DateFrom = new DateTime( 2014, 7, 14, 15, 0, 0, DateTimeKind.Local );
		public static readonly DateTime DateTo = new DateTime( 2014, 8, 27, 15, 0, 0, DateTimeKind.Local );
	}

	internal static class ExistingOrdersIds
	{
		public static readonly List< string > OrdersIds = new List< string >
		{
			"110141989389-27286939001",
			"110143658228-27291236001",
			"110143658380-27291239001",
			"271269010",
			"110142503362-27291905001",
			"110142503362-27291907001",
			"110142503362-27291908001",
			"110142503362-27292237001",
			"110142503362-27292246001",
			"110142503362-27292300001",
			"110142503362-27292302001",
			"271289010",
			"271293010",
			"110143660198-27292342001"
		};

		public static readonly List< string > SaleNumers = new List< string >
		{
			//"186",
			//"190",
			//"191",
			//"189"
			//"237",
			//"235",
			//"239"
			
			//"261",
			//"262",
			//"265"

			//"299",
			//"300",
			//"301",
			"324",
			"323"
		};
	}

	internal static class NotExistingBecauseOfCombinedOrdersIds
	{
		public static readonly List< string > OrdersIds = new List< string >
		{
			"110142503362-27292309001",
			"110142503362-27292308001"
		};
	}

	internal static class ExistingOrdersModifiedInRange
	{
		public static readonly DateTime DateFrom = new DateTime( 2014, 7, 2, 15, 0, 0, DateTimeKind.Local );
		public static readonly DateTime DateTo = new DateTime( 2014, 7, 4, 15, 0, 0, DateTimeKind.Local );
	}

	internal static class NotExistingOrdersInRange
	{
		public static readonly DateTime DateFrom = new DateTime( 2000, 4, 14, 15, 0, 0, DateTimeKind.Local );
		public static readonly DateTime DateTo = new DateTime( 2000, 5, 24, 15, 0, 0, DateTimeKind.Local );
	}

	internal static class ExistingProducts
	{
		public static readonly InventoryStatusRequest FixedPrice1WithVariation1 = new InventoryStatusRequest { ItemId = 110449844104, Sku = "RLauren_Wom_TShirt_Pnk_S", Quantity = 6 };
		public static readonly InventoryStatusRequest FixedPrice1WithVariation2 = new InventoryStatusRequest { ItemId = 110449844104, Sku = "RLauren_Wom_TShirt_Pnk_M", Quantity = 7 };
		public static readonly InventoryStatusRequest FixedPrice1WithVariation3 = new InventoryStatusRequest { ItemId = 110449844104, Sku = "RLauren_Wom_TShirt_Blk_S", Quantity = 8 };
		public static readonly InventoryStatusRequest FixedPrice1WithVariation4 = new InventoryStatusRequest { ItemId = 110449844104, Sku = "RLauren_Wom_TShirt_Blk_M", Quantity = 9 };
		public static readonly InventoryStatusRequest FixedPrice1WithVariation5 = new InventoryStatusRequest { ItemId = 110449844104, Sku = "RLauren_Wom_TShirt_Blu_S", Quantity = 10 };
		public static readonly InventoryStatusRequest FixedPrice1WithVariation6 = new InventoryStatusRequest { ItemId = 110449844104, Sku = "RLauren_Wom_TShirt_Blu_M", Quantity = 11 };
		public static readonly InventoryStatusRequest FixedPrice1WithoutVariations = new InventoryStatusRequest { ItemId = 110449813053, Sku = "testsku1", Quantity = 5 };
		public static readonly InventoryStatusRequest FixedPrice2WithoutVariations = new InventoryStatusRequest { ItemId = 110449844035, Sku = "testsku3", Quantity = 10 };
	}

	public static class TestItemsDescriptions

	{
		public static string AnyExistingNonVariationItem { get; set; }
		public static string ExistingFixedPriceItemWithVariationsSku { get; set; }

		static TestItemsDescriptions()
		{
			AnyExistingNonVariationItem = "AnyExistingNonVariationItem";
			ExistingFixedPriceItemWithVariationsSku = "ExistingFixedPriceItemWithVariationsSku";
		}
	}
}