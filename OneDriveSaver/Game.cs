using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Timers;

namespace OneDriveSaver
{
    [Serializable]
    public class Game
    {
        [JsonIgnore()] public string m_Path { get; set; }

        // serialized variables
        public string Name { get; set; }
        public ConcurrentDictionary<string, GameSettings> Settings { get; set; } = new ConcurrentDictionary<string, GameSettings>(StringComparer.InvariantCultureIgnoreCase);
        public List<string> Ignore { get; set; } = new();

        [JsonIgnore()] private ConcurrentDictionary<string, GameSettings> m_CreateQueue = new ConcurrentDictionary<string, GameSettings>(StringComparer.InvariantCultureIgnoreCase);
        [JsonIgnore()] private Timer m_CreateQueueTimer;

        [JsonIgnore()] private ConcurrentDictionary<string, GameSettings> m_DeleteQueue = new ConcurrentDictionary<string, GameSettings>(StringComparer.InvariantCultureIgnoreCase);
        [JsonIgnore()] private Timer m_DeleteQueueTimer;

        [JsonIgnore()] private Timer m_SerializeTimer;

        public Game()
        { }

        public void Initialize()
        {
            // monitors settings queue(s)
            m_CreateQueueTimer = new Timer(500) { Enabled = false, AutoReset = true };
            m_CreateQueueTimer.Elapsed += BrowseCreatedQueue;

            m_DeleteQueueTimer = new Timer(500) { Enabled = false, AutoReset = true };
            m_DeleteQueueTimer.Elapsed += BrowseDeletedQueue;

            m_SerializeTimer = new Timer(500) { Enabled = false, AutoReset = false };
            m_SerializeTimer.Elapsed += Serialize;

            foreach (KeyValuePair<string, GameSettings> pair in Settings)
            {
                GameSettings setting = pair.Value;
                setting.Initialize(this);
            }
        }

        private void Serialize(object sender, ElapsedEventArgs e)
        {
            if (!m_DeleteQueue.IsEmpty || !m_CreateQueue.IsEmpty)
                return;

            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(this, options);

            // write to disk
            string target_folder = System.IO.Path.GetDirectoryName(this.m_Path);

            if (!Directory.Exists(target_folder))
                Directory.CreateDirectory(target_folder);

            File.WriteAllText(this.m_Path, jsonString);
        }

        private void BrowseDeletedQueue(object sender, ElapsedEventArgs e)
        {
            if (m_DeleteQueue.IsEmpty)
                return;

            bool changed = false;

            foreach (var pair in m_DeleteQueue)
            {
                GameSettings setting = pair.Value;

                GameSettings result;
                bool removed = Settings.ContainsKey(setting.fileName) ? Settings.TryRemove(setting.fileName, out result) : true;
                if (removed)
                    changed = m_DeleteQueue.TryRemove(setting.fileName, out result);
            }

            if (changed)
            {
                m_SerializeTimer.Stop();
                m_SerializeTimer.Start();
            }
        }

        private void BrowseCreatedQueue(object sender, ElapsedEventArgs e)
        {
            if (m_CreateQueue.IsEmpty)
                return;

            bool changed = false;

            foreach (var pair in m_CreateQueue)
            {
                GameSettings setting = pair.Value;

                setting.Initialize(this);
                bool symlinked = setting.status == GameSettingsCode.ValidSymlink ? true : setting.SetSymlink();

                if (symlinked)
                {
                    bool stored = Settings.ContainsKey(setting.fileName) ? true : Settings.TryAdd(setting.fileName, setting);
                    if (stored)
                    {
                        GameSettings result;
                        changed = m_CreateQueue.TryRemove(setting.fileName, out result);
                    }
                }
            }

            if (changed)
            {
                m_SerializeTimer.Stop();
                m_SerializeTimer.Start();
            }
        }

        public void EnqueueCreate(GameSettings setting)
        {
            if (!m_CreateQueue.ContainsKey(setting.fileName))
                m_CreateQueue.TryAdd(setting.fileName, setting);

            m_CreateQueueTimer.Stop();
            m_CreateQueueTimer.Start();
        }

        public void EnqueueDelete(GameSettings setting)
        {
            if (!m_DeleteQueue.ContainsKey(setting.fileName))
                m_DeleteQueue.TryAdd(setting.fileName, setting);

            m_DeleteQueueTimer.Stop();
            m_DeleteQueueTimer.Start();
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
