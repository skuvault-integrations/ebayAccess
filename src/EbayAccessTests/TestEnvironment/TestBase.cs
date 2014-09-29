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
	}
}