using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using TrackableEntities.Common.Core;

namespace NetCoreSample.Entities.WebApi
{
    public class Customer : ITrackable, IMergeable
    {
        public Customer()
        {
            Orders = new List<Order>();
        }

        public string CustomerId { get; set; }
        public string CompanyName { get; set; }
        public string ContactName { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public ICollection<Order> Orders { get; set; }

		[NotMapped]
        public TrackingState TrackingState { get; set; }

		[NotMapped]
        public ICollection<string> ModifiedProperties { get; set; }

		[NotMapped]
        public Guid EntityIdentifier { get; set; }
    }
}
