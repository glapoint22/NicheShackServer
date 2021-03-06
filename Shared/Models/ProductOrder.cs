﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models
{
    public class ProductOrder
    {
        [MaxLength(21)]
        public string Id { get; set; }
        [ForeignKey("Customer")]
        [Required]
        public string CustomerId { get; set; }
        [ForeignKey("Product")]
        [Required]
        public int ProductId { get; set; }
        public DateTime Date { get; set; }
        public int PaymentMethod { get; set; }
        public double Subtotal { get; set; }
        public double ShippingHandling { get; set; }
        public double Discount { get; set; }
        public double Tax { get; set; }
        public double Total { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual Product Product { get; set; }
        public virtual ICollection<OrderProduct> OrderProducts { get; set; }

        public ProductOrder()
        {
            OrderProducts = new HashSet<OrderProduct>();
        }
    }
}
