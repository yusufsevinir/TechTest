using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using AnyCompany.Database;

namespace AnyCompany.Tests
{

    [TestClass]
    public class OrderServiceTests
    {
        [TestMethod]
        public void PlaceOrderWithValidCustomer()
        {
            //mock db layers
            var customerDb = MockRepository.GenerateMock<IDatabase>();
            var orderDb = MockRepository.GenerateMock<IDatabase>();

            OrderService orderService = new OrderService(customerDb, orderDb);

            //stub customerDb to retunr the customer
            customerDb.Stub(cdb => cdb.ExecuteReader(Arg<string>.Is.Anything, Arg<Dictionary<string, object>>.Is.Anything)).Return(new List<Dictionary<string, object>> { new Dictionary<string, object> { { "Name", "Cust01" }, { "DateOfBirth", "12/12/2000" }, { "Country", "AU" }, { "CustomerId", "1" } } });
            Assert.IsTrue(orderService.PlaceOrder(new Order { Amount = 10, OrderId = 1 }, 1));
        }

        [TestMethod]
        public void PlaceOrderWithNonExistentCustomer()
        {
            //mock db layers
            var customerDb = MockRepository.GenerateMock<IDatabase>();
            var orderDb = MockRepository.GenerateMock<IDatabase>();

            OrderService orderService = new OrderService(customerDb, orderDb);

            //stub customerDb to retunr the customer
            customerDb.Stub(cdb => cdb.ExecuteReader(Arg<string>.Is.Anything, Arg<Dictionary<string, object>>.Is.Anything)).Return(new List<Dictionary<string, object>> { });
            Assert.IsFalse(orderService.PlaceOrder(new Order { Amount = 10, OrderId = 1 }, 1));
        }

        [TestMethod]
        public void PlaceOrderWithDuplicateCustomer()
        {
            //mock db layers
            var customerDb = MockRepository.GenerateMock<IDatabase>();
            var orderDb = MockRepository.GenerateMock<IDatabase>();

            OrderService orderService = new OrderService(customerDb, orderDb);

            //stub customerDb to return 2 customers for same id
            customerDb.Stub(cdb => cdb.ExecuteReader(Arg<string>.Is.Anything, Arg<Dictionary<string, object>>.Is.Anything)).
                Return(new List<Dictionary<string, object>>
                    {
                    new Dictionary<string, object> { { "Name", "Cust01" }, { "DateOfBirth", "12/12/2000" }, { "Country", "AU" }, { "CustomerId", "1" } },
                    new Dictionary<string, object> { { "Name", "Cust02" }, { "DateOfBirth", "12/12/2000" }, { "Country", "CA" }, { "CustomerId", "1" } }
                });
            Assert.IsFalse(orderService.PlaceOrder(new Order { Amount = 10, OrderId = 1 }, 1));
        }

        [TestMethod]
        public void PlaceOrderAndVerifyVATCalculation()
        {
            //mock db layers
            var customerDb = MockRepository.GenerateMock<IDatabase>();
            var orderDb = MockRepository.GenerateMock<IDatabase>();

            OrderService orderService = new OrderService(customerDb, orderDb);

            var order = new Order { Amount = 10, OrderId = 1 };

            //stub customerDb to return non UK customer
            customerDb.Stub(cdb => cdb.ExecuteReader(Arg<string>.Is.Anything, Arg<Dictionary<string, object>>.Is.Anything)).Return(new List<Dictionary<string, object>> { new Dictionary<string, object> { { "Name", "Cust01" }, { "DateOfBirth", "12/12/2000" }, { "Country", "AU" }, { "CustomerId", "1" } } }).Repeat.Once();
            Assert.IsTrue(orderService.PlaceOrder(order, 1));
            Assert.AreEqual(order.VAT, 0);

            //stub customerDb to return a UK customer
            customerDb.Stub(cdb => cdb.ExecuteReader(Arg<string>.Is.Anything, Arg<Dictionary<string, object>>.Is.Anything)).Return(new List<Dictionary<string, object>> { new Dictionary<string, object> { { "Name", "Cust02" }, { "DateOfBirth", "12/12/2000" }, { "Country", "UK" }, { "CustomerId", "1" } } });
            Assert.IsTrue(orderService.PlaceOrder(order, 1));
            Assert.AreEqual(order.VAT, 0.2);
        }


        [TestMethod]
        public void LoadAllCustomersFromEmptyCustomerDb()
        {
            //mock db layers
            var customerDb = MockRepository.GenerateMock<IDatabase>();
            var orderDb = MockRepository.GenerateMock<IDatabase>();

            OrderService orderService = new OrderService(customerDb, orderDb);

            //stub customerDb to retunr the customer
            customerDb.Stub(cdb => cdb.ExecuteReader(Arg<string>.Is.Anything, Arg<Dictionary<string, object>>.Is.Anything)).Return(new List<Dictionary<string, object>> { });
            Assert.IsTrue(orderService.GetAllCustomersAndTheirLinkedOrders().FirstOrDefault() == null);
        }

        [TestMethod]
        public void LoadAllCustomersFromNonEmptyCustomerDb()
        {
            //mock db layers
            var customerDb = MockRepository.GenerateMock<IDatabase>();
            var orderDb = MockRepository.GenerateMock<IDatabase>();

            OrderService orderService = new OrderService(customerDb, orderDb);

            //stub customerDb to retunr the customers
            customerDb.Stub(cdb => cdb.ExecuteReader(Arg<string>.Is.Anything, Arg<Dictionary<string, object>>.Is.Anything)).
                Return(new List<Dictionary<string, object>>
                    {
                    new Dictionary<string, object> { { "Name", "Cust01" }, { "DateOfBirth", "12/12/2000" }, { "Country", "AU" }, { "CustomerId", "1" } },
                    new Dictionary<string, object> { { "Name", "Cust02" }, { "DateOfBirth", "10/10/2000" }, { "Country", "UK" }, { "CustomerId", "2" } },
                    new Dictionary<string, object> { { "Name", "Cust03" }, { "DateOfBirth", "8/8/2000" }, { "Country", "CA" }, { "CustomerId", "3" } }
                });

            //stub orders db for above customers
            orderDb.Stub(cdb => cdb.ExecuteReader(Arg<string>.Is.Anything, Arg<Dictionary<string, object>>.Is.Anything)).
                Return(new List<Dictionary<string, object>>
                    {
                    new Dictionary<string, object> { { "Amount", "100" }, { "VAT", "0.0" }, { "CustomerId", "1" }, { "OrderId", "100" } },
                    new Dictionary<string, object> { { "Amount", "202.3" }, { "VAT", "0.0" }, { "CustomerId", "1" }, { "OrderId", "101" } }
                }).Repeat.Once();

            orderDb.Stub(cdb => cdb.ExecuteReader(Arg<string>.Is.Anything, Arg<Dictionary<string, object>>.Is.Anything)).
                Return(new List<Dictionary<string, object>>
                    {
                    new Dictionary<string, object> { { "Amount", "300" }, { "VAT", "0.2" }, { "CustomerId", "2" }, { "OrderId", "200" } },
                }).Repeat.Once();

            orderDb.Stub(cdb => cdb.ExecuteReader(Arg<string>.Is.Anything, Arg<Dictionary<string, object>>.Is.Anything)).
                Return(new List<Dictionary<string, object>> {}).Repeat.Once();

            var result = orderService.GetAllCustomersAndTheirLinkedOrders().ToList();
            Assert.AreEqual(result.Count, 3);

            Assert.AreEqual(result[0].CustomerId, 1);
            Assert.AreEqual(result[0].CustomerOrders.Count(), 2);
            Assert.AreEqual(result[0].CustomerOrders.First().OrderId, 100);

            Assert.AreEqual(result[1].CustomerId, 2);
            Assert.AreEqual(result[1].CustomerOrders.Count(), 1);
            Assert.AreEqual(result[1].CustomerOrders.First().OrderId, 200);

            Assert.AreEqual(result[2].CustomerId, 3);
            Assert.AreEqual(result[2].CustomerOrders.Count(), 0);
        }

    }
}
