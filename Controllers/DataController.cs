using AKVN_Backend.Classes;
using AKVN_Backend.Data;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.CodeDom;
using System.Security.AccessControl;
using AKVN_Backend.Classes.DTO;
using System.Net;
using Microsoft.AspNetCore.Routing.Constraints;
using System.IO;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Runtime.ExceptionServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace AKVN_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataController : ControllerBase
    {
        private readonly AKVNDBContext _context;
        public DataController(AKVNDBContext context)
        {
            _context = context;

        }


        [HttpGet]
        public Response<ChapterDTO> GetChapterByID(string name)
        {

            //Generate Response.
            Response<ChapterDTO> response = new Response<ChapterDTO>();

            //Generate DTO to return.
            ChapterDTO chapterDTO = new ChapterDTO();
            try
            {
                //Lookup chapter according to chapter name.
                Chapter chapter = _context.Chapters.Where(o => o.Name == name).First();

                chapterDTO.Name = chapter.Name;

                //Look for each actor that appears in the chapter.
                List<Actor> Actors = new List<Actor>();
                if (chapter.Actors != "No Actors")
                {
                    string[] ActorsString = chapter.Actors.Split(",");
                    List<int> ActorIds = new List<int>();
                    foreach (string actor in ActorsString)
                    {
                        ActorIds.Add(Int32.Parse(actor));
                    }
                    foreach (int actorId in ActorIds)
                    {
                        Actor actor = _context.Actors.Where(o => o.Id == actorId).First();
                        Actors.Add(actor);
                    }
                }

                //Look for each Background that appears in the chapter
                List<Background> Backgrounds = new List<Background>();
                if (chapter.Backgrounds != "No Backgrounds")
                {
                    string[] BackgroundsString = chapter.Backgrounds.Split(",");
                    List<int> BackgroundIds = new List<int>();
                    int i = 1;
                    foreach (string background in BackgroundsString)
                    {
                        BackgroundIds.Add(Int32.Parse(background));
                    }
                    foreach (int backgroundId in BackgroundIds)
                    {
                        Background background = _context.Backgrounds.Where(o => o.Id == backgroundId).First();
                        if (background.Name.Contains("Background-Black"))
                        {
                            background.Name = "black";
                        }
                        else
                        {
                            background.Name = i.ToString();
                            i++;
                        }
                        Backgrounds.Add(background);
                    }
                }

                //Look for each Scene that appears in the chapter.
                List<Scene> Scenes = new List<Scene>();
                if (chapter.SceneList != "No Scenes")
                {
                    string[] ScenelistString = chapter.SceneList.Split(",");
                    List<int> SceneIds = new List<int>();

                    foreach (string scene in ScenelistString)
                    {
                        SceneIds.Add(Int32.Parse(scene));
                    }
                    foreach (int sceneId in SceneIds)
                    {
                        Scene scene = _context.Scenes.Where(o => o.Id == sceneId).First();
                        Scenes.Add(scene);
                    }
                }
                chapterDTO.Actors = Actors;
                chapterDTO.Backgrounds = Backgrounds;
                chapterDTO.Scenes = Scenes;
                response.Data = chapterDTO;
                response.Success = true;
            }
            catch
            {
                response.RequestError();
            }            
            return response;
        }

        [HttpGet("/api/Images/Characters")]
        public async Task<Response<ImageDTO>> DownloadCharacterImages()
        {
            Response<ImageDTO> response = new Response<ImageDTO>();
            List<Actor> Actors = _context.Actors.Where(a => a.Sprite != "").ToList();
            HttpClient client = new HttpClient();
            List<string> urls = new List<string>();
            foreach (Actor actor in Actors)
            {
                urls.Add(actor.Sprite);
            }
            var returnObj=DownloadImages(urls,@"Data\Images\Characters" ,client);

            response.Data.ImagesOld = returnObj.Result.ImagesOld;
            response.Data.ImagesNew = returnObj.Result.ImagesNew;
            return response;
        }

        [HttpGet("/api/Images/Backgrounds")]
        public async Task<Response<ImageDTO>> DownloadBackgroundImages()
        {
            Response<ImageDTO> response = new Response<ImageDTO>();
            List<Background> Backgrounds = _context.Backgrounds.Where(a => a.Sprite != "").ToList();
            HttpClient client = new HttpClient();
            List<string> urls = new List<string>();
            foreach (Background background in Backgrounds)
            {
                urls.Add(background.Sprite);
            }
            var returnObj = DownloadImages(urls, @"Data\Images\Backgrounds", client);

            response.Data.ImagesOld = returnObj.Result.ImagesOld;
            response.Data.ImagesNew = returnObj.Result.ImagesNew;
            return response;
        }

        [HttpGet("/api/Images/Characters/renpy")]
        public Response<string> CreateRenpyCharacters()
        {
            Response<string> response= new Response<string>();

            List<Actor> actors = _context.Actors.Where(o => o.Name != "").ToList();
            List<string> actorNames= new List<string>();
            StringBuilder pythonScript = new StringBuilder();

            foreach (Actor actor in actors)
            {
                pythonScript.AppendLine(CreateRenpyChar(actor));
            }

            System.IO.File.WriteAllText(@"Data\charscript.py",pythonScript.ToString());
            
            
            response.Data = "Script generated";
            return response;

        }

        [HttpPost]
        public async Task<ActionResult<Object>> UpdateData()
        {
            Object obj = new Object();
            string Arknightswiki = "https://arknights.fandom.com/wiki/";
            string storyType = "Story/Main_Theme";

            List<List<Chapter>> Episodes = GetChapters(Arknightswiki + storyType);
            


            foreach (List<Chapter> StoryNodes in Episodes)
            {
                foreach (Chapter chapter in StoryNodes)
                {
                    if (!_context.Chapters.Any(o => o.Name == chapter.Name))
                    {
                        string storyNode = chapter.Name;
                        HtmlDocument doc = GetDocument(Arknightswiki + storyNode + "/Story");
                        List<Actor> Actors = GetActors(Arknightswiki + storyNode + "/Story", doc);
                        Actors.Add(new Actor("Empty", ""));
                        Actors.Add(new Actor("???", ""));
                        List<int> ActorNames = new List<int>();
                        List<int> BackgroundNames = new List<int>();
                        List<int> SceneList = new List<int>();

                        foreach (Actor actor in Actors)
                        {
                            int ActorId = AddActor(actor, _context);
                            ActorNames.Add(ActorId);
                        }

                        List<Background> Backgrounds = GetBackgrounds(Arknightswiki + storyNode + "/Story", doc);

                        foreach (var background in Backgrounds)
                        {
                            if (!_context.Backgrounds.Any(o => o.Name == background.Name))
                            {
                                _context.Add(background);
                                await _context.SaveChangesAsync();
                            }
                            int BackgroundId = _context.Backgrounds.Where(o => o.Name == background.Name).First().Id;
                            BackgroundNames.Add(BackgroundId);
                        }

                        List<Scene> Scenes = GetScenes(Arknightswiki + storyNode + "/Story", _context, chapter.Name, doc);
                        foreach (var scene in Scenes)
                        {
                            if (!_context.Scenes.Any(o => o.Dialogue == scene.Dialogue))
                            {
                                _context.Add(scene);
                                await _context.SaveChangesAsync();
                            }
                            int SceneID = _context.Scenes.Where(o => o.Dialogue == scene.Dialogue).First().Id;
                            int ActorID = _context.Scenes.Where(o => o.Dialogue == scene.Dialogue).First().ActorId;
                            SceneList.Add(SceneID);
                            if (!ActorNames.Contains(ActorID))
                            {
                                ActorNames.Add(ActorID);
                            }

                        }
                        if (ActorNames.Count > 0)
                        {
                            string ActorNamesString = string.Empty;
                            foreach (int a in ActorNames)
                            {
                                ActorNamesString += a.ToString() + ",";
                            }
                            chapter.Actors = ActorNamesString.Remove(ActorNamesString.Length - 1);
                        }
                        else
                        {
                            chapter.Actors = "No Actors";
                        }


                        if (BackgroundNames.Count > 0)
                        {
                            string BackgroundNamesString = string.Empty;
                            foreach (int b in BackgroundNames)
                            {
                                BackgroundNamesString += b.ToString() + ",";
                            }
                            chapter.Backgrounds = BackgroundNamesString.Remove(BackgroundNamesString.Length - 1);
                        }
                        else
                        {
                            chapter.Backgrounds = "No Backgrounds";

                        }
                        if (Scenes.Count > 0)
                        {
                            string SceneListString = string.Empty;
                            foreach (int s in SceneList)
                            {
                                SceneListString += s.ToString() + ",";
                            }
                            chapter.SceneList = SceneListString.Remove(SceneListString.Length - 1);
                        }
                        else
                        {
                            chapter.SceneList = "No Scenes";
                        }

                        if (!_context.Chapters.Any(o => o.Name == chapter.Name))
                        {
                            _context.Add(chapter);
                            await _context.SaveChangesAsync();
                        }
                    }

                }
            }
            
            return Ok(obj);
        }


        [HttpPost("/api/Images/Characters")]
        public async Task<Response<string>> GetCharacterSprites()
        {
            string aceshipjson = "https://raw.githubusercontent.com/Aceship/AN-EN-Tags/master/json/ace/gallerylist.json";
            Response<string> response  = new Response<string>();
            List<int> chars= new List<int> {1,2,3,4,5,6,7,8,9,10};
            HttpClient client = new HttpClient();
            Stream stream = await client.GetStreamAsync(aceshipjson);
            string jsonFileName = @"Data\aceshipjson.json";
            var file = new FileStream(jsonFileName, FileMode.Create);
            await stream.CopyToAsync(file);
            file.Close();


            string jsonData=System.IO.File.ReadAllText(jsonFileName);
            JsonObject jsonItems = JsonConvert.DeserializeObject<JsonObject>(jsonData);

            List<Actor> actors = _context.Actors.ToList();
            List<List<CharacterSprite>>spriteList= new List<List<CharacterSprite>>();
            
            foreach (Actor actor in actors)
            {
                List<CharacterSprite> sprites= new List<CharacterSprite>();
                foreach (string character in jsonItems.characters)
                {
                    if (actor.ShortName != "")
                    {
                        if (character.Contains(actor.ShortName))
                        {
                            CharacterSprite characterSprite = new CharacterSprite(character, actor.Name);
                            sprites.Add(characterSprite);

                            if (!_context.Sprites.Any(o => o.Name == characterSprite.Name&&o.ImagePath==characterSprite.ImagePath))
                            {
                                _context.Sprites.Add(characterSprite);

                            }

                        }
                    }                                        
                }
                spriteList.Add(sprites);
            }
            _context.SaveChanges();
            return response;
        }


        static HtmlDocument GetDocument(string url)
        {
            //Load the webpage of the url provided and make an html document out of that.
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);
            return doc;
        }
        static List<List<Chapter>> GetChapters(string url)
        {
            //Create a list to return
            List<List<Chapter>> episodes= new List<List<Chapter>>();
            List<Chapter> chapters = new List<Chapter>();

            //Download the htmlpage to parse
            HtmlDocument doc = GetDocument(url);

            //Get a list of the episodes and cycle through them to find the story nodes.
            HtmlNodeCollection Episodes = doc.DocumentNode.SelectNodes("//*[@class='mrfz-wtable']/tbody/tr/td");
            foreach (HtmlNode episode in Episodes)
            {
                HtmlNodeCollection StoryNodes = episode.ChildNodes;
                string episodename= doc.DocumentNode.SelectSingleNode(episode.ParentNode.XPath.ToString() + "/th/div").InnerText.ToString();

                //Cycle through each storynode and make a new chapter object out of it.
                foreach (HtmlNode storyNode in StoryNodes)
                {
                    if (storyNode.Name == "a")
                    {
                        Chapter chapter = new Chapter(storyNode.InnerText,episodename);
                        chapters.Add(chapter);
                    }
                }
                episodes.Add(chapters);
            }
            return episodes;
        }
        static List<Scene> GetScenes(string url, AKVNDBContext _context, string storyNode,HtmlDocument doc)
        {
            //Create a list to return
            List<Scene> Scenes = new List<Scene>();

            bool PathCheck = false;
            //Create a list of different parts of a node
            HtmlNodeCollection PartNodes = doc.DocumentNode.SelectNodes("//*[starts-with(@class,'wds-tab__content')]");


            //If no PartNodes exists in the above XPath, Get a new PartNodes variable with the corrected URL and XPath.
            if (PartNodes == null)
            {
                HtmlDocument doc2 = GetDocument(url.Substring(0,url.IndexOf("/Story")));
                PartNodes = doc2.DocumentNode.SelectNodes("//*[@class='mrfz-wtable']/tbody");

                PathCheck = true;
            }
            try
            {
                //Cycle through "#Before-Operation","#During-Operation" and "After-operation" parts of the story.
                foreach (HtmlNode part in PartNodes)
                {
                    string XPath = "//*[@class='mrfz-wtable']/tbody";
                    HtmlNode SceneTable = part;

                    if (!PathCheck)
                    {
                        SceneTable = part.SelectSingleNode(part.XPath + XPath);
                    }

                    //Check if TextTable is not null, otherwise return empty list.
                    if (SceneTable != null)
                    {
                        //Go through each childnode in TextTable
                        foreach (HtmlNode child in SceneTable.ChildNodes)
                        {
                            //Check if childnode is a <tr>
                            if (child.Name == "tr")
                            {
                                string text = "";
                                Actor owner = new Actor();
                                HtmlNode node = child.SelectSingleNode(child.XPath + "/th");
                                bool check = false;
                                HtmlNode textnode = child.SelectSingleNode(child.XPath + "/td");
                                StringBuilder sb = new StringBuilder();
                                if (textnode != null && node == null)
                                {
                                    check = true;
                                }
                                else if (textnode == null && node != null)
                                {
                                    check = true;
                                    text = node.InnerText;
                                }

                                //Check if textnode is not null, if so, cyclethrough all the childnodes and check they are #text nodes and grab their innertext. 
                                if (textnode != null)
                                {

                                    foreach (HtmlNode cnode in textnode.ChildNodes)
                                    {
                                        string TextToAdd = "";
                                        if (cnode.Name == "#text")
                                        {
                                            string innerText = cnode.InnerHtml;
                                            innerText = innerText.Replace("<br>", "\n");
                                            TextToAdd = cnode.InnerText;
                                        }
                                        text += TextToAdd+"\n";
                                    }
                                }

                                //If check is false, it means there's a character speaking. Take their name from the innertext and see if they exist in the database. If not, make a new Actor for them.
                                if (check == false && node != null)
                                {
                                    string ownername = node.InnerText.Replace("\n", "");
                                    string[] ownerNameArray=ownername.Split(' ');
                                    bool ownerCheck = _context.Actors.Any(o => o.Name==ownername);
                                    string newOwnerName = ownername;

                                    if (ownerCheck)
                                    {
                                        owner = _context.Actors.Where(o => o.Name == ownername).First();
                                    }
                                    else
                                    {
                                        bool ownerCheck2 = false;
                                        while (!ownerCheck2)
                                        {
                                            string trimmedOwnerName = newOwnerName.Remove(newOwnerName.Length - 1, 1).Trim();
                                            double lengthCheck = (double)trimmedOwnerName.Length / (double)ownername.Length;
                                            if (lengthCheck >= 0.5)
                                            {
                                                if (_context.Actors.Any(o => o.Name == trimmedOwnerName))
                                                {
                                                    owner = _context.Actors.Where(o => o.Name == trimmedOwnerName).First();
                                                    ownerCheck2 = true;
                                                }
                                                else
                                                {
                                                    newOwnerName = trimmedOwnerName;
                                                }
                                            }
                                            else
                                            {
                                                owner = new Actor(ownername, "");
                                                AddActor(owner, _context);
                                                break;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    owner = _context.Actors.Where(o => o.Name == "Empty").First();
                                }
                                HtmlNode imageNode = child.SelectSingleNode(child.XPath + "/td/a");
                                //Check if the innertext is empty
                                if (imageNode != null&& imageNode.Attributes["href"].Value.ToString().Contains(".png"))
                                {
                                    //If empty, get the image path.
                                    string imagepath = imageNode.Attributes["href"].Value.ToString();
                                    text = imagepath.Substring(0, imagepath.IndexOf(".png") + ".png".Length);
                                }else if(imageNode != null && !imageNode.Attributes["href"].Value.ToString().Contains(".png") && textnode != null)
                                {
                                    text = textnode.InnerText;
                                }
                                string[] stringsToBuild = text.Split("\n");
                                string sceneText = "";
                                foreach( string s in stringsToBuild)
                                {
                                    sb.AppendLine(s);
                                }
                                if (!sb.ToString().Contains(".png"))
                                {
                                    sceneText = sb.ToString();
                                }
                                else
                                {
                                    sceneText =text;
                                }
                                sceneText = sceneText.Replace("&lt;", "<");
                                sceneText = sceneText.Replace("&gt;", ">");
                                sceneText = sceneText.Replace("&#91;", "[");
                                sceneText = sceneText.Replace("&#93;", "]");
                                sceneText = sceneText.Replace("&#8212;", "—");

                                Scenes.Add(new Scene(owner.Name, sceneText, owner.Id));
                            }
                        }
                    }
                }

            }catch (NullReferenceException err)
            {
                Console.WriteLine(err.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return Scenes;
        }
        static List<Background> GetBackgrounds(string url,HtmlDocument doc)
        {

            //Create a list to return.
            List<Background> Backgrounds = new List<Background>();

            //Select Node according to btable class and check if it is not null.
            HtmlNode BackgroundTable = doc.DocumentNode.SelectSingleNode("//*[@class='mrfz-btable']/tbody/tr[4]/td");
            if (BackgroundTable != null)
            {
                //Check if the node needs adjustment according to innertext.
                if (BackgroundTable.InnerText == "Backgrounds\n")
                {
                    BackgroundTable = doc.DocumentNode.SelectSingleNode("//*[@class='mrfz-btable']/tbody/tr[5]/td");
                }


                //Cycle through the childnodes and see if its a <div>. If so, grab the background image path and name.
                foreach (HtmlNode child in BackgroundTable.ChildNodes)
                {
                    if (child.Name == "div")
                    {
                        string relativeXPath = "/div[1]";
                        string fullXPath = child.XPath + relativeXPath;
                        string name;
                        string imagePath;
                        string imagePathFull;

                        imagePathFull = doc.DocumentNode.SelectSingleNode(fullXPath + "/div[1]/a").Attributes["href"].Value.ToString();
                        imagePath = imagePathFull.Substring(0, imagePathFull.IndexOf(".png") + ".png".Length);
                        int pathStart = imagePath.IndexOf("Background-");
                        int pathEnd = imagePathFull.IndexOf(".png");
                        name = imagePath.Substring(pathStart, pathEnd - pathStart);
                        Backgrounds.Add(new Background(name, imagePath));
                    }
                }

            }
            return Backgrounds;
        }
        static List<Actor> GetActors(string url,HtmlDocument doc)
        {
 
            //Create response to return.
            List<Actor> Characters = new List<Actor>();

            //Define XPath and if node exists for it.
            string XPath = "//*[@class='mrfz-btable']/tbody/tr[2]/td";
            if (doc.DocumentNode.SelectSingleNode(XPath) != null)
            {

                //Check if the node needs adjustment according to innertext.
                if (doc.DocumentNode.SelectSingleNode(XPath).InnerText == "Characters\n")
                {
                    XPath = "//*[@class='mrfz-btable']/tbody/tr[3]/td";
                }

                //Get the table node.
                HtmlNode CharTable = doc.DocumentNode.SelectSingleNode(XPath);

                
                //Cycle through the childnodes of Chartable and see if its a Div
                foreach (HtmlNode child in CharTable.ChildNodes)
                {
                    if (child.Name == "div")
                    {
                        string relativeXPath = $"/div[2]/a";
                        string fullXPath = child.XPath + relativeXPath;

                        string name;
                        string imagePath;

                        //Check if the a-tag fullXPath node exists, if so, grab the name  of the character and grab the image path of the character.
                        if (doc.DocumentNode.SelectSingleNode(fullXPath) != null)
                        {
                            name = doc.DocumentNode.SelectSingleNode(fullXPath).Attributes["title"].Value.ToString();
                            string imageUrl = doc.DocumentNode.SelectSingleNode(fullXPath).Attributes["title"].Value.ToString();
                            string webPage = url.Substring(0, url.IndexOf("/wiki") + "/wiki".Length) + "/" + imageUrl;
                            imagePath = GetImagePath(webPage);
                        }
                        else //Else just get the innertext of the div as the name of the character.
                        {
                            relativeXPath = $"/div[2]";
                            fullXPath = child.XPath + relativeXPath;
                            name = doc.DocumentNode.SelectSingleNode(fullXPath).InnerText.ToString();
                            imagePath = "";
                        }
                        Characters.Add(new Actor(name, imagePath));
                    }
                }
            }
            return Characters;
        }
        static string GetImagePath(string url)
        {
            if (url.Contains("&#39;"))
            {
                url=url.Replace("&#39;", "'");
            }
            HtmlDocument doc = GetDocument(url);
            string ImagePathFull = doc.DocumentNode.SelectSingleNode("//meta[@property='og:image']").GetAttributeValue("content", null);
            string PngExtention = ".png";
            string ImagePath = ImagePathFull.Substring(0, ImagePathFull.IndexOf(PngExtention) + PngExtention.Length);
            return ImagePath;
        }
        async Task<ImageDTO> DownloadImages(List<string> urls,string imageDirectory,HttpClient client)
        {
            List<string>imagesNew= new List<string>();
            List<string> imagesOld= new List<string>();
            ImageDTO returnDTO = new ImageDTO();
            foreach (string url in urls)
            {
                string fileName = Path.GetFileName(url);
                string path = Path.Combine(Environment.CurrentDirectory, imageDirectory, fileName);
                if (!System.IO.File.Exists(path))
                {
                    var httpResult = client.GetAsync(url).Result;
                    using var resultStream = await httpResult.Content.ReadAsStreamAsync();
                    using var fileStream = System.IO.File.Create(path);
                    resultStream.CopyTo(fileStream);
                    imagesNew.Add(fileName);
                }
                else
                {
                    imagesOld.Add(fileName);
                }
            }
            returnDTO.ImagesNew= imagesNew;
            returnDTO.ImagesOld= imagesOld;
            return returnDTO;
           

        }
        static string CreateRenpyChar(Actor actor)
        {
            StringBuilder pythonScript= new StringBuilder();
            string name = actor.Name;
            pythonScript.AppendLine($"image {name}Image:");
            pythonScript.Append("    ");
            pythonScript.AppendLine($"'images/Characters/{name}.png' ");
            pythonScript.Append("    ");
            pythonScript.AppendLine("zoom 0.65 ");
            pythonScript.AppendLine($"define {name}=Character('{name}')");

            return pythonScript.ToString();
        }
        static int AddActor(Actor actor, AKVNDBContext _context)
        {
            if(actor.Name.Contains(" (NPC)"))
            {
                actor.Name=actor.Name.Replace(" (NPC)","");
            }
            if (actor.Name.Contains(" (enemy)"))
            {
                actor.Name = actor.Name.Replace(" (enemy)", "");
            }
            if (actor.Name.Contains("'"))
            {
                actor.Name = actor.Name.Replace("'", "");
            }
            if (actor.Name.Contains("&#39;"))
            {
                actor.Name = actor.Name.Replace("&#39;", "");
            }
            if (actor.Name.Contains("\""))
            {
                actor.Name = actor.Name.Replace("\"", "");
            }
            if (!_context.Actors.Any(o => o.Name.StartsWith(actor.Name)))
            {
                _context.Add(actor);
                _context.SaveChanges();
            }
            int ActorId = _context.Actors.Where(o => o.Name.Contains(actor.Name)).First().Id;
            return ActorId;
        }

    }
}
