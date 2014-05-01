using EbayAccess.Models.Credentials;

namespace EbayAccess.Interfaces
{
	public interface IEbayFactory
	{
		IEbayService CreateService( EbayUserCredentials userCredentials );

		IEbayService CreateService(EbayUserCredentials userCredentials, string endPoint);
	}
}