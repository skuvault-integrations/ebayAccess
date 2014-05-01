using System;
using System.Linq;
using EbayAccess;
using EbayAccessTests.Integration.TestEnvironment;
using FluentAssertions;
using NUnit.Framework;

namespace EbayAccessTests.Integration
{
	[ TestFixture ]
	public class Programm : TestBase
	{
		[ Test ]
		public void EbayServiceGetOrders_EbayServiceWithProducts_HookupProducts()
		{
			//------------ Arrange
			var ebayFactory = new EbayFactory( this._credentials.GetEbayDevCredentials() );
			var ebayService = ebayFactory.CreateService( this._credentials.GetEbayUserCredentials(), this._credentials.GetEbayEndPoint() );

			//------------ Act
			var orders = ebayService.GetOrders( new DateTime( 2014, 1, 1, 0, 0, 0 ), new DateTime( 2014, 1, 21, 10, 0, 0 ) );

			//------------ Assert
			orders.Count().Should().Be( 2, "because on site there is 2 orders" );
		}
	}
}