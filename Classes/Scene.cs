using System.Collections.Generic;

namespace AKVN_Backend.Classes
{
    public class Scene 
    {
        public int Id { get; set; }

        public Actor Actor { get; set; }
        public string Dialogue { get; set; }

        public Scene( Actor actor, string text) 
        {

            Actor = actor;
            Dialogue = text;
        }
        public Scene()
        {

            Actor = new Actor();
            Dialogue = string.Empty;
        }

    }
}