using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Recorder.Models;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Recorder
{
    static class HelperExtensions
    {
        public static R Let<T, R>(this T self, Func<T, R> block)
        {
            return block(self);
        }

        public static string ToLocalString(this Dictionary<string, string> dictionary)
        {
            return new LanguageString(dictionary).Localized;
        }

        public static async Task PushAsyncThenRemove(this INavigation nav, Page push, Page remove)
        {
            await nav.PushAsync(push);
            nav.RemovePage(remove);
        }

        public async static void PushAsyncThenClearHistory(this INavigation nav, Page push)
        {
            List<Page> history = new List<Page>(nav.NavigationStack);
            await nav.PushAsync(push);

            foreach (Page p in history)
            {
                if (p != null)
                {
                    nav.RemovePage(p);
                }
            }
        }

        public static object GetOrNull(this ResourceDictionary dictionary, string key)
        {
            if (dictionary.ContainsKey(key))
            {
                return dictionary[key];
            }
            else
            {
                return null;
            }
        }

        public static double GetHeightInSixteenNine(this DisplayInfo display)
        {
            double width = display.Width / display.Density;
            double aspect = 16.0 / 9.0;
            return width / aspect;
        }
    }
}
