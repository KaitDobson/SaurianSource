using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaurianSource.Models
{ 

    public class Message 
    { 
        [Key]
        public int MessageId { get; set; }

        public int UserId { get; set; }

        // user who created Message
        public User User { get; set; }

        public string Text { get; set; }

        public int ThreadId { get; set; }

        // thread message belongs to
        public Thread Thread { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now; 
    }
}