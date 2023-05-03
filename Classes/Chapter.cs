namespace AKVN_Backend.Classes
{
    public class Chapter
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Scene> Scenes { get; set; }

        public Chapter(string name, List<Scene> scenes) 
        {
            Name = name;
            Scenes = scenes;
        }
        public Chapter(string name)
        {
            Name = name;
            Scenes= new List<Scene>();
        }

        public Chapter() 
        {
            Name = string.Empty;
            Scenes = new List<Scene>();
        }
    }
}
