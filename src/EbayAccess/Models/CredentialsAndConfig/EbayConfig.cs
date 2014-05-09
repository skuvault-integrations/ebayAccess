using System.Configuration;
using CuttingEdge.Conditions;

namespace EbayAccess.Models.CredentialsAndConfig
{
	public class EbayConfig
	{
		public string AppName { get; private set; }
		public string DevName { get; private set; }
		public string CertName { get; private set; }
		public string EndPoint { get; private set; }

		public EbayConfig( string appName, string devName, string certName, string endPoint = null )
		{
			Condition.Requires( appName, "devName" ).IsNotNullOrWhiteSpace();
			Condition.Requires( devName, "appName" ).IsNotNullOrWhiteSpace();
			Condition.Requires( certName, "certName" ).IsNotNullOrWhiteSpace();

			this.AppName = appName;
			this.DevName = devName;
			this.CertName = certName;
			this.EndPoint = string.IsNullOrWhiteSpace( endPoint ) ? ConfigurationManager.AppSettings[ "EndPoint" ] : endPoint;
		}
	}
}