using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace OneDriveSaver
{
    class LibraryMgr
    {
        public Dictionary<string, Game> games = new Dictionary<string, Game>(StringComparer.InvariantCultureIgnoreCase);
        private string path;

        public event HasUpdatedEventHandler Updated;
        public delegate void HasUpdatedEventHandler(Game sender);

        public LibraryMgr(string path)
        {
            this.path = path;
        }

        public void Process()
        {
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
                game.m_Path = fileName;
            }
            catch (Exception ex)
            {
            }

            // failed to parse
            if (game == null || game.Name == null)
                return;

            if (games.ContainsKey(game.Name))
                return;

            games[game.Name] = game;
            game.Initialize();

            Updated?.Invoke(game);
        }

        internal void SerializeGame(Game game)
        {
            throw new NotImplementedException();
        }
    }
}
