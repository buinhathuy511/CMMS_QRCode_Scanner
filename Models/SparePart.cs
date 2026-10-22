namespace CMMS_QRCode_Scanner.Models
{
    public class SparePart : BaseItem
    {
        public string Type { get; set; } = string.Empty;
        public string PartCode { get; set; } = string.Empty;
        public string PartName { get; set; } = string.Empty;
        public string ImportDate { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int MinStock { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string Specification { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public string Supplier { get; set; } = string.Empty;
        public string Price { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public int AgeDays { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        
        public SparePart()
        {
            EntityType = "Non-Coded Spare Part";
        }
    }
}
