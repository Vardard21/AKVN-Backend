namespace AKVN_Backend.Classes
{
    public class Actor
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Sprite { get; set; }
        public string ShortName { get; set; }

        public Actor(string name, string sprite, string shortName) 
        {
            Name = name;
            Sprite = sprite;
            ShortName = shortName;
        }
        public Actor(string name, string sprite)
        {
            Name = name;
            Sprite = sprite;
            ShortName = string.Empty;
        }

        public Actor() 
        {
            Name = string.Empty;
            Sprite = string.Empty;
            ShortName= string.Empty;
        }
    }
}