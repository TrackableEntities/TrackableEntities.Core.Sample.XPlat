using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using TrackableEntities.Common.Core;

namespace NetCoreSample.Entities.WebApi
{
    public class Order : ITrackable, IMergeable
    {
        public Order()
        {
            OrderDetails = new List<OrderDetail>();
        }

        public int OrderId { get; set; }
        public string CustomerId { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? ShippedDate { get; set; }
        public int? ShipVia { get; set; }
        public decimal? Freight { get; set; }
        public Customer Customer { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; }

		[NotMapped]
        public TrackingState TrackingState { get; set; }

		[NotMapped]
        public ICollection<string> ModifiedProperties { get; set; }

		[NotMapped]
        public Guid EntityIdentifier { get; set; }
    }
}
