using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SaurianSource.Models
{ 

    public class UserPageWrapper 
    { 
        public User LoggedInUser { get; set; }

        public User DetailUser { get; set; }

        public int TotalPostsForDetailUser { get; set; }
    }
}