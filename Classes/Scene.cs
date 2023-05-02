using System.Collections.Generic;

namespace AKVN_Backend.Classes
{
    public class Scene 
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Actor> Actors { get; set; }
        public List<Text> Texts { get; set; }

        public Scene(string name, List<Actor> actors, List<Text> text) 
        {
            Name = name;
            Actors = actors;
            Texts = text;
        }
        public Scene()
        {
            Name = string.Empty;
            Actors = new List<Actor>();
            Texts = new List<Text>();
        }

    }
}