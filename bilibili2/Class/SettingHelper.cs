using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;

namespace bilibili2.Class
{
    class SettingHelper
    {

        ApplicationDataContainer container = ApplicationData.Current.LocalSettings;
        PackageId pack = (Package.Current).Id;
        public object GetSettingValue(string SettingName)
        {
            if (container.Values[SettingName] != null)
            {
                return container.Values[SettingName];
            }
            else
            {
                return null;
            }
        }
        public void SetSettingValue(string SettingName,object value)
        {
            container.Values[SettingName] = value;
        }
        public bool SettingContains(string SettingName)
        {
            if (container.Values[SettingName]!=null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public string GetVersion()
        {
            return  string.Format("{0}.{1}.{2}.{3}", pack.Version.Major, pack.Version.Minor, pack.Version.Build, pack.Version.Revision);
        }

    }
}
