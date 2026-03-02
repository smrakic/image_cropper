namespace ImageCropper.API.Models
{
    public class CropRequestStruct
    {
        public byte[] ImageData { get; set; } = new byte[0]; //problem za null mora se inicijalizirat ili required
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}