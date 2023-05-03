using AKVN_Backend.Classes;
using AKVN_Backend.Data;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AKVN_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class SceneController : ControllerBase
    {
        private readonly AKVNDBContext _context;
        public SceneController(AKVNDBContext context)
        {
            _context = context;

        }

        [HttpPost]
        public async Task<ActionResult<List<Scene>>> AddBackground()
        {
            List<Scene> Scenes = new List<Scene>();
            string Arknightswiki = "https://arknights.fandom.com/wiki/";
            List<string> StoryNodes = new List<string> { "0-0", "0-1", "0-2", "0-3", "0-4", "0-5", "0-6", "0-7", "0-8", "0-9" };

            foreach (string StoryNode in StoryNodes)
            {

                Scenes = GetScenes(Arknightswiki + StoryNode + "/Story",_context,StoryNode);

                foreach (var scene in Scenes)
                {
                    if (!_context.Scenes.Any(o => o.Dialogue == scene.Dialogue))
                    {
                        _context.Add(scene);
                        await _context.SaveChangesAsync();
                    }
                }
            }

            return Ok(await _context.Scenes.ToListAsync());

        }

        static HtmlDocument GetDocument(string url)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);
            return doc;
        }

        static List<Scene> GetScenes(string url, AKVNDBContext _context,string storyNode)
        {
            //Create a list to return
            List<Scene> Scenes = new List<Scene>();

            //Download the htmlpage to parse
            HtmlDocument doc = GetDocument(url);
            
            //Create a list of different parts of a node
            HtmlNodeCollection PartNodes = doc.DocumentNode.SelectNodes("//*[starts-with(@class,'wds-tab__content')]");

            foreach (HtmlNode part in PartNodes) 
            {
                //Select the body of the text table
                HtmlNode SceneTable = part.SelectSingleNode(part.XPath+"//*[@class='mrfz-wtable']/tbody");

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




                            //Check if the innertext is empty
                            if (child.SelectSingleNode(child.XPath + "/td/a") != null)
                            {
                                //If empty, get the image path.
                                string imagepath = doc.DocumentNode.SelectSingleNode(child.XPath + "/td/a").Attributes["href"].Value.ToString();
                                text = imagepath.Substring(0, imagepath.IndexOf(".png") + ".png".Length);
                            }
                            Scenes.Add(new Scene(owner, text));
                        }
                    }
                }



            }



            return Scenes;
        }



    }
}
