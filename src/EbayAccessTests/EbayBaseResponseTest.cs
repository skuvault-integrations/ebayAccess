using System.Collections.Generic;
using System.Linq;
using EbayAccess.Models;
using EbayAccess.Models.BaseResponse;
using FluentAssertions;
using NUnit.Framework;

namespace EbayAccessTests
{
	[ TestFixture ]
	public class EbayBaseResponseTest
	{
		private readonly List< ResponseError > _errorsForIgnoring = new List< ResponseError >
		{
			EbayErrors.EbayPixelSizeError,
			EbayErrors.LvisBlockedError,
			EbayErrors.UnsupportedListingType,
			EbayErrors.ReplaceableValue,
			EbayErrors.MpnHasAnInvalidValue,
			EbayErrors.DuplicateListingPolicy,
			EbayErrors.OperationIsNotAllowedForInventoryItems,
			EbayErrors.InvalidMultiSkuItemId,
			EbayErrors.AuctionEnded
		};

		[ TestCase( EbayErrors.EbayPixelSizeErrorCode ) ]
		[ TestCase( EbayErrors.LvisBlockedErrorCode ) ]
		[ TestCase( EbayErrors.UnsupportedListingTypeErrorCode ) ]
		[ TestCase( EbayErrors.ReplaceableValueErrorCode ) ]
		[ TestCase( EbayErrors.MpnHasAnInvalidValueErrorCode ) ]
		[ TestCase( EbayErrors.DuplicateListingPolicyErrorCode ) ]
		[ TestCase( EbayErrors.OperationIsNotAllowedForInventoryItemsErrorCode ) ]
		[ TestCase( EbayErrors.InvalidMultiSkuItemIdErrorCode ) ]
		[ TestCase( EbayErrors.AuctionEndedErrorCode ) ]
		public void SkipErrorsAndDo_ReturnResponseWithoutIgnoringErrors( string errorCode )
		{
			// Arrange
			var response = new EbayBaseResponse
			{
				Errors = new[]
				{
					new ResponseError
					{
						ErrorCode = errorCode
					}
				}
			};

			// Act
			response.SkipErrorsAndDo( null, this._errorsForIgnoring );

			// Assert
			response.Errors.Any( x => x.ErrorCode.Equals( errorCode ) ).Should().BeFalse();
		}
	}
}