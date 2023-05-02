using System.Collections.Generic;

namespace AKVN_Backend.Classes
{
    public class Text
    {
        public int Id { get; set; }
        public string Dialogue { get; set; }
        public Actor Owner { get; set; }

        public Text(string dialogue, Actor owner)
        {

            Dialogue = dialogue;
            Owner = owner;
        }
        public Text() 
        {
            Dialogue = string.Empty;
            Owner = new Actor();

        }
    }
}