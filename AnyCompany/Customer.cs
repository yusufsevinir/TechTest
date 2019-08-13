using System;
using System.Collections.Generic;

namespace AnyCompany
{
    public class Customer
    {
        public string Country { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string Name { get; set; }

        public int CustomerId { get; set; }

        public virtual IEnumerable<Order> Orders { get; set; }
    }
}
