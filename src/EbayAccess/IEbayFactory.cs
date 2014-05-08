using EbayAccess.Models.Credentials;

namespace EbayAccess
{
	public interface IEbayFactory
	{
		IEbayService CreateService( EbayUserCredentials userCredentials );
	}
}