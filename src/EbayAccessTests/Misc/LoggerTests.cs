using System;
using EbayAccess.Misc;
using Netco.Logging;
using NSubstitute;
using NUnit.Framework;

namespace EbayAccessTests.Misc
{
	[ TestFixture ]
	public class LoggerTests
	{
		private ILogger _logger;

		[ SetUp ]
		public void Init()
		{
			this._logger = Substitute.For< ILogger >();
			NetcoLogger.LoggerFactory = new LoggerFactoryStub( this._logger );
		}

		[ Test ]
		public void GivenInfoStringWithNormalSize_WhenTraceInfoCalled_ThenSameInfoShouldBeWrittenToLog()
		{
			EbayLogger.MaxLogLineSize = 5;
			var infoMessage = "hello";

			EbayLogger.LogTrace( infoMessage );
			this._logger.Received().Trace( "[{channel}] {type}:{info}", "ebay", "Trace call", infoMessage );
		}

		[ Test ]
		public void GivenInfoStringWithAbnormalSize_WhenTraceInfoCalled_ThenTruncatedInfoShouldBeWrittenToLog()
		{
			EbayLogger.MaxLogLineSize = 5;
			var infoMessage = "hello world!";

			EbayLogger.LogTrace( infoMessage );
			this._logger.Received().Trace( "[{channel}] {type}:{info}", "ebay", "Trace call", infoMessage.Substring( 0, EbayLogger.MaxLogLineSize ) );
		}

		[ Test ]
		public void GivenNullInfoString_WhenTraceInfoCalled_ThenNullShouldBeWrittenToLog()
		{
			EbayLogger.MaxLogLineSize = 5;

			EbayLogger.LogTrace( null );
			this._logger.Received().Trace( "[{channel}] {type}:{info}", "ebay", "Trace call", null );
		}
	}

	internal class LoggerFactoryStub : ILoggerFactory
	{
		public ILogger Logger { get; private set; }

		public LoggerFactoryStub( ILogger logger )
		{
			this.Logger = logger;
		}

		public ILogger GetLogger( Type objectToLogType )
		{
			return this.Logger;
		}

		public ILogger GetLogger( string loggerName )
		{
			return this.Logger;
		}
	}
}
