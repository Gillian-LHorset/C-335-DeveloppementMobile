namespace P_AppMobile.Resources.Models {
    public class EpubFile {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string UploadedAt { get; set; } = string.Empty;
    }
}