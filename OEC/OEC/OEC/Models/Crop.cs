﻿using System;
using System.Collections.Generic;

namespace OEC.Models
{
    public partial class Crop
    {
        public Crop()
        {
            Variety = new HashSet<Variety>();
        }

        public int CropId { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }

        public virtual ICollection<Variety> Variety { get; set; }
    }
}
