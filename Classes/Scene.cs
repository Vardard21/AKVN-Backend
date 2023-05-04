using System.Collections.Generic;

namespace AKVN_Backend.Classes
{
    public class Scene 
    {
        public int Id { get; set; }

        public string ActorName { get; set; }
        public int ActorId { get; set; }
        public string Dialogue { get; set; }

        public Scene( string actorName, string text,int actorId) 
        {

            ActorName = actorName;
            Dialogue = text;
            ActorId = actorId;
        }
        public Scene()
        {
            ActorName = string.Empty;
            Dialogue = string.Empty;
            ActorId = 0;
        }

    }
}