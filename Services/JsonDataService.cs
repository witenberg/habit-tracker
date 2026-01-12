using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using HabitTracker.Models;

namespace HabitTracker.Services
{
    /// <summary>
    /// Implementacja IDataService używająca pliku JSON do przechowywania danych
    /// Obsługuje polimorfizm dla klas dziedziczących po Habit
    /// </summary>
    public class JsonDataService : IDataService
    {
        private readonly string _dataFilePath;
        private readonly JsonSerializerOptions _jsonOptions;

        public JsonDataService(string? dataFilePath = null)
        {
            // Domyślna ścieżka w folderze aplikacji
            _dataFilePath = dataFilePath ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "HabitTracker",
                "data.json"
            );

            // Konfiguracja opcji serializacji JSON z obsługą polimorfizmu
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Converters = { new JsonStringEnumConverter() }
            };

            // Konfiguracja polimorfizmu dla klasy Habit
            _jsonOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver
            {
                Modifiers = { AddPolymorphicTypeInfo }
            };
        }

        /// <summary>
        /// Dodaje informacje o typach pochodnych dla klasy Habit (polimorfizm)
        /// </summary>
        private static void AddPolymorphicTypeInfo(JsonTypeInfo typeInfo)
        {
            if (typeInfo.Type == typeof(Habit))
            {
                typeInfo.PolymorphismOptions = new JsonPolymorphismOptions
                {
                    TypeDiscriminatorPropertyName = "$type",
                    IgnoreUnrecognizedTypeDiscriminators = true,
                    UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
                    DerivedTypes =
                    {
                        new JsonDerivedType(typeof(BooleanHabit), "boolean"),
                        new JsonDerivedType(typeof(QuantitativeHabit), "quantitative")
                    }
                };
            }
        }

        public void Save(List<User> users)
        {
            try
            {
                // Utwórz katalog, jeśli nie istnieje
                var directory = Path.GetDirectoryName(_dataFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Serializuj dane do JSON
                var json = JsonSerializer.Serialize(users, _jsonOptions);

                // Zapisz do pliku
                File.WriteAllText(_dataFilePath, json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Błąd podczas zapisywania danych do pliku: {ex.Message}", ex);
            }
        }

        public List<User> Load()
        {
            try
            {
                // Jeśli plik nie istnieje, zwróć pustą listę
                if (!File.Exists(_dataFilePath))
                {
                    return new List<User>();
                }

                // Odczytaj zawartość pliku
                var json = File.ReadAllText(_dataFilePath);

                // Jeśli plik jest pusty, zwróć pustą listę
                if (string.IsNullOrWhiteSpace(json))
                {
                    return new List<User>();
                }

                // Deserializuj dane z JSON
                var users = JsonSerializer.Deserialize<List<User>>(json, _jsonOptions);

                // Zwróć pustą listę zamiast null, jeśli deserializacja zwróciła null
                return users ?? new List<User>();
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Błąd podczas odczytywania danych z pliku JSON: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Błąd podczas odczytywania danych z pliku: {ex.Message}", ex);
            }
        }
    }
}
