using System;
using System.Collections.Generic;
using System.IO;
using Server;
using Server.Commands;
using Server.Items;
using Server.Prompts;
using Server.Targeting;

namespace Server.Commands
{
    public class ItemSearchCommand
    {
        public static void Initialize()
        {
            CommandSystem.Register("SearchItems", AccessLevel.Administrator, new CommandEventHandler(SearchItems_OnCommand));
        }

        private static void SearchItems_OnCommand(CommandEventArgs e)
        {
            Mobile from = e.Mobile;

            if (e.Length == 1)
            {
                string searchTerm = e.GetString(0);
                SearchItems(from, searchTerm);
            }
            else
            {
                from.SendMessage("Please provide a search term as an argument or enter it in the following prompt.");
                from.Prompt = new SearchTermPrompt(from);
            }
        }

        private class SearchTermPrompt : Prompt
        {
            private readonly Mobile m_From;

            public SearchTermPrompt(Mobile from)
            {
                m_From = from;
            }

            public override void OnResponse(Mobile from, string text)
            {
                SearchItems(from, text);
            }
        }

        private static void SearchItems(Mobile from, string searchTerm)
        {
            string fileName = $"Items_{searchTerm}.txt";
            string filePath = Path.Combine(Core.BaseDirectory, fileName);

            using (StreamWriter writer = new StreamWriter(filePath, false))
            {
                for (int i = 0; i < TileData.MaxItemValue; i++)
                {
                    ItemData itemData = TileData.ItemTable[i];

                    string name = itemData.Name ?? string.Empty;
                    TileFlag flags = itemData.Flags;

                    if (name.ToLower().Contains(searchTerm.ToLower()) || flags.ToString().ToLower().Contains(searchTerm.ToLower()))
                    {
                        string hexID = "0x" + i.ToString("X").PadLeft(3, '0');
                        writer.Write($"{hexID}, {name}, {flags}\n");
                    }
                }
            }

            from.SendMessage($"Item data containing '{searchTerm}' have been saved to the file '{fileName}'.");
        }

    }
}
