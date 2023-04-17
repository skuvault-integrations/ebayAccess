using EbayAccess.Models.GetOrdersResponse;
using FluentAssertions;
using NUnit.Framework;

namespace EbayAccessTests.Models.GetOrdersResponse
{
	public class OrderExtendedTests
	{
		[ Test ]
		public void GetOrderId_ShouldReturnNull_WhenSalesRecordNumberIsDefault_andUseSellingManagerRecordNumberInsteadIsTrue()
		{
			var order = new Order
			{
				ShippingDetails = new ShippingDetails
				{
					SellingManagerSalesRecordNumber = default
				}
			};

			var result = order.GetOrderId( useSellingManagerRecordNumberInstead: true );

			result.Should().BeNull();
		}
	}
}