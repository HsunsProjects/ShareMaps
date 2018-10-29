using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ShareMaps.Models
{
    public class HomeIndexViewModel
    {
        public List<StoreEditDeleteViewModel> stores { get; set; }
        public List<TagCountViewModel> tags { get; set; }
    }

    public class StoreCreateViewModel
    {
        public Stores store { get; set; }

        public List<string> filenames { get; set; }

        public List<TagCheckedViewModel> storeTags { get; set; }
    }

    public class StoreEditViewModel
    {
        public Stores store { get; set; }

        public List<Photos> storePhotos { get; set; }

        public List<TagCheckedViewModel> storeTags { get; set; }
    }

    public class TagsManagementViewModel
    {
        public IconTagViewModel addIconTag { get; set; }
        public List<Icons> iconList { get; set; }
        public List<IconTagViewModel> iconTagList { get; set; }
    }
}