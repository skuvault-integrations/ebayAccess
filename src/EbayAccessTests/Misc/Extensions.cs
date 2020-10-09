using System;
using System.Collections.Generic;
using System.Text;
using EbayAccess.Misc;
using EbayAccess.Models;
using EbayAccess.Models.BaseResponse;
using EbayAccess.Models.GetOrdersResponse;
using EbayAccess.Models.ReviseFixedPriceItemRequest;
using EbayAccess.Models.ReviseInventoryStatusRequest;
using FluentAssertions;
using NUnit.Framework;
using Item = EbayAccess.Models.ReviseFixedPriceItemResponse.Item;

namespace EbayAccessTests.Misc
{
	[ TestFixture ]
	public class Extensions
	{
		/// <summary>
		/// Ebay accepts format: YYYY-MM-DDTHH:MM:SS.SSSZ
		/// </summary>
		[ Test ]
		public void ToStringIso8601_DateTimeInUtc_StringRepresenatOfDateTimeInUtcEbay8601Format()
		{
			//A
			var testDate = new DateTime( 2004, 8, 4, 19, 09, 02, 768, DateTimeKind.Utc );

			//A
			var testDateIso8601StringRepresentation = testDate.ToStringUtcIso8601();

			//A
			testDateIso8601StringRepresentation.Should().Be( "2004-08-04T19:09:02.768Z", "It is the right representation of iso 8601 format" );
		}

		[ Test ]
		public void ToJson_OrderList_StringReturnedNowExceptionOccured()
		{
			//A
			IEnumerable< Order > nullOrderList = null;
			IEnumerable< Order > OrderListWithEmptyOrder = new List< Order >() { new Order() { } };
			IEnumerable< Order > OrderListWithNullOrder = new List< Order >() { null };

			//A
			var str1 = "";
			var str2 = "";
			var str3 = "";
			Action act1 = () => str1 = nullOrderList.ToJson();
			Action act2 = () => str2 = OrderListWithEmptyOrder.ToJson();
			Action act3 = () => str3 = OrderListWithNullOrder.ToJson();

			//A
			act1.ShouldNotThrow< Exception >();
			act2.ShouldNotThrow< Exception >();
			act3.ShouldNotThrow< Exception >();
			str1.Should().NotBeNullOrWhiteSpace();
			str2.Should().NotBeNullOrWhiteSpace();
			str3.Should().NotBeNullOrWhiteSpace();
		}

		[ Test ]
		public void ToJson_StringList_StringReturnedNowExceptionOccured()
		{
			//A
			IEnumerable< string > list = null;
			IEnumerable< string > listWithEmptyString = new List< string >() { string.Empty };
			IEnumerable< string > listWithNullString = new List< string >() { null };

			//A
			var str1 = "";
			var str2 = "";
			var str3 = "";
			Action act1 = () => str1 = list.ToJson();
			Action act2 = () => str2 = listWithEmptyString.ToJson();
			Action act3 = () => str3 = listWithNullString.ToJson();

			//A
			act1.ShouldNotThrow< Exception >();
			act2.ShouldNotThrow< Exception >();
			act3.ShouldNotThrow< Exception >();
			str1.Should().NotBeNullOrWhiteSpace();
			str2.Should().NotBeNullOrWhiteSpace();
			str3.Should().NotBeNullOrWhiteSpace();
		}

		[ Test ]
		public void ToJson_DictionaryList_StringReturnedNowExceptionOccured()
		{
			//A
			Dictionary< string, string > list = null;
			var listWithEmptyString = new Dictionary< string, string >() { };

			//A
			var str1 = "";
			var str2 = "";
			Action act1 = () => str1 = list.ToJson();
			Action act2 = () => str2 = listWithEmptyString.ToJson();

			//A
			act1.ShouldNotThrow< Exception >();
			act2.ShouldNotThrow< Exception >();
			str1.Should().NotBeNullOrWhiteSpace();
			str2.Should().NotBeNullOrWhiteSpace();
		}

		[ Test ]
		public void ToJson_ResponseErrorList_StringReturnedNowExceptionOccured()
		{
			//A
			IEnumerable< ResponseError > list = null;
			IEnumerable< ResponseError > listWithEmptyString = new List< ResponseError >() { new ResponseError() };
			IEnumerable< ResponseError > listWithNullString = new List< ResponseError >() { null };

			//A
			var str1 = "";
			var str2 = "";
			var str3 = "";
			Action act1 = () => str1 = list.ToJson();
			Action act2 = () => str2 = listWithEmptyString.ToJson();
			Action act3 = () => str3 = listWithNullString.ToJson();

			//A
			act1.ShouldNotThrow< Exception >();
			act2.ShouldNotThrow< Exception >();
			act3.ShouldNotThrow< Exception >();
			str1.Should().NotBeNullOrWhiteSpace();
			str2.Should().NotBeNullOrWhiteSpace();
			str3.Should().NotBeNullOrWhiteSpace();
		}

		[ Test ]
		public void ToJson_ItemList_StringReturnedNowExceptionOccured()
		{
			//A
			IEnumerable< Item > list = null;
			IEnumerable< Item > listWithEmptyItem = new List< Item >() { new Item() };
			IEnumerable< Item > listWithNullItem = new List< Item >() { null };

			//A
			var str1 = "";
			var str2 = "";
			var str3 = "";
			Action act1 = () => str1 = list.ToJson();
			Action act2 = () => str2 = listWithEmptyItem.ToJson();
			Action act3 = () => str3 = listWithNullItem.ToJson();

			//A
			act1.ShouldNotThrow< Exception >();
			act2.ShouldNotThrow< Exception >();
			act3.ShouldNotThrow< Exception >();
			str1.Should().NotBeNullOrWhiteSpace();
			str2.Should().NotBeNullOrWhiteSpace();
			str3.Should().NotBeNullOrWhiteSpace();
		}

		[ Test ]
		public void ToJson_UpdateInventoryRequestList_StringReturnedNowExceptionOccured()
		{
			//A
			IEnumerable< UpdateInventoryRequest > list = null;
			IEnumerable< UpdateInventoryRequest > listWithEmptyRequest = new List< UpdateInventoryRequest >() { new UpdateInventoryRequest() };
			IEnumerable< UpdateInventoryRequest > listWithNullRequest = new List< UpdateInventoryRequest >() { null };

			//A
			var str1 = "";
			var str2 = "";
			var str3 = "";
			Action act1 = () => str1 = list.ToJson();
			Action act2 = () => str2 = listWithEmptyRequest.ToJson();
			Action act3 = () => str3 = listWithNullRequest.ToJson();

			//A
			act1.ShouldNotThrow< Exception >();
			act2.ShouldNotThrow< Exception >();
			act3.ShouldNotThrow< Exception >();
			str1.Should().NotBeNullOrWhiteSpace();
			str2.Should().NotBeNullOrWhiteSpace();
			str3.Should().NotBeNullOrWhiteSpace();
		}

		[ Test ]
		public void ToJson_UpdateInventoryResponseList_StringReturnedNowExceptionOccured()
		{
			//A
			IEnumerable< UpdateInventoryResponse > list = null;
			IEnumerable< UpdateInventoryResponse > listWithEmptyString = new List< UpdateInventoryResponse >() { new UpdateInventoryResponse() };
			IEnumerable< UpdateInventoryResponse > listWithNullString = new List< UpdateInventoryResponse >() { null };

			//A
			var str1 = "";
			var str2 = "";
			var str3 = "";
			Action act1 = () => str1 = list.ToJson();
			Action act2 = () => str2 = listWithEmptyString.ToJson();
			Action act3 = () => str3 = listWithNullString.ToJson();

			//A
			act1.ShouldNotThrow< Exception >();
			act2.ShouldNotThrow< Exception >();
			act3.ShouldNotThrow< Exception >();
			str1.Should().NotBeNullOrWhiteSpace();
			str2.Should().NotBeNullOrWhiteSpace();
			str3.Should().NotBeNullOrWhiteSpace();
		}

		[ Test ]
		public void ToJson_ReviseFixedPriceItemRequestList_StringReturnedNowExceptionOccured()
		{
			//A
			IEnumerable< ReviseFixedPriceItemRequest > list = null;
			IEnumerable< ReviseFixedPriceItemRequest > listWithEmptyRequest = new List< ReviseFixedPriceItemRequest >() { new ReviseFixedPriceItemRequest() };
			IEnumerable< ReviseFixedPriceItemRequest > listWithNullRequest = new List< ReviseFixedPriceItemRequest >() { null };

			//A
			var str1 = "";
			var str2 = "";
			var str3 = "";
			Action act1 = () => str1 = list.ToJson();
			Action act2 = () => str2 = listWithEmptyRequest.ToJson();
			Action act3 = () => str3 = listWithNullRequest.ToJson();

			//A
			act1.ShouldNotThrow< Exception >();
			act2.ShouldNotThrow< Exception >();
			act3.ShouldNotThrow< Exception >();
			str1.Should().NotBeNullOrWhiteSpace();
			str2.Should().NotBeNullOrWhiteSpace();
			str3.Should().NotBeNullOrWhiteSpace();
		}

		[ Test ]
		public void ToJson_InventoryStatusRequestList_StringReturnedNowExceptionOccured()
		{
			//A
			IEnumerable< InventoryStatusRequest > list = null;
			IEnumerable< InventoryStatusRequest > listWithEmptyRequest = new List< InventoryStatusRequest >() { new InventoryStatusRequest() };
			IEnumerable< InventoryStatusRequest > listWithNullRequests = new List< InventoryStatusRequest >() { null };

			//A
			var str1 = "";
			var str2 = "";
			var str3 = "";
			Action act1 = () => str1 = list.ToJson();
			Action act2 = () => str2 = listWithEmptyRequest.ToJson();
			Action act3 = () => str3 = listWithNullRequests.ToJson();

			//A
			act1.ShouldNotThrow< Exception >();
			act2.ShouldNotThrow< Exception >();
			act3.ShouldNotThrow< Exception >();
			str1.Should().NotBeNullOrWhiteSpace();
			str2.Should().NotBeNullOrWhiteSpace();
			str3.Should().NotBeNullOrWhiteSpace();
		}

		[ Test ]
		public void ToJson_GetSellerListCustomResponseList_StringReturnedNowExceptionOccured()
		{
			//A
			IEnumerable< EbayAccess.Models.GetSellerListCustomResponse.Item > list = null;
			IEnumerable< EbayAccess.Models.GetSellerListCustomResponse.Item > listWithEmptyString = new List< EbayAccess.Models.GetSellerListCustomResponse.Item >() { new EbayAccess.Models.GetSellerListCustomResponse.Item() };
			IEnumerable< EbayAccess.Models.GetSellerListCustomResponse.Item > listWithNullString = new List< EbayAccess.Models.GetSellerListCustomResponse.Item >() { null };

			//A
			var str1 = "";
			var str2 = "";
			var str3 = "";
			Action act1 = () => str1 = list.ToJson();
			Action act2 = () => str2 = listWithEmptyString.ToJson();
			Action act3 = () => str3 = listWithNullString.ToJson();

			//A
			act1.ShouldNotThrow< Exception >();
			act2.ShouldNotThrow< Exception >();
			act3.ShouldNotThrow< Exception >();
			str1.Should().NotBeNullOrWhiteSpace();
			str2.Should().NotBeNullOrWhiteSpace();
			str3.Should().NotBeNullOrWhiteSpace();
		}

		[ Test ]
		public void ToJson_ReviseInventoryStatusResponseList_StringReturnedNowExceptionOccured()
		{
			//A
			IEnumerable< EbayAccess.Models.ReviseInventoryStatusResponse.Item > list = null;
			IEnumerable< EbayAccess.Models.ReviseInventoryStatusResponse.Item > listWithEmptyItem = new List< EbayAccess.Models.ReviseInventoryStatusResponse.Item >() { new EbayAccess.Models.ReviseInventoryStatusResponse.Item() };
			IEnumerable< EbayAccess.Models.ReviseInventoryStatusResponse.Item > listWithNullItem = new List< EbayAccess.Models.ReviseInventoryStatusResponse.Item >() { null };

			//A
			var str1 = "";
			var str2 = "";
			var str3 = "";
			Action act1 = () => str1 = list.ToJson();
			Action act2 = () => str2 = listWithEmptyItem.ToJson();
			Action act3 = () => str3 = listWithNullItem.ToJson();

			//A
			act1.ShouldNotThrow< Exception >();
			act2.ShouldNotThrow< Exception >();
			act3.ShouldNotThrow< Exception >();
			str1.Should().NotBeNullOrWhiteSpace();
			str2.Should().NotBeNullOrWhiteSpace();
			str3.Should().NotBeNullOrWhiteSpace();
		}

		[ Test ]
		public void GivenNullResponse_WhenLimitResponseLogSizeCalled_ThenSameResponseIsReturned()
		{
			string response = null;
			var truncatedResponse = response.LimitResponseLogSize();
			truncatedResponse.Should().BeNull();
		}

		[ Test ]
		public void GivenResponseWithSizeHigherThanMaxLogSize_WhenLimitResponseLogSizeCalled_ThenTruncatedResponseLogIsReturned()
		{
			var response = "SkuVault";
			EbayAccess.Misc.Extensions.MaxResponseLogSize = 2;

			var truncatedResponse = response.LimitResponseLogSize();

			truncatedResponse.Length.Should().Be( EbayAccess.Misc.Extensions.MaxResponseLogSize );
		}

		[ Test ]
		public void GivenBodyWithSizeHigherThanMaxLogSize_WhenLimitBodyLogSizeCalled_ThenTruncatedBodyLogIsReturned()
		{
			var body = "Some huge payload from SkuVault here!";
			EbayAccess.Misc.Extensions.MaxBodyLogSize = 2;

			var truncatedBody = body.LimitBodyLogSize();

			truncatedBody.Length.Should().Be( EbayAccess.Misc.Extensions.MaxBodyLogSize );
		}
	}
}