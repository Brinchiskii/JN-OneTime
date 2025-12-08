using OneTime.Core.Models;

namespace OneTime.Core.Services.Interfaces
{
    /// <summary>
    /// Handles data operations for projects.
    /// </summary>
    public interface IProjectRepository
    {
        /// <summary>
        /// Gets all the projects from the database.
        /// </summary>
        /// <returns>A collection of all projects.</returns>
        Task<IEnumerable<Project>> GetAll();

        /// <summary>
        /// Retrieves a project by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier for the project.</param>
        /// <returns>The project, which matches the unique identifier.</returns>
        Task<Project> GetById(int id);
    }
}