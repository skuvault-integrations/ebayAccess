using System.IO;
using EbayAccess.Services;
using FluentAssertions;
using NUnit.Framework;

namespace EbayAccessTests.Services
{
	[TestFixture]
	public class EbayOrdersParserTest
	{
		[Test]
		public void FileStreamWithCorrectXml_ParseOrdersResponse_HookupCorrectDeserializedObject()
		{
			using (var fs = new FileStream(@".\EbayServiceGetOrdersResponseWithItems.xml", FileMode.Open, FileAccess.Read))
			{
				var parser = new EbayOrdersParser();
				var orders = parser.ParseOrdersResponse(fs);
				orders.Should().HaveCount(1, "because in source file there is {0} order",1);
			}
		}
	}
}