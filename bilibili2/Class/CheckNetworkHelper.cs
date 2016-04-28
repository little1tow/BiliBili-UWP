/**
——————此类由 旋风小伙(微页开发者) 提供——————
    ——————  http://toosame.com/——————
**/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;

namespace bilibili2
{
    /// <summary>
    /// 表示网络连接的类型。
    /// </summary>
    public enum InternetConnectionType
    {
        /// <summary>
        /// 表示无网络连接。
        /// </summary>
        NoConnection = 0,
        /// <summary>
        /// 表示 WWAN（移动）连接。
        /// </summary>
        WwanConnection = 1,
        /// <summary>
        /// 表示 WLAN (WiFi) 连接。
        /// </summary>
        WlanConnection = 2
    }

    public class CheckNetworkHelper
    {
        /// <summary>
        /// 检查网络是否连接。
        /// </summary>
        /// <returns>如果网络已连接，则为 true；否则为 false。</returns>
        public static bool CheckNetwork()
        {
            bool isConnection = false;
            ConnectionProfile InternetConnectionProfile = NetworkInformation.GetInternetConnectionProfile();

            if (InternetConnectionProfile != null)
            {
                isConnection = true;
            }

            return isConnection;
        }

        /// <summary>
        /// 检查网络连接类型。
        /// </summary>
        /// <returns>返回一个 <see cref="InternetConnectionType"/>。</returns>
        public static InternetConnectionType CheckInternetConnectionType()
        {
            InternetConnectionType connectionType = InternetConnectionType.NoConnection;
            ConnectionProfile InternetConnectionProfile = NetworkInformation.GetInternetConnectionProfile();

            if (InternetConnectionProfile != null)
            {
                if (InternetConnectionProfile.IsWwanConnectionProfile)
                {
                    connectionType = InternetConnectionType.WwanConnection;
                }
                else if (InternetConnectionProfile.IsWlanConnectionProfile)
                {
                    connectionType = InternetConnectionType.WlanConnection;
                }
            }

            return connectionType;
        }
    }
}
