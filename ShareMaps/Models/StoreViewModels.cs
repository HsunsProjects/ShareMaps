using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShareMaps.Models
{
    public class StoreViewModel
    {
        public bool canEditDelete { get; set; }

        public Stores store { get; set; }

        public List<Photos> storePhotos { get; set; }

        public List<Tags> storeTags { get; set; }
    }
}