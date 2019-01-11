using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Security.Authentication.Web;
using Windows.Storage;

namespace MediaImport.Models.Google
{
    public class GooglePhotoService
    {
        public static GooglePhotoService Instance { get; private set; }

        readonly string clientId;
        readonly string clientSecret;

        public static bool IsAuthorized = false;

        public HttpClient GoogleClient { get; private set; }

        static Uri startAuthUri;
        static Uri endAuthUri;
        
        private GooglePhotoService(string clientId , string clientSecret)
        {
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            if (!IsAuthorized)
            {
                HttpClientHandler handler = new HttpClientHandler
                {
                    AllowAutoRedirect = true
                };
                GoogleClient = new System.Net.Http.HttpClient(handler);

                var googleUrl = new System.Text.StringBuilder();
                googleUrl.Append("https://accounts.google.com/o/oauth2/v2/auth?client_id=");
                googleUrl.Append(Uri.EscapeDataString(clientId));
                googleUrl.Append("&scope=");
                googleUrl.Append(Uri.EscapeDataString("https://www.googleapis.com/auth/photoslibrary"));//accessor
                googleUrl.Append("&redirect_uri=");
                googleUrl.Append(Uri.EscapeDataString("urn:ietf:wg:oauth:2.0:oob:auto"));
                googleUrl.Append("&response_type=code");
                string endURL = "https://accounts.google.com/o/oauth2/approval";
                startAuthUri = new Uri(googleUrl.ToString());
                endAuthUri = new Uri(endURL);
            }
        }

        public static async Task<bool> InitializeInstanceAndLogIn(string clientId , string clientSecret)
        {
            Instance = new GooglePhotoService(clientId , clientSecret);
            try
            {
                await Instance.LogIn(startAuthUri, endAuthUri);
             //   await Instance.GetAlbums();
                IsAuthorized = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);//better use debug
                IsAuthorized = false;
            }
            return IsAuthorized;
        }

        private async Task LogIn(Uri startUri, Uri endUri)
        {
            WebAuthenticationResult webAuthenticationResult = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, startUri, endUri);
            if (webAuthenticationResult.ResponseStatus == WebAuthenticationStatus.Success)
            {
                string AuthorizationCode = GetAuthorizationCode(webAuthenticationResult.ResponseData);
                GoogleClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await GetAccessToken(AuthorizationCode));
            }
        }

        private string GetAuthorizationCode(string ResponseData)
        {
            var indexsubstr = ResponseData.IndexOf("response");
            string afterCode = ResponseData.Substring(indexsubstr);
            Dictionary<string, string> queryStringParams =
                afterCode.Substring(1).Split('&')
                     .ToDictionary(c => c.Split('=')[0],
                                   c => Uri.UnescapeDataString(c.Split('=')[1]));
            
            var authorizationCode = queryStringParams["approvalCode"];
            return authorizationCode;
        }

        private async Task<string> GetAccessToken(string authorizationCode)
        {
            string tokenRequestBody = string.Format("code={0}&redirect_uri={1}&client_id={2}&client_secret={3}&scope=&grant_type=authorization_code",
                                              authorizationCode,
                                              System.Uri.EscapeDataString("urn:ietf:wg:oauth:2.0:oob:auto"),
                                              clientId,
                                              clientSecret);
            StringContent content = new StringContent(tokenRequestBody, Encoding.UTF8, "application/x-www-form-urlencoded");

            System.Net.Http.HttpResponseMessage response = await GoogleClient.PostAsync("https://www.googleapis.com/oauth2/v4/token", content);
            string responseString = await response.Content.ReadAsStringAsync();
            JsonObject tokens = JsonObject.Parse(responseString);
            return tokens.GetNamedString("access_token");
        }

        public void LogOut()
        {
            IsAuthorized = false;
        }

        //Get albums
        //private async Task<List<GooglePhotoAlbum>> GetAlbums()
        //{
        //    List<GooglePhotoAlbum> albums = new List<GooglePhotoAlbum>();
        //    var mResponseAlbums = await GoogleClient.GetAsync("https://photoslibrary.googleapis.com/v1/albums");
        //    string responseString = await mResponseAlbums.Content.ReadAsStringAsync();
        //    JsonArray jsonAlbums = JsonObject.Parse(responseString).GetNamedArray("albums");
        //    foreach(var jAlb in jsonAlbums)
        //    {
        //        JsonObject alb = jAlb.GetObject();
        //        albums.Add(new GooglePhotoAlbum(alb.GetNamedString("id"), alb.GetNamedString("title")));
        //    }
        //    Albums = albums;
        //    return albums;
        //}

        private async Task<string> GetResumableUploadTokenAsync(StorageFile file)
        {
            byte[] FileByteArray = System.IO.File.ReadAllBytes(file.Path);
            var FirstMessage = new HttpRequestMessage(new HttpMethod("POST"), "https://photoslibrary.googleapis.com/v1/uploads");

            FirstMessage.Headers.Add("X-Goog-Upload-Command", "start");
            FirstMessage.Headers.Add("X-Goog-Upload-Content-Type", file.ContentType);
            FirstMessage.Headers.Add("X-Goog-Upload-File-Name", file.Name);
            FirstMessage.Headers.Add("X-Goog-Upload-Protocol", "resumable");
            FirstMessage.Headers.Add("X-Goog-Upload-Raw-Size", FileByteArray.Length.ToString());

            var sessionUrl = await GoogleClient.SendAsync(FirstMessage);
            string responseString = await sessionUrl.Content.ReadAsStringAsync();

            var uploadRequest = sessionUrl.Headers.GetValues("X-Goog-Upload-URL").First();

            var SecondMessage = new HttpRequestMessage(new HttpMethod("POST"), uploadRequest);
            SecondMessage.Headers.Add("X-Goog-Upload-Command", "upload, finalize");
            SecondMessage.Headers.Add("X-Goog-Upload-Offset", "0");
            System.IO.Stream s = new System.IO.MemoryStream(FileByteArray);
            SecondMessage.Content = new StreamContent(s);

            var answer = await GoogleClient.SendAsync(SecondMessage);
            return await answer.Content.ReadAsStringAsync();
        }

        private async Task<string> GetMediaUploadTokenAsync(StorageFile file)
        {
            byte[] FileByteArray = System.IO.File.ReadAllBytes(file.Path);

            var FirstMessage = new HttpRequestMessage(new HttpMethod("POST"), "https://photoslibrary.googleapis.com/v1/uploads");

            FirstMessage.Headers.Add("X-Goog-Upload-File-Name", file.Name);
            FirstMessage.Headers.Add("X-Goog-Upload-Protocol", "raw");
            System.IO.Stream s = new System.IO.MemoryStream(FileByteArray);
            FirstMessage.Content = new StreamContent(s);
            var sessionUrl = await GoogleClient.SendAsync(FirstMessage);
            string responseString = await sessionUrl.Content.ReadAsStringAsync();
            return responseString;
        }

        //upload file to library
        public async Task UploadFileToLibrary(StorageFile file)
        {
            string uploadToken;
            //if (file.ContentType.Contains("video"))
                uploadToken = await GetMediaUploadTokenAsync(file);
            //else uploadToken = await GetResumableUploadTokenAsync(file);

            var ThirdMessage = new HttpRequestMessage(new HttpMethod("POST"), "https://photoslibrary.googleapis.com/v1/mediaItems:batchCreate");
            string uploadingFile = "{\"newMediaItems\":[{" + "\"description\":\"ITEM_DESCRIPTION\",\"simpleMediaItem\":{" + "\"uploadToken\":\"" + uploadToken + "\"" + "}" + "}]}";
            ThirdMessage.Content = new StringContent(uploadingFile);
            var answer = await GoogleClient.SendAsync(ThirdMessage);
           
            ///
            ////
            ///
            ////

        }

        public async Task<string> CreateNewAlbum()
        {
            string AlbumName = await Models.TextDialogBox.InputTextDialogAsync("Album Name");
            var AlbumMessage = new HttpRequestMessage(new HttpMethod("POST"), "https://photoslibrary.googleapis.com/v1/albums")
            {
                Content = new StringContent("{\"album\":{\"title\":\"" + AlbumName + "\"}}")
            };
            var responseAlbum = await GoogleClient.SendAsync(AlbumMessage);
            var stringAlbumId = await responseAlbum.Content.ReadAsStringAsync();
            var jsonAlbumId = JsonObject.Parse(stringAlbumId);
            return jsonAlbumId.GetNamedString("id");
        }

        public async Task UploadFileToAlbum(StorageFile file, string AlbumId)
        {
            string uploadToken;
            if (file.ContentType.Contains("video"))
                uploadToken = await GetMediaUploadTokenAsync(file);
            else uploadToken = await GetResumableUploadTokenAsync(file);

            var ThirdMessage = new HttpRequestMessage(new HttpMethod("POST"), "https://photoslibrary.googleapis.com/v1/mediaItems:batchCreate");
            string uploadingFile = "{\"albumId\":\"" + AlbumId + "\",\"newMediaItems\":[{" + "\"description\":\"\",\"simpleMediaItem\":{" + "\"uploadToken\":\"" + uploadToken + "\"" + "}" + "}]}";
            ThirdMessage.Content = new StringContent(uploadingFile);
            var really = await GoogleClient.SendAsync(ThirdMessage);
            var respooo = await really.Content.ReadAsStringAsync();
        }
    }
}
