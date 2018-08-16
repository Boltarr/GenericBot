using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Discord;
using GenericBot.Entities;

namespace GenericBot.CommandModules
{
    public class FunCommands
    {
        public List<Command> GetFunCommands()
        {
            List<Command> FunCommands = new List<Command>();

            Command wat = new Command("wat");
            wat.Description = "The best command";
            wat.ToExecute += async (client, msg, parameters) =>
            {
                await msg.ReplyAsync($"**-wat-**\nhttp://destroyallsoftware.com/talks/wat");
            };

            FunCommands.Add(wat);

            Command roll = new Command("roll");
            roll.Aliases.Add("dice");
            roll.Description = "Roll a specified number of dices. Defaults to 1d20 if no parameters";
            roll.Usage = "roll [count]d[sides]";
            roll.ToExecute += async (client, msg, parameters) =>
            {
                uint count = 1;
                uint sides = 20;
                int add = 0;
                if (!parameters.Empty())
                {
                    string param = parameters.reJoin("").ToLower();
                    if (!param.Contains("d"))
                    {
                        if (!uint.TryParse(param, out count))
                        {
                            await msg.ReplyAsync("Input improperly formatted");
                            return;
                        }
                    }
                    else
                    {
                        if (!(param.Contains("+") || param.Contains("-")))
                        {
                            var list = param.Split('d');
                            if (!(uint.TryParse(list[0], out count) && uint.TryParse(list[1], out sides)))
                            {
                                await msg.ReplyAsync("Input improperly formatted");
                                return;
                            }
                        }
                        else
                        {
                            string c = param.Split('d')[0];
                            string second = param.Split('d')[1];
                            string s = "";
                            string a = "";
                            if (param.Contains("+"))
                            {
                                s = second.Split('+')[0];
                                a = second.Split('+')[1];
                            }
                            else if (param.Contains("-"))
                            {
                                s = second.Split('-')[0];
                                a = second.Replace(s, "");
                            }

                            if (!(uint.TryParse(c, out count) && uint.TryParse(s, out sides) && int.TryParse(a, out add)))
                            {
                                await msg.ReplyAsync("Input improperly formatted");
                                return;
                            }
                        }
                    }
                }

                if (count > 100)
                {
                    await msg.ReplyAsync("I don't think you can hold that many!");
                    return;
                }
                if (sides < 2)
                {
                    await msg.ReplyAsync("You can't have a 1-sided dice!");
                    return;
                }
                if (sides > 100)
                {
                    await msg.ReplyAsync("That's an awefully large dice, I can't hold that");
                    return;
                }
                RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
                List<int> results = new List<int>();
                while (results.Count < count)
                {
                    byte[] bytes = new byte[4];
                    crypto.GetNonZeroBytes(bytes);
                    long rand = Math.Abs(BitConverter.ToInt32(bytes, 0)) % sides;
                    results.Add((int)rand + 1 + add);
                }

                string res = $"{msg.Author.Mention}, you rolled ";
                results.Sort();
                res += results.SumAnd();
                if (count > 1) res += $" with a total of {results.Sum()}";
                await msg.ReplyAsync(res);
            };

            FunCommands.Add(roll);

            Command cat = new Command("cat");
            cat.Description = "Link a cat pic";
            cat.SendTyping = false;
            cat.ToExecute += async (client, msg, parameters) =>
            {
                try
                {
                    await msg.ReplyAsync(GenericBot.Animols.GetCat());
                    GenericBot.Animols.RenewCats();
                }
                catch (Exception ex)
                {
                    await msg.ReplyAsync("Uh oh, something borked a bit. Wait a sec and try again.");
                    GenericBot.Animols.RenewCats();
                }
            };
            FunCommands.Add(cat);

            Command dog = new Command("dog");
            dog.Description = "Link a dog pic";
            dog.SendTyping = false;
            dog.ToExecute += async (client, msg, parameters) =>
            {
                await msg.ReplyAsync(GenericBot.Animols.GetDog());
                GenericBot.Animols.RenewDogs();
            };
            FunCommands.Add(dog);


            Command addQuote = new Command("addQuote");
            addQuote.SendTyping = false;
            addQuote.Description = "Add a quote to the server's list";
            addQuote.ToExecute += async (client, msg, parameters) =>
            {
                var dbGuild = new DBGuild(msg.GetGuild().Id);
                var q = dbGuild.AddQuote(parameters.reJoin());
                dbGuild.Save();
                await msg.ReplyAsync($"Added {q.ToString()}");
            };
            FunCommands.Add(addQuote);

            Command removeQuote = new Command("removeQuote");
            removeQuote.Description = "Remove a quote from the server's list";
            removeQuote.SendTyping = false;
            removeQuote.RequiredPermission = Command.PermissionLevels.Moderator;
            removeQuote.ToExecute += async (client, msg, parameters) =>
            {
                if (parameters.Empty())
                {
                    await msg.ReplyAsync("You must supply a number");
                    return;
                }

                var dbGuild = new DBGuild(msg.GetGuild().Id);
                if (int.TryParse(parameters[0], out int quid))
                {
                    if (dbGuild.RemoveQuote(quid))
                    {
                        await msg.ReplyAsync($"Succefully removed quote #{quid}");
                    }
                    else
                    {
                        await msg.ReplyAsync($"The number was greater than the number of quotes saved");
                        return;
                    }
                }
                else
                {
                    await msg.ReplyAsync("You must pass in a number");
                }
                dbGuild.Save();
            };
            FunCommands.Add(removeQuote);

            Command quote = new Command("quote");
            quote.SendTyping = false;
            quote.Description = "Get a random quote from the server's list";
            quote.ToExecute += async (client, msg, parameters) =>
            {
                if (!parameters.Empty() && parameters[0] == "all" && quote.GetPermissions(msg.Author, msg.GetGuild().Id) >=
                    Command.PermissionLevels.Admin)
                {
                    System.IO.File.WriteAllText("quotes.txt", new DBGuild(msg.GetGuild().Id).Quotes.Where(q => q.Active).Select(q => $"{q.Content} (#{q.Id})").Aggregate((i, j) => i + "\n" + j));
                    await msg.Channel.SendFileAsync("quotes.txt");
                    System.IO.File.Delete("quotes.txt");
                    return;
                }
                await msg.ReplyAsync(new DBGuild(msg.GetGuild().Id).GetQuote(parameters.reJoin()));
            };
            FunCommands.Add(quote);

            Command redact = new Command(nameof(redact));
            redact.ToExecute += async (client, msg, parameters) =>
            {
                msg.DeleteAsync();
                char block = '█';
                int rcont = 1;
                StringBuilder resp = new StringBuilder();
                foreach (var str in parameters)
                {
                    int rand = new Random().Next(0, rcont) + 1;
                    if (rand == rcont)
                    {
                        resp.Append(' ');
                        resp.Append(str);
                        resp.Append(' ');
                        rcont = 1;
                    }
                    else
                    {
                        resp.Append(block.Multiply(str.Length));
                        if (rcont == 16) rcont = 1;
                    }
                    rcont++;
                }

                await msg.ReplyAsync(resp.ToString().Replace("  ", " "));
            };

            FunCommands.Add(redact);
            
            Command height = new Command("height");
            height.Description = "Converts a height in inches to a height in Poppies";
            height.ToExecute += async (client, msg, parameters) =>
            {
                if(parameters.Empty() || parameters.Count > 1)
                {
                    await msg.ReplyAsync("Input improperly formatted");
                    return;
                }
                int inches = Int32.Parse(parameters[0]);
                double poppies = inches / 65.0;
                
                await msg.ReplyAsync($"{poppies} Poppies!");
            }
            
            FunCommands.Add(height);

            return FunCommands;
        }
    }
}
