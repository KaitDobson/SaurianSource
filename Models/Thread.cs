using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaurianSource.Models
{ 

    public class Thread 
    { 
        [Key]
        public int ThreadId { get; set; }

        public int UserId { get; set; }

        // user who created thread
        public User User { get; set; }

        public string Title { get; set; }

        public List<Message> Messages { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now; 
    }
}