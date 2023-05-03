using AKVN_Backend.Classes;
using AKVN_Backend.Data;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.CodeDom;
using System.Security.AccessControl;

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

        [HttpPost]
        public async Task<ActionResult<Object>> GetData()
        {
            Object obj = new Object();
            string Arknightswiki = "https://arknights.fandom.com/wiki/";
            string storyType = "Story/Main_Theme";

            List<Chapter> StoryNodes = GetChapters(Arknightswiki + storyType);

            foreach (Chapter chapter in StoryNodes)
            {

                string storyNode = chapter.Name;
                List<Actor> Actors = GetActors(Arknightswiki + storyNode + "/Story");
                Actors.Add(new Actor("Empty", ""));
                Actors.Add(new Actor("???", ""));

                foreach (Actor actor in Actors)
                {
                    if (actor.Name.Contains(" (NPC)"))
                    {
                        actor.Name = actor.Name.Substring(0, actor.Name.IndexOf(" (NPC)"));
                    }

                    if (!_context.Actors.Any(o => o.Name == actor.Name))
                    {
                        _context.Add(actor);
                        await _context.SaveChangesAsync();
                    }
                }

                List<Background> Backgrounds = GetBackgrounds(Arknightswiki + storyNode + "/Story");

                foreach (var background in Backgrounds)
                {
                    if (!_context.Backgrounds.Any(o => o.Name == background.Name))
                    {
                        _context.Add(background);
                        await _context.SaveChangesAsync();
                    }
                }

                List<Scene> Scenes = GetScenes(Arknightswiki + storyNode + "/Story", _context, chapter.Name);
                foreach (var scene in Scenes)
                {
                    if (!_context.Scenes.Any(o => o.Dialogue == scene.Dialogue))
                    {
                        _context.Add(scene);
                        await _context.SaveChangesAsync();
                    }
                }

                chapter.Scenes = Scenes;

                if (!_context.Chapters.Any(o => o.Name == chapter.Name))
                {
                    _context.Add(chapter);
                    await _context.SaveChangesAsync();
                }



            }

            return Ok(obj);
        }






        static HtmlDocument GetDocument(string url)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);
            return doc;
        }
        static List<Chapter> GetChapters(string url)
        {
            //Create a list to return
            List<Chapter> chapters = new List<Chapter>();

            //Download the htmlpage to parse
            HtmlDocument doc = GetDocument(url);

            HtmlNodeCollection Episodes = doc.DocumentNode.SelectNodes("//*[@class='mrfz-wtable']/tbody/tr/td");

            foreach (HtmlNode episode in Episodes)
            {
                HtmlNodeCollection StoryNodes = episode.ChildNodes;
                foreach (HtmlNode storyNode in StoryNodes)
                {

                    if (storyNode.Name == "a")
                    {
                        Chapter chapter = new Chapter(storyNode.InnerText);
                        chapters.Add(chapter);
                    }
                }
            }
            return chapters;
        }
        static List<Scene> GetScenes(string url, AKVNDBContext _context, string storyNode)
        {
            //Create a list to return
            List<Scene> Scenes = new List<Scene>();

            //Download the htmlpage to parse
            HtmlDocument doc = GetDocument(url);
            bool PathCheck = false;

            //Create a list of different parts of a node
            HtmlNodeCollection PartNodes = doc.DocumentNode.SelectNodes("//*[starts-with(@class,'wds-tab__content')]");
            if (PartNodes == null)
            {
                HtmlDocument doc2 = GetDocument(url.Substring(0,url.IndexOf("/Story")));
                PartNodes = doc2.DocumentNode.SelectNodes("//*[@class='mrfz-wtable']/tbody");

                PathCheck = true;
            }

            foreach (HtmlNode part in PartNodes)
            {
                string XPath = "//*[@class='mrfz-wtable']/tbody";
                HtmlNode SceneTable = part;

                if (!PathCheck)
                {
                    SceneTable=part.SelectSingleNode(part.XPath + XPath);
                }

                //Select the body of the text table


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
                            if (textnode != null && node == null)
                            {
                                check = true;
                            }
                            else if (textnode == null && node != null)
                            {
                                check = true;
                                text = node.InnerText;

                            }



                            if (textnode != null)
                            {
                                foreach (HtmlNode cnode in textnode.ChildNodes)
                                {
                                    string TextToAdd = "";
                                    if (cnode.Name == "#text")
                                    {
                                        TextToAdd = cnode.InnerText + "<br>";

                                    }
                                    text += TextToAdd;
                                }
                            }

                            if (check == false)
                            {
                                string ownername = node.InnerText.Replace("\n", "");
                                bool ownerCheck = _context.Actors.Any(o => o.Name == ownername);

                                if (ownerCheck)
                                {
                                    owner = _context.Actors.Where(o => o.Name == ownername).First();
                                }
                                else
                                {
                                    owner = new Actor(ownername, "");
                                    _context.Add(owner);
                                    _context.SaveChanges();
                                }


                            }
                            else
                            {
                                owner = _context.Actors.Where(o => o.Name == "Empty").First();
                            }



                            HtmlNode imageNode = child.SelectSingleNode(child.XPath + "/td/a");
                            //Check if the innertext is empty
                            if (imageNode != null)
                            {
                                //If empty, get the image path.
                                string imagepath = imageNode.Attributes["href"].Value.ToString();
                                text = imagepath.Substring(0, imagepath.IndexOf(".png") + ".png".Length);
                            }
                            Scenes.Add(new Scene(owner, text));
                        }
                    }
                }



            }



            return Scenes;
        }
        static List<Background> GetBackgrounds(string url)
        {
            //Download the htmlpage to parse
            HtmlDocument doc = GetDocument(url);


            HtmlNode BackgroundTable = doc.DocumentNode.SelectSingleNode("//*[@class='mrfz-btable']/tbody/tr[4]/td");

            //Create a list to return
            List<Background> Backgrounds = new List<Background>();

            //Select the body of the Background table

            if (BackgroundTable != null)
            {
                if (BackgroundTable.InnerText == "Backgrounds\n")
                {
                    BackgroundTable = doc.DocumentNode.SelectSingleNode("//*[@class='mrfz-btable']/tbody/tr[5]/td");
                }



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
        static List<Actor> GetActors(string url)
        {
            HtmlDocument doc = GetDocument(url);
            List<Actor> Characters = new List<Actor>();
            string XPath = "//*[@class='mrfz-btable']/tbody/tr[2]/td";
            if (doc.DocumentNode.SelectSingleNode(XPath) != null)
            {
                if (doc.DocumentNode.SelectSingleNode(XPath).InnerText == "Characters\n")
                {
                    XPath = "//*[@class='mrfz-btable']/tbody/tr[3]/td";
                }
                HtmlNode CharTable = doc.DocumentNode.SelectSingleNode(XPath);
                int CharAmounts = CharTable.ChildNodes.Count / 2;

                foreach (HtmlNode child in CharTable.ChildNodes)
                {
                    if (child.Name == "div")
                    {
                        string relativeXPath = $"/div[2]/a";
                        string fullXPath = child.XPath + relativeXPath;

                        string name;
                        string imagePath;
                        if (doc.DocumentNode.SelectSingleNode(fullXPath) != null)
                        {
                            name = doc.DocumentNode.SelectSingleNode(fullXPath).Attributes["title"].Value.ToString();
                            string webPage = url.Substring(0, url.IndexOf("/wiki") + "/wiki".Length) + "/" + (doc.DocumentNode.SelectSingleNode(fullXPath).Attributes["title"].Value.ToString());
                            imagePath = GetImagePath(webPage);
                        }
                        else
                        {
                            relativeXPath = $"/div[2]";
                            fullXPath = child.XPath + relativeXPath;
                            name = doc.DocumentNode.SelectSingleNode(fullXPath).InnerText.ToString();
                            imagePath = "";
                        }
                        Characters.Add(new Actor(name, imagePath));
                    }
                }


                for (int i = 0; i < CharAmounts; i++)
                {

                }
            }
            return Characters;
        }
        static string GetImagePath(string url)
        {
            HtmlDocument doc = GetDocument(url);
            string ImagePathFull = doc.DocumentNode.SelectSingleNode("//meta[@property='og:image']").GetAttributeValue("content", null);
            string PngExtention = ".png";
            string ImagePath = ImagePathFull.Substring(0, ImagePathFull.IndexOf(PngExtention) + PngExtention.Length);
            return ImagePath;
        }
    }
}
