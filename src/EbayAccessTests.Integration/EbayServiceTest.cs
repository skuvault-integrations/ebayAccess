using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EbayAccessTests.Integration
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Text;
	using System.Threading.Tasks;
	using EbayAccess;
	using EbayAccess.Models;
	using EbayAccess.Models.GetOrdersResponse;
	using EbayAccess.Models.ReviseInventoryStatusRequest;
	using EbayAccess.Services;
	using FluentAssertions;
	using Moq;
	using NUnit.Framework;
	using Item = EbayAccess.Models.GetSellerListResponse.Item;

	[TestFixture]
	public class EbayServiceTest
	{
		private TestCredentials _credentials;

		[SetUp]
		public void Init()
		{
			const string credentialsFilePath = @"..\..\Files\ebay_test_credentials.csv";
			_credentials = new TestCredentials(credentialsFilePath);
		}

		[Test]
		public async Task EbayServiceExistingItems_GetItemsAsync_NotEmptyItemsCollection()
		{
			//A
			var ebayService = new EbayService(_credentials.GetEbayCredentials(), _credentials.GetEbayEndPoint());

			//A
			IEnumerable<Item> orders =
				await ebayService.GetItemsAsync(new DateTime(2014, 1, 1, 0, 0, 0), new DateTime(2014, 1, 28, 10, 0, 0));

			//A
			orders.Count().Should().BeGreaterThan(0, "because on site there are items started in specified time");
		}

		[Test]
		public void EbayServiceExistingItems_GetItemsSmart_NotEmptyItemsCollection()
		{
			//A
			var ebayService = new EbayService(_credentials.GetEbayCredentials(), _credentials.GetEbayEndPoint());

			//A
			IEnumerable<Item> orders = ebayService.GetItems(new DateTime(2014, 1, 1, 0, 0, 0),
				new DateTime(2014, 1, 28, 10, 0, 0));

			//A
			orders.Count().Should().BeGreaterThan(0, "because on site there are items started in specified time");
		}

		[Test]
		public void EbayServiceExistingItems_GetItems_NotEmptyItemsCollection()
		{
			//A
			var ebayService = new EbayService(_credentials.GetEbayCredentials(), _credentials.GetEbayEndPoint());

			//A
			IEnumerable<Item> orders = ebayService.GetItems(new DateTime(2014, 1, 1, 0, 0, 0),
				new DateTime(2014, 1, 28, 10, 0, 0));

			//A
			orders.Count().Should().BeGreaterThan(0, "because on site there are items started in specified time");
		}

		[Test]
		public async Task EbayServiceWithExistingOrders_GetOrdersAsync_HookInOrders()
		{
			//A

			var ebayService = new EbayService(_credentials.GetEbayCredentials(), _credentials.GetEbayEndPoint());

			//A
			IEnumerable<Order> orders =
				await ebayService.GetOrdersAsync(new DateTime(2014, 1, 1, 0, 0, 0), new DateTime(2014, 1, 21, 10, 0, 0));

			//A
			orders.Count().Should().Be(2, "because on site there is 2 orders");
		}

		[Test]
		public void EbayServiceWithExistingOrders_GetOrders_HookInOrders()
		{
			//A
			var ebayService = new EbayService(_credentials.GetEbayCredentials(), _credentials.GetEbayEndPoint());

			//A
			IEnumerable<Order> orders = ebayService.GetOrders(new DateTime(2014, 1, 1, 0, 0, 0),
				new DateTime(2014, 1, 21, 10, 0, 0));

			//A
			orders.Count().Should().Be(2, "because on site there is 2 orders");
		}

		[Test]
		public async Task EbayServiceWithNotExistingOrders_GetOrdersAsync_EmptyOrdersCollection()
		{
			//A
			var ebayService = new EbayService(_credentials.GetEbayCredentials(), _credentials.GetEbayEndPoint());

			//A
			IEnumerable<Order> orders =
				await ebayService.GetOrdersAsync(new DateTime(1999, 1, 1, 0, 0, 0), new DateTime(1999, 1, 21, 10, 0, 0));

			//A
			orders.Count().Should().Be(0, "because on site there is no orders in specified time");
		}

		[Test]
		public void EbayServiceWithNotExistingOrders_GetOrders_EmptyOrdersCollection()
		{
			//A
			var ebayService = new EbayService(_credentials.GetEbayCredentials(), _credentials.GetEbayEndPoint());

			//A
			IEnumerable<Order> orders = ebayService.GetOrders(new DateTime(1999, 1, 1, 0, 0, 0),
				new DateTime(1999, 1, 21, 10, 0, 0));

			//A
			orders.Count().Should().Be(0, "because on site there is no orders in specified time");
		}
	}
}

