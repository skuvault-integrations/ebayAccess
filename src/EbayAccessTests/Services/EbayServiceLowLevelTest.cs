using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using EbayAccess.Misc;
using EbayAccess.Models.ReviseFixedPriceItemRequest;
using EbayAccess.Models.ReviseInventoryStatusRequest;
using EbayAccess.Services;
using EbayAccessTests.TestEnvironment;
using EbayAccessTests.TestEnvironment.TestResponses;
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

				stubWebRequestService.GetResponseStreamAsync( Arg.Any< WebRequest >(), Arg.Any< string >(), CancellationToken.None ).Returns( Task.Factory.StartNew< Stream >( () => fs ) );

				var ebayService = new EbayServiceLowLevel( this._testEmptyCredentials.GetEbayUserCredentials(), this._testEmptyCredentials.GetEbayDevCredentials(), stubWebRequestService );

				//A
				var getSellerListTask = ebayService.GetSellerListAsync( new DateTime( 2014, 1, 1, 0, 0, 0 ), new DateTime( 2014, 1, 28, 10, 0, 0 ), GetSellerListTimeRangeEnum.StartTime, new Guid().ToString() );
				getSellerListTask.Wait();
				var items = getSellerListTask.Result;

				//A
				items.Items.Count().Should().Be( 5, "because on site there are 5 items started in specified time" );
			}
		}

		[ Test ]
		public void GetOrdersAsync_EbayServiceRespondContainsTotalNumberOfEntitiesZeroAndHasMoreOrdersFalse_Only1WebRequestServiceCallMaked()
		{
			//A
			string respstring;
			using( var fs = new FileStream( @".\Files\GetOrdersResponse\EbayServiceGetOrdersResponseWithTotalNumberOfEntities0AndHasMoreOrdersFalse.xml", FileMode.Open, FileAccess.Read ) )
				respstring = new StreamReader( fs ).ReadToEnd();
			var getResponseStreamAsyncCallCounter = 0;

			var stubWebRequestService = Substitute.For< IWebRequestServices >();
			stubWebRequestService.GetResponseStreamAsync( Arg.Any< WebRequest >(), Arg.Any< string >(), CancellationToken.None ).Returns( Task.FromResult( respstring.ToStream() ) ).AndDoes( x => getResponseStreamAsyncCallCounter++ );

			var ebayService = new EbayServiceLowLevel( this._testEmptyCredentials.GetEbayUserCredentials(), this._testEmptyCredentials.GetEbayDevCredentials(), stubWebRequestService );

			//A
			var ordersTask = ebayService.GetOrdersAsync( new DateTime( 2014, 1, 1, 0, 0, 0 ), new DateTime( 2014, 1, 21, 10, 0, 0 ), GetOrdersTimeRangeEnum.CreateTime );
			ordersTask.Wait();
			var orders = ordersTask.Result;

			//A
			getResponseStreamAsyncCallCounter.Should().Be( 1 );
		}

		[ Test ]
		public void GetOrders_EbayServiceRespondContainsTotalNumberOfEntitiesZeroAndHasMoreOrdersFalse_Only1WebRequestServiceCallMaked()
		{
			//A
			string respstring;
			using( var fs = new FileStream( @".\Files\GetOrdersResponse\EbayServiceGetOrdersResponseWithTotalNumberOfEntities0AndHasMoreOrdersFalse.xml", FileMode.Open, FileAccess.Read ) )
				respstring = new StreamReader( fs ).ReadToEnd();
			var getResponseStreamCallCounter = 0;
			;

			var stubWebRequestService = Substitute.For< IWebRequestServices >();
			stubWebRequestService.GetResponseStream( Arg.Any< WebRequest >(), Arg.Any< string >() ).Returns( respstring.ToStream() ).AndDoes( x => getResponseStreamCallCounter++ );

			var ebayServiceLowLevel = new EbayServiceLowLevel( this._testEmptyCredentials.GetEbayUserCredentials(), this._testEmptyCredentials.GetEbayDevCredentials(), stubWebRequestService );

			//A
			var orders = ebayServiceLowLevel.GetOrders( new DateTime( 2014, 1, 1, 0, 0, 0 ), new DateTime( 2014, 1, 21, 10, 0, 0 ), GetOrdersTimeRangeEnum.CreateTime, new Guid().ToString() );

			//A
			getResponseStreamCallCounter.Should().Be( 1 );
		}

		[ Test ]
		public void ReviseFixedPriceItemAsync_ModelContsinsSymbolsThatMustBeReplaced_SybolsReplasedByAliasesInRequest()
		{
			//A
			var stubWebRequestService = Substitute.For< IWebRequestServices >();
			stubWebRequestService.GetResponseStreamAsync( Arg.Any< WebRequest >(), Arg.Any< string >(), CancellationToken.None ).Returns( Task.FromResult( ReviseFixedPriceItemResponse.Success.ToStream() ) );
			var ebayServiceLowLevel = new EbayServiceLowLevel( this._testEmptyCredentials.GetEbayUserCredentials(), this._testEmptyCredentials.GetEbayDevCredentials(), stubWebRequestService );

			//A
			Action act = () =>
			{
				var reviseFixedPriceItemAsync = ebayServiceLowLevel.ReviseFixedPriceItemAsync( new ReviseFixedPriceItemRequest()
				{
					ItemId = 1,
					Quantity = 1,
					Sku = "some sku with &<>'\""
				}, "mark", true );
				reviseFixedPriceItemAsync.Wait();
			};

			//A
			act.ShouldNotThrow< Exception >();
			stubWebRequestService.Received().CreateServicePostRequestAsync(
				Arg.Any< string >(),
				Arg.Is< string >( x => new XmlDocument().TryParse( x ) ),
				Arg.Any< Dictionary< string, string > >(), CancellationToken.None, Arg.Any< string >() );
		}

		[ Test ]
		public void ReviseInventoriesStatusAsync_ModelContsinsSymbolsThatMustBeReplaced_SybolsReplasedByAliasesInRequest()
		{
			//A
			var stubWebRequestService = Substitute.For< IWebRequestServices >();
			stubWebRequestService.GetResponseStreamAsync( Arg.Any< WebRequest >(), Arg.Any< string >(), CancellationToken.None ).Returns( Task.FromResult( ReviseInventoryStatusResponse.Success.ToStream() ) );
			var ebayServiceLowLevel = new EbayServiceLowLevel( this._testEmptyCredentials.GetEbayUserCredentials(), this._testEmptyCredentials.GetEbayDevCredentials(), stubWebRequestService );

			//A
			Action act = () =>
			{
				var reviseFixedPriceItemAsync = ebayServiceLowLevel.ReviseInventoriesStatusAsync( new List< InventoryStatusRequest >
				{
					new InventoryStatusRequest()
					{
						ItemId = 1,
						Quantity = 1,
						Sku = "some sku with &<>'\""
					}
				}, "mark" );
				reviseFixedPriceItemAsync.Wait();
			};

			//A
			act.ShouldNotThrow< Exception >();
			stubWebRequestService.Received( 1 ).CreateServicePostRequestAsync(
				Arg.Any< string >(),
				Arg.Is< string >( x => new XmlDocument().TryParse( x ) ),
				Arg.Any< Dictionary< string, string > >(), CancellationToken.None, Arg.Any< string >() );
		}
	}
}