using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace Clone
{
    public static class Images
    {
        public readonly static ImageSource Empty = LoadImage("Empty.png");
        public readonly static ImageSource Body = LoadImage("Body.png");
        public readonly static ImageSource Head = LoadImage("Head.png");
        public readonly static ImageSource Food = LoadImage("cherry1.png");
        public readonly static ImageSource SpecialFood = LoadImage("banana.png"); // New image for special food
        public readonly static ImageSource RareFood = LoadImage("pineapple.png");     // New image for rare food
        public readonly static ImageSource DeadBody = LoadImage("DeadBody.png");
        public readonly static ImageSource DeadHead = LoadImage("DeadHead.png");


        // New monster images
        public readonly static ImageSource Alligator = LoadImage("alligator.png"); // Example image for alligator monster
        public readonly static ImageSource Dragonfly = LoadImage("vampire.png");     // Example image for vampire monster
        public readonly static ImageSource Knight = LoadImage("knight.png"); // Example image for knight monster 

        private static ImageSource LoadImage(String fileName)
        {
            return new BitmapImage(new Uri($"Assets/{fileName}", UriKind.Relative));
        }
    }
}



