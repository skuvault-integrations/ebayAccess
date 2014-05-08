using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EbayAccess.Services;
using EbayAccessTests.TestEnvironment;
using FluentAssertions;
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
	}
}