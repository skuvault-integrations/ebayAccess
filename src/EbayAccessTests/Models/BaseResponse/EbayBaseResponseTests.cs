using EbayAccess.Models.BaseResponse;
using FluentAssertions;
using NUnit.Framework;

namespace EbayAccessTests.Models.BaseResponse
{
	public class EbayBaseResponseTests
	{
		[ Test ]
		public void IfThereAreEbayInvalidTokenErrorsDo_WhenErrorCode21916013_ShouldPerformAction()
		{
			var response = new EbayBaseResponse
			{
				Errors = new [] 
				{
					new ResponseError { ErrorCode = "21916013" }
				}
			};
			var actionPerformed = false;

			response.IfThereAreEbayInvalidTokenErrorsDo( action => 
			{
				actionPerformed = true;
			} );

			actionPerformed.Should().BeTrue();
		}
	}
}
