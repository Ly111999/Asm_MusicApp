using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MusicApp.Entity;
using Newtonsoft.Json;

namespace MusicApp.Services
{
    class APIHandle
    {
        public static string GET_UPLOAD_URL = "https://2-dot-backup-server-002.appspot.com/get-upload-token";
        public static string MEMBER_REGISTER = "https://2-dot-backup-server-002.appspot.com/_api/v2/members";
        public static string API_INFORMATION = "https://2-dot-backup-server-002.appspot.com/_api/v2/members/information";
        public static string API_LOGIN = "http://2-dot-backup-server-002.appspot.com/_api/v2/members/authentication";
        public static string GET_SONG = "https://2-dot-backup-server-002.appspot.com/_api/v2/songs/";
        public static string REGISTER_SONG = "https://2-dot-backup-server-002.appspot.com/_api/v2/songs";
        public static string TOKEN_STRING = " ";
        public static Member Loggedin_Member;

        public static async Task<bool> GetInformation()
        {
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", "Basic " + TOKEN_STRING);
            var resp = httpClient.GetAsync(API_INFORMATION).Result;
            var content = await resp.Content.ReadAsStringAsync();
            Loggedin_Member = JsonConvert.DeserializeObject<Member>(content);
            return Loggedin_Member != null;
        }
    }
}
