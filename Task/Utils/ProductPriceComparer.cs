using System.Collections.Generic;

namespace Task
{
    public class ProductPriceComparer : IEqualityComparer<decimal>
    {
        public bool Equals(decimal x, decimal y)
        {
            return ProductPriceHelper.GetPriceCategory(x) == ProductPriceHelper.GetPriceCategory(y);
        }

        public int GetHashCode(decimal obj)
        {
            return ProductPriceHelper.GetPriceCategory(obj).GetHashCode();
        }
    }
}
