﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models
{
    public class RefreshToken
    {
        [MaxLength(256)]
        public string Id { get; set; }
        [ForeignKey("Customer")]
        public string CustomerId { get; set; }
        public DateTime Expiration { get; set; }
        public virtual Customer Customer { get; set; }
    }
}
