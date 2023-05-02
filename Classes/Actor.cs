namespace AKVN_Backend.Classes
{
    public class Actor
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Sprite { get; set; }

        public Actor(string name, string sprite) 
        {
            Name = name;
            Sprite = sprite;
        }
        public Actor() 
        {
            Name = string.Empty;
            Sprite = string.Empty;
        }
    }
}