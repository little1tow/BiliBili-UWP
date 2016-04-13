using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bilibili2
{
    //Banner
    public class BannerModel
    {
        public int results { get; set; }
        public object data { get; set; }
        public string title { get; set; }
        public int type { get; set; }
        public string image { get; set; }
        public string value { get; set; }

    }
    //视频详细信息
    public class VideoInfoModel
    {
        public int code { get; set; }
        public string error { get; set; }

        public object list { get; set; }
        public string pic { get; set; }
        public string title { get; set; }
        public string play { get; set; }
        public string author { get; set; }
        public string video_review { get; set; }
        public string description { get; set; }
        public string mid { get; set; }
        public string aid { get; set; }
        public string created_at { get; set; }
        public string favorites { get; set; }
        public string face { get; set; }

        public string coins { get; set; }
        public string page { get; set; }
        public string part { get; set; }
        public string cid { get; set; }
        public string tag { get; set; }

        public string Play
        {
            get
            {
                if (Convert.ToInt32(play) > 10000)
                {
                    double d = (double)Convert.ToDouble(play) / 10000;
                    return d.ToString("0.0") + "万";
                }
                else
                {
                    return play;
                }
            }
        }
        public string Video_review
        {
            get
            {
                if (Convert.ToInt32(video_review) > 10000)
                {
                    double d = (double)Convert.ToDouble(video_review) / 10000;
                    return d.ToString("0.0") + "万";
                }
                else
                {
                    return video_review;
                }
            }
        }
        public string Favorites
        {
            get
            {
                if (Convert.ToInt32(favorites) > 10000)
                {
                    double d = (double)Convert.ToDouble(favorites) / 10000;
                    return d.ToString("0.0") + "万";
                }
                else
                {
                    return favorites;
                }
            }
        }
        public string Coins
        {
            get
            {
                if (Convert.ToInt32(coins) > 10000)
                {
                    double d = (double)Convert.ToDouble(coins) / 10000;
                    return d.ToString("0.0") + "万";
                }
                else
                {
                    return coins;
                }
            }
        }

        public string Created_at {
            get
            {
                DateTime dt = Convert.ToDateTime(created_at);
                if (dt.Date==DateTime.Now.Date)
                {
                    TimeSpan ts = DateTime.Now - dt;
                    return ts.Hours+"小时前";
                }
                else
                {
                    return created_at;
                }
            }
        }
    }
    //评论
    public class CommentModel
    {

        public object data { get; set; }
        public object replies { get; set; }
        public object member { get; set; }
        public object content { get; set; }
        public string avatar { get; set; }
        public string uname { get; set; }
        public string floor { get; set; }
        public string rpid { get; set; }
        public long ctime { set; get; }
        public int plat { get; set; }
        public string Plat
        {
            get
            {
                switch (plat)
                {
                    case 2:
                        return "安卓客户端";
                    case 3:
                        return "IOS客户端";
                    case 4:
                        return "Windows客户端";
                    case 6:
                        return "Windows客户端";
                    default:
                        return "";
                }
            }
        }
        public string mid { get; set; }
        public string time
        {
            get
            {
                DateTime dtStart = new DateTime(1970, 1, 1);
                long lTime = long.Parse(ctime + "0000000");
                //long lTime = long.Parse(textBox1.Text);
                TimeSpan toNow = new TimeSpan(lTime);
                return dtStart.Add(toNow).ToString();
            }
        }
        public string rcount { get; set; }
        public string like { get; set; }
        public string message { get; set; }

    }
    //视频相关
    public class RecommendModel
    {
        public string dm_count { get; set; }
        public string title { get; set; }
        public string id { get; set; }
        public string click { get; set; }
        public string pic { get; set; }
        public string author_name { get; set; }
    }
    //这个Model用来保存登录请求的access_key
    public class LoginModel
    {
        private string _access_key;
        public string access_key
        {
            get { return _access_key; }
            set { _access_key = value; }
        }
        public string mid { get; set; }
        public int code { get; set; }
        public string expires
        {
            get; set;
        }
    }
    //用户信息
   public  class GetLoginInfoModel
    {
        public string mid { get; set; }//ID
        public string name { get; set; }//昵称
        public string sex { get; set; }//性别
        public string coins { get; set; }//硬币
        public string face { get; set; }//头像
        public string birthday { get; set; }//生日
        public long regtime { get; set; }//注册时间
        public string Regtime
        {
            get
            {
                DateTime dtStart = new DateTime(1970, 1, 1);
                long lTime = long.Parse(regtime + "0000000");
                TimeSpan toNow = new TimeSpan(lTime);
                return dtStart.Add(toNow).ToString("d");
            }
        }//转换后注册时间
        public string sign { get; set; }//个性签名
        public int fans { get; set; }//粉丝
        public string attention { get; set; }//关注
        public object level_info { get; set; }//等级信息

        public string current_level_string { get { return "LV" + current_level; } }//等级
        public int current_level { get; set; }
        public string place { get; set; }//地址
    }
    //首页信息
    public class InfoModel
    {
        public object list { get; set; }
        public string pic { get; set; }
        public string title { get; set; }
        public string play { get; set; }
        public string author { get; set; }
        public string video_review { get; set; }
        public string description { get; set; }
        public string mid { get; set; }
        public string aid { get; set; }
        public int num { get; set; }

    }
    //热门搜索
    public class HotModel
    {
        public object list { get; set; }
        public string keyword { get; set; }
        public string status { get; set; }
    }
    //用户追番
    public class GetUserBangumi
    {
        //Josn：http://space.bilibili.com/ajax/Bangumi/getList?mid=XXX&pagesize=9999
        //第一层
        public bool status { get; set; }//状态
        public object data { get; set; }//数据，包含第二层
                                        //第二层
        public int count { get; set; }//总数量
        public object result { get; set; }//结果，包含第三层
                                          //第三层
        public string season_id { get; set; }//专题ID，重要！！！
        public string title { get; set; }//标题
        public int is_finish { get; set; }//是否完结，0为连载，1为完结
        public string favorites { get; set; }//有多少人关注
        public int newest_ep_index { get; set; }//最新话
        public int total_count { get; set; }//一共多少话
        public string NewOver
        {
            get
            {
                if (is_finish == 0)
                {
                    return "更新至第" + newest_ep_index + "话";
                }
                else
                {
                    return total_count + "话全";
                }
            }
        }
        public string cover { get; set; }//封面
        public string brief { get; set; }//简介
        public int pages { get; set; }
    }
    //用户动态
    public class GetAttentionUpdate
    {
        //必须有登录Cookie
        //Josn：http://api.bilibili.com/x/feed/pull?jsonp=jsonp&ps=20&type=1&pn=1
        //第一层
        public int code { get; set; }//状态，0为正常
        public object data { get; set; }//数据，包含第二层
                                        //第二层
        public object feeds { get; set; }//结果，包含第三层
        public object page { get; set; }//结果数量，包含第三层
                                        //第三层
        public string add_id { get; set; }//视频ID
        public object source { get; set; }//作者信息，包含第四层
        public object addition { get; set; }//视频信息，包含第四层
                                            //第四层
        public string author { get; set; }//上传人员
        public string mid { get; set; }//上传人员ID
        public string aid { get; set; }//视频ID
        public string title { get; set; }//标题
        public string play { get; set; }//播放数
        public string video_review { get; set; }//弹幕数
        public string create { get; set; }//上传时间
        public string pic { get; set; }//封面
    }
    //话题
    public class TopicModel
    {
        public int results { get; set; }
        public object list { get; set; }
        public string title { get; set; }
        public string img { get; set; }
        public string link { get; set; }
    }
    //排行
    public class RankModel
    {
        public object hot_original { get; set; }
        public object list { get; set; }
        public string pic { get; set; }
        public string title { get; set; }
        public string play { get; set; }
        public string author { get; set; }
        public string video_review { get; set; }
        public string description { get; set; }
        public string mid { get; set; }
        public string aid { get; set; }
        public int num { get; set; }
    }
    //番剧更新表
    public class BangumiTimeLineModel
    {
        public object list { get; set; }
        public string bgmcount { get; set; }
        public string cover { get; set; }
        public string lastupdate_at { get; set; }
        public string title { get; set; }
        public string square_cover { get; set; }
        public int weekday { get; set; }
        public string spid { get; set; }
        public string season_id { get; set; }
    }
    //番剧索引
    public class TagModel
    {
        public object result { get; set; }
        public string cover { get; set; }
        public int index { get; set; }
        public int tag_id { get; set; }
        public string tag_name { get; set; }
    }
    //番剧信息
    public class BangumiInfoModel
    {
        public int code { get; set; }
        public string message { get; set; }
        public object result { get; set; }

        public string alias { get; set; }//别名
        public string area { get; set; }//地区
        public string bangumi_title { get; set; }//番剧名。与season_title不同
        public string season_title { get; set; }//专题名
        public string title { get; set; }
        public string evaluate { get; set; }//介绍
        public int favorites { get; set; }//订阅
        public int coins { get; set; }//硬币
        public int play_count { get; set; }
        public int danmaku_count { get; set; }
        public int is_finish { get; set; }//是否完结
        public int newest_ep_index { get; set; }//最新话
        public string staff { get; set; }
        public string cover { get; set; }
        public DateTime pub_time { get; set; }

        public object user_season { get; set; }
        public int attention { get; set; }//是否关注
                                          // public double last_ep_index { get; set; }//看到

        public object tags { get; set; }
        public string index { get; set; }
        public int tag_id { get; set; }
        public string tag_name { get; set; }

        public object actor { get; set; }
        public string role { get; set; }

        public object episodes { get; set; }
        public string av_id { get; set; }
        public int danmaku { get; set; }
        public string index_title { get; set; }

        public int total_count { get; set; }
        public string status
        {
            get
            {
                if (is_finish == 1)
                {
                    return string.Format("已完结，共{0}话", total_count);
                }
                else
                {
                    return string.Format("连载中，更新至{0}话", newest_ep_index) ?? "";
                }
            }
        }

        public string upTime
        {
            get
            {
                if (pub_time != null)
                {
                    return pub_time.Date.ToString("d");
                }
                else
                {
                    return "";
                }
            }

        }

        public string PlayCount
        {
            get
            {
                if (play_count > 10000)
                {
                    return ((double)play_count / 10000).ToString("0.0" + "万");
                }
                else
                {
                    return play_count.ToString();
                }
            }
        }

        public string favoritesCount
        {
            get
            {
                if (favorites > 10000)
                {
                    return ((double)favorites / 10000).ToString("0.0" + "万");
                }
                else
                {
                    return favorites.ToString();
                }
            }
        }


    }
    //番剧Tag
    public class BanSeasonTagModel
    {
        public int count { get; set; }
        public int pages { get; set; }
        public object result { get; set; }

        public string bangumi_title { get; set; }//标题
        public string brief { get; set; }//简介
        public string pub_time { get; set; }//时间
        public string title { get; set; }
        public string Time
        {
            get
            {
                try
                {
                    return String.Format("{0}年{1}月", DateTime.Parse(pub_time).Year, DateTime.Parse(pub_time).Month);
                }
                catch (Exception)
                {
                    return "";
                }
                //string a = pub_time.Remove(11, pub_time.Length - 1);
            }
        }
        public string squareCover { get; set; }//封面
        public string season_id { get; set; }//SID
        public int is_finish { get; set; }//是否完结
        public string newest_ep_index { get; set; }//最新话
        public string Is_finish
        {
            get
            {
                if (is_finish == 1)
                {
                    return String.Format("已完结,共{0}话", newest_ep_index);
                }
                else
                {
                    return String.Format("更新至{0}话", newest_ep_index);
                }
            }
        }
        public int favorites { get; set; }//订阅数
        public string Favorites
        {
            get
            {
                return String.Format("{0}万人订阅", (double)favorites / 10000);
            }
        }
    }
}
