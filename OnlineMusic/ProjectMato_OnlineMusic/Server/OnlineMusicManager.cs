using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OnlineMusic.Helper;
using ProjectMato.Model;
using ProjectMato.Model.Interface;
using Xamarin.Forms;

/*
   此库基于第三方库改写
   原作者信息:Twilight./Lemon QQ:2728578956 

*/
namespace OnlineMusic.Server
{
    public class OnlineMusicManager
    {

        public IHttpHelper HttpHelper => DependencyService.Get<IHttpHelper>();

        public Dictionary<string, string> mldata = new Dictionary<string, string>();// mid,name
        public async Task<List<MusicInfo>> SearchMusicAsync(string Content, int osx = 1)
        {
            if (HttpHelper.IsNetworkTrue())
            {
                JObject o = JObject.Parse(await HttpHelper.GetWebAsync($"http://59.37.96.220/soso/fcgi-bin/client_search_cp?format=json&t=0&inCharset=GB2312&outCharset=utf-8&qqmusic_ver=1302&catZhida=0&p={osx}&n=20&w={Content}&flag_qc=0&remoteplace=sizer.newclient.song&new_json=1&lossless=0&aggr=1&cr=1&sem=0&force_zonghe=0"));
                List<MusicInfo> dt = new List<MusicInfo>();
                int i = 0;
                while (i < o["data"]["song"]["list"].Count())
                {
                    MusicInfo m = new MusicInfo();
                    m.Title = o["data"]["song"]["list"][i]["name"].ToString().Replace("\\", "-").Replace("?", "").Replace("/", "").Replace(":", "").Replace("*", "").Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", "");
                    string Artist = "";
                    for (int osxc = 0; osxc != o["data"]["song"]["list"][i]["singer"].Count(); osxc++)
                    { Artist += o["data"]["song"]["list"][i]["singer"][osxc]["name"] + "&"; }
                    m.Artist = Artist.Substring(0, Artist.LastIndexOf("&"));
                    m.OnlineId = o["data"]["song"]["list"][i]["mid"].ToString();
                    m.AlbumArtPath = $"http://y.gtimg.cn/music/photo_new/T002R300x300M000{o["data"]["song"]["list"][i]["album"]["mid"]}.jpg";
                    m.GC = o["data"]["song"]["list"][i]["id"].ToString();
                    m.Url = await GetUrlAsync(m.OnlineId);

                    dt.Add(m);
                    try
                    {
                        if (!mldata.ContainsKey(m.OnlineId))
                            mldata.Add(m.OnlineId, (m.Title + " - " + m.Artist).Replace("\\", "-").Replace("?", "").Replace("/", "").Replace(":", "").Replace("*", "").Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", ""));
                    }
                    catch { }
                    i++;
                }
                return dt;
            }
            else return null;
        }
        public async Task<Billboard> GetGDAsync(string id = "2591355982")
        {
            var s = await HttpHelper.GetWebDatacAsync($"https://c.y.qq.com/qzone/fcg-bin/fcg_ucc_getcdinfo_byids_cp.fcg?type=1&json=1&utf8=1&onlysong=0&disstid={id}&format=json&g_tk=1157737156&loginUin=2728578956&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0", Encoding.UTF8);
            JObject o = JObject.Parse(s);
            var dt = new Billboard();
            dt.Title = o["cdlist"][0]["dissname"].ToString();
            dt.AlbumArtPath = o["cdlist"][0]["logo"].ToString();
            dt.OnlineId = id;
            int i = 0;
            while (i != o["cdlist"][0]["songlist"].Count())
            {
                try
                {
                    MusicInfo m = new MusicInfo()
                    {
                        Title = o["cdlist"][0]["songlist"][i]["songname"].ToString().Replace("\\", "-").Replace("?", "").Replace("/", "").Replace(":", "").Replace("*", "").Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", ""),
                        Artist = o["cdlist"][0]["songlist"][i]["singer"][0]["name"].ToString(),
                        GC = o["cdlist"][0]["songlist"][i]["songid"].ToString(),
                        OnlineId = o["cdlist"][0]["songlist"][i]["songmid"].ToString(),
                        AlbumArtPath = $"http://y.gtimg.cn/music/photo_new/T002R300x300M000{o["cdlist"][0]["songlist"][i]["albummid"]}.jpg"

                    };
                    m.Url = await GetUrlAsync(m.OnlineId);
                    dt.Musics.Add(m);
                    if (!mldata.ContainsKey(m.OnlineId))
                        mldata.Add(m.OnlineId, (m.Title + " - " + m.Artist).Replace("\\", "-").Replace("?", "").Replace("/", "").Replace(":", "").Replace("*", "").Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", ""));
                    i++;
                }
                catch { i++; }
            }
            return dt;
        }
        public async Task<string> GetUrlAsync(string mid)
        {
            string guid = "20D919A4D7700FBC424740E8CED80C5F";
            string ioo = await HttpHelper.GetWebAsync($"http://59.37.96.220/base/fcgi-bin/fcg_musicexpress2.fcg?version=12&miniversion=92&key=19914AA57A96A9135541562F16DAD6B885AC8B8B5420AC567A0561D04540172E&guid={guid}");
            string vkey = TextHelper.XtoYGetTo(ioo, "key=\"", "\" speedrpttype", 0);
            return $"http://182.247.250.19/streamoc.music.tc.qq.com/M500{mid}.mp3?vkey={vkey}&guid={guid}";
        }

        public async Task<List<OnlineMusicItem>> GetTopIndexAsync()
        {
            var dt = await HttpHelper.GetWebAsync("https://c.y.qq.com/v8/fcg-bin/fcg_v8_toplist_opt.fcg?page=index&format=html&tpl=macv4&v8debug=1");
            var sh = "{\"data\":" + dt.Replace("jsonCallback(", "").Replace("}]\n)", "") + "}]" + "}";
            var o = JObject.Parse(sh);
            var data = new List<OnlineMusicItem>();
            int i = 0;
            while (i < o["data"][0]["List"].Count())
            {
                data.Add(new OnlineMusicItem
                {
                    Name = o["data"][0]["List"][i]["ListName"].ToString(),
                    Photo = o["data"][0]["List"][i]["pic_v12"].ToString(),
                    ID = o["data"][0]["List"][i]["topID"].ToString()
                });
                i++;
            }
            i = 0;
            while (i < o["data"][1]["List"].Count())
            {
                data.Add(new OnlineMusicItem
                {
                    Name = o["data"][1]["List"][i]["ListName"].ToString(),
                    Photo = o["data"][1]["List"][i]["pic_v12"].ToString(),
                    ID = o["data"][1]["List"][i]["topID"].ToString()
                });
                i++;
            }
            return data;
        }

        public async Task<List<MusicInfo>> GetToplistAsync(int TopID)
        {
            JObject o = JObject.Parse(await HttpHelper.GetWebAsync($"https://c.y.qq.com/v8/fcg-bin/fcg_v8_toplist_cp.fcg?tpl=3&page=detail&topid={TopID}&type=top&song_begin=0&song_num=30&g_tk=1206122277&loginUin=2728578956&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0"));
            List<MusicInfo> dt = new List<MusicInfo>();
            int i = 0;
            while (i < o["songlist"].Count())
            {
                MusicInfo m = new MusicInfo();
                m.Title = o["songlist"][i]["data"]["songname"].ToString().Replace("\\", "-").Replace("?", "").Replace("/", "").Replace(":", "").Replace("*", "").Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", "");
                string Artist = "";
                for (int osxc = 0; osxc != o["songlist"][i]["data"]["singer"].Count(); osxc++)
                { Artist += o["songlist"][i]["data"]["singer"][osxc]["name"] + "&"; }
                m.Artist = Artist.Substring(0, Artist.LastIndexOf("&"));
                m.OnlineId = o["songlist"][i]["data"]["songmid"].ToString();
                m.AlbumArtPath = $"http://y.gtimg.cn/music/photo_new/T002R300x300M000{o["songlist"][i]["data"]["albummid"]}.jpg";
                m.Url =await GetUrlAsync(m.OnlineId);
                m.GC = o["songlist"][i]["data"]["songmid"].ToString();
                dt.Add(m);
                if (!mldata.ContainsKey(m.OnlineId))
                    mldata.Add(m.OnlineId, (m.Title + " - " + m.Artist).Replace("\\", "-").Replace("?", "").Replace("/", "").Replace(":", "").Replace("*", "").Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", ""));
                i++;
            }
            return dt;
        }
        public async Task<List<OnlineMusicItem>> GetSingerAsync(string key, int page = 1)
        {
            var o = JObject.Parse(await HttpHelper.GetWebAsync($"https://c.y.qq.com/v8/fcg-bin/v8.fcg?channel=singer&page=list&key={key}&pagesize=100&pagenum={page}&g_tk=5381&loginUin=0&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0"));
            var data = new List<OnlineMusicItem>();
            int i = 0;
            while (i < o["data"]["list"].Count())
            {
                data.Add(new OnlineMusicItem
                {
                    Name = o["data"]["list"][i]["Fsinger_name"].ToString(),
                    Photo = $"https://y.gtimg.cn/music/photo_new/T001R150x150M000{o["data"]["list"][i]["Fsinger_mid"]}.jpg?max_age=2592000"
                });
                i++;
            }
            return data;
        }
        public async Task<MusicFLGDIndexItemsList> GetFLGDIndexAsync()
        {
            var o = JObject.Parse(await HttpHelper.GetWebAsync("https://c.y.qq.com/splcloud/fcgi-bin/fcg_get_diss_tag_conf.fcg?g_tk=1206122277&loginUin=2728578956&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0"));
            var data = new MusicFLGDIndexItemsList();
            data.Hot.Add(new OnlineMusicItem { ID = "10000000", Name = "全部" });
            int i = 0;
            while (i < o["data"]["categories"][1]["items"].Count())
            {
                data.Lauch.Add(new OnlineMusicItem
                {
                    ID = o["data"]["categories"][1]["items"][i]["categoryId"].ToString(),
                    Name = o["data"]["categories"][1]["items"][i]["categoryName"].ToString()
                });
                i++;
            }
            i = 0;
            while (i < o["data"]["categories"][2]["items"].Count())
            {
                data.LiuPai.Add(new OnlineMusicItem
                {
                    ID = o["data"]["categories"][2]["items"][i]["categoryId"].ToString(),
                    Name = o["data"]["categories"][2]["items"][i]["categoryName"].ToString()
                });
                i++;
            }
            i = 0;
            while (i < o["data"]["categories"][3]["items"].Count())
            {
                data.Theme.Add(new OnlineMusicItem
                {
                    ID = o["data"]["categories"][3]["items"][i]["categoryId"].ToString(),
                    Name = o["data"]["categories"][3]["items"][i]["categoryName"].ToString()
                });
                i++;
            }
            i = 0;
            while (i < o["data"]["categories"][4]["items"].Count())
            {
                data.Heart.Add(new OnlineMusicItem
                {
                    ID = o["data"]["categories"][4]["items"][i]["categoryId"].ToString(),
                    Name = o["data"]["categories"][4]["items"][i]["categoryName"].ToString()
                });
                i++;
            }
            i = 0;
            while (i < o["data"]["categories"][5]["items"].Count())
            {
                data.Changjing.Add(new OnlineMusicItem
                {
                    ID = o["data"]["categories"][5]["items"][i]["categoryId"].ToString(),
                    Name = o["data"]["categories"][5]["items"][i]["categoryName"].ToString()
                });
                i++;
            }
            return data;
        }
        public async Task<List<OnlineMusicItem>> GetFLGDAsync(int id)
        {
            var o = JObject.Parse(await HttpHelper.GetWebDatadAsync($"https://c.y.qq.com/splcloud/fcgi-bin/fcg_get_diss_by_tag.fcg?picmid=1&rnd=0.38615680484561965&g_tk=5381&loginUin=0&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0&categoryId={id}&sortId=5&sin=0&ein=29", Encoding.UTF8));
            var data = new List<OnlineMusicItem>();
            int i = 0;
            while (i < o["data"]["list"].Count())
            {
                data.Add(new OnlineMusicItem
                {
                    Name = o["data"]["list"][i]["dissname"].ToString(),
                    Photo = o["data"]["list"][i]["imgurl"].ToString(),
                    ID = o["data"]["list"][i]["dissid"].ToString()
                });
                i++;
            }
            return data;
        }
        public async Task<MusicRadioList> GetRadioList()
        {
            var o = JObject.Parse(await HttpHelper.GetWebAsync("https://c.y.qq.com/v8/fcg-bin/fcg_v8_radiolist.fcg?channel=radio&format=json&page=index&tpl=wk&new=1&p=0.8663229811059507&g_tk=5381&loginUin=0&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0"));
            var data = new MusicRadioList();
            int i = 0;
            while (i < o["data"]["data"]["groupList"][0]["radioList"].Count())
            {
                data.Hot.Add(new OnlineMusicItem
                {
                    Name = o["data"]["data"]["groupList"][0]["radioList"][i]["radioName"].ToString(),
                    Photo = o["data"]["data"]["groupList"][0]["radioList"][i]["radioImg"].ToString(),
                    ID = o["data"]["data"]["groupList"][0]["radioList"][i]["radioId"].ToString()
                });
                i++;
            }
            i = 0;
            while (i < o["data"]["data"]["groupList"][1]["radioList"].Count())
            {
                data.Evening.Add(new OnlineMusicItem
                {
                    Name = o["data"]["data"]["groupList"][1]["radioList"][i]["radioName"].ToString(),
                    Photo = o["data"]["data"]["groupList"][1]["radioList"][i]["radioImg"].ToString(),
                    ID = o["data"]["data"]["groupList"][1]["radioList"][i]["radioId"].ToString()
                });
                i++;
            }
            i = 0;
            while (i < o["data"]["data"]["groupList"][2]["radioList"].Count())
            {
                data.Love.Add(new OnlineMusicItem
                {
                    Name = o["data"]["data"]["groupList"][2]["radioList"][i]["radioName"].ToString(),
                    Photo = o["data"]["data"]["groupList"][2]["radioList"][i]["radioImg"].ToString(),
                    ID = o["data"]["data"]["groupList"][2]["radioList"][i]["radioId"].ToString()
                });
                i++;
            }
            i = 0;
            while (i < o["data"]["data"]["groupList"][3]["radioList"].Count())
            {
                data.Theme.Add(new OnlineMusicItem
                {
                    Name = o["data"]["data"]["groupList"][3]["radioList"][i]["radioName"].ToString(),
                    Photo = o["data"]["data"]["groupList"][3]["radioList"][i]["radioImg"].ToString(),
                    ID = o["data"]["data"]["groupList"][3]["radioList"][i]["radioId"].ToString()
                });
                i++;
            }
            i = 0;
            while (i < o["data"]["data"]["groupList"][4]["radioList"].Count())
            {
                data.Changjing.Add(new OnlineMusicItem
                {
                    Name = o["data"]["data"]["groupList"][4]["radioList"][i]["radioName"].ToString(),
                    Photo = o["data"]["data"]["groupList"][4]["radioList"][i]["radioImg"].ToString(),
                    ID = o["data"]["data"]["groupList"][4]["radioList"][i]["radioId"].ToString()
                });
                i++;
            }
            i = 0;
            while (i < o["data"]["data"]["groupList"][5]["radioList"].Count())
            {
                data.Style.Add(new OnlineMusicItem
                {
                    Name = o["data"]["data"]["groupList"][5]["radioList"][i]["radioName"].ToString(),
                    Photo = o["data"]["data"]["groupList"][5]["radioList"][i]["radioImg"].ToString(),
                    ID = o["data"]["data"]["groupList"][5]["radioList"][i]["radioId"].ToString()
                });
                i++;
            }
            i = 0;
            while (i < o["data"]["data"]["groupList"][6]["radioList"].Count())
            {
                data.Lauch.Add(new OnlineMusicItem
                {
                    Name = o["data"]["data"]["groupList"][6]["radioList"][i]["radioName"].ToString(),
                    Photo = o["data"]["data"]["groupList"][6]["radioList"][i]["radioImg"].ToString(),
                    ID = o["data"]["data"]["groupList"][6]["radioList"][i]["radioId"].ToString()
                });
                i++;
            }
            i = 0;
            while (i < o["data"]["data"]["groupList"][7]["radioList"].Count())
            {
                data.People.Add(new OnlineMusicItem
                {
                    Name = o["data"]["data"]["groupList"][7]["radioList"][i]["radioName"].ToString(),
                    Photo = o["data"]["data"]["groupList"][7]["radioList"][i]["radioImg"].ToString(),
                    ID = o["data"]["data"]["groupList"][7]["radioList"][i]["radioId"].ToString()
                });
                i++;
            }
            i = 0;
            while (i < o["data"]["data"]["groupList"][8]["radioList"].Count())
            {
                data.MusicTools.Add(new OnlineMusicItem
                {
                    Name = o["data"]["data"]["groupList"][8]["radioList"][i]["radioName"].ToString(),
                    Photo = o["data"]["data"]["groupList"][8]["radioList"][i]["radioImg"].ToString(),
                    ID = o["data"]["data"]["groupList"][8]["radioList"][i]["radioId"].ToString()
                });
                i++;
            }
            i = 0;
            while (i < o["data"]["data"]["groupList"][9]["radioList"].Count())
            {
                data.Diqu.Add(new OnlineMusicItem
                {
                    Name = o["data"]["data"]["groupList"][9]["radioList"][i]["radioName"].ToString(),
                    Photo = o["data"]["data"]["groupList"][9]["radioList"][i]["radioImg"].ToString(),
                    ID = o["data"]["data"]["groupList"][9]["radioList"][i]["radioId"].ToString()
                });
                i++;
            }
            return data;
        }
        public async Task<MusicInfo> GetRadioMusicAsync(string id)
        {
            if (id == "99")
            {
                var o = JObject.Parse(await HttpHelper.GetWebAsync($"https://c.y.qq.com/rcmusic2/fcgi-bin/fcg_guess_youlike_pc.fcg?g_tk=1206122277&loginUin=2728578956&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0&cid=703&uin=2728578956"));
                string Artist = "";
                for (int osxc = 0; osxc != o["songlist"][0]["singer"].Count(); osxc++)
                { Artist += o["songlist"][0]["singer"][osxc]["name"] + "&"; }
                var m = new MusicInfo
                {
                    Title = o["songlist"][0]["name"].ToString().Replace("\\", "-").Replace("?", "").Replace("/", "").Replace(":", "").Replace("*", "").Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", ""),
                    Artist = Artist.Substring(0, Artist.LastIndexOf("&")),
                    GC = o["songlist"][0]["mid"].ToString(),
                    OnlineId = o["songlist"][0]["mid"].ToString(),
                    AlbumArtPath = $"http://y.gtimg.cn/music/photo_new/T002R300x300M000{o["songlist"][0]["album"]["mid"]}.jpg"
                };
                m.Url = await GetUrlAsync(m.OnlineId);

                return m;
            }
            else
            {
                var o = JObject.Parse(await HttpHelper.GetWebAsync($"https://u.y.qq.com/cgi-bin/musicu.fcg?g_tk=1206122277&loginUin=2728578956&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0&data=%7B\"songlist\"%3A%7B\"module\"%3A\"pf.radiosvr\"%2C\"method\"%3A\"GetRadiosonglist\"%2C\"param\"%3A%7B\"id\"%3A{id}%2C\"firstplay\"%3A1%2C\"num\"%3A10%7D%7D%2C\"radiolist\"%3A%7B\"module\"%3A\"pf.radiosvr\"%2C\"method\"%3A\"GetRadiolist\"%2C\"param\"%3A%7B\"ct\"%3A\"24\"%7D%7D%2C\"comm\"%3A%7B\"ct\"%3A\"24\"%7D%7D"));
                string Artist = "";
                for (int osxc = 0; osxc != o["songlist"]["data"]["track_list"][0]["singer"].Count(); osxc++)
                { Artist += o["songlist"]["data"]["track_list"][0]["singer"][osxc]["name"] + "&"; }
                var m = new MusicInfo
                {
                    Title = o["songlist"]["data"]["track_list"][0]["name"].ToString().Replace("\\", "-").Replace("?", "").Replace("/", "").Replace(":", "").Replace("*", "").Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", ""),
                    Artist = Artist.Substring(0, Artist.LastIndexOf("&")),
                    GC = o["songlist"]["data"]["track_list"][0]["mid"].ToString(),
                    OnlineId = o["songlist"]["data"]["track_list"][0]["mid"].ToString(),
                    AlbumArtPath = $"http://y.gtimg.cn/music/photo_new/T002R300x300M000{o["songlist"]["data"]["track_list"][0]["album"]["mid"]}.jpg"
                };
                m.Url = await GetUrlAsync(m.OnlineId);

                return m;
            }
        }
        public async Task<Billboard> GetGDbyWYAsync(string id)
        {
            string data = HttpHelper.PostWeb("http://lab.mkblog.cn/music/api.php", "types=playlist&id=" + id);
            JObject o = JObject.Parse(data);
            var dt = new Billboard();
            dt.Title = o["playlist"]["name"].ToString();
            dt.OnlineId = o["playlist"]["id"].ToString();
            dt.AlbumArtPath = o["playlist"]["coverImgUrl"].ToString();
            for (int i = 0; i != o["playlist"]["tracks"].Count(); i++)
            {
                try
                {
                    var dtname = o["playlist"]["tracks"][i]["name"].ToString();
                    var dtsinger = "";
                    for (int dx = 0; dx != o["playlist"]["tracks"][i]["ar"].Count(); dx++)
                        dtsinger += o["playlist"]["tracks"][i]["ar"][dx]["name"] + "&";
                    dtsinger = dtsinger.Substring(0, dtsinger.LastIndexOf("&"));
                    var dtf = await SearchMusicAsync(dtname + "-" + dtsinger);
                    if (dtf.Count > 0)
                    {
                        dt.Musics.Add(dtf[0]);
                        
                    }
                   
                }
                catch { }
            }
            return dt;
        }
        public async Task<List<MusicPL>> GetPLAsync(string name, int page = 1)
        {
            string Page = ((page - 1) * 20).ToString();
            string id = GetWYIdByName(name);
            var data = await HttpHelper.GetWebAsync($"https://music.163.com/api/v1/resource/comments/R_SO_4_{id}?offset={Page}");
            JObject o = JObject.Parse(data);
            var d = new List<MusicPL>();
            for (int i = 0; i != o["hotComments"].Count(); i++)
            {
                d.Add(new MusicPL()
                {
                    text = o["hotComments"][i]["content"].ToString(),
                    name = o["hotComments"][i]["user"]["nickname"].ToString(),
                    img = o["hotComments"][i]["user"]["avatarUrl"].ToString(),
                    like = o["hotComments"][i]["likedCount"].ToString()
                });
            }
            return d;
        }
        public async Task<List<MusicPL>> GetPLByQQAsync(string mid)
        {
            var id = JObject.Parse(await HttpHelper.GetWebAsync($"https://c.y.qq.com/v8/fcg-bin/fcg_play_single_song.fcg?songmid={mid}&tpl=yqq_song_detail&format=json&g_tk=268405378&loginUin=2728578956&hostUin=0&format=json&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0"))["data"][0]["id"].ToString();
            var ds = JObject.Parse(await HttpHelper.GetWebAsync($"https://c.y.qq.com/base/fcgi-bin/fcg_global_comment_h5.fcg?g_tk=268405378&hostUin=0&format=json&inCharset=utf8&outCharset=utf8&notice=0&platform=yqq&needNewCode=0&cid=205360772&reqtype=2&biztype=1&topid={id}&cmd=8&needmusiccrit=0&pagenum=0&pagesize=25&lasthotcommentid=&domain=qq.com&ct=24&cv=101010"));
            var data = new List<MusicPL>();
            for (int i = 0; i > ds["hot_comment"]["commentlist"].Count(); i++)
            {
                data.Add(new MusicPL()
                {
                    img = ds["hot_comment"]["commentlist"][i]["avatarurl"].ToString(),
                    like = ds["hot_comment"]["commentlist"][i]["praisenum"].ToString(),
                    name = ds["hot_comment"]["commentlist"][i]["nick"].ToString(),
                    text = ds["hot_comment"]["commentlist"][i]["rootcommentcontent"].ToString()
                });
            }
            return data;
        }
        public string GetWYIdByName(string name)
        {
            var ds = "{\"data\":" + HttpHelper.PostWeb("http://lab.mkblog.cn/music/api.php", "types=search&count=20&source=netease&pages=1&name=" + Uri.EscapeDataString(name)) + "}";
            var s = JObject.Parse(ds);
            return s["data"][0]["id"].ToString();
        }
        public async Task<string> GetLyricByWYAsync(string name)
        {
            string id = GetWYIdByName(name);
            string s = await HttpHelper.GetWebAsync($"http://music.163.com/api/song/lyric?os=pc&id={id}&lv=-1&kv=-1&tv=-1");
            JObject o = JObject.Parse(s);
            string t = o["lrc"]["lyric"].ToString();
            return t;
        }
    }
}