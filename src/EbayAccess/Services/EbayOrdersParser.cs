using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using EbayAccess.Models;

namespace EbayAccess.Services
{
	public class EbayOrdersParser
	{
		public List<EbayOrder> ParseOrdersResponse(String str)
		{
			//todo: make parser
			throw new NotImplementedException();
		}

		public List<EbayOrder> ParseOrdersResponse(Stream stream)
		{
			var orders = new List<EbayOrder>();

			using (var reader = new StreamReader(stream))
			{
				string dataFromStream = reader.ReadToEnd();

				orders = ParseOrdersResponse(dataFromStream);
			}

			return orders;
		}

		public List<EbayOrder> ParseOrdersResponse(WebResponse response)
		{
			var stream = response.GetResponseStream();
			var memStream = new MemoryStream();
			if (stream != null)
			{
				stream.CopyTo(memStream, 0x100);
			}

			var result = ParseOrdersResponse(memStream);
			return result;
		}
	}
}