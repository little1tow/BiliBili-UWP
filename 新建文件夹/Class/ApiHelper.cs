using BILIBILI_UWP.Class;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace bilibili2
{
    class ApiHelper
    {
        public const string _appSecret = "ba3a4e554e9a6e15dc4d1d70c2b154e3";
        public const string _appKey = "422fd9d7289a1dd9";
        public static string access_key = string.Empty;

        public static string GetSign(string url)
        {
            string lower;
            string str = url.Substring(url.IndexOf("?", 4) + 1);
            string[] strArray = new string[] { "&" };
            List<string> list = str.Split('&').ToList<string>();
            list.Sort();
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string str1 in list)
            {
                stringBuilder.Append((stringBuilder.Length > 0 ? "&" : string.Empty));
                stringBuilder.Append(str1);
            }
            stringBuilder.Append("ba3a4e554e9a6e15dc4d1d70c2b154e3");
            lower = MD5.GetMd5String(stringBuilder.ToString()).ToLower();

            return lower;
        }
        public static long GetTimeSpen
        {
            get { return Convert.ToInt64((DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds); }
        }

        public static async Task<string> GetEncryptedPassword(string pw)
        {
            string base64String;
            try
            {
                //https://secure.bilibili.com/login?act=getkey&rnd=4928
                //https://passport.bilibili.com/login?act=getkey&rnd=4928
                HttpBaseProtocolFilter httpBaseProtocolFilter = new HttpBaseProtocolFilter();
                httpBaseProtocolFilter.IgnorableServerCertificateErrors.Add(Windows.Security.Cryptography.Certificates.ChainValidationResult.Expired);
                httpBaseProtocolFilter.IgnorableServerCertificateErrors.Add(Windows.Security.Cryptography.Certificates.ChainValidationResult.Untrusted);
                Windows.Web.Http.HttpClient httpClient = new Windows.Web.Http.HttpClient(httpBaseProtocolFilter);
               //WebClientClass wc = new WebClientClass();
                string stringAsync =  await httpClient.GetStringAsync((new Uri("https://secure.bilibili.com/login?act=getkey&rnd=" + new Random().Next(1000,9999), UriKind.Absolute)));
                JObject jObjects = JObject.Parse(stringAsync);
                string str = jObjects["hash"].ToString();
                string str1 = jObjects["key"].ToString();
                string str2 = string.Concat(str, pw);
                string str3 = Regex.Match(str1, "BEGIN PUBLIC KEY-----(?<key>[\\s\\S]+)-----END PUBLIC KEY").Groups["key"].Value.Trim();
                byte[] numArray = Convert.FromBase64String(str3);
                AsymmetricKeyAlgorithmProvider asymmetricKeyAlgorithmProvider = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(AsymmetricAlgorithmNames.RsaPkcs1);
                CryptographicKey cryptographicKey = asymmetricKeyAlgorithmProvider.ImportPublicKey(WindowsRuntimeBufferExtensions.AsBuffer(numArray), 0);
                IBuffer buffer = CryptographicEngine.Encrypt(cryptographicKey, WindowsRuntimeBufferExtensions.AsBuffer(Encoding.UTF8.GetBytes(str2)), null);
                base64String = Convert.ToBase64String(WindowsRuntimeBufferExtensions.ToArray(buffer));
        }
            catch (Exception)
            {
                throw;
                //base64String = pw;
            }
            return base64String;
        }

        public static async Task<string> GetUserCookie(string accessKey)
        {
            string empty;
            try
            {
                string[] strArray = new string[] { "http://api.bilibili.com/login/sso?gourl=", WebUtility.UrlEncode("http://www.bilibili.com"), "&access_key=", accessKey, "&appkey=", _appKey, "&platform=wp" };
                string str = string.Concat(strArray);
                str = string.Concat(str, "&sign=", GetSign(str));
                System.Net.Http.HttpClient httpClient = new System.Net.Http.HttpClient(new HttpClientHandler()
                {
                    AllowAutoRedirect = false
                });
                System.Net.Http.HttpResponseMessage async = await httpClient.GetAsync(new Uri(str));
                IEnumerable<string> values = async.Headers.GetValues("Set-Cookie");
                string empty1 = string.Empty;
                foreach (string value in values)
                {
                    empty1 = string.Concat(empty1, value);
                    empty1 = string.Concat(empty1, ";");
                }
                Match match = Regex.Match(empty1, "DedeUserID=(?<uid>.*?);");
                Match match1 = Regex.Match(empty1, "DedeUserID__ckMd5=.*?;");
                Match match2 = Regex.Match(empty1, "SESSDATA=.*?;");

                if (match.Success && match1.Success && match2.Success)
                {
                    object[] objArray = new object[] { match.Value, match1.Value, match2.Value };
                    empty = string.Format("{0} {1} {2}", objArray);
                    return empty;
                }
            }
            catch (Exception)
            {
            }
            empty = string.Empty;
            return empty;
        }

        public static async Task<string> loginBilibili(string UserName, string Password)
        {
            try
            {
                //发送第一次请求，得到access_key
                WebClientClass wc = new WebClientClass();
                string results =await wc.GetResults(new Uri("https://api.bilibili.com/login?appkey=422fd9d7289a1dd9&platform=wp&pwd=" + Password + "&type=json&userid=" + UserName));
                //Json解析及数据判断
                LoginModel model = new LoginModel();
                model = JsonConvert.DeserializeObject<LoginModel>(results);
                if (model.code == -627)
                {
                    return "登录失败，密码错误！";
                }
                if (model.code == -626)
                {
                    return "登录失败，账号不存在！";
                }
                if (model.code == -625)
                {
                    return "密码错误多次";
                }
                if (model.code==-628)
                {
                    return "未知错误";
                }
                if (model.code == -1)
                {
                    return "登录失败，程序注册失败！请联系作者！";
                }
                Windows.Web.Http.HttpClient hc = new Windows.Web.Http.HttpClient();
                if (model.code == 0)
                {
                    access_key = model.access_key;
                    Windows.Web.Http.HttpResponseMessage hr2 = await hc.GetAsync(new Uri("http://api.bilibili.com/login/sso?&access_key=" + model.access_key + "&appkey=422fd9d7289a1dd9&platform=wp"));
                    hr2.EnsureSuccessStatusCode();
                    StorageFolder folder = ApplicationData.Current.LocalFolder;
                    StorageFile file = await folder.CreateFileAsync("us.bili", CreationCollisionOption.OpenIfExists);
                    await FileIO.WriteTextAsync(file, model.access_key);
                }
                //看看存不存在Cookie
                HttpBaseProtocolFilter hb = new HttpBaseProtocolFilter();
                HttpCookieCollection cookieCollection = hb.CookieManager.GetCookies(new Uri("http://bilibili.com/"));
                List<string> ls = new List<string>();
                foreach (HttpCookie item in cookieCollection)
                {
                    ls.Add(item.Name);
                }
                if (ls.Contains("DedeUserID"))
                {
                    return "登录成功";
                }
                else
                {
                    return "登录失败";
                }
            }
            catch (Exception)
            {
                return "登录发生错误";
            }

        }


    }
}
