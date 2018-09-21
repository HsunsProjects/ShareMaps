using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShareMaps.Models
{
    public class TagViewModel
    {
        public Tags tag { get; set; }

        public bool isChecked { get; set; }
    }

    public class TagCountViewModel
    {
        public Tags tag { get; set; }

        public int count { get; set; }
    }
}