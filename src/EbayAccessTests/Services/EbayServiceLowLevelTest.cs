using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EbayAccess.Services;
using EbayAccessTests.TestEnvironment;
using FluentAssertions;
using Moq;
using NSubstitute;
using NUnit.Framework;

namespace EbayAccessTests.Services
{
	[ TestFixture ]
	public class EbayServiceLowLevelTest : TestBase
	{
		[ Test ]
		public void GetSellerListAsync_EbayServiceWithExistingItems_NotEmptyItemsCollection()
		{
			//A
			using( var fs = new FileStream( @".\Files\GetSellerListResponse\EbayServiceGetSellerListResponseWith5ItemsWithSku_DetailLevelAll.xml", FileMode.Open, FileAccess.Read ) )
			{
				var stubWebRequestService = Substitute.For< IWebRequestServices >();

				stubWebRequestService.GetResponseStreamAsync( Arg.Any< WebRequest >() ).Returns( Task.Factory.StartNew< Stream >( () => fs ) );

				var ebayService = new EbayServiceLowLevel( this._testEmptyCredentials.GetEbayUserCredentials(), this._testEmptyCredentials.GetEbayDevCredentials(), stubWebRequestService );

				//A
				var ordersTask = ebayService.GetSellerListAsync( new DateTime( 2014, 1, 1, 0, 0, 0 ), new DateTime( 2014, 1, 28, 10, 0, 0 ), TimeRangeEnum.StartTime );
				ordersTask.Wait();
				var orders = ordersTask.Result;

				//A
				orders.Count().Should().Be( 5, "because on site there are 5 items started in specified time" );
			}
		}

		[ Test ]
		public void GetOrdersAsync_EbayServiceRespondContainsTotalNumberOfEntitiesZeroAndHasMoreOrdersFalse_Only1WebRequestServiceCallMaked()
		{
			//A
			string respstring;
			using( var fs = new FileStream( @".\Files\GetOrdersResponse\EbayServiceGetOrdersResponseWithTotalNumberOfEntities0AndHasMoreOrdersFalse.xml", FileMode.Open, FileAccess.Read ) )
				respstring = new StreamReader( fs ).ReadToEnd();
			var getResponseStreamAsyncCallCounter = 0; ;

			var stubWebRequestService = Substitute.For<IWebRequestServices>();
			stubWebRequestService.GetResponseStreamAsync( Arg.Any< WebRequest >() ).Returns( Task.FromResult( GetStream( respstring ) )).AndDoes( x => getResponseStreamAsyncCallCounter++ );

			var ebayService = new EbayServiceLowLevel( this._testEmptyCredentials.GetEbayUserCredentials(), this._testEmptyCredentials.GetEbayDevCredentials(), stubWebRequestService );

			//A
			var ordersTask = ebayService.GetOrdersAsync( new DateTime( 2014, 1, 1, 0, 0, 0 ), new DateTime( 2014, 1, 21, 10, 0, 0 ) );
			ordersTask.Wait();
			var orders = ordersTask.Result;

			//A
			getResponseStreamAsyncCallCounter.Should().Be( 1 );
		}

		[Test]
		public void GetOrders_EbayServiceRespondContainsTotalNumberOfEntitiesZeroAndHasMoreOrdersFalse_Only1WebRequestServiceCallMaked()
		{
			//A
			string respstring;
			using (var fs = new FileStream(@".\Files\GetOrdersResponse\EbayServiceGetOrdersResponseWithTotalNumberOfEntities0AndHasMoreOrdersFalse.xml", FileMode.Open, FileAccess.Read))
				respstring = new StreamReader(fs).ReadToEnd();
			var getResponseStreamCallCounter = 0; ;

			var stubWebRequestService = Substitute.For<IWebRequestServices>();
			stubWebRequestService.GetResponseStream(Arg.Any<WebRequest>()).Returns(GetStream(respstring)).AndDoes(x => getResponseStreamCallCounter++);

			var ebayService = new EbayServiceLowLevel(this._testEmptyCredentials.GetEbayUserCredentials(), this._testEmptyCredentials.GetEbayDevCredentials(), stubWebRequestService);

			//A
			var orders = ebayService.GetOrders(new DateTime(2014, 1, 1, 0, 0, 0), new DateTime(2014, 1, 21, 10, 0, 0));

			//A
			getResponseStreamCallCounter.Should().Be(1);
		}
	}
}
