using EbayAccess.Models;
using EbayAccess.Services;

namespace EbayAccess
{
	public interface IEbayFactory
	{
		IEbayService CreateService(EbayCredentials credentials, EbayDevCredentials ebayDevCredentials);
	}

	public sealed class EbayFactory:IEbayFactory
	{
		public IEbayService CreateService(EbayCredentials credentials, EbayDevCredentials ebayDevCredentials)
		{
			return new EbayService(credentials, ebayDevCredentials, "https://api.sandbox.ebay.com/ws/api.dll");
		}
	}
}