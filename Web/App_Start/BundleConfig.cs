﻿using System.Configuration;
using System.Web;
using System.Web.Optimization;

namespace Compilify.Web
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            var minifyAssets = MinifyAssets();

            var css = new StyleBundle("~/css");

            if (!minifyAssets)
            {
                css.Transforms.Clear();
            }

            css.Include(
                "~/assets/css/vendor/bootstrap-2.0.2.css",
                "~/assets/css/vendor/font-awesome.css",
                "~/assets/css/vendor/codemirror-2.23.css",
                "~/assets/css/vendor/codemirror-neat-2.23.css",
                "~/assets/css/compilify.css");

            bundles.Add(css);

            var vendorjs = new ScriptBundle("~/js/libs");
            if (!minifyAssets)
            {
                vendorjs.Transforms.Clear();
            }

            vendorjs.Include(
                "~/assets/js/vendor/json2.js",
                "~/assets/js/vendor/underscore-1.3.1.js",
                "~/assets/js/vendor/bootstrap-2.0.2.js",
                "~/assets/js/vendor/codemirror-2.23.js",
                "~/assets/js/vendor/codemirror-clike-2.23.js",
                "~/assets/js/vendor/jquery.signalr-0.5.2.js",
                "~/assets/js/vendor/jquery.validate-1.8.0.js",
                "~/assets/js/vendor/jquery.validate.unobtrusive.js",
                "~/assets/js/vendor/jquery.validate-hooks.js",
                "~/assets/js/vendor/shortcut.js");
            
            vendorjs.ConcatenationToken = ";";
            bundles.Add(vendorjs);

            var js = new ScriptBundle("~/js/app");
            if (!minifyAssets)
            {
                js.Transforms.Clear();
            }

            js.Include("~/assets/js/compilify.js");
            js.ConcatenationToken = ";";
            bundles.Add(js);
        }

        private static bool MinifyAssets()
        {
            bool setting;
            if (bool.TryParse(ConfigurationManager.AppSettings["Compilify.MinifyAssets"], out setting))
            {
                return setting;
            }

            return !HttpContext.Current.IsDebuggingEnabled;
        }
    }
}