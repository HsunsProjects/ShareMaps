using System.Web.Mvc;

namespace ShareMaps.App_Start
{
    public class ViewEngineConfig
    {
        public static void Register(ViewEngineCollection viewEngines)
        {
            viewEngines.Clear();
            viewEngines.Add(new CSharpRazorViewEngine());
        }
        internal class CSharpRazorViewEngine : RazorViewEngine
        {
            public CSharpRazorViewEngine()
            {
                AreaViewLocationFormats = new[]
                {
                    "~/Areas/{2}/Views/{1}/{0}.cshtml",
                    "~/Areas/{2}/Views/Shared/{0}.cshtml"
                };
                AreaMasterLocationFormats = new[]
                {
                    "~/Areas/{2}/Views/{1}/{0}.cshtml",
                    "~/Areas/{2}/Views/Shared/{0}.cshtml"
                };
                AreaPartialViewLocationFormats = new[]
                {
                    "~/Areas/{2}/Views/{1}/{0}.cshtml",
                    "~/Areas/{2}/Views/Shared/{0}.cshtml"
                };
                ViewLocationFormats = new[]
                {
                    "~/Views/{1}/{0}.cshtml",
                    "~/Views/Shared/{0}.cshtml"
                };
                MasterLocationFormats = new[]
                {
                    "~/Views/{1}/{0}.cshtml",
                    "~/Views/Shared/{0}.cshtml",
                };
                PartialViewLocationFormats = new[]
                {
                    "~/Views/{1}/{0}.cshtml",
                    "~/Views/Shared/{0}.cshtml"
                };
                FileExtensions = new[]
                {
                    "cshtml"
                };
            }
        }
    }
}