namespace AKVN_Backend.Classes
{
    public class Chapter
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Backgrounds { get; set; }
        public string Actors { get; set; }
        public string SceneList { get; set; }

        public Chapter(string name, string backgrounds, string actors, string scenes)
        {
            Name = name;
            Backgrounds = backgrounds;
            Actors = actors;
            SceneList = scenes;
        }

        public Chapter(string name) 
        {
            Name = name;
            Backgrounds = string.Empty;
            Actors = string.Empty;
            SceneList = string.Empty;
        }

        public Chapter() 
        {
            Name = string.Empty;
            Backgrounds = string.Empty;
            Actors = string.Empty;
            SceneList = string.Empty;
        }
    }
}
