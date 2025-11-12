using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EbayAccess.Services;
using FluentAssertions;
using Netco.Logging;
using NUnit.Framework;

namespace EbayAccessTests.Misc
{
	[ TestFixture ]
	public class TimeoutTests
	{
		[ Test ]
		public void GivenLowTimeout_WhenCallGetResponseStreamAsync_ThenThrowsTaskCanceledException()
		{
			const int reallyShortTimeout = 1;
			var webRequestServices = new WebRequestServices();
			var token = new CancellationTokenSource( reallyShortTimeout );

			Action act = () =>
			{
				var request = webRequestServices.CreateServiceGetRequest( "http://localhost", new Dictionary< string, string >() );
				var orderTask = webRequestServices.GetResponseStreamAsync( request, Mark.Blank(), token.Token );
				orderTask.Wait();
			};
			
			act.Should().Throw< TaskCanceledException >();
		}
	}
}
