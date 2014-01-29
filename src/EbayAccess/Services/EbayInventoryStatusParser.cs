using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using EbayAccess.Models.ReviseInventoryStatusRequest;

namespace EbayAccess.Services
{
	public class EbayInventoryStatusParser
	{
		public InventoryStatus ParseReviseInventoryStatusResponse(string str)
		{
			//todo: make parser
			throw new NotImplementedException();
		}

		public InventoryStatus ParseReviseInventoryStatusResponse(Stream stream)
		{
			try
			{
				XNamespace ns = "urn:ebay:apis:eBLBaseComponents";

				XElement root = XElement.Load(stream);

				InventoryStatus inventoryStatus = null;

				var elInventoryStatus = root.Element(ns + "InventoryStatus");
				if (elInventoryStatus != null)
				{
					object temp = null;

					inventoryStatus = new InventoryStatus();

					if (GetElementValue(elInventoryStatus, ref temp, ns, "SKU"))
						inventoryStatus.SKU = (string)temp;

					if (GetElementValue(elInventoryStatus, ref temp, ns, "ItemID"))
					{
						long tempRes;
						if (long.TryParse((string)temp, out tempRes))
						{
							inventoryStatus.ItemID = tempRes;
						}
					}

					if (GetElementValue(elInventoryStatus, ref temp, ns, "Quantity"))
					{
						long tempRes;
						if (long.TryParse((string)temp, out tempRes))
						{
							inventoryStatus.Quantity = tempRes;
						}
					}

					if (GetElementValue(elInventoryStatus, ref temp, ns, "StartPrice"))
					{
						double tempRes;
						if (double.TryParse((string)temp, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out tempRes))
						{
							inventoryStatus.StartPrice = tempRes;
						}
					}
				}

				return inventoryStatus;
			}
			catch (Exception ex)
			{
				byte[] buffer = new byte[stream.Length];
				stream.Read(buffer, 0, (int)stream.Length);
				var utf8Encoding = new UTF8Encoding();
				var bufferStr = utf8Encoding.GetString(buffer);
				throw new Exception("Can't parse: " + bufferStr, ex);
			}
		}

		private bool GetElementValue(XElement x, ref object parsedElement, XNamespace ns, params string[] elementName)
		{
			if (elementName.Length > 0)
			{
				var element = x.Element(ns + elementName[0]);
				if (element != null)
				{
					if (elementName.Length > 1)
						return GetElementValue(element, ref parsedElement, ns, elementName.Skip(1).ToArray());
					else
					{
						parsedElement = element.Value;
						return true;
					}
				}
			}

			return false;
		}

		private object GetElementValue(XElement x, XNamespace ns, params string[] elementName)
		{
			object parsedElement = null;
			GetElementValue(x, ref parsedElement, ns, elementName);
			return parsedElement;
		}

		public InventoryStatus ParseReviseInventoryStatusResponse(WebResponse response)
		{
			InventoryStatus result = null;
			using (var responseStream = response.GetResponseStream())
			{
				if (responseStream != null)
				{
					using (var memStream = new MemoryStream())
					{
						responseStream.CopyTo(memStream, 0x100);
						result = ParseReviseInventoryStatusResponse(memStream);
					}
				}
			}

			return result;
		} 
	}
}