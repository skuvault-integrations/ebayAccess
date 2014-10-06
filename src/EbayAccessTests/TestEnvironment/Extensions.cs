using System;
using System.Xml;

namespace EbayAccessTests.TestEnvironment
{
	public static class Extensions
	{
		public static bool TryParse( this XmlDocument doc, string srcString )
		{
			try
			{
				doc.LoadXml( srcString );
				return true;
			}
			catch( Exception )
			{
				return false;
			}
		}
	}
}