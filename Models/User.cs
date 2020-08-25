using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaurianSource.Models
{ 

    public class User 
    { 
        [Key]
        public int UserId { get; set; }

        [Required]
        [MinLength(2)]
        public string Name { get; set; }
    
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(8)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [NotMapped]
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string Confirm { get; set; }

        public string AvatarUrl { get; set; }

        public List<Message> Messages { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now; 
    }
}