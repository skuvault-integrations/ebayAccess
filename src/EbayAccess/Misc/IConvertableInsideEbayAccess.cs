using System.Collections.Generic;
using System.Linq;

namespace EbayAccess.Misc
{
	public interface IConvertableInsideEbayAccess< T >
	{
		T ConvertTo();
	}
}