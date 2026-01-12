using System.Collections.Generic;
using HabitTracker.Models;

namespace HabitTracker.Services
{
    /// <summary>
    /// Interfejs dla serwisu obsługującego zapis i odczyt danych
    /// </summary>
    public interface IDataService
    {
        /// <summary>
        /// Zapisuje listę użytkowników do magazynu danych
        /// </summary>
        /// <param name="users">Lista użytkowników do zapisania</param>
        void Save(List<User> users);

        /// <summary>
        /// Wczytuje listę użytkowników z magazynu danych
        /// </summary>
        /// <returns>Lista użytkowników lub pusta lista, jeśli nie ma danych</returns>
        List<User> Load();
    }
}
