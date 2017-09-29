using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using TrackableEntities.Common.Core;

namespace NetCoreSample.Entities.WebApi
{
    public class Category : ITrackable, IMergeable
    {
        public Category()
        {
            Products = new List<Product>();
        }

        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public ICollection<Product> Products { get; set; }

		[NotMapped]
        public TrackingState TrackingState { get; set; }

		[NotMapped]
        public ICollection<string> ModifiedProperties { get; set; }

		[NotMapped]
        public Guid EntityIdentifier { get; set; }
    }
}
