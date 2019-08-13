using AnyCompany.db;
using System;
using System.Collections.Generic;

namespace AnyCompany
{
    public class OrderService
    {
        private readonly IDatabase _customerDb;

        private readonly OrderRepository _orderRepository;

        //ideally it will get 2 database implementations injected
        //this default constructor is just the remanent of old code
        //can be removed completely 
        public OrderService() : this(new MsSqlDatabase(@"Data Source=(local);Database=Customers;User Id=admin;Password=password;"),
            new MsSqlDatabase(@"Data Source=(local);Database=Orders;User Id=admin;Password=password;"))
        {
        }

        public OrderService(IDatabase customerDb, IDatabase orderDb)
        {
            _customerDb = customerDb;

            _orderRepository = new OrderRepository(orderDb);
        }

        public bool PlaceOrder(Order order, int customerId)
        {
            //verify the input
            if (order.Amount == 0)
                return false;

            //apply business logic, based on the customer
            Customer customer;
            try
            {
                customer = CustomerRepository.Load(customerId, _customerDb);
            }
            catch (Exception exp)
            {
                //log
                return false; // problem loading customer
            }

            if (customer.Country == "UK")
                order.VAT = 0.2d;
            else
                order.VAT = 0;

            order.CustomerId = customerId;

            _orderRepository.Save(order);

            return true;
        }


        public IEnumerable<Customer> GetAllCustomersAndTheirLinkedOrders()
        {
            //get customers
            IEnumerable<Customer> allCustomers = CustomerRepository.LoadAll(_customerDb);

            //populate all customers with their corresponding orders
            foreach(Customer customer in allCustomers)
            {
                customer.CustomerOrders = _orderRepository.GetAllOrdersForCustomer(customer.CustomerId);
            }

            return allCustomers;
        }
    }
}
