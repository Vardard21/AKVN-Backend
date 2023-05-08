namespace AKVN_Backend.Classes
{
    public class CharacterSprite
    {
        public int Id { get; set; }
        public string ImagePath { get; set; }
        public string Name { get; set; }
        public CharacterSprite()
        {
            Id = 0;
            ImagePath = string.Empty;
            Name = string.Empty;
        }

        public CharacterSprite(string imagePath,string name)
        {
            ImagePath = imagePath;
            Name = name;
            Id = 0;
        }
    }
}
