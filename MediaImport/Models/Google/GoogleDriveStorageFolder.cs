using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Windows.Data.Json;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Net;
using System.IO;

namespace MediaImport.Models.Google
{
    public class GoogleDriveStorageFolder
    {
        public static GoogleDriveStorageFolder RootFolder;

        public string Id { get; private set; }
        public string Title { get; private set; }
      
        public string CreatedDate { get; private set; }
        public string ModifiedDate { get; private set; }
        public string Parent { get; private set; }

        public GoogleDriveStorageFolder(string id,string title,
            string createdDate , string modifiedDate , string parentId)
        {
            Id = id;
            Title = title;
            
            CreatedDate = createdDate;
            ModifiedDate = modifiedDate;
            Parent = parentId;
        }

        public async Task<List<GoogleDriveStorageFolder>> GetFoldersAsync()
        {
            List<GoogleDriveStorageFolder> folders = new List<GoogleDriveStorageFolder>();
            var client = GoogleDriveService.Instance.GoogleDriveClient;
            HttpResponseMessage testChildren = await client.GetAsync(string.Format("https://www.googleapis.com/drive/v2/files/{0}/children?q=mimeType%3D%27application%2Fvnd.google-apps.folder%27and%20trashed%3Dfalse", Id));
            string childrenResponseContent = await testChildren.Content.ReadAsStringAsync();

            JsonObject FolderChildren = JsonObject.Parse(childrenResponseContent);

            JsonArray items = FolderChildren.GetNamedArray("items");//GoogleDriveFolder.DeleteFolder(client, a[4]);
            foreach (var i in items)
            {
                var id = i.GetObject().GetNamedString("id");
                var FolderContent = await client.GetAsync(string.Format("https://www.googleapis.com/drive/v2/files/{0}", id));
                var stringFolderContent = await FolderContent.Content.ReadAsStringAsync();
                JsonObject FolderInfo = JsonObject.Parse(stringFolderContent);
                var date = FolderInfo.GetNamedValue("createdDate").GetString();
               
                var moddate = FolderInfo.GetNamedValue("modifiedDate").GetString();

                folders.Add(new GoogleDriveStorageFolder(FolderInfo.GetNamedString("id"), FolderInfo.GetNamedString("title"),
                    date,moddate ,Id));
            }
            return folders;
        }

        public async Task CreateNewFolder(string title)
        {
           string newFolderId = await GoogleDriveService.Instance.GenerateId();
           var Message = new HttpRequestMessage(new HttpMethod("POST"),
               string.Format("https://www.googleapis.com/drive/v2/files"));
           string contentString = "{\"mimeType\":\"application/vnd.google-apps.folder\",\"parents\":[{\"id\":\"" + Id + "\"}],\"title\":\"" + title + "\"}";
           Message.Content = new StringContent(contentString, Encoding.UTF8, "application/json");
            var response = await GoogleDriveService.Instance.GoogleDriveClient.SendAsync(Message);

        }

        public async void Rename(string newName)
        {
            var mess = new HttpRequestMessage(new HttpMethod("PATCH"),
             string.Format("https://www.googleapis.com/drive/v2/files/" + Id));
            Title = newName;
            string str = "{\"title\":\"" + newName + "\"}";
            mess.Content = new StringContent(str, Encoding.UTF8, "application/json");
            await GoogleDriveService.Instance.GoogleDriveClient.SendAsync(mess);
        }

        public async void Delete()
        {
            var mess = new HttpRequestMessage(new HttpMethod("POST"),
               string.Format("https://www.googleapis.com/drive/v2/files/{0}/trash", Id));
            await GoogleDriveService.Instance.GoogleDriveClient.SendAsync(mess);
        }

        public async Task UploadFile(StorageFile file)
        {
            string footer = "\r\n";

            List<string> _postData = new List<string>
            {
                "{",
                "\"parents\":[{\"id\":\"" + Id + "\"}],",
                "\"title\": \"" + file.Name + "\"",
                "}"
            };
            string postData = string.Join(" ", _postData.ToArray());
            byte[] MetaDataByteArray = Encoding.UTF8.GetBytes(postData);
            byte[] FileByteArray = System.IO.File.ReadAllBytes(file.Path);
            string url = "https://www.googleapis.com/upload/drive/v2/files?uploadType=resumable";
            int headerLenght = MetaDataByteArray.Length + footer.Length;

            WebRequest request = WebRequest.Create(url);
            request.Method = "POST";
            request.Headers["Authorization"] = string.Format("Bearer {0}", GoogleDriveService.Instance.GoogleDriveClient.DefaultRequestHeaders.Authorization.Parameter);
            request.Headers["Content-Length"] = headerLenght.ToString();
            request.ContentType = "application/json; charset=UTF-8";
            request.Headers["X-Upload-Content-Type"] = file.ContentType;
            request.Headers["X-Upload-Content-Length"] = FileByteArray.Length.ToString();
            request.Headers["Accept-Encoding"] = "gzip, deflate";

            Stream dataStream = await request.GetRequestStreamAsync();
            dataStream.Write(MetaDataByteArray, 0, MetaDataByteArray.Length); // write the MetaData 
            dataStream.Write(Encoding.UTF8.GetBytes(footer), 0, Encoding.UTF8.GetByteCount(footer));  // done writeing add return just 
            try
            {
                HttpWebResponse response = (HttpWebResponse)(await request.GetResponseAsync());
                dataStream = response.GetResponseStream();
                var UrlUpload = response.Headers["Location"];

                WebRequest request2 = WebRequest.Create(UrlUpload);
                request2.Method = "PUT";
                request2.Headers["Content-Length"] = FileByteArray.Count().ToString();
                request2.Headers["Accept-Encoding"] = "gzip, deflate";
                Stream data2 = await request2.GetRequestStreamAsync();
                data2.Write(FileByteArray, 0, FileByteArray.Length);
                await request2.GetResponseAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static async Task<GoogleDriveStorageFolder> GetFolderAsync(string id)
        {
            var client = GoogleDriveService.Instance.GoogleDriveClient;
            var FolderContent = await client.GetAsync(string.Format("https://www.googleapis.com/drive/v2/files/{0}", id));
            var stringFolderContent = await FolderContent.Content.ReadAsStringAsync();
            JsonObject FolderInfo = JsonObject.Parse(stringFolderContent);
            var date = FolderInfo.GetNamedValue("createdDate").GetString();
            var parentArray = FolderInfo.GetNamedArray("parents");
            string parentid = null;
            foreach (var p in parentArray)
            {
                var objPar = p.GetObject();             
                parentid = objPar.GetNamedString("id");
            }

            return new GoogleDriveStorageFolder(FolderInfo.GetNamedString("id"), FolderInfo.GetNamedString("title"),
             FolderInfo.GetNamedValue("createdDate").GetString(), FolderInfo.GetNamedValue("modifiedDate").GetString(),parentid);

           // return null;
        }
        
        private static async Task<byte[]> GetBytesAsync(StorageFile file)
        {
            byte[] fileBytes = null;
            if (file == null) return null;
            using (var stream = await file.OpenReadAsync())
            {
                fileBytes = new byte[stream.Size];
                using (var reader = new DataReader(stream))
                {
                    await reader.LoadAsync((uint)stream.Size);
                    reader.ReadBytes(fileBytes);
                }
            }
            return fileBytes;
        }

    }
}
