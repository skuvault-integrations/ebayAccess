using System;
using System.Linq;
using EbayAccess;
using EbayAccess.Models.Credentials;
using EbayAccess.Services;
using FluentAssertions;
using NUnit.Framework;

namespace EbayAccessTests.Integration
{
	[TestFixture]
	public class EbayServiceTest
	{
		private TestCredentials _credentials;

		[SetUp]
		public void Init()
		{
			const string credentialsFilePath = @"..\..\Files\ebay_test_credentials.csv";
			const string devCredentialsFilePath = @"..\..\Files\ebay_test_devcredentials.csv";
			const string saleItemsIdsFilePath = @"..\..\Files\ebay_test_saleitemsids.csv";
			this._credentials = new TestCredentials(credentialsFilePath, devCredentialsFilePath, saleItemsIdsFilePath);
		}

		[ Test ]
		public void GetOrders_ServiceWithExistingOrdersInSpecifiedTimeRange_HookupOrders()
		{
			//------------ Arrange
			var service = new EbayService(this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayDevCredentials(), _credentials.GetEbayEndPoint());

			//------------ Act
			var orders = service.GetOrders( new DateTime( 2014, 1, 1, 0, 0, 0 ), new DateTime( 2014, 1, 21, 10, 0, 0 ) );

			//------------ Assert
			orders.Count().Should().Be(2, "because on site there is 2 orders");
		}
	}
}