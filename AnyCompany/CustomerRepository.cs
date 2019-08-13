using AnyCompany.db;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace AnyCompany
{
    public static class CustomerRepository
    {
        //private static string ConnectionString = 

        public static Customer Load(int customerId, IDatabase database)
        {
            IEnumerable<Dictionary<string, object>> rawResult = database.ExecuteReader("SELECT * FROM Customer WHERE CustomerId = " + customerId, null);

            if (!rawResult.Any())
                throw new Exception("Customer not found. CustomerId = " + customerId);

            if(rawResult.Count() > 1)
                throw new Exception("Inconsistent data, multiple customers found. CustomerId = " + customerId);

            //sweet spot, extract the customer
            return ExtractCustomer(rawResult.First());
        }

        public static IEnumerable<Customer> LoadAll(IDatabase database)
        {
            IEnumerable<Dictionary<string, object>> rawResult = database.ExecuteReader("SELECT * FROM Customer", null);

            List<Customer> customers = new List<Customer>();
            foreach(Dictionary<string, object> rawCustomer in rawResult)
            {
                customers.Add(ExtractCustomer(rawCustomer));
            }

            return customers;
        }

        private static Customer ExtractCustomer(Dictionary<string, object> customerRow)
        {
            Customer customer = new Customer();
            customer.Name = customerRow["Name"].ToString();
            customer.DateOfBirth = DateTime.Parse(customerRow["DateOfBirth"].ToString());
            customer.Country = customerRow["Country"].ToString();
            customer.CustomerId = Int32.Parse(customerRow["CustomerId"].ToString());

            return customer;
        }
    }
}
