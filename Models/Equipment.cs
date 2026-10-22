namespace CMMS_QRCode_Scanner.Models
{
    public class Equipment : BaseItem
    {
        public string EquipmentCode { get; set; } = string.Empty;
        public string EquipmentName { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Serial { get; set; } = string.Empty;
        public string AssetNo { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string NextMaintenance { get; set; } = string.Empty;

        public Equipment()
        {
            EntityType = "Equipment";
        }
    }
}
