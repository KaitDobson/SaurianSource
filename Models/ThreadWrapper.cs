using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaurianSource.Models
{ 

    public class ThreadWrapper 
    { 
        public User LoggedInUser { get; set; }

        public List<Thread> Threads { get; set; }
    }
}