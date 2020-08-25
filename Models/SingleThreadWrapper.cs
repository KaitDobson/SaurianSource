using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaurianSource.Models
{ 

    public class SingleThreadWrapper 
    { 
        public User LoggedInUser { get; set; }

        public Thread Thread { get; set; }
    }
}