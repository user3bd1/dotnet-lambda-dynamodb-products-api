namespace productsApi.Models
{
    public class Meta
    {
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Barcode { get; set; } = string.Empty;
        public string QrCode { get; set; } = string.Empty;
    }
}
