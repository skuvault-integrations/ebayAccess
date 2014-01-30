using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EbayAccess;
using EbayAccess.Models;
using EbayAccess.Models.GetOrdersResponse;
using EbayAccess.Models.ReviseInventoryStatusRequest;
using EbayAccess.Services;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace EbayAccessTests
{
	[TestFixture]
	//todo: there are an integration tests here, move they to correspond place!
	public class EbayServiceTest
	{
		[Test]
		public void EbayServiceWithExistingOrders_GetOrders_HookInOrders()
		{
			EbayCredentials ebayCredentials = new EbayCredentials()
			{
				AccountName = null,
				Token =
					"AgAAAA**AQAAAA**aAAAAA**Z6PZUg**nY+sHZ2PrBmdj6wVnY+sEZ2PrA2dj6wFk4GhC5eEpg2dj6x9nY+seQ**OX8CAA**AAMAAA**SEIoL5SqnyD4fbOhrRTCxShlrCVPyQEp4R++AkBuR3abexAYvgHkUOJvJ6EIBNvqCyDj9MTbIuft2lY/EJyWeze0NG/zVa1E3wRagdAOZXYGnSYaEJBkcynOEfQ7J8vEbG4dd1NoKixUBARbVH9jBoMHTuDy8Bj36NNvr5/iQbaMm+VnGgezBeerdl5S8M/5EzLpbYk1l6cRWJRmVN41fY/ERwj6dfNdD1JqKnDmuGXjVN4KF4k44UKkAv9Zigx+QWJgXOTFCvbwL8iXni079cZNwL35YA6NC2O8IDm7TKooJwsUhbWjNWO2Rxb5MowYS8ls1X/SRZ4VcRDYnnaeCzhLsUTOGCoUvsKumXn3WkGJhLD7CH671suim3vrl9XB+oyCev22goM3P7wr5uhMknN4mxE178Pyd0F/X2+DbfxgpJyVs/gBV7Ym11bGC6wmPHZO2zSSqVIKdkmLf0Uw8q/aqUEiHDVl8IwuvVXsW7hCbZeBkdRzr5JEkuI0FYZ8e3WS5BcGrvcEJaC0ZjMxAW/LkFktQooy9UckjWp/6l+rVKgeJYsCik/OrPWJKVmekBSUeKYEmm/Mo5QeU6Hqlrz+S3m+WR2NOyc8F0Wqk2zDTNpLlAh/RbhmUoHtmLtdgu9ESwBWz0L9B11ME3rB7udeuaEf9Rd48H77pZ1UKoK9C7mrJMHFNSvLG1Gq6SCWe2KxDij7DvKe5vYmy2rS1sdJDCfBq0GFnUBZOmh+N64KqxkIUY26nPeqm/KoqQ7R"
			};
			var webRequestServices = new WebRequestServices(ebayCredentials);

			var orders = webRequestServices.GetOrders("https://api.sandbox.ebay.com/ws/api.dll",
				new DateTime(2014, 1, 1, 0, 0, 0), new DateTime(2014, 1, 21, 10, 0, 0));

			orders.Count().Should().Be(2, "because on site there is 2 orders");
		}

		[Test]
		public async Task EbayServiceWithExistingOrders_GetOrdersAsync_HookInOrders()
		{
			EbayCredentials ebayCredentials = new EbayCredentials()
			{
				AccountName = null,
				Token =
					"AgAAAA**AQAAAA**aAAAAA**Z6PZUg**nY+sHZ2PrBmdj6wVnY+sEZ2PrA2dj6wFk4GhC5eEpg2dj6x9nY+seQ**OX8CAA**AAMAAA**SEIoL5SqnyD4fbOhrRTCxShlrCVPyQEp4R++AkBuR3abexAYvgHkUOJvJ6EIBNvqCyDj9MTbIuft2lY/EJyWeze0NG/zVa1E3wRagdAOZXYGnSYaEJBkcynOEfQ7J8vEbG4dd1NoKixUBARbVH9jBoMHTuDy8Bj36NNvr5/iQbaMm+VnGgezBeerdl5S8M/5EzLpbYk1l6cRWJRmVN41fY/ERwj6dfNdD1JqKnDmuGXjVN4KF4k44UKkAv9Zigx+QWJgXOTFCvbwL8iXni079cZNwL35YA6NC2O8IDm7TKooJwsUhbWjNWO2Rxb5MowYS8ls1X/SRZ4VcRDYnnaeCzhLsUTOGCoUvsKumXn3WkGJhLD7CH671suim3vrl9XB+oyCev22goM3P7wr5uhMknN4mxE178Pyd0F/X2+DbfxgpJyVs/gBV7Ym11bGC6wmPHZO2zSSqVIKdkmLf0Uw8q/aqUEiHDVl8IwuvVXsW7hCbZeBkdRzr5JEkuI0FYZ8e3WS5BcGrvcEJaC0ZjMxAW/LkFktQooy9UckjWp/6l+rVKgeJYsCik/OrPWJKVmekBSUeKYEmm/Mo5QeU6Hqlrz+S3m+WR2NOyc8F0Wqk2zDTNpLlAh/RbhmUoHtmLtdgu9ESwBWz0L9B11ME3rB7udeuaEf9Rd48H77pZ1UKoK9C7mrJMHFNSvLG1Gq6SCWe2KxDij7DvKe5vYmy2rS1sdJDCfBq0GFnUBZOmh+N64KqxkIUY26nPeqm/KoqQ7R"
			};
			var webRequestServices = new WebRequestServices(ebayCredentials);

			var orders = await webRequestServices.GetOrdersAsync("https://api.sandbox.ebay.com/ws/api.dll",
				new DateTime(2014, 1, 1, 0, 0, 0), new DateTime(2014, 1, 21, 10, 0, 0));

			orders.Count().Should().Be(2, "because on site there is 2 orders");
		}

		[Test]
		public void EbayServiceWithNotExistingOrders_GetOrders_EmptyOrdersCollection()
		{
			EbayCredentials ebayCredentials = new EbayCredentials()
			{
				AccountName = null,
				Token =
					"AgAAAA**AQAAAA**aAAAAA**Z6PZUg**nY+sHZ2PrBmdj6wVnY+sEZ2PrA2dj6wFk4GhC5eEpg2dj6x9nY+seQ**OX8CAA**AAMAAA**SEIoL5SqnyD4fbOhrRTCxShlrCVPyQEp4R++AkBuR3abexAYvgHkUOJvJ6EIBNvqCyDj9MTbIuft2lY/EJyWeze0NG/zVa1E3wRagdAOZXYGnSYaEJBkcynOEfQ7J8vEbG4dd1NoKixUBARbVH9jBoMHTuDy8Bj36NNvr5/iQbaMm+VnGgezBeerdl5S8M/5EzLpbYk1l6cRWJRmVN41fY/ERwj6dfNdD1JqKnDmuGXjVN4KF4k44UKkAv9Zigx+QWJgXOTFCvbwL8iXni079cZNwL35YA6NC2O8IDm7TKooJwsUhbWjNWO2Rxb5MowYS8ls1X/SRZ4VcRDYnnaeCzhLsUTOGCoUvsKumXn3WkGJhLD7CH671suim3vrl9XB+oyCev22goM3P7wr5uhMknN4mxE178Pyd0F/X2+DbfxgpJyVs/gBV7Ym11bGC6wmPHZO2zSSqVIKdkmLf0Uw8q/aqUEiHDVl8IwuvVXsW7hCbZeBkdRzr5JEkuI0FYZ8e3WS5BcGrvcEJaC0ZjMxAW/LkFktQooy9UckjWp/6l+rVKgeJYsCik/OrPWJKVmekBSUeKYEmm/Mo5QeU6Hqlrz+S3m+WR2NOyc8F0Wqk2zDTNpLlAh/RbhmUoHtmLtdgu9ESwBWz0L9B11ME3rB7udeuaEf9Rd48H77pZ1UKoK9C7mrJMHFNSvLG1Gq6SCWe2KxDij7DvKe5vYmy2rS1sdJDCfBq0GFnUBZOmh+N64KqxkIUY26nPeqm/KoqQ7R"
			};
			var webRequestServices = new WebRequestServices(ebayCredentials);

			var orders = webRequestServices.GetOrders("https://api.sandbox.ebay.com/ws/api.dll",
				new DateTime(1999, 1, 1, 0, 0, 0), new DateTime(1999, 1, 21, 10, 0, 0));

			orders.Count().Should().Be(0, "because on site there is no orders in specified time");
		}

		[Test]
		public async Task EbayServiceWithNotExistingOrders_GetOrdersAsync_EmptyOrdersCollection()
		{
			EbayCredentials ebayCredentials = new EbayCredentials()
			{
				AccountName = null,
				Token =
					"AgAAAA**AQAAAA**aAAAAA**Z6PZUg**nY+sHZ2PrBmdj6wVnY+sEZ2PrA2dj6wFk4GhC5eEpg2dj6x9nY+seQ**OX8CAA**AAMAAA**SEIoL5SqnyD4fbOhrRTCxShlrCVPyQEp4R++AkBuR3abexAYvgHkUOJvJ6EIBNvqCyDj9MTbIuft2lY/EJyWeze0NG/zVa1E3wRagdAOZXYGnSYaEJBkcynOEfQ7J8vEbG4dd1NoKixUBARbVH9jBoMHTuDy8Bj36NNvr5/iQbaMm+VnGgezBeerdl5S8M/5EzLpbYk1l6cRWJRmVN41fY/ERwj6dfNdD1JqKnDmuGXjVN4KF4k44UKkAv9Zigx+QWJgXOTFCvbwL8iXni079cZNwL35YA6NC2O8IDm7TKooJwsUhbWjNWO2Rxb5MowYS8ls1X/SRZ4VcRDYnnaeCzhLsUTOGCoUvsKumXn3WkGJhLD7CH671suim3vrl9XB+oyCev22goM3P7wr5uhMknN4mxE178Pyd0F/X2+DbfxgpJyVs/gBV7Ym11bGC6wmPHZO2zSSqVIKdkmLf0Uw8q/aqUEiHDVl8IwuvVXsW7hCbZeBkdRzr5JEkuI0FYZ8e3WS5BcGrvcEJaC0ZjMxAW/LkFktQooy9UckjWp/6l+rVKgeJYsCik/OrPWJKVmekBSUeKYEmm/Mo5QeU6Hqlrz+S3m+WR2NOyc8F0Wqk2zDTNpLlAh/RbhmUoHtmLtdgu9ESwBWz0L9B11ME3rB7udeuaEf9Rd48H77pZ1UKoK9C7mrJMHFNSvLG1Gq6SCWe2KxDij7DvKe5vYmy2rS1sdJDCfBq0GFnUBZOmh+N64KqxkIUY26nPeqm/KoqQ7R"
			};
			var webRequestServices = new WebRequestServices(ebayCredentials);

			var orders = await webRequestServices.GetOrdersAsync("https://api.sandbox.ebay.com/ws/api.dll",
				new DateTime(1999, 1, 1, 0, 0, 0), new DateTime(1999, 1, 21, 10, 0, 0));

			orders.Count().Should().Be(0, "because on site there is no orders in specified time");
		}

		[Test]
		public void EbayServiceExistingItems_GetItems_NotEmptyItemsCollection()
		{
			EbayCredentials ebayCredentials = new EbayCredentials()
			{
				AccountName = null,
				Token =
					"AgAAAA**AQAAAA**aAAAAA**Z6PZUg**nY+sHZ2PrBmdj6wVnY+sEZ2PrA2dj6wFk4GhC5eEpg2dj6x9nY+seQ**OX8CAA**AAMAAA**SEIoL5SqnyD4fbOhrRTCxShlrCVPyQEp4R++AkBuR3abexAYvgHkUOJvJ6EIBNvqCyDj9MTbIuft2lY/EJyWeze0NG/zVa1E3wRagdAOZXYGnSYaEJBkcynOEfQ7J8vEbG4dd1NoKixUBARbVH9jBoMHTuDy8Bj36NNvr5/iQbaMm+VnGgezBeerdl5S8M/5EzLpbYk1l6cRWJRmVN41fY/ERwj6dfNdD1JqKnDmuGXjVN4KF4k44UKkAv9Zigx+QWJgXOTFCvbwL8iXni079cZNwL35YA6NC2O8IDm7TKooJwsUhbWjNWO2Rxb5MowYS8ls1X/SRZ4VcRDYnnaeCzhLsUTOGCoUvsKumXn3WkGJhLD7CH671suim3vrl9XB+oyCev22goM3P7wr5uhMknN4mxE178Pyd0F/X2+DbfxgpJyVs/gBV7Ym11bGC6wmPHZO2zSSqVIKdkmLf0Uw8q/aqUEiHDVl8IwuvVXsW7hCbZeBkdRzr5JEkuI0FYZ8e3WS5BcGrvcEJaC0ZjMxAW/LkFktQooy9UckjWp/6l+rVKgeJYsCik/OrPWJKVmekBSUeKYEmm/Mo5QeU6Hqlrz+S3m+WR2NOyc8F0Wqk2zDTNpLlAh/RbhmUoHtmLtdgu9ESwBWz0L9B11ME3rB7udeuaEf9Rd48H77pZ1UKoK9C7mrJMHFNSvLG1Gq6SCWe2KxDij7DvKe5vYmy2rS1sdJDCfBq0GFnUBZOmh+N64KqxkIUY26nPeqm/KoqQ7R"
			};
			var webRequestServices = new WebRequestServices(ebayCredentials);

			var orders = webRequestServices.GetItems("https://api.sandbox.ebay.com/ws/api.dll", new DateTime(2014, 1, 1, 0, 0, 0),
				new DateTime(2014, 1, 28, 10, 0, 0));

			orders.Count().Should().BeGreaterThan(0, "because on site there are items started in specified time");
		}

		[Test]
		public async Task EbayServiceExistingItems_GetItemsAsync_NotEmptyItemsCollection()
		{
			//A
			EbayCredentials ebayCredentials = new EbayCredentials()
			{
				AccountName = null,
				Token =
					"AgAAAA**AQAAAA**aAAAAA**Z6PZUg**nY+sHZ2PrBmdj6wVnY+sEZ2PrA2dj6wFk4GhC5eEpg2dj6x9nY+seQ**OX8CAA**AAMAAA**SEIoL5SqnyD4fbOhrRTCxShlrCVPyQEp4R++AkBuR3abexAYvgHkUOJvJ6EIBNvqCyDj9MTbIuft2lY/EJyWeze0NG/zVa1E3wRagdAOZXYGnSYaEJBkcynOEfQ7J8vEbG4dd1NoKixUBARbVH9jBoMHTuDy8Bj36NNvr5/iQbaMm+VnGgezBeerdl5S8M/5EzLpbYk1l6cRWJRmVN41fY/ERwj6dfNdD1JqKnDmuGXjVN4KF4k44UKkAv9Zigx+QWJgXOTFCvbwL8iXni079cZNwL35YA6NC2O8IDm7TKooJwsUhbWjNWO2Rxb5MowYS8ls1X/SRZ4VcRDYnnaeCzhLsUTOGCoUvsKumXn3WkGJhLD7CH671suim3vrl9XB+oyCev22goM3P7wr5uhMknN4mxE178Pyd0F/X2+DbfxgpJyVs/gBV7Ym11bGC6wmPHZO2zSSqVIKdkmLf0Uw8q/aqUEiHDVl8IwuvVXsW7hCbZeBkdRzr5JEkuI0FYZ8e3WS5BcGrvcEJaC0ZjMxAW/LkFktQooy9UckjWp/6l+rVKgeJYsCik/OrPWJKVmekBSUeKYEmm/Mo5QeU6Hqlrz+S3m+WR2NOyc8F0Wqk2zDTNpLlAh/RbhmUoHtmLtdgu9ESwBWz0L9B11ME3rB7udeuaEf9Rd48H77pZ1UKoK9C7mrJMHFNSvLG1Gq6SCWe2KxDij7DvKe5vYmy2rS1sdJDCfBq0GFnUBZOmh+N64KqxkIUY26nPeqm/KoqQ7R"
			};
			var webRequestServices = new WebRequestServices(ebayCredentials);

			var ebayServiceEndpoint = "https://api.sandbox.ebay.com/ws/api.dll";

			var ebayService = new EbayService(ebayCredentials, ebayServiceEndpoint);

			//A
			var orders = await ebayService.GetItemsAsync(new DateTime(2014, 1, 1, 0, 0, 0), new DateTime(2014, 1, 28, 10, 0, 0));

			//A
			orders.Count().Should().BeGreaterThan(0, "because on site there are items started in specified time");
		}

		[Test]
		public void EbayServiceExistingItems_GetItemsSmart_NotEmptyItemsCollection()
		{
			EbayCredentials ebayCredentials = new EbayCredentials()
			{
				AccountName = null,
				Token =
					"AgAAAA**AQAAAA**aAAAAA**Z6PZUg**nY+sHZ2PrBmdj6wVnY+sEZ2PrA2dj6wFk4GhC5eEpg2dj6x9nY+seQ**OX8CAA**AAMAAA**SEIoL5SqnyD4fbOhrRTCxShlrCVPyQEp4R++AkBuR3abexAYvgHkUOJvJ6EIBNvqCyDj9MTbIuft2lY/EJyWeze0NG/zVa1E3wRagdAOZXYGnSYaEJBkcynOEfQ7J8vEbG4dd1NoKixUBARbVH9jBoMHTuDy8Bj36NNvr5/iQbaMm+VnGgezBeerdl5S8M/5EzLpbYk1l6cRWJRmVN41fY/ERwj6dfNdD1JqKnDmuGXjVN4KF4k44UKkAv9Zigx+QWJgXOTFCvbwL8iXni079cZNwL35YA6NC2O8IDm7TKooJwsUhbWjNWO2Rxb5MowYS8ls1X/SRZ4VcRDYnnaeCzhLsUTOGCoUvsKumXn3WkGJhLD7CH671suim3vrl9XB+oyCev22goM3P7wr5uhMknN4mxE178Pyd0F/X2+DbfxgpJyVs/gBV7Ym11bGC6wmPHZO2zSSqVIKdkmLf0Uw8q/aqUEiHDVl8IwuvVXsW7hCbZeBkdRzr5JEkuI0FYZ8e3WS5BcGrvcEJaC0ZjMxAW/LkFktQooy9UckjWp/6l+rVKgeJYsCik/OrPWJKVmekBSUeKYEmm/Mo5QeU6Hqlrz+S3m+WR2NOyc8F0Wqk2zDTNpLlAh/RbhmUoHtmLtdgu9ESwBWz0L9B11ME3rB7udeuaEf9Rd48H77pZ1UKoK9C7mrJMHFNSvLG1Gq6SCWe2KxDij7DvKe5vYmy2rS1sdJDCfBq0GFnUBZOmh+N64KqxkIUY26nPeqm/KoqQ7R"
			};
			var ebayServiceEndpoint = "https://api.sandbox.ebay.com/ws/api.dll";

			EbayService ebayService = new EbayService(ebayCredentials, ebayServiceEndpoint);

			var orders = ebayService.GetItems(new DateTime(2014, 1, 1, 0, 0, 0), new DateTime(2014, 1, 28, 10, 0, 0));

			orders.Count().Should().BeGreaterThan(0, "because on site there are items started in specified time");
		}

		[Test]
		public void EbayServiceExistingItemsDevidedIntoMultiplePages_GetItemsSmart_HookUpItemsFromAllPages()
		{
			//A
			EbayCredentials ebayCredentials = new EbayCredentials
			{
				AccountName = null,
				Token = "AgAAAA**AQAAAA**aAAAAA**Z6PZUg**nY+sHZ2PrBmdj6wVnY+sEZ2PrA2dj6wFk4GhC5eEpg2dj6x9nY+seQ**OX8CAA**AAMAAA**SEIoL5SqnyD4fbOhrRTCxShlrCVPyQEp4R++AkBuR3abexAYvgHkUOJvJ6EIBNvqCyDj9MTbIuft2lY/EJyWeze0NG/zVa1E3wRagdAOZXYGnSYaEJBkcynOEfQ7J8vEbG4dd1NoKixUBARbVH9jBoMHTuDy8Bj36NNvr5/iQbaMm+VnGgezBeerdl5S8M/5EzLpbYk1l6cRWJRmVN41fY/ERwj6dfNdD1JqKnDmuGXjVN4KF4k44UKkAv9Zigx+QWJgXOTFCvbwL8iXni079cZNwL35YA6NC2O8IDm7TKooJwsUhbWjNWO2Rxb5MowYS8ls1X/SRZ4VcRDYnnaeCzhLsUTOGCoUvsKumXn3WkGJhLD7CH671suim3vrl9XB+oyCev22goM3P7wr5uhMknN4mxE178Pyd0F/X2+DbfxgpJyVs/gBV7Ym11bGC6wmPHZO2zSSqVIKdkmLf0Uw8q/aqUEiHDVl8IwuvVXsW7hCbZeBkdRzr5JEkuI0FYZ8e3WS5BcGrvcEJaC0ZjMxAW/LkFktQooy9UckjWp/6l+rVKgeJYsCik/OrPWJKVmekBSUeKYEmm/Mo5QeU6Hqlrz+S3m+WR2NOyc8F0Wqk2zDTNpLlAh/RbhmUoHtmLtdgu9ESwBWz0L9B11ME3rB7udeuaEf9Rd48H77pZ1UKoK9C7mrJMHFNSvLG1Gq6SCWe2KxDij7DvKe5vYmy2rS1sdJDCfBq0GFnUBZOmh+N64KqxkIUY26nPeqm/KoqQ7R"
			};

			var ebayServiceEndpoint = "https://api.sandbox.ebay.com/ws/api.dll";

			int stubCallCounter = 0;
			string[] serverResponsePages =
			{
				"<?xml version=\"1.0\" encoding=\"UTF-8\"?><GetSellerListResponse xmlns=\"urn:ebay:apis:eBLBaseComponents\"><Timestamp>2014-01-29T12:28:01.609Z</Timestamp><Ack>Success</Ack><Version>855</Version><Build>E855_CORE_API_16613206_R1</Build><PaginationResult><TotalNumberOfPages>3</TotalNumberOfPages><TotalNumberOfEntries>3</TotalNumberOfEntries></PaginationResult><HasMoreItems>true</HasMoreItems><ItemArray><Item><AutoPay>false</AutoPay><BuyerProtection>ItemIneligible</BuyerProtection><BuyItNowPrice currencyID=\"USD\">1.0</BuyItNowPrice><Country>US</Country><Currency>USD</Currency><GiftIcon>0</GiftIcon><HitCounter>NoHitCounter</HitCounter><ItemID>110136274391</ItemID><ListingDetails><Adult>false</Adult><BindingAuction>false</BindingAuction><CheckoutEnabled>true</CheckoutEnabled><ConvertedBuyItNowPrice currencyID=\"USD\">1.0</ConvertedBuyItNowPrice><ConvertedStartPrice currencyID=\"USD\">0.4</ConvertedStartPrice><ConvertedReservePrice currencyID=\"USD\">0.0</ConvertedReservePrice><HasReservePrice>false</HasReservePrice><StartTime>2014-01-19T17:13:33.000Z</StartTime><EndTime>2014-01-20T15:50:33.000Z</EndTime><ViewItemURL>http://cgi.sandbox.ebay.com/levis-501-womens-jeans-/110136274391</ViewItemURL><HasUnansweredQuestions>false</HasUnansweredQuestions><HasPublicMessages>false</HasPublicMessages><ViewItemURLForNaturalSearch>http://cgi.sandbox.ebay.com/levis-501-womens-jeans-/110136274391</ViewItemURLForNaturalSearch></ListingDetails><ListingDuration>Days_7</ListingDuration><ListingType>Chinese</ListingType><Location>Seattle, Washington</Location><PaymentMethods>PayPal</PaymentMethods><PayPalEmailAddress>slav-facilitator@agileharbor.com</PayPalEmailAddress><PrimaryCategory><CategoryID>11554</CategoryID><CategoryName>Clothing, Shoes &amp; Accessories:Women's Clothing:Jeans</CategoryName></PrimaryCategory><PrivateListing>false</PrivateListing><Quantity>1</Quantity><ReservePrice currencyID=\"USD\">0.0</ReservePrice><ReviseStatus><ItemRevised>false</ItemRevised></ReviseStatus><SellingStatus><BidCount>1</BidCount><BidIncrement currencyID=\"USD\">0.25</BidIncrement><ConvertedCurrentPrice currencyID=\"USD\">1.0</ConvertedCurrentPrice><CurrentPrice currencyID=\"USD\">1.0</CurrentPrice><HighBidder><AboutMePage>false</AboutMePage><EIASToken>nY+sHZ2PrBmdj6wVnY+sEZ2PrA2dj6wFk4CoAJaGpASdj6x9nY+seQ==</EIASToken><Email>external_api_buyer5@unicorn.qa.ebay.com</Email><FeedbackScore>21</FeedbackScore><PositiveFeedbackPercent>100.0</PositiveFeedbackPercent><FeedbackPrivate>false</FeedbackPrivate><FeedbackRatingStar>Yellow</FeedbackRatingStar><IDVerified>false</IDVerified><eBayGoodStanding>true</eBayGoodStanding><NewUser>false</NewUser><RegistrationDate>1995-01-01T00:00:00.000Z</RegistrationDate><Site>US</Site><Status>Confirmed</Status><UserID>external_api_buyer</UserID><UserIDChanged>false</UserIDChanged><UserIDLastChanged>2010-09-27T22:38:52.000Z</UserIDLastChanged><VATStatus>NoVATTax</VATStatus><BuyerInfo><ShippingAddress><Country>US</Country><PostalCode>98102</PostalCode></ShippingAddress></BuyerInfo><UserAnonymized>false</UserAnonymized></HighBidder><MinimumToBid currencyID=\"USD\">1.25</MinimumToBid><QuantitySold>1</QuantitySold><ReserveMet>true</ReserveMet><SecondChanceEligible>false</SecondChanceEligible><ListingStatus>Completed</ListingStatus><SoldAsBin>true</SoldAsBin></SellingStatus><ShippingDetails><ApplyShippingDiscount>false</ApplyShippingDiscount><CalculatedShippingRate><WeightMajor measurementSystem=\"English\" unit=\"lbs\">0</WeightMajor><WeightMinor measurementSystem=\"English\" unit=\"oz\">0</WeightMinor></CalculatedShippingRate><SalesTax><SalesTaxPercent>0.0</SalesTaxPercent><ShippingIncludedInTax>false</ShippingIncludedInTax></SalesTax><ShippingServiceOptions><ShippingService>USPSParcel</ShippingService><ShippingServiceCost currencyID=\"USD\">1.0</ShippingServiceCost><ShippingServicePriority>1</ShippingServicePriority><ExpeditedService>false</ExpeditedService><ShippingTimeMin>2</ShippingTimeMin><ShippingTimeMax>9</ShippingTimeMax></ShippingServiceOptions><ShippingType>Flat</ShippingType><ThirdPartyCheckout>false</ThirdPartyCheckout><ShippingDiscountProfileID>0</ShippingDiscountProfileID><InternationalShippingDiscountProfileID>0</InternationalShippingDiscountProfileID><SellerExcludeShipToLocationsPreference>false</SellerExcludeShipToLocationsPreference></ShippingDetails><ShipToLocations>US</ShipToLocations><Site>US</Site><StartPrice currencyID=\"USD\">0.4</StartPrice><TimeLeft>PT0S</TimeLeft><Title>levis 501 women's jeans</Title><LocationDefaulted>true</LocationDefaulted><GetItFast>false</GetItFast><PostalCode>98102</PostalCode><PictureDetails><GalleryType>Gallery</GalleryType><GalleryURL>http://i.ebayimg.sandbox.ebay.com/00/s/MTY3WDMwMQ==/$(KGrHqZHJCwFJgTgPdkTBS3!ekFmWg~~60_1.JPG?set_id=8800005007</GalleryURL><PhotoDisplay>PicturePack</PhotoDisplay><PictureURL>http://i.ebayimg.sandbox.ebay.com/00/s/MTY3WDMwMQ==/$(KGrHqZHJCwFJgTgPdkTBS3!ekFmWg~~60_1.JPG?set_id=8800005007</PictureURL></PictureDetails><DispatchTimeMax>3</DispatchTimeMax><ProxyItem>false</ProxyItem><BuyerGuaranteePrice currencyID=\"USD\">20000.0</BuyerGuaranteePrice><BuyerRequirementDetails><ShipToRegistrationCountry>true</ShipToRegistrationCountry><MinimumFeedbackScore>-1</MinimumFeedbackScore><LinkedPayPalAccount>true</LinkedPayPalAccount><MaximumUnpaidItemStrikesInfo><Count>2</Count><Period>Days_30</Period></MaximumUnpaidItemStrikesInfo><MaximumBuyerPolicyViolations><Count>4</Count><Period>Days_30</Period></MaximumBuyerPolicyViolations></BuyerRequirementDetails><ReturnPolicy><ReturnsAcceptedOption>ReturnsNotAccepted</ReturnsAcceptedOption><ReturnsAccepted>No returns accepted</ReturnsAccepted></ReturnPolicy><PaymentAllowedSite>eBayMotors</PaymentAllowedSite><PaymentAllowedSite>CanadaFrench</PaymentAllowedSite><PaymentAllowedSite>Canada</PaymentAllowedSite><PaymentAllowedSite>US</PaymentAllowedSite><ConditionID>1000</ConditionID><ConditionDisplayName>New with tags</ConditionDisplayName><PostCheckoutExperienceEnabled>false</PostCheckoutExperienceEnabled><SellerProfiles /><ShippingPackageDetails><ShippingIrregular>false</ShippingIrregular><ShippingPackage>PackageThickEnvelope</ShippingPackage><WeightMajor measurementSystem=\"English\" unit=\"lbs\">0</WeightMajor><WeightMinor measurementSystem=\"English\" unit=\"oz\">0</WeightMinor></ShippingPackageDetails><HideFromSearch>false</HideFromSearch></Item></ItemArray><ItemsPerPage>1</ItemsPerPage><PageNumber>1</PageNumber><ReturnedItemCountActual>1</ReturnedItemCountActual><Seller><AboutMePage>false</AboutMePage><FeedbackScore>500</FeedbackScore><PositiveFeedbackPercent>0.0</PositiveFeedbackPercent><FeedbackPrivate>false</FeedbackPrivate><FeedbackRatingStar>Purple</FeedbackRatingStar><IDVerified>true</IDVerified><eBayGoodStanding>true</eBayGoodStanding><NewUser>false</NewUser><RegistrationDate>1995-01-01T00:00:00.000Z</RegistrationDate><Site>US</Site><Status>Confirmed</Status><UserID>testuser_sv02</UserID><UserIDChanged>true</UserIDChanged><UserIDLastChanged>2014-01-17T21:08:45.000Z</UserIDLastChanged><VATStatus>NoVATTax</VATStatus><SellerInfo><AllowPaymentEdit>true</AllowPaymentEdit><CheckoutEnabled>true</CheckoutEnabled><CIPBankAccountStored>false</CIPBankAccountStored><GoodStanding>true</GoodStanding><LiveAuctionAuthorized>false</LiveAuctionAuthorized><MerchandizingPref>OptIn</MerchandizingPref><QualifiesForB2BVAT>false</QualifiesForB2BVAT><StoreOwner>true</StoreOwner><StoreURL>http://www.stores.sandbox.ebay.com/id=133037671</StoreURL><SafePaymentExempt>true</SafePaymentExempt></SellerInfo><MotorsDealer>false</MotorsDealer></Seller></GetSellerListResponse>",
				"<?xml version=\"1.0\" encoding=\"UTF-8\"?><GetSellerListResponse xmlns=\"urn:ebay:apis:eBLBaseComponents\"><Timestamp>2014-01-29T18:00:16.504Z</Timestamp><Ack>Success</Ack><Version>855</Version><Build>E855_CORE_API_16613206_R1</Build><PaginationResult><TotalNumberOfPages>3</TotalNumberOfPages><TotalNumberOfEntries>3</TotalNumberOfEntries></PaginationResult><HasMoreItems>true</HasMoreItems><ItemArray><Item><AutoPay>false</AutoPay><BuyerProtection>ItemIneligible</BuyerProtection><BuyItNowPrice currencyID=\"USD\">1.0</BuyItNowPrice><Country>US</Country><Currency>USD</Currency><GiftIcon>0</GiftIcon><HitCounter>NoHitCounter</HitCounter><ItemID>110136348096</ItemID><ListingDetails><Adult>false</Adult><BindingAuction>false</BindingAuction><CheckoutEnabled>true</CheckoutEnabled><ConvertedBuyItNowPrice currencyID=\"USD\">1.0</ConvertedBuyItNowPrice><ConvertedStartPrice currencyID=\"USD\">0.5</ConvertedStartPrice><ConvertedReservePrice currencyID=\"USD\">0.0</ConvertedReservePrice><HasReservePrice>false</HasReservePrice><StartTime>2014-01-20T20:32:41.000Z</StartTime><EndTime>2014-01-20T20:37:33.000Z</EndTime><ViewItemURL>http://cgi.sandbox.ebay.com/levis-501-mans-jeans-/110136348096</ViewItemURL><HasUnansweredQuestions>false</HasUnansweredQuestions><HasPublicMessages>false</HasPublicMessages><ViewItemURLForNaturalSearch>http://cgi.sandbox.ebay.com/levis-501-mans-jeans-/110136348096</ViewItemURLForNaturalSearch></ListingDetails><ListingDuration>Days_7</ListingDuration><ListingType>Chinese</ListingType><Location>Seattle, Washington</Location><PaymentMethods>PayPal</PaymentMethods><PayPalEmailAddress>slav-facilitator@agileharbor.com</PayPalEmailAddress><PrimaryCategory><CategoryID>11483</CategoryID><CategoryName>Clothing, Shoes &amp; Accessories:Men's Clothing:Jeans</CategoryName></PrimaryCategory><PrivateListing>false</PrivateListing><Quantity>1</Quantity><ReservePrice currencyID=\"USD\">0.0</ReservePrice><ReviseStatus><ItemRevised>false</ItemRevised></ReviseStatus><SellingStatus><BidCount>1</BidCount><BidIncrement currencyID=\"USD\">0.25</BidIncrement><ConvertedCurrentPrice currencyID=\"USD\">1.0</ConvertedCurrentPrice><CurrentPrice currencyID=\"USD\">1.0</CurrentPrice><HighBidder><AboutMePage>false</AboutMePage><EIASToken>nY+sHZ2PrBmdj6wVnY+sEZ2PrA2dj6wFk4CoAJaGpASdj6x9nY+seQ==</EIASToken><Email>external_api_buyer5@unicorn.qa.ebay.com</Email><FeedbackScore>21</FeedbackScore><PositiveFeedbackPercent>100.0</PositiveFeedbackPercent><FeedbackPrivate>false</FeedbackPrivate><FeedbackRatingStar>Yellow</FeedbackRatingStar><IDVerified>false</IDVerified><eBayGoodStanding>true</eBayGoodStanding><NewUser>false</NewUser><RegistrationDate>1995-01-01T00:00:00.000Z</RegistrationDate><Site>US</Site><Status>Confirmed</Status><UserID>external_api_buyer</UserID><UserIDChanged>false</UserIDChanged><UserIDLastChanged>2010-09-27T22:38:52.000Z</UserIDLastChanged><VATStatus>NoVATTax</VATStatus><BuyerInfo><ShippingAddress><Country>US</Country><PostalCode>98102</PostalCode></ShippingAddress></BuyerInfo><UserAnonymized>false</UserAnonymized></HighBidder><MinimumToBid currencyID=\"USD\">1.25</MinimumToBid><QuantitySold>1</QuantitySold><ReserveMet>true</ReserveMet><SecondChanceEligible>false</SecondChanceEligible><ListingStatus>Completed</ListingStatus><SoldAsBin>true</SoldAsBin></SellingStatus><ShippingDetails><ApplyShippingDiscount>false</ApplyShippingDiscount><CalculatedShippingRate><WeightMajor measurementSystem=\"English\" unit=\"lbs\">0</WeightMajor><WeightMinor measurementSystem=\"English\" unit=\"oz\">0</WeightMinor></CalculatedShippingRate><SalesTax><SalesTaxPercent>0.0</SalesTaxPercent><ShippingIncludedInTax>false</ShippingIncludedInTax></SalesTax><ShippingServiceOptions><ShippingService>USPSParcel</ShippingService><ShippingServiceCost currencyID=\"USD\">1.0</ShippingServiceCost><ShippingServicePriority>1</ShippingServicePriority><ExpeditedService>false</ExpeditedService><ShippingTimeMin>2</ShippingTimeMin><ShippingTimeMax>9</ShippingTimeMax></ShippingServiceOptions><ShippingType>Flat</ShippingType><ThirdPartyCheckout>false</ThirdPartyCheckout><ShippingDiscountProfileID>0</ShippingDiscountProfileID><InternationalShippingDiscountProfileID>0</InternationalShippingDiscountProfileID><SellerExcludeShipToLocationsPreference>false</SellerExcludeShipToLocationsPreference></ShippingDetails><ShipToLocations>US</ShipToLocations><Site>US</Site><StartPrice currencyID=\"USD\">0.5</StartPrice><Storefront><StoreCategoryID>1</StoreCategoryID><StoreCategory2ID>0</StoreCategory2ID><StoreURL>http://www.stores.sandbox.ebay.com/id=133037671</StoreURL></Storefront><TimeLeft>PT0S</TimeLeft><Title>levis 501 man's jeans</Title><LocationDefaulted>true</LocationDefaulted><GetItFast>false</GetItFast><PostalCode>98102</PostalCode><PictureDetails><GalleryType>Gallery</GalleryType><GalleryURL>http://i.ebayimg.sandbox.ebay.com/00/s/MjU4WDMzOQ==/$(KGrHqNHJBUFJ)PSNFp9BS3YdisUsQ~~60_1.JPG?set_id=8800005007</GalleryURL><PhotoDisplay>PicturePack</PhotoDisplay><PictureURL>http://i.ebayimg.sandbox.ebay.com/00/s/MjU4WDMzOQ==/$(KGrHqNHJBUFJ)PSNFp9BS3YdisUsQ~~60_1.JPG?set_id=8800005007</PictureURL></PictureDetails><DispatchTimeMax>3</DispatchTimeMax><ProxyItem>false</ProxyItem><BuyerGuaranteePrice currencyID=\"USD\">20000.0</BuyerGuaranteePrice><BuyerRequirementDetails><MinimumFeedbackScore>-1</MinimumFeedbackScore><MaximumUnpaidItemStrikesInfo><Count>2</Count><Period>Days_30</Period></MaximumUnpaidItemStrikesInfo><MaximumBuyerPolicyViolations><Count>4</Count><Period>Days_30</Period></MaximumBuyerPolicyViolations></BuyerRequirementDetails><ReturnPolicy><ReturnsAcceptedOption>ReturnsNotAccepted</ReturnsAcceptedOption><ReturnsAccepted>No returns accepted</ReturnsAccepted></ReturnPolicy><PaymentAllowedSite>eBayMotors</PaymentAllowedSite><PaymentAllowedSite>CanadaFrench</PaymentAllowedSite><PaymentAllowedSite>Canada</PaymentAllowedSite><PaymentAllowedSite>US</PaymentAllowedSite><ConditionID>1000</ConditionID><ConditionDisplayName>New with tags</ConditionDisplayName><PostCheckoutExperienceEnabled>false</PostCheckoutExperienceEnabled><SellerProfiles /><ShippingPackageDetails><ShippingIrregular>false</ShippingIrregular><ShippingPackage>PackageThickEnvelope</ShippingPackage><WeightMajor measurementSystem=\"English\" unit=\"lbs\">0</WeightMajor><WeightMinor measurementSystem=\"English\" unit=\"oz\">0</WeightMinor></ShippingPackageDetails><HideFromSearch>false</HideFromSearch></Item></ItemArray><ItemsPerPage>1</ItemsPerPage><PageNumber>2</PageNumber><ReturnedItemCountActual>1</ReturnedItemCountActual><Seller><AboutMePage>false</AboutMePage><FeedbackScore>500</FeedbackScore><PositiveFeedbackPercent>0.0</PositiveFeedbackPercent><FeedbackPrivate>false</FeedbackPrivate><FeedbackRatingStar>Purple</FeedbackRatingStar><IDVerified>true</IDVerified><eBayGoodStanding>true</eBayGoodStanding><NewUser>false</NewUser><RegistrationDate>1995-01-01T00:00:00.000Z</RegistrationDate><Site>US</Site><Status>Confirmed</Status><UserID>testuser_sv02</UserID><UserIDChanged>true</UserIDChanged><UserIDLastChanged>2014-01-17T21:08:45.000Z</UserIDLastChanged><VATStatus>NoVATTax</VATStatus><SellerInfo><AllowPaymentEdit>true</AllowPaymentEdit><CheckoutEnabled>true</CheckoutEnabled><CIPBankAccountStored>false</CIPBankAccountStored><GoodStanding>true</GoodStanding><LiveAuctionAuthorized>false</LiveAuctionAuthorized><MerchandizingPref>OptIn</MerchandizingPref><QualifiesForB2BVAT>false</QualifiesForB2BVAT><StoreOwner>true</StoreOwner><StoreURL>http://www.stores.sandbox.ebay.com/id=133037671</StoreURL><SafePaymentExempt>true</SafePaymentExempt></SellerInfo><MotorsDealer>false</MotorsDealer></Seller></GetSellerListResponse>",
				"<?xml version=\"1.0\" encoding=\"UTF-8\"?><GetSellerListResponse xmlns=\"urn:ebay:apis:eBLBaseComponents\"><Timestamp>2014-01-29T18:08:21.010Z</Timestamp><Ack>Success</Ack><Version>855</Version><Build>E855_CORE_API_16613206_R1</Build><PaginationResult><TotalNumberOfPages>3</TotalNumberOfPages><TotalNumberOfEntries>3</TotalNumberOfEntries></PaginationResult><HasMoreItems>false</HasMoreItems><ItemArray><Item><AutoPay>false</AutoPay><BuyerProtection>ItemIneligible</BuyerProtection><BuyItNowPrice currencyID=\"USD\">0.0</BuyItNowPrice><Country>US</Country><Currency>USD</Currency><GiftIcon>0</GiftIcon><HitCounter>NoHitCounter</HitCounter><ItemID>110136942332</ItemID><ListingDetails><Adult>false</Adult><BindingAuction>false</BindingAuction><CheckoutEnabled>true</CheckoutEnabled><ConvertedBuyItNowPrice currencyID=\"USD\">0.0</ConvertedBuyItNowPrice><ConvertedStartPrice currencyID=\"USD\">1.0</ConvertedStartPrice><ConvertedReservePrice currencyID=\"USD\">0.0</ConvertedReservePrice><HasReservePrice>false</HasReservePrice><StartTime>2014-01-27T21:12:54.000Z</StartTime><EndTime>2014-02-03T21:12:54.000Z</EndTime><ViewItemURL>http://cgi.sandbox.ebay.com/levis-501-kids-jeans-/110136942332</ViewItemURL><HasUnansweredQuestions>false</HasUnansweredQuestions><HasPublicMessages>false</HasPublicMessages><ViewItemURLForNaturalSearch>http://cgi.sandbox.ebay.com/levis-501-kids-jeans-/110136942332</ViewItemURLForNaturalSearch></ListingDetails><ListingDuration>Days_7</ListingDuration><ListingType>FixedPriceItem</ListingType><Location>Seattle, Washington</Location><PaymentMethods>PayPal</PaymentMethods><PayPalEmailAddress>slav-facilitator@agileharbor.com</PayPalEmailAddress><PrimaryCategory><CategoryID>11554</CategoryID><CategoryName>Clothing, Shoes &amp; Accessories:Women's Clothing:Jeans</CategoryName></PrimaryCategory><PrivateListing>false</PrivateListing><Quantity>100</Quantity><ReservePrice currencyID=\"USD\">0.0</ReservePrice><ReviseStatus><ItemRevised>false</ItemRevised></ReviseStatus><SellingStatus><BidCount>0</BidCount><BidIncrement currencyID=\"USD\">0.0</BidIncrement><ConvertedCurrentPrice currencyID=\"USD\">1.0</ConvertedCurrentPrice><CurrentPrice currencyID=\"USD\">1.0</CurrentPrice><HighBidder><AboutMePage>false</AboutMePage><EIASToken>nY+sHZ2PrBmdj6wVnY+sEZ2PrA2dj6wFk4CoAJaGpASdj6x9nY+seQ==</EIASToken><Email>external_api_buyer5@unicorn.qa.ebay.com</Email><FeedbackScore>21</FeedbackScore><PositiveFeedbackPercent>100.0</PositiveFeedbackPercent><FeedbackPrivate>false</FeedbackPrivate><FeedbackRatingStar>Yellow</FeedbackRatingStar><IDVerified>false</IDVerified><eBayGoodStanding>true</eBayGoodStanding><NewUser>false</NewUser><RegistrationDate>1995-01-01T00:00:00.000Z</RegistrationDate><Site>US</Site><Status>Confirmed</Status><UserID>external_api_buyer</UserID><UserIDChanged>false</UserIDChanged><UserIDLastChanged>2010-09-27T22:38:52.000Z</UserIDLastChanged><VATStatus>NoVATTax</VATStatus><BuyerInfo><ShippingAddress><Country>US</Country><PostalCode>98102</PostalCode></ShippingAddress></BuyerInfo><UserAnonymized>false</UserAnonymized></HighBidder><MinimumToBid currencyID=\"USD\">1.0</MinimumToBid><QuantitySold>1</QuantitySold><ReserveMet>true</ReserveMet><SecondChanceEligible>false</SecondChanceEligible><ListingStatus>Active</ListingStatus></SellingStatus><ShippingDetails><ApplyShippingDiscount>false</ApplyShippingDiscount><CalculatedShippingRate><WeightMajor measurementSystem=\"English\" unit=\"lbs\">0</WeightMajor><WeightMinor measurementSystem=\"English\" unit=\"oz\">0</WeightMinor></CalculatedShippingRate><SalesTax><SalesTaxPercent>0.0</SalesTaxPercent><ShippingIncludedInTax>false</ShippingIncludedInTax></SalesTax><ShippingServiceOptions><ShippingService>USPSParcel</ShippingService><ShippingServiceCost currencyID=\"USD\">1.0</ShippingServiceCost><ShippingServiceAdditionalCost currencyID=\"USD\">1.0</ShippingServiceAdditionalCost><ShippingServicePriority>1</ShippingServicePriority><ExpeditedService>false</ExpeditedService><ShippingTimeMin>2</ShippingTimeMin><ShippingTimeMax>9</ShippingTimeMax></ShippingServiceOptions><ShippingType>Flat</ShippingType><ThirdPartyCheckout>false</ThirdPartyCheckout><ShippingDiscountProfileID>0</ShippingDiscountProfileID><InternationalShippingDiscountProfileID>0</InternationalShippingDiscountProfileID><SellerExcludeShipToLocationsPreference>false</SellerExcludeShipToLocationsPreference></ShippingDetails><ShipToLocations>US</ShipToLocations><Site>US</Site><StartPrice currencyID=\"USD\">1.0</StartPrice><Storefront><StoreCategoryID>1</StoreCategoryID><StoreCategory2ID>0</StoreCategory2ID><StoreURL>http://www.stores.sandbox.ebay.com/id=133037671</StoreURL></Storefront><TimeLeft>P5DT3H4M34S</TimeLeft><Title>levis 501 kids jeans</Title><LocationDefaulted>true</LocationDefaulted><GetItFast>false</GetItFast><PostalCode>98102</PostalCode><PictureDetails><GalleryType>Gallery</GalleryType><GalleryURL>http://i.ebayimg.sandbox.ebay.com/00/s/MTY3WDMwMQ==/$(KGrHqZHJCwFJgTgPdkTBS3!ekFmWg~~60_1.JPG?set_id=8800005007</GalleryURL><PhotoDisplay>PicturePack</PhotoDisplay><PictureURL>http://i.ebayimg.sandbox.ebay.com/00/s/MTY3WDMwMQ==/$(KGrHqZHJCwFJgTgPdkTBS3!ekFmWg~~60_1.JPG?set_id=8800005007</PictureURL></PictureDetails><DispatchTimeMax>3</DispatchTimeMax><ProxyItem>false</ProxyItem><BuyerGuaranteePrice currencyID=\"USD\">20000.0</BuyerGuaranteePrice><BuyerRequirementDetails><ShipToRegistrationCountry>true</ShipToRegistrationCountry><MinimumFeedbackScore>-1</MinimumFeedbackScore><LinkedPayPalAccount>true</LinkedPayPalAccount><MaximumUnpaidItemStrikesInfo><Count>2</Count><Period>Days_30</Period></MaximumUnpaidItemStrikesInfo><MaximumBuyerPolicyViolations><Count>4</Count><Period>Days_30</Period></MaximumBuyerPolicyViolations></BuyerRequirementDetails><ReturnPolicy><ReturnsAcceptedOption>ReturnsNotAccepted</ReturnsAcceptedOption><ReturnsAccepted>No returns accepted</ReturnsAccepted></ReturnPolicy><PaymentAllowedSite>eBayMotors</PaymentAllowedSite><PaymentAllowedSite>CanadaFrench</PaymentAllowedSite><PaymentAllowedSite>Canada</PaymentAllowedSite><PaymentAllowedSite>US</PaymentAllowedSite><ConditionID>1000</ConditionID><ConditionDisplayName>New with tags</ConditionDisplayName><PostCheckoutExperienceEnabled>false</PostCheckoutExperienceEnabled><SellerProfiles /><ShippingPackageDetails><ShippingIrregular>false</ShippingIrregular><ShippingPackage>PackageThickEnvelope</ShippingPackage><WeightMajor measurementSystem=\"English\" unit=\"lbs\">0</WeightMajor><WeightMinor measurementSystem=\"English\" unit=\"oz\">0</WeightMinor></ShippingPackageDetails><RelistParentID>110136274391</RelistParentID><HideFromSearch>false</HideFromSearch></Item></ItemArray><ItemsPerPage>1</ItemsPerPage><PageNumber>3</PageNumber><ReturnedItemCountActual>1</ReturnedItemCountActual><Seller><AboutMePage>false</AboutMePage><FeedbackScore>500</FeedbackScore><PositiveFeedbackPercent>0.0</PositiveFeedbackPercent><FeedbackPrivate>false</FeedbackPrivate><FeedbackRatingStar>Purple</FeedbackRatingStar><IDVerified>true</IDVerified><eBayGoodStanding>true</eBayGoodStanding><NewUser>false</NewUser><RegistrationDate>1995-01-01T00:00:00.000Z</RegistrationDate><Site>US</Site><Status>Confirmed</Status><UserID>testuser_sv02</UserID><UserIDChanged>true</UserIDChanged><UserIDLastChanged>2014-01-17T21:08:45.000Z</UserIDLastChanged><VATStatus>NoVATTax</VATStatus><SellerInfo><AllowPaymentEdit>true</AllowPaymentEdit><CheckoutEnabled>true</CheckoutEnabled><CIPBankAccountStored>false</CIPBankAccountStored><GoodStanding>true</GoodStanding><LiveAuctionAuthorized>false</LiveAuctionAuthorized><MerchandizingPref>OptIn</MerchandizingPref><QualifiesForB2BVAT>false</QualifiesForB2BVAT><StoreOwner>true</StoreOwner><StoreURL>http://www.stores.sandbox.ebay.com/id=133037671</StoreURL><SafePaymentExempt>true</SafePaymentExempt></SellerInfo><MotorsDealer>false</MotorsDealer></Seller></GetSellerListResponse>"
			};

			var stub = new Mock<IWebRequestServices>();
			stub.Setup(x => x.GetResponseStream(It.IsAny<WebRequest>())).Returns(() =>
			{
				MemoryStream ms = new MemoryStream();
				UTF8Encoding encoding = new UTF8Encoding();
				byte[] buf = encoding.GetBytes(serverResponsePages[stubCallCounter]);
				ms.Write(buf, 0, buf.Length);
				ms.Position = 0;
				return ms;
			}).Callback(() => stubCallCounter++);
			stub.Setup(x => x.CreateEbayStandartPostRequest(It.IsAny<string>(), It.IsAny<List<Tuple<string, string>>>(), It.IsAny<string>())).Returns(() =>
			{
				return null;
			});

			EbayService ebayService = new EbayService(ebayCredentials, ebayServiceEndpoint, stub.Object);

			//A
			var orders = ebayService.GetItems(new DateTime(2014, 1, 1, 0, 0, 0), new DateTime(2014, 1, 28, 10, 0, 0));

			//A
			orders.Count().Should().Be(3, "because sutb gives 3 pages, 1 item per page");
		}

		[Test]
		public void EbayServiceWithExistingInventoryItesm_UpdateItemQuantity_QuantityUpdated()
		{
			//A
			EbayCredentials ebayCredentials = new EbayCredentials
			{
				AccountName = null,
				Token = "AgAAAA**AQAAAA**aAAAAA**Z6PZUg**nY+sHZ2PrBmdj6wVnY+sEZ2PrA2dj6wFk4GhC5eEpg2dj6x9nY+seQ**OX8CAA**AAMAAA**SEIoL5SqnyD4fbOhrRTCxShlrCVPyQEp4R++AkBuR3abexAYvgHkUOJvJ6EIBNvqCyDj9MTbIuft2lY/EJyWeze0NG/zVa1E3wRagdAOZXYGnSYaEJBkcynOEfQ7J8vEbG4dd1NoKixUBARbVH9jBoMHTuDy8Bj36NNvr5/iQbaMm+VnGgezBeerdl5S8M/5EzLpbYk1l6cRWJRmVN41fY/ERwj6dfNdD1JqKnDmuGXjVN4KF4k44UKkAv9Zigx+QWJgXOTFCvbwL8iXni079cZNwL35YA6NC2O8IDm7TKooJwsUhbWjNWO2Rxb5MowYS8ls1X/SRZ4VcRDYnnaeCzhLsUTOGCoUvsKumXn3WkGJhLD7CH671suim3vrl9XB+oyCev22goM3P7wr5uhMknN4mxE178Pyd0F/X2+DbfxgpJyVs/gBV7Ym11bGC6wmPHZO2zSSqVIKdkmLf0Uw8q/aqUEiHDVl8IwuvVXsW7hCbZeBkdRzr5JEkuI0FYZ8e3WS5BcGrvcEJaC0ZjMxAW/LkFktQooy9UckjWp/6l+rVKgeJYsCik/OrPWJKVmekBSUeKYEmm/Mo5QeU6Hqlrz+S3m+WR2NOyc8F0Wqk2zDTNpLlAh/RbhmUoHtmLtdgu9ESwBWz0L9B11ME3rB7udeuaEf9Rd48H77pZ1UKoK9C7mrJMHFNSvLG1Gq6SCWe2KxDij7DvKe5vYmy2rS1sdJDCfBq0GFnUBZOmh+N64KqxkIUY26nPeqm/KoqQ7R"
			};

			var ebayServiceEndpoint = "https://api.sandbox.ebay.com/ws/api.dll";

			var ebayService = new EbayService(ebayCredentials, ebayServiceEndpoint);

			//A
			var inventoryStat = ebayService.ReviseInventoryStatus(new InventoryStatus() {ItemID = 110136942332, Quantity = 300});

			inventoryStat.Quantity.Should().Be(301, "because 1 item sold, added 300.");
		}

		[Test]
		public async Task EbayServiceWithExistingInventoryItem_UpdateItemQuantityAsync_QuantityUpdated()
		{
			//A
			EbayCredentials ebayCredentials = new EbayCredentials
			{
				AccountName = null,
				Token = "AgAAAA**AQAAAA**aAAAAA**Z6PZUg**nY+sHZ2PrBmdj6wVnY+sEZ2PrA2dj6wFk4GhC5eEpg2dj6x9nY+seQ**OX8CAA**AAMAAA**SEIoL5SqnyD4fbOhrRTCxShlrCVPyQEp4R++AkBuR3abexAYvgHkUOJvJ6EIBNvqCyDj9MTbIuft2lY/EJyWeze0NG/zVa1E3wRagdAOZXYGnSYaEJBkcynOEfQ7J8vEbG4dd1NoKixUBARbVH9jBoMHTuDy8Bj36NNvr5/iQbaMm+VnGgezBeerdl5S8M/5EzLpbYk1l6cRWJRmVN41fY/ERwj6dfNdD1JqKnDmuGXjVN4KF4k44UKkAv9Zigx+QWJgXOTFCvbwL8iXni079cZNwL35YA6NC2O8IDm7TKooJwsUhbWjNWO2Rxb5MowYS8ls1X/SRZ4VcRDYnnaeCzhLsUTOGCoUvsKumXn3WkGJhLD7CH671suim3vrl9XB+oyCev22goM3P7wr5uhMknN4mxE178Pyd0F/X2+DbfxgpJyVs/gBV7Ym11bGC6wmPHZO2zSSqVIKdkmLf0Uw8q/aqUEiHDVl8IwuvVXsW7hCbZeBkdRzr5JEkuI0FYZ8e3WS5BcGrvcEJaC0ZjMxAW/LkFktQooy9UckjWp/6l+rVKgeJYsCik/OrPWJKVmekBSUeKYEmm/Mo5QeU6Hqlrz+S3m+WR2NOyc8F0Wqk2zDTNpLlAh/RbhmUoHtmLtdgu9ESwBWz0L9B11ME3rB7udeuaEf9Rd48H77pZ1UKoK9C7mrJMHFNSvLG1Gq6SCWe2KxDij7DvKe5vYmy2rS1sdJDCfBq0GFnUBZOmh+N64KqxkIUY26nPeqm/KoqQ7R"
			};

			var ebayServiceEndpoint = "https://api.sandbox.ebay.com/ws/api.dll";

			var ebayService = new EbayService(ebayCredentials, ebayServiceEndpoint);

			const int qty1 = 100;

			const int qty2 = 200;

			const long itemId = 110136942332;

			//A
			var inventoryStat1 = await ebayService.ReviseInventoryStatusAsync(new InventoryStatus() { ItemID = itemId, Quantity = qty1 });
			var inventoryStat2 = await ebayService.ReviseInventoryStatusAsync(new InventoryStatus() { ItemID = itemId, Quantity = qty2 });

			//A
			(inventoryStat1.Quantity - inventoryStat2.Quantity).Should().Be(qty1 - qty2, String.Format("because we set 1 qty {0}, then set 2 qty{1}", qty1, qty2));
		}
	}
}
