using AKVN_Backend.Classes;

namespace AKVN_Backend.Classes.DTO
{
    public class ChapterDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Scene> Scenes { get; set; }
        public List<Background> Backgrounds { get; set; }
        public List<Actor> Actors { get; set; }
        public string Episode { get; set; }

        public ChapterDTO(string name, List<Scene> scenes, List<Background> backgrounds, List<Actor> actors,string episode)
        {
            Name = name;
            Scenes = scenes;
            Backgrounds = backgrounds;
            Actors = actors;
            Episode = episode;
        }

        public ChapterDTO()
        {
            Name = string.Empty;
            Scenes = new List<Scene>();
            Backgrounds = new List<Background>();
            Actors = new List<Actor>();
            Episode= string.Empty;
        }
    }
}
