// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// Microsoft Public License (MS-PL, http://opensource.org/licenses/ms-pl.html.)
//
//Copyright (C) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using SampleSupport;
using Task.Data;
using System.Text.RegularExpressions;
using Task;
using Task.Utils;

// Version Mad01

namespace SampleQueries
{
	[Title("LINQ Module")]
	[Prefix("Linq")]
	public class LinqSamples : SampleHarness
	{

		private DataSource dataSource = new DataSource();

		[Category("Restriction Operators")]
		[Title("Where - Task 1")]
		[Description("This sample uses the where clause to find all elements of an array with a value less than 5.")]
		public void Linq1()
		{
			int[] numbers = { 5, 4, 1, 3, 9, 8, 6, 7, 2, 0 };

			var lowNums =
				from num in numbers
				where num < 5
				select num;

			Console.WriteLine("Numbers < 5:");
			foreach (var x in lowNums)
			{
				Console.WriteLine(x);
			}
		}

		[Category("Restriction Operators")]
		[Title("Where - Task 2")]
		[Description("This sample return return all presented in market products")]

		public void Linq2()
		{
			var products =
				from p in dataSource.Products
				where p.UnitsInStock > 0
				select p;

			foreach (var p in products)
			{
				ObjectDumper.Write(p);
			}
		}

        [Category("Mentoring")]
        [Title("Mentoring 001")]
        [Description("Выдайте список всех клиентов, чей суммарный оборот (сумма всех заказов) превосходит некоторую величину X. " +
            "Продемонстрируйте выполнение запроса с различными X " +
            "(подумайте, можно ли обойтись без копирования запроса несколько раз)")]
        public void Linq001()
        {
            decimal minTotalSum = 25000.00M;
            Console.WriteLine($"Customers with Total Sum of Orders greater than {minTotalSum}:");
            IEnumerable<Customer> customers = CustomerHelper.FilterCustomersByMinOrderTotalSum(dataSource.Customers, minTotalSum);
            PrintHelper.PrintItems(customers);
            
            minTotalSum = 100000.00M;
            Console.WriteLine($"Customers with Total Sum of Orders greater than {minTotalSum}:");
            customers = CustomerHelper.FilterCustomersByMinOrderTotalSum(dataSource.Customers, minTotalSum);
            PrintHelper.PrintItems(customers);
        }

        [Category("Mentoring")]
        [Title("Mentoring 002")]
        [Description("Для каждого клиента составьте список поставщиков, находящихся в той же стране и том же городе. " +
            "Сделайте задания с использованием группировки и без.")]
        public void Linq002()
        {
            Console.WriteLine("With GroupJoin");

            var query = dataSource.Customers.GroupJoin(dataSource.Suppliers,
                c => new { c.Country, c.City },
                s => new { s.Country, s.City },
                (c, result) => new {
                    CustomerId = c.CustomerID,
                    Country = c.Country,
                    City = c.City,
                    Suppliers = result.DefaultIfEmpty() }).OrderBy(c => c.CustomerId);

            foreach (var customer in query)
            {
                Console.WriteLine($"{customer.CustomerId} ({customer.Country}, {customer.City})");
                foreach (var supplier in customer.Suppliers)
                {
                    if (supplier == null)
                    {
                        Console.WriteLine("-- Not Found");
                        continue;
                    }
                    Console.WriteLine($"-- {supplier.SupplierName}");
                }
            }

            Console.WriteLine(string.Empty);
            Console.WriteLine("With GroupBy");

            var query2 = dataSource.Customers.Join(dataSource.Suppliers,
                c => new { c.Country, c.City },
                s => new { s.Country, s.City },
                (c, s) => new {
                    CustomerId = c.CustomerID,
                    Country = c.Country,
                    City = c.City,
                    SupplierName = s.SupplierName
                }).GroupBy(c => c.CustomerId);

            foreach (var keyCustomer in query2)
            {
                Console.WriteLine($"{keyCustomer.Key}");
                foreach (var customerWithSupplier in keyCustomer)
                {
                    Console.WriteLine($"-- {customerWithSupplier.SupplierName} ({customerWithSupplier.Country}, {customerWithSupplier.City})");
                }
            }
        }

        [Category("Mentoring")]
        [Title("Mentoring 003")]
        [Description("Найдите всех клиентов, у которых были заказы, превосходящие по сумме величину X")]
        public void Linq003()
        {
            decimal cutOffSum = 10.000M;
            var query = dataSource.Customers.Where(c => c.Orders.Any(o => o.Total > cutOffSum));

            PrintHelper.PrintItems(query);
        }

        [Category("Mentoring")]
        [Title("Mentoring 004")]
        [Description("Выдайте список клиентов с указанием, начиная с какого месяца какого года они стали клиентами "+
            "(принять за таковые месяц и год самого первого заказа)")]
        public void Linq004()
        {
            var query = dataSource.Customers
                .Where(c => c.Orders.Any())
                .Select(c => new {
                    CustomerId = c.CustomerID,
                    FirstOrderDate = c.Orders.Min(o => o.OrderDate)
                });

            foreach (var customer in query)
            {
                Console.WriteLine($"{customer.CustomerId}, {customer.FirstOrderDate.ToString("MM-yyyy")}");
            }
        }

        [Category("Mentoring")]
        [Title("Mentoring 005")]
        [Description("Сделайте предыдущее задание, но выдайте список отсортированным по году, месяцу, оборотам клиента " + 
            "(от максимального к минимальному) и имени клиента")]
        public void Linq005()
        {
            var query = dataSource.Customers
                .Where(c => c.Orders.Any())
                .Select(c => new {
                    CompanyName = c.CompanyName,
                    FirstOrderDate = c.Orders.Min(o => o.OrderDate),
                    TotalSum = c.Orders.Sum(o => o.Total)
                })
                .OrderBy(c => c.FirstOrderDate.Year)
                .ThenBy(c => c.FirstOrderDate.Month)
                .ThenByDescending(c => c.TotalSum)
                .ThenBy(c => c.CompanyName);

            foreach (var customer in query)
            {
                Console.WriteLine($"Year: {customer.FirstOrderDate.Year}, Month: {customer.FirstOrderDate.Month}, Turnover: {customer.TotalSum}, Company Name: {customer.CompanyName}");
            }
        }

        [Category("Mentoring")]
        [Title("Mentoring 006")]
        [Description("Укажите всех клиентов, у которых указан нецифровой код или не заполнен регион " + 
            "или в телефоне не указан код оператора (для простоты считаем, что это равнозначно «нет круглых скобочек в начале»).")]
        public void Linq006()
        {
            var query = dataSource.Customers.Where(c => 
                string.IsNullOrEmpty(c.Region) ||
                !Regex.IsMatch(c.PostalCode?? string.Empty, @"^[0-9-]*$") || // include numbers and dashes
                !c.Phone.StartsWith("("));
            
            foreach (var customer in query)
            {
                Console.WriteLine($"{customer.CustomerID}, Region: {customer.Region}, PostalCode: {customer.PostalCode}, Phone: {customer.Phone}");
            }
        }

        [Category("Mentoring")]
        [Title("Mentoring 007")]
        [Description("Сгруппируйте все продукты по категориям, внутри – по наличию на складе, внутри последней группы отсортируйте по стоимости")]
        public void Linq007()
        {
            var query = dataSource.Products
                .GroupBy(p => p.Category)
                .Select(categoryGroup => new
                {
                    Category = categoryGroup.Key,
                    Products = categoryGroup
                        .GroupBy(p => p.UnitsInStock > 0)
                        .Select(unitsInStockGroup => new
                        {
                            UnitsInStockGroup = unitsInStockGroup.Key ? "In Stock" : "Out Of Stock",
                            Products = unitsInStockGroup
                                .OrderBy(p => p.UnitPrice)
                        })
                });
            
            foreach (var category in query)
            {
                Console.WriteLine(category.Category);
                foreach (var unitsGroup in category.Products)
                {
                    Console.WriteLine($" - {unitsGroup.UnitsInStockGroup}");
                    foreach (var product in unitsGroup.Products)
                    {
                        Console.WriteLine($" - - Price: {product.UnitPrice}, Product Name: {product.ProductName}");
                    }
                }
            }
        }

        [Category("Mentoring")]
        [Title("Mentoring 008")]
        [Description("Сгруппируйте товары по группам «дешевые», «средняя цена», «дорогие». " +
            "Границы каждой группы задайте сами")]
        public void Linq008()
        {
            var comp = new ProductPriceComparer();
            var query = dataSource.Products.GroupBy(p => p.UnitPrice, comp);

            foreach (var priceCategory in query)
            {
                Console.WriteLine(ProductPriceHelper.GetPriceCategory(priceCategory.Key));
                foreach (var product in priceCategory)
                {
                    Console.Write(" - ");
                    ObjectDumper.Write(product);
                }
            }
        }

        [Category("Mentoring")]
        [Title("Mentoring 009")]
        [Description("Рассчитайте среднюю прибыльность каждого города " +
            "(среднюю сумму заказа по всем клиентам из данного города) " +
            "и среднюю интенсивность (среднее количество заказов, приходящееся на клиента из каждого города)")]
        public void Linq009()
        {
            var query = dataSource.Customers
                .GroupBy(c => c.City)
                .Select(cityGroup =>
                    new
                    {
                        City = cityGroup.Key,
                        AverageTotal = Math.Round(cityGroup.SelectMany(c => c.Orders).Average(o => o.Total), 2),
                        AverageNumberOfOrdersByCustomer = cityGroup.Select(c =>
                            new
                            {
                                CustomerId = c.CustomerID,
                                NumberOfOrders = c.Orders.Count()
                            }).Average(c => c.NumberOfOrders)
                    })
                .OrderBy(c => c.City);

            foreach (var cityGroup in query)
            {
                ObjectDumper.Write(cityGroup);
            }

            // Query for testing
            //var query2 = dataSource.Customers.Select(c => new
            //{
            //    City = c.City,
            //    CustomerId = c.CustomerID,
            //    OrdersNumber = c.Orders.Count()
            //}).OrderBy(c => c.City);
            //foreach (var c in query2)
            //{
            //    ObjectDumper.Write(c);
            //}

        }

        [Category("Mentoring")]
        [Title("Mentoring 010")]
        [Description("Сделайте среднегодовую статистику активности клиентов по месяцам (без учета года), " + 
            "статистику по годам, " +
            "по годам и месяцам (т.е. когда один месяц в разные годы имеет своё значение)")]
        public void Linq010()
        {
            var mainQuery = dataSource.Customers.SelectMany(c => c.Orders).OrderBy(o => o.OrderDate.Month);

            Console.WriteLine("months statistics");
            var monthQuery = mainQuery
                .GroupBy(c => new { c.OrderDate.Month, MonthName = c.OrderDate.ToString("MMMM") })
                .Select(c => new { Month = c.Key.Month, MonthName = c.Key.MonthName, NumberOfOrders = c.Count()})
                .OrderBy(c => c.Month);
            PrintHelper.PrintItems(monthQuery);

            Console.WriteLine("year statistics");
            var yearQuery = mainQuery
                .GroupBy(c => c.OrderDate.Year)
                .Select(c => new { Year = c.Key, NumberOfOrders = c.Count() })
                .OrderBy(c => c.Year);
            PrintHelper.PrintItems(yearQuery);

            Console.WriteLine("month - year statistics");
            var myQuery = mainQuery
                .GroupBy(c => new { c.OrderDate.Year, c.OrderDate.Month })
                .Select(c => new { Year = c.Key.Year, Month = c.Key.Month, NumberOfOrders = c.Count() })
                .OrderBy(c => c.Year)
                .ThenBy(c => c.Month);
            PrintHelper.PrintItems(myQuery);
        }
    }
}
