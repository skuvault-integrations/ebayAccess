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
		private readonly List< ResponseError > _errorsForIgnoring = new()
		{
			EbayErrors.InvalidMultiSkuItemId
		};

		[ TestCase( EbayErrors.InvalidMultiSkuItemIdErrorCode ) ]
		public void SkipErrorsAndDo_ReturnResponseWithoutIgnoredErrors( string errorCode )
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

		[ TestCase( EbayErrors.EbayPixelSizeErrorCode ) ]
		[ TestCase( EbayErrors.LvisBlockedErrorCode ) ]
		[ TestCase( EbayErrors.UnsupportedListingTypeErrorCode ) ]
		[ TestCase( EbayErrors.ReplaceableValueErrorCode ) ]
		[ TestCase( EbayErrors.MpnHasAnInvalidValueErrorCode ) ]
		[ TestCase( EbayErrors.DuplicateListingPolicyErrorCode ) ]
		[ TestCase( EbayErrors.OperationIsNotAllowedForInventoryItemsErrorCode ) ]
		[ TestCase( EbayErrors.AuctionEndedErrorCode ) ]
		public void SkipErrorsAndDo_Throws_WhenErrorIsNotIgnored( string errorCode )
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
			response.Errors.Any( x => x.ErrorCode.Equals( errorCode ) ).Should().BeTrue();
		}
	}
}