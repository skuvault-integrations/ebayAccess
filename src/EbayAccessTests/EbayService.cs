﻿using System;
using System.Linq;
using EbayAccess;
using EbayAccess.Models;
using EbayAccess.Services;
using FluentAssertions;
using NUnit.Framework;

namespace EbayAccessTests
{
	[TestFixture]
	public class EbayService
	{
		[Test]
		public void EbayServiceWithExistingOrders_GetOrders_HookInOrders()
		{
			EbayCredentials ebayCredentials = new EbayCredentials() { AccountName = null, Token = "AgAAAA**AQAAAA**aAAAAA**Z6PZUg**nY+sHZ2PrBmdj6wVnY+sEZ2PrA2dj6wFk4GhC5eEpg2dj6x9nY+seQ**OX8CAA**AAMAAA**SEIoL5SqnyD4fbOhrRTCxShlrCVPyQEp4R++AkBuR3abexAYvgHkUOJvJ6EIBNvqCyDj9MTbIuft2lY/EJyWeze0NG/zVa1E3wRagdAOZXYGnSYaEJBkcynOEfQ7J8vEbG4dd1NoKixUBARbVH9jBoMHTuDy8Bj36NNvr5/iQbaMm+VnGgezBeerdl5S8M/5EzLpbYk1l6cRWJRmVN41fY/ERwj6dfNdD1JqKnDmuGXjVN4KF4k44UKkAv9Zigx+QWJgXOTFCvbwL8iXni079cZNwL35YA6NC2O8IDm7TKooJwsUhbWjNWO2Rxb5MowYS8ls1X/SRZ4VcRDYnnaeCzhLsUTOGCoUvsKumXn3WkGJhLD7CH671suim3vrl9XB+oyCev22goM3P7wr5uhMknN4mxE178Pyd0F/X2+DbfxgpJyVs/gBV7Ym11bGC6wmPHZO2zSSqVIKdkmLf0Uw8q/aqUEiHDVl8IwuvVXsW7hCbZeBkdRzr5JEkuI0FYZ8e3WS5BcGrvcEJaC0ZjMxAW/LkFktQooy9UckjWp/6l+rVKgeJYsCik/OrPWJKVmekBSUeKYEmm/Mo5QeU6Hqlrz+S3m+WR2NOyc8F0Wqk2zDTNpLlAh/RbhmUoHtmLtdgu9ESwBWz0L9B11ME3rB7udeuaEf9Rd48H77pZ1UKoK9C7mrJMHFNSvLG1Gq6SCWe2KxDij7DvKe5vYmy2rS1sdJDCfBq0GFnUBZOmh+N64KqxkIUY26nPeqm/KoqQ7R" };
			var webRequestServices = new WebRequestServices(ebayCredentials);

			var orders = webRequestServices.GetOrders("https://api.sandbox.ebay.com/ws/api.dll", new DateTime(2014, 1, 1, 0, 0, 0), new DateTime(2014, 1, 21, 10, 0, 0));

			orders.Count().Should().Be(2, "because on seite there is 2 orders");
		}
	}
}