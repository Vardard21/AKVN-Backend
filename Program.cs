
using AKVN_Backend.Data;
using System.Data.SQLite;

using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using AKVN_Backend.Classes;
using HtmlAgilityPack;

namespace AKVN_Backend
{
    public class Program
    {
        public static void Main(string[] args)
        {

            List<Text> Texts = new List<Text>();
            List<string> StoryNodes = new List<string> { "0-0", "0-1", "0-2", "0-3", "0-4", "0-5", "0-6", "0-7", "0-8", "0-9" };
            string StoryNode = StoryNodes[0];
            string Arknightswiki = "https://arknights.fandom.com/wiki/";
            Texts = GetTexts(Arknightswiki + StoryNode + "/Story");


            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddDbContext<AKVNDBContext>(options=>options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();



            app.Run();
        }

        static HtmlDocument GetDocument(string url)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);
            return doc;
        }

        static List<Text> GetTexts(string url)
        {
            //Download the htmlpage to parse
            HtmlDocument doc = GetDocument(url);

            //Create a list to return
            List<Text> Texts = new List<Text>();

            //Select the body of the text table
            HtmlNode TextTable = doc.DocumentNode.SelectSingleNode("//*[@class='mrfz-wtable']/tbody");

            //Check if TextTable is not null, otherwise return empty list.
            if(TextTable != null) 
            {
                //Go through each childnode in TextTable
                foreach (HtmlNode  child in TextTable.ChildNodes)
                {
                    //Check if childnode is a <tr>
                    if (child.Name == "tr")
                    {
                        //Get the innertext of the node
                        string text = child.InnerText;

                        //Check if the innertext is empty
                        if (text == "\n\n")
                        {
                            //If empty, get the image path.
                            string imagepath = doc.DocumentNode.SelectSingleNode(child.XPath + "/td/a").Attributes["href"].Value.ToString();
                            text = imagepath.Substring(0, imagepath.IndexOf(".png") + ".png".Length);
                        }
                        Texts.Add(new Text(text));
                    }
                }
            }
            return Texts;
        }
    }
}