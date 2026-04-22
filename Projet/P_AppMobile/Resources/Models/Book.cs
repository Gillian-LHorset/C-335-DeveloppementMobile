namespace P_AppMobile.Resources.Models {
    public class Book {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string EpubFilePath { get; set; } = string.Empty;
        public int FileSizeBytes { get; set; }
        public string UploadedAt { get; set; } = string.Empty;
    }
}