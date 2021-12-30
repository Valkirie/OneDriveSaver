﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DropboxMe
{
    class LibraryMgr
    {
        public Dictionary<string, Game> games = new();
        private Dictionary<string, DateTime> dateTimeDictionary = new();

        private Form1 form;

        public FileSystemWatcher profileWatcher { get; set; }

        public LibraryMgr(Form1 form, string path)
        {
            this.form = form;

            string[] dirs = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);
            foreach (string directory in dirs)
            {
                string settingsPath = Path.Combine(directory, "Settings.json");
                if (!File.Exists(settingsPath))
                    continue; // game has broken settings

                ProcessGame(settingsPath);
            }
        }

        private void ProcessGame(string fileName)
        {
            Game game = null;

            try
            {
                string outputraw = File.ReadAllText(fileName);
                game = JsonSerializer.Deserialize<Game>(outputraw);
                game.Path = fileName;
            }
            catch (Exception ex) { }

            // failed to parse
            if (game == null || game.Name == null)
                return;

            if (games.ContainsKey(game.Name) && games[game.Name].GetHashCode() == game.GetHashCode())
                return;

            games[game.Name] = game;
            game.Initialize();
            game.SetJunctions();

            form.UpdateList(game.Name);
        }

        internal void SerializeGame(Game game)
        {
            throw new NotImplementedException();
        }
    }
}
