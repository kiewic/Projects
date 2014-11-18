using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace Tasks
{
    class LocalValues
    {
        private static IPropertySet localValues;

        internal static ApplicationDataCompositeValue GetFeeds()
        {
            CheckLocalValues();

            if (!localValues.Keys.Contains("feeds"))
            {
                localValues["feeds"] = new ApplicationDataCompositeValue();
            }

            return localValues["feeds"] as ApplicationDataCompositeValue;
        }

        internal static void SetFeeds(ApplicationDataCompositeValue feeds)
        {
            CheckLocalValues();

            localValues["feeds"] = feeds;
        }

        private static void CheckLocalValues()
        {
            if (localValues == null)
            {
                localValues = ApplicationData.Current.LocalSettings.Values;
            }
        }
    }
}
