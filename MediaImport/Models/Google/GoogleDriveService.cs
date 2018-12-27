using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Security.Authentication.Web;

namespace MediaImport.Models.Google
{
    public class GoogleDriveService
    {
        readonly string clientId;
        public HttpClient GoogleDriveClient { get; private set; }
        public GoogleDriveStorageFolder RootFolder { get;private set; }
        public static bool IsAuthorized = false;
        static Uri startAuthUri;
        static Uri endAuthUri;
        public static GoogleDriveService Instance { get; private set; }
        public static async Task<bool> InitializeInstanceAndLogIn(string clientId)
        {
            
            Instance = new GoogleDriveService(clientId);
            try
            {
                await Instance.LogIn(startAuthUri, endAuthUri);
                await Instance.SetRootFolder();
                IsAuthorized = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);//better use debug
                IsAuthorized = false;
            }
            return IsAuthorized;
        }


        private GoogleDriveService(string clientId)
        {
            this.clientId = clientId;
            if (!IsAuthorized)
            {
                HttpClientHandler handler = new HttpClientHandler
                {
                    AllowAutoRedirect = true
                };
                GoogleDriveClient = new System.Net.Http.HttpClient(handler);

                var googleUrl = new System.Text.StringBuilder();
                googleUrl.Append("https://accounts.google.com/o/oauth2/v2/auth?client_id=");
                googleUrl.Append(Uri.EscapeDataString(clientId));
                googleUrl.Append("&scope=");
                googleUrl.Append(Uri.EscapeDataString("https://www.googleapis.com/auth/drive"));//accessor
                googleUrl.Append("&redirect_uri=");
                googleUrl.Append(Uri.EscapeDataString("urn:ietf:wg:oauth:2.0:oob:auto"));
                googleUrl.Append("&response_type=code");
                string endURL = "https://accounts.google.com/o/oauth2/approval";
                startAuthUri = new Uri(googleUrl.ToString());
                endAuthUri = new Uri(endURL);
               
            }
        }
   
        private async Task LogIn(Uri startUri, Uri endUri)
        {
                WebAuthenticationResult webAuthenticationResult = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, startUri, endUri);
                if(webAuthenticationResult.ResponseStatus == WebAuthenticationStatus.Success)
                {
                    string AuthorizationCode = GetAuthorizationCode(webAuthenticationResult.ResponseData);
                    GoogleDriveClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await GetAccessToken(AuthorizationCode));
                }
        }

        private async Task<string> GetAccessToken(string authorizationCode)
        {
            string tokenRequestBody = string.Format("code={0}&redirect_uri={1}&client_id={2}&scope=&grant_type=authorization_code&clientSecret={3}",
                                              authorizationCode,
                                              System.Uri.EscapeDataString("urn:ietf:wg:oauth:2.0:oob:auto"),
                                              clientId,""
                                              );
            StringContent content = new StringContent(tokenRequestBody, Encoding.UTF8, "application/x-www-form-urlencoded");
           
            System.Net.Http.HttpResponseMessage response = await GoogleDriveClient.PostAsync("https://www.googleapis.com/oauth2/v4/token", content);
            string responseString = await response.Content.ReadAsStringAsync();
            JsonObject tokens = JsonObject.Parse(responseString);
            return tokens.GetNamedString("access_token");
        }

        private string GetAuthorizationCode(string ResponseData)
        {
            var indexsubstr = ResponseData.IndexOf("response");
            string afterCode = ResponseData.Substring(indexsubstr);
            Dictionary<string, string> queryStringParams =
                afterCode.Substring(1).Split('&')
                     .ToDictionary(c => c.Split('=')[0],
                                   c => Uri.UnescapeDataString(c.Split('=')[1]));

            //string code_verifier = randomDataBase64url(32);
            var authorizationCode = queryStringParams["approvalCode"];
            return authorizationCode;
        }

        private async Task SetRootFolder()
        {
            string adress = await GetUserAdress(GoogleDriveClient);
            System.Net.Http.HttpResponseMessage userFolders = await GoogleDriveClient.GetAsync(string.Format("https://www.googleapis.com/drive/v2/files?corpus=default&includeTeamDriveItems=false&orderBy=folder&supportsTeamDrives=false&q=mimeType%3D%27application%2Fvnd.google-apps.folder%27and%20trashed%3Dfalseand%20%27{0}%27%20in%20owners", adress));
            string userfolderResponseContent = await userFolders.Content.ReadAsStringAsync();
            JsonObject googledrivecontent = JsonObject.Parse(userfolderResponseContent);//gets folders in json format
            JsonArray arrayOfFolders = googledrivecontent.GetNamedArray("items");
            bool found = false;
            foreach (var f in arrayOfFolders)
            {
                if (found)
                    break;
                JsonObject folder = f.GetObject();
                var parentArray = folder.GetNamedArray("parents");
                
                foreach (var p in parentArray)
                {

                    var objPar = p.GetObject();
                    if (objPar.GetNamedValue("isRoot").GetBoolean())
                    {
                        string rootFolderId = objPar.GetNamedString("id");
                        RootFolder = new GoogleDriveStorageFolder(rootFolderId, "My Drive", null,  null, null);
                        found = true;
                        break;
                    }
                }
            }
        }

        async Task<string> GetUserAdress(System.Net.Http.HttpClient client)
        {
            System.Net.Http.HttpResponseMessage userinfoResponse = await client.GetAsync("https://www.googleapis.com/drive/v2/about");
            string userinfoResponseContent = await userinfoResponse.Content.ReadAsStringAsync();
            JsonObject content = JsonObject.Parse(userinfoResponseContent);

            JsonObject UserInfo = content.GetNamedObject("user");
            string email = UserInfo.GetNamedString("emailAddress");
            email = email.Replace("@", "%40");
            return email;
        }
        
        public void LogOut()
        {
            IsAuthorized = false;
        }

        public  async Task<string> GenerateId()
        {
            System.Net.Http.HttpResponseMessage idResponce = await GoogleDriveClient.GetAsync("https://www.googleapis.com/drive/v2/files/generateIds?maxResults=1");
            string responseString = await idResponce.Content.ReadAsStringAsync();
            JsonObject jsonResponce = JsonObject.Parse(responseString);
            JsonArray ids = jsonResponce.GetNamedArray("ids");
            return ids.GetStringAt(0);
        }
    }


}
