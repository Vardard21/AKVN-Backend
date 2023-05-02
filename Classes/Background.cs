namespace AKVN_Backend.Classes
{
    public class Background
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Sprite { get; set; }

        public Background(string name, string sprite)
        {
            Name = name;
            Sprite = sprite;
        }
        public Background() 
        {
            Name = string.Empty;
            Sprite = string.Empty;
        }

    }
}
