namespace Jubi.VKontakte.Models
{
    public struct Crop
    {
        public int X { get; }
        
        public int Y { get; }

        public Crop(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}