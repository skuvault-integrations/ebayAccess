using EbayAccess.Models;
using EbayAccess.Services;

namespace EbayAccess
{
	public interface IEbayFactory
	{
		IEbayService CreateService(EbayUserCredentials userCredentials, EbayDevCredentials ebayDevCredentials, string endpoint);
	}

	public sealed class EbayFactory:IEbayFactory
	{
		public IEbayService CreateService(EbayUserCredentials userCredentials, EbayDevCredentials ebayDevCredentials, string endpoint)
		{
			return new EbayService(userCredentials, ebayDevCredentials, endpoint);
		}
	}
}