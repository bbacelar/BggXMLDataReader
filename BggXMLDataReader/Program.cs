using ns;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BggXMLDataReader
{
    class Program
    {
        static void Main(string[] args)
        {
            DirectoryInfo dInfo = new DirectoryInfo(@"D:\Desenvolvimento\BggXMLDataReader\201703\boardgame_batches");
            FileInfo[] files = dInfo.GetFiles();
            NumericComparer ns = new NumericComparer();
            Array.Sort(files.Select(f => f.Name).ToArray(), files, ns);
            string atualGame = string.Empty;
            foreach (FileInfo fi in files)
            {
                var xDoc = XDocument.Load(fi.FullName);
                var root = xDoc.Root;

                try
                {
                    using (BGEventManagerEntities dbContext = new BGEventManagerEntities())
                    {
                        int bggID, minPlayers, maxPlayers, anoPublicacao, tempojogo;
                        string description, nome;
                        List<string> alternateNames;
                        foreach (var item in root.Elements("item"))
                        {
                            bggID = Convert.ToInt32(item.Attribute("id").Value);
                            nome = item.Elements("name").Where(i => i.Attribute("type").Value == "primary").First().Attribute("value").Value;
                            atualGame = nome;
                            alternateNames = item.Elements("name").Where(i => i.Attribute("type").Value == "alternate").Select(i => i.Attribute("value").Value).ToList();
                            description = System.Net.WebUtility.HtmlDecode(item.Element("description").Value);
                            try
                            {
                                minPlayers = Convert.ToInt32(item.Element("minplayers").Attribute("value").Value);
                            }
                            catch
                            {
                                minPlayers = 0;
                            }
                            try
                            {
                                maxPlayers = Convert.ToInt32(item.Element("maxplayers").Attribute("value").Value);
                            }
                            catch
                            {
                                maxPlayers = 0;
                            }
                            try
                            {
                                anoPublicacao = Convert.ToInt32(item.Element("yearpublished").Attribute("value").Value);
                            }
                            catch
                            {
                                anoPublicacao = 0;
                            }
                            try
                            {
                                tempojogo = Convert.ToInt32(item.Element("playingtime").Attribute("value").Value);
                            }
                            catch
                            {
                                tempojogo = 0;
                            }

                            BoardGame bg = new BoardGame();
                            bg.ID_BGG = bggID;
                            bg.Descricao = description;
                            bg.MaxJogadores = maxPlayers;
                            bg.MinJogadores = minPlayers;
                            bg.AnoPublicacao = anoPublicacao;
                            bg.TempoJogo = tempojogo;

                            BoardgameNome bgName = new BoardgameNome();
                            bgName.BoardGame = bg;
                            bgName.Nome = nome;
                            bgName.IsPrincipal = true;

                            foreach (var aName in alternateNames)
                            {
                                BoardgameNome aBgName = new BoardgameNome();
                                aBgName.BoardGame = bg;
                                aBgName.Nome = aName;
                                aBgName.IsPrincipal = false;
                                dbContext.BoardgameNome.Add(aBgName);
                            }

                            dbContext.BoardGame.Add(bg);
                            dbContext.BoardgameNome.Add(bgName);

                            dbContext.SaveChanges();
                        }
                    }
                }
                catch
                {
                    Console.WriteLine("Erro ao tentar ler/salvar o seguinte jogo:" + atualGame);
                }
            }

            Console.WriteLine("Terminou!!!");
            Console.Read();
        }
    }
}
