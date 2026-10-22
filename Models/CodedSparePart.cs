namespace CMMS_QRCode_Scanner.Models
{
    public class CodedSparePart : SparePart
    {
        public string SerialCode { get; set; } = string.Empty;

        public CodedSparePart()
        {
            EntityType = "Coded Spare Part";
        }
    }
}
