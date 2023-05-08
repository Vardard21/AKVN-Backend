namespace AKVN_Backend.Classes
{
    public class JsonObject
    {
        public List<string> backgrounds { get; set; }
        public List<string> characters { get; set; }
        public List<string> images { get; set; }
        public List<string> items { get; set; }
        public List<string> ui { get; set; }

        public JsonObject()
        {
            backgrounds = new List<string>();
            characters = new List<string>();
            images = new List<string>();              
            items = new List<string>();
            ui = new List<string>();
        }
    }
}
