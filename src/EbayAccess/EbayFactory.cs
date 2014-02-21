using CuttingEdge.Conditions;
using EbayAccess.Models;
using EbayAccess.Services;

namespace EbayAccess
{
	public interface IEbayFactory
	{
		IEbayService CreateService( EbayUserCredentials userCredentials );
	}

	public sealed class EbayFactory : IEbayFactory
	{
		private readonly EbayDevCredentials _devCredentials;

		public EbayFactory( EbayDevCredentials devCredentials )
		{
			Condition.Requires( devCredentials, "devCredentials" ).IsNotNull();

			this._devCredentials = devCredentials;
		}

		public IEbayService CreateService( EbayUserCredentials userCredentials )
		{
			return new EbayService( userCredentials, this._devCredentials );
		}
	}
}