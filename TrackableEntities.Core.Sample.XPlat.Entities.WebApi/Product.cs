using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using TrackableEntities.Common.Core;

namespace NetCoreSample.Entities.WebApi
{
    public class Product : ITrackable, IMergeable
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int? CategoryId { get; set; }
        public decimal? UnitPrice { get; set; }
        public bool Discontinued { get; set; }
        public byte[] RowVersion { get; set; }
        public Category Category { get; set; }

		[NotMapped]
        public TrackingState TrackingState { get; set; }

		[NotMapped]
        public ICollection<string> ModifiedProperties { get; set; }

		[NotMapped]
        public Guid EntityIdentifier { get; set; }
    }
}
