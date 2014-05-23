using System.IO;
using NUnit.Framework;

namespace EbayAccessTests.TestEnvironment
{
	public class TestBase
	{
		protected TestEmptyCredentials _testEmptyCredentials;

		[ SetUp ]
		public void Init()
		{
			this._testEmptyCredentials = new TestEmptyCredentials();
		}

		protected Stream GetStream( string str )
		{
			var ms = new MemoryStream();
			var sw = new StreamWriter( ms );
			sw.Write( str );
			sw.Flush();
			ms.Position = 0;
			return ms;
		}
	}
}