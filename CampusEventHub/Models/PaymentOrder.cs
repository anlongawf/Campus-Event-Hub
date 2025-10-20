using System;
using System.ComponentModel.DataAnnotations;

namespace CampusEventHub.Models
{
    public class PaymentOrder
    {
        [Key]
        public int Id { get; set; }
        public int OrderCode { get; set; }
        public string UserId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public User User { get; set; }
    }
}