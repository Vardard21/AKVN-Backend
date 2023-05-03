using System.Collections.Generic;

namespace AKVN_Backend.Classes
{
    public class Text
    {
        public int Id { get; set; }
        public string Dialogue { get; set; }

        public Text(string dialogue)
        {

            Dialogue = dialogue;

        }
        public Text() 
        {
            Dialogue = string.Empty;

        }
    }
}