using EbayAccess.Models;

namespace EbayAccess
{
	public interface IEbayFactory
	{
		IEbayService CreateService(EbayCredentials credentials);
	}

	public sealed class EbayFactory:IEbayFactory
	{
		public IEbayService CreateService(EbayCredentials credentials)
		{
			return new EbayService(credentials, "https://api.sandbox.ebay.com/ws/api.dll");
		}
	}
}