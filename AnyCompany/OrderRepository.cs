using AnyCompany.db;
using System.Collections.Generic;
using System.Data.SqlClient;
using System;

namespace AnyCompany
{
    internal class OrderRepository
    {
        //private static string ConnectionString = ;

        private IDatabase _database;

        public OrderRepository(IDatabase database)
        {
            _database = database;
        }

        public void Save(Order order)
        {
            string command = "INSERT INTO Orders VALUES (@OrderId, @Amount, @VAT, @CustomerId)";

            _database.ExecuteNonQuery(command, new Dictionary<string, object> {
                { "@OrderId", order.OrderId }, { "@Amount", order.Amount }, { "@VAT", order.VAT},  { "@CustomerId", order.CustomerId}});
        }

        internal IEnumerable<Order> GetAllOrdersForCustomer(int customerId)
        {
            IEnumerable<Dictionary<string, object>> rawResult = _database.ExecuteReader("SELECT * FROM Orders where CustomerId = " + customerId, null);

            List<Order> orders = new List<Order>();
            foreach (Dictionary<string, object> rawOrder in rawResult)
            {
                orders.Add(ExtractOrder(rawOrder));
            }

            return orders;
        }

        private Order ExtractOrder(Dictionary<string, object> row)
        {
            Order order = new Order();
            order.VAT = Double.Parse(row["VAT"].ToString());
            order.Amount = Double.Parse(row["Amount"].ToString());
            order.CustomerId = Int32.Parse(row["CustomerId"].ToString());
            order.OrderId = Int32.Parse(row["OrderId"].ToString());

            return order;
        }
    }
}
