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

		[ Test ]
		public void SkipErrorsAndDo_ErrorDoesNotRemainInResponse_WhenErrorIsIgnored()
		{
			// Arrange
			var response = new EbayBaseResponse
			{
				Errors = new[]
				{
					new ResponseError
					{
						ErrorCode = EbayErrors.InvalidMultiSkuItemIdErrorCode
					}
				}
			};

			// Act
			response.SkipErrorsAndDo( null, this._errorsForIgnoring );

			// Assert
			response.Errors.Any( x => x.ErrorCode.Equals( EbayErrors.InvalidMultiSkuItemIdErrorCode ) ).Should().BeFalse();
		}

		[ Test ]
		public void SkipErrorsAndDo_ErrorRemainsInResponse_WhenErrorIsNotIgnored()
		{
			// Arrange
			var response = new EbayBaseResponse
			{
				Errors = new[]
				{
					new ResponseError
					{
						ErrorCode = EbayErrors.EbayPixelSizeErrorCode
					}
				}
			};

			// Act
			response.SkipErrorsAndDo( null, this._errorsForIgnoring );

			// Assert
			response.Errors.Any( x => x.ErrorCode.Equals( EbayErrors.EbayPixelSizeErrorCode ) ).Should().BeTrue();
		}
	}
}