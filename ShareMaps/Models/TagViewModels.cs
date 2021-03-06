﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShareMaps.Models
{
    public class TagViewModel
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    public class TagCheckedViewModel
    {
        public Tags tag { get; set; }
        public bool isChecked { get; set; }
    }

    public class TagCountViewModel
    {
        public int tagId { get; set; }
        public string tagName { get; set; }
        public int tagCount { get; set; }
        public string iconValue { get; set; }
    }

    public class IconTagViewModel
    {
        public Icons icon { get; set; }
        public Tags tag { get; set; }
    }
}