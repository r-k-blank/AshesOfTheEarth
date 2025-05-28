using System;
using System.IO;
using System.Text.Json;
using AshesOfTheEarth.Core.Serialization;
using AshesOfTheEarth.Core.Services; // Pentru ServiceLocator

namespace AshesOfTheEarth.Core
{
    public class SaveLoadManager
    {
        private readonly string _saveDirectory;
        private const string SAVE_FILE_NAME = "savegame.json";
        private JsonSerializerOptions GetJsonSerializerOptions()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true, // Pentru lizibilitate
                Converters = { new Vector2JsonConverter() } // ADAUGĂ CONVERTER-UL AICI
            };
            return options;
        }
        public SaveLoadManager()
        {
            // Creează un folder specific pentru save-uri în AppData/Roaming sau similar
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _saveDirectory = Path.Combine(appDataPath, "AshesOfTheEarth", "Saves"); // Asigură-te că "AshesOfTheEarth" e numele jocului tău

            // Creează directorul dacă nu există
            if (!Directory.Exists(_saveDirectory))
            {
                Directory.CreateDirectory(_saveDirectory);
                System.Diagnostics.Debug.WriteLine($"Created save directory: {_saveDirectory}");
            }
        }

        private string GetSaveFilePath()
        {
            return Path.Combine(_saveDirectory, SAVE_FILE_NAME);
        }

        public bool DoesSaveExist()
        {
            return File.Exists(GetSaveFilePath());
        }

        public void SaveGame(GameStateMemento memento)
        {
            if (memento == null)
            {
                System.Diagnostics.Debug.WriteLine("Save Error: Memento is null.");
                return;
            }

            string filePath = GetSaveFilePath();
            try
            {
                JsonSerializerOptions options = GetJsonSerializerOptions();
                string jsonString = JsonSerializer.Serialize(memento, options);
                File.WriteAllText(filePath, jsonString);
                System.Diagnostics.Debug.WriteLine($"Game saved successfully to {filePath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving game to {filePath}: {ex.Message}");
                // Poate arunca excepția mai departe sau afișează un mesaj utilizatorului
            }
        }

        public GameStateMemento LoadGame()
        {
            string filePath = GetSaveFilePath();
            if (!File.Exists(filePath))
            {
                System.Diagnostics.Debug.WriteLine($"Load Error: Save file not found at {filePath}");
                return null;
            }

            try
            {
                string jsonString = File.ReadAllText(filePath);
                JsonSerializerOptions options = GetJsonSerializerOptions();
                GameStateMemento memento = JsonSerializer.Deserialize<GameStateMemento>(jsonString, options);
                System.Diagnostics.Debug.WriteLine($"Game loaded successfully from {filePath}");
                return memento;
            }
            catch (JsonException jsonEx)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading game from {filePath} (JSON Error): {jsonEx.Message}");
                // Aici ai putea încerca să ștergi/renumești fișierul corupt
                // File.Move(filePath, filePath + ".corrupt");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading game from {filePath}: {ex.Message}");
                return null;
            }
        }

        public void DeleteSave()
        {
            string filePath = GetSaveFilePath();
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                    System.Diagnostics.Debug.WriteLine($"Save file deleted: {filePath}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error deleting save file {filePath}: {ex.Message}");
                }
            }
        }
    }
}