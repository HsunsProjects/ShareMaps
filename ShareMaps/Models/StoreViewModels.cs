using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShareMaps.Models
{
    public class StoreViewModel
    {
        public string id { get; set; }

        public string name { get; set; }

        public string address { get; set; }

        public string phoneNumber { get; set; }

        public string description { get; set; }

        public decimal lat { get; set; }

        public decimal lng { get; set; }

        public int shareTime { get; set; }
    }

    public class StoreStatusViewModel
    {
        public string id { get; set; }

        public string name { get; set; }

        public decimal lat { get; set; }

        public decimal lng { get; set; }

        public bool myStore { get; set; }
    }

    public class StoreInfoViewModel
    {
        public bool canEditDelete { get; set; }

        public StoreViewModel store { get; set; }

        public List<PhotoViewModel> storePhotos { get; set; }

        public List<IconTagViewModel> storeTags { get; set; }
    }
}