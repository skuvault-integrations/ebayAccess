using System;
using EbayAccess.Models.ReviseInventoryStatusRequest;
using Netco.Logging;
using Netco.Logging.NLogIntegration;
using NUnit.Framework;

namespace EbayAccessTests.Integration.TestEnvironment
{
	[ TestFixture ]
	public abstract class TestBase
	{
		protected TestCredentials _credentials;

		protected string FilesEbayTestCredentialsCsv = @"..\..\Files\ebay_test_credentials.csv";
		protected string FilesEbayTestDevcredentialsCsv = @"..\..\Files\ebay_test_devcredentials.csv";
		protected string FilesEbayTestSaleitemsidsCsv = @"..\..\Files\ebay_test_saleitemsids.csv";
		protected string FilesEbayTestRunameCsv = @"..\..\Files\ebay_test_runame.csv";

		protected uint QtyUpdateFor = 500;

		[ SetUp ]
		public void Init()
		{
			this._credentials = new TestCredentials( this.FilesEbayTestCredentialsCsv, this.FilesEbayTestDevcredentialsCsv, this.FilesEbayTestSaleitemsidsCsv, this.FilesEbayTestRunameCsv );
			NetcoLogger.LoggerFactory = new NLogLoggerFactory();
		}
	}

	internal static class ExistsProductsCreatedInRange
	{
		public static readonly DateTime DateFrom = new DateTime( 2014, 2, 14, 15, 0, 0, DateTimeKind.Local );
		public static readonly DateTime DateTo = new DateTime( 2014, 5, 24, 15, 0, 0, DateTimeKind.Local );
	}

	internal static class ExistingOrdersCreatedInRange
	{
		public static readonly DateTime DateFrom = new DateTime( 2014, 4, 14, 15, 0, 0, DateTimeKind.Local );
		public static readonly DateTime DateTo = new DateTime( 2014, 5, 27, 15, 0, 0, DateTimeKind.Local );
	}

	internal static class NotExistingOrdersInRange
	{
		public static readonly DateTime DateFrom = new DateTime( 2000, 4, 14, 15, 0, 0, DateTimeKind.Local );
		public static readonly DateTime DateTo = new DateTime( 2000, 5, 24, 15, 0, 0, DateTimeKind.Local );
	}

	internal static class ExistingProducts
	{
		public static readonly InventoryStatusRequest FixedPrice1WithVariation1 = new InventoryStatusRequest { ItemId = 110142400319, Sku = "testSku501_W", Quantity = 30 };
		public static readonly InventoryStatusRequest FixedPrice1WithVariation2 = new InventoryStatusRequest { ItemId = 110142400319, Sku = "testSku501_B", Quantity = 30 };
		public static readonly InventoryStatusRequest FixedPrice1WithVariation3 = new InventoryStatusRequest { ItemId = 110142400319, Sku = "testSku501_Y", Quantity = 30 };
		public static readonly InventoryStatusRequest FixedPrice1WithoutVariations = new InventoryStatusRequest { ItemId = 110141553531, Sku = "testSku11014", Quantity = 50 };
		public static readonly InventoryStatusRequest FixedPrice2WithoutVariations = new InventoryStatusRequest { ItemId = 110141989389, Sku = "testSku110141989389", Quantity = 80 };
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