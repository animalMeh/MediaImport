namespace MediaImport.Models.Google
{
    public class GooglePhotoAlbum
    {
        public GooglePhotoAlbum(string id , string title )
        {
            Id = id;
            Title = title;
        }

        public string Id { get; private set; }
        public string Title { get; private set; }
    }
}
