namespace CMMS_QRCode_Scanner.Models
{
    public abstract class BaseItem
    {
        public int Id { get; set; }
        public string BarcodeId { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
    }
}
