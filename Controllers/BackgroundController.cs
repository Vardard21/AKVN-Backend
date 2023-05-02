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
    public class BackgroundController : ControllerBase
    {
        private readonly AKVNDBContext _context;
        public BackgroundController(AKVNDBContext context)
        {
            _context = context;

        }
        [HttpPost]
        public async Task<ActionResult<List<Background>>> AddBackground()
        {
            List<Background> Backgrounds = new List<Background>();
            string Arknightswiki = "https://arknights.fandom.com/wiki/";
            List<string> StoryNodes = new List<string> { "0-0", "0-1", "0-2","0-3","0-4", "0-5","0-6", "0-7", "0-8", "0-9" };

            foreach (string StoryNode in StoryNodes)
            {
                
                Backgrounds = GetBackgrounds(Arknightswiki + StoryNode + "/Story");

                foreach (var background in Backgrounds)
                {
                    if (!_context.Backgrounds.Any(o => o.Name == background.Name))
                    {
                        _context.Add(background);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {

                    }
                }
            }
           
            return Ok(await _context.Backgrounds.ToListAsync());

        }
        static HtmlDocument GetDocument(string url)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);
            return doc;
        }


        static List<Background> GetBackgrounds(string url)
        {
            HtmlDocument doc = GetDocument(url);
            string XPath = "//*[@id='mw-content-text']/div[1]/table[2]/tbody/tr[4]/td";
            List<Background> Backgrounds = new List<Background>();

            if (doc.DocumentNode.SelectSingleNode(XPath) != null)
            {
                if (doc.DocumentNode.SelectSingleNode(XPath).InnerText == "Backgrounds\n")
                {
                    XPath = "//*[@id='mw-content-text']/div[1]/table[2]/tbody/tr[5]/td";
                }
                HtmlNode BackgroundTable = doc.DocumentNode.SelectSingleNode(XPath);

                int BackgroundAmounts = BackgroundTable.ChildNodes.Count / 2;

                for (int i = 0; i < (BackgroundAmounts); i++)
                {
                string relativeXPath = $"/div[{i + 1}]/div[1]";
                string fullXPath = XPath + relativeXPath;
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
            return Backgrounds;
        }
    }
}
