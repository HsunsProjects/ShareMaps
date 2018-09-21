using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ShareMaps.Models
{
    public class HomeIndexViewModel
    {
        public List<StoreViewModel> stores { get; set; }
        public List<TagCountViewModel> tags { get; set; }
    }

    public class StoreCreateViewModel
    {
        public Stores store { get; set; }

        public List<string> filenames { get; set; }

        public List<TagViewModel> storeTags { get; set; }
    }

    public class StoreEditViewModel
    {
        public Stores store { get; set; }

        public List<Photos> storePhotos { get; set; }

        public List<TagViewModel> storeTags { get; set; }
    }

    public class TagsManagementViewModel
    {
        public Tags tags { get; set; }
        public List<Tags> tagList { get; set; }
    }
}