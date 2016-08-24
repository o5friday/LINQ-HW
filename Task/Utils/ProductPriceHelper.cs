namespace Task
{
    public static class ProductPriceHelper
    {
        private const decimal cheapProductMaxPrice = 10.00M;
        private const decimal averageProductMaxPrice = 20.00M;

        public static ProductPriceCategory GetPriceCategory(decimal price)
        {
            if (price < cheapProductMaxPrice)
            {
                return ProductPriceCategory.Cheap;
            }
            if (price < averageProductMaxPrice)
            {
                return ProductPriceCategory.Average;
            }
            return ProductPriceCategory.Expensive;
        }
    }
}
