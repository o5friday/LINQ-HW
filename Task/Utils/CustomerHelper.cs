using System.Collections.Generic;
using System.Linq;
using Task.Data;

namespace Task.Utils
{
    public static class CustomerHelper
    {
        public static IEnumerable<Customer> FilterCustomersByMinOrderTotalSum(IEnumerable<Customer> customers, decimal minTotal)
        {
            return customers.Where(c => c.Orders.Sum(o => o.Total) > minTotal);
        }
    }
}
