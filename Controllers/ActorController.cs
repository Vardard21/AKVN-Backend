using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AKVN_Backend.Data;
using AKVN_Backend.Classes;
using Microsoft.EntityFrameworkCore;
using HtmlAgilityPack;



namespace AKVN_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActorController : ControllerBase
    {
        private readonly AKVNDBContext _context;
        public ActorController(AKVNDBContext context)
        {
            _context = context;

        }
        [HttpPost]
        public async Task<ActionResult<List<Actor>>> AddActor()
        {
            List<Actor> Actors = new List<Actor>();
            List<string> StoryNodes = new List<string> { "0-0", "0-1", "0-2", "0-3", "0-4", "0-5", "0-6", "0-7", "0-8", "0-9" };
            string Arknightswiki = "https://arknights.fandom.com/wiki/";


            foreach (string StoryNode in StoryNodes)
            {
                Actors = GetActors(Arknightswiki + StoryNode + "/Story");

                foreach (Actor actor in Actors)
                {
                    if (_context.Actors.Any(o => o.Name == actor.Name))
                    {
                    }
                    else
                    {
                        _context.Add(actor);
                        await _context.SaveChangesAsync();
                    }
                }
            }

            return Ok(await _context.Actors.ToListAsync());
        }

        [HttpGet]
        public async Task<ActionResult<List<Actor>>> GetAllActor()
        {
            return Ok(await _context.Actors.ToListAsync());
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Actor>> GetActor(int id)
        {
            Actor? actor = await _context.Actors.FindAsync(id);
            if (actor == null)
            {
                return BadRequest("Actor not found.");
            }
            else
            {
                return Ok(actor);
            }
        }
        static HtmlDocument GetDocument(string url)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);
            return doc;
        }

        static string GetImagePath(string url)
        {
            HtmlDocument doc = GetDocument(url);
            string ImagePathFull = doc.DocumentNode.SelectSingleNode("//meta[@property='og:image']").GetAttributeValue("content", null);
            string PngExtention = ".png";
            string ImagePath = ImagePathFull.Substring(0, ImagePathFull.IndexOf(PngExtention) + PngExtention.Length);
            return ImagePath;
        }

        static List<Actor> GetActors(string url)
        {
            HtmlDocument doc = GetDocument(url);
            List<Actor> Characters = new List<Actor>();
            string XPath = "//*[@id='mw-content-text']/div[1]/table[2]/tbody/tr[2]/td";
            if (doc.DocumentNode.SelectSingleNode(XPath) != null)
            {
                if (doc.DocumentNode.SelectSingleNode(XPath).InnerText == "Characters\n")
                {
                    XPath = "//*[@id='mw-content-text']/div[1]/table[2]/tbody/tr[3]/td";
                }
                HtmlNode CharTable = doc.DocumentNode.SelectSingleNode(XPath);


                for (int i = 0; i < (CharTable.ChildNodes.Count / 2); i++)
                {
                    string relativeXPath = $"/div[{i + 1}]/div[2]/a";
                    string fullXPath = XPath + relativeXPath;

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
                        relativeXPath = $"/div[{i + 1}]/div[2]";
                        fullXPath = XPath + relativeXPath;
                        name = doc.DocumentNode.SelectSingleNode(fullXPath).InnerText.ToString();
                        imagePath = "";
                    }
                    Characters.Add(new Actor(name, imagePath));
                }
            }
            return Characters;
        }
    }
}
