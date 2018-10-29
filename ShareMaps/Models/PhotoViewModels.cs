using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShareMaps.Models
{
    public class PhotoViewModel
    {
        public int id { get; set; }
        public string path { get; set; }
        public bool isMain { get; set; }
        public Nullable<int> sequence { get; set; }
    }
}