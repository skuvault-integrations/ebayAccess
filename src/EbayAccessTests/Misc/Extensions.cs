using System;
using EbayAccess.Misc;
using FluentAssertions;
using NUnit.Framework;

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
	}
}