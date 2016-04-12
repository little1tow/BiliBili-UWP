using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.Web.Http;

namespace bilibili2
{
    class WebClientClass
    {
        public async Task<string> GetResults(Uri url)
        {
                using (HttpClient hc = new HttpClient())
                {
                    HttpResponseMessage hr = await hc.GetAsync(url);
                    hr.EnsureSuccessStatusCode();
                    string results = await hr.Content.ReadAsStringAsync();
                    return results;
                }
        }

        public async Task<IBuffer> GetBuffer(Uri url)
        {
            using (HttpClient hc = new HttpClient())
            {
                HttpResponseMessage hr = await hc.GetAsync(url);
                hr.EnsureSuccessStatusCode();
                IBuffer results = await hr.Content.ReadAsBufferAsync();
                return results;
            }
        }

        public async Task<string> PostResults(Uri url, string PostContent)
        {
            try
            {
                using (HttpClient hc = new HttpClient())
                {
                    hc.DefaultRequestHeaders.Referer = new Uri("http://www.bilibili.com/");
                    var response = await hc.PostAsync(url, new HttpStringContent(PostContent, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/x-www-form-urlencoded"));
                    response.EnsureSuccessStatusCode();
                    string result = await response.Content.ReadAsStringAsync();
                    return result;
                }
            }
            catch (Exception)
            {
                return "";
            }
        }

        public async Task<string> GetResultsUTF8Encode(Uri url)
        {
            try
            {
                using (HttpClient hc = new HttpClient())
                {
                    HttpResponseMessage hr = await hc.GetAsync(url);
                    hr.EnsureSuccessStatusCode();
                    var encodeResults = await hr.Content.ReadAsBufferAsync();
                    string results = Encoding.UTF8.GetString(encodeResults.ToArray(), 0, encodeResults.ToArray().Length);
                    return results;
                }
            }
            catch (Exception)
            {
                return "";
            }
        }
    }

    public class DHModel
    {
        public object result { get; set; }
        public object list { get; set; }

        public object recommends { get; set; }
        public string aid { get; set; }
        public string title { get; set; }
        public string play { get; set; }
        public string video_review { get; set; }
        public string mid { get; set; }
        public string pic { get; set; }
        public string author { get; set; }

        public object banners { get; set; }
        public string img { get; set; }

        public object news { get; set; }

    }
}
