using System;
using System.IO;
using System.Net.Http;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace StephanePlazaImmoExtractor
{
    class Program
    {
        const string HEADER = "AGENCE;NOM;MAIL;TELEPHONE";
        const string SEPARATOR = ";";

        static void Main(string[] args)
        {

            List<string> listUrls = new List<string>();
            //using(var reader = new StreamReader(@".\\Data-cold-Agences-Stephane-Plaza.csv"))
            using(var reader = new StreamReader(@".\\Data-cold-Agences-Stephane-Plaza.csv"))
            {
                
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    //var values = line.Split(';');

                    listUrls.Add(line);
                }
            }

            var outputCsvPath = ".\\stephaneplazaimmo_extract.csv";
            using (StreamWriter writer = new StreamWriter(new FileStream(outputCsvPath,FileMode.Create, FileAccess.Write)))
            {
                writer.WriteLine(HEADER);

                foreach(string url in listUrls)
                {                  
                    
                    using (HttpClient client = new HttpClient())
                    {
                        using (HttpResponseMessage response = client.GetAsync(url).Result)
                        {
                            using (HttpContent content = response.Content)
                            {
                                string result = content.ReadAsStringAsync().Result;
                                var htmlDoc = new HtmlDocument();
                                htmlDoc.LoadHtml(result);
                                string[] urlSplitted = url.Split('/');
                                string agence = urlSplitted[urlSplitted.Length - 1];
                                Console.WriteLine("START OF : " + agence);

                                HtmlNodeCollection divGridInfoNodes = htmlDoc.DocumentNode.SelectNodes("//div[@class='grid-info']");
                                if(divGridInfoNodes == null) {
                                    Console.WriteLine("NO CONTACT IN " + agence);
                                    Console.WriteLine("END OF : " + agence);
                                } else {                               
                                    foreach (HtmlNode node in htmlDoc.DocumentNode.SelectNodes("//div[@class='grid-info']"))
                                    {
                                        string csvLine = agence + SEPARATOR;
                                        
                                        HtmlNode divClassTitleNode = node.SelectSingleNode(".//div[@class='title']");
                                        string name = divClassTitleNode.ChildNodes[0].InnerText.Trim();
                                        csvLine += name;
                                        csvLine += SEPARATOR;
                                        
                                        HtmlNode divClassContactNode = node.SelectSingleNode(".//div[@class='contact']");
                                        string mail = "";
                                        string telephone = "";
                                        foreach (HtmlNode tooltipComponentNode in divClassContactNode.SelectNodes(".//el-tooltip")) 
                                        {
                                            HtmlNode divSlotContentNode = tooltipComponentNode.SelectSingleNode(".//div[@slot='content']");
                                            if (divSlotContentNode.InnerText != "" && divSlotContentNode.InnerText != null) {
                                                telephone = divSlotContentNode.InnerText;
                                            }
                                            HtmlNode componentMail = tooltipComponentNode.SelectSingleNode(".//app-obfuscate-email[@rel='noopener noreferrer']");
                                            if (componentMail != null) {
                                                mail = componentMail.Attributes["email"].Value;
                                            }
                                                
                                        }
                                        csvLine += mail;
                                        csvLine += SEPARATOR;
                                        csvLine += telephone;
                                        writer.WriteLine(csvLine);
                                        
                                    }
                                }
                            }
                        }
                    }
                    Console.WriteLine("END OF " + url + " : OK");
                }
            }
        }
    }
}
