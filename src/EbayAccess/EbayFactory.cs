using EbayAccess.Models;
using EbayAccess.Services;

namespace EbayAccess
{
	public interface IEbayFactory
	{
		IEbayService CreateService(EbayUserCredentials userCredentials, EbayDevCredentials ebayDevCredentials);
	}

	public sealed class EbayFactory:IEbayFactory
	{
		public IEbayService CreateService(EbayUserCredentials userCredentials, EbayDevCredentials ebayDevCredentials)
		{
			return new EbayService(userCredentials, ebayDevCredentials);
		}
	}
}