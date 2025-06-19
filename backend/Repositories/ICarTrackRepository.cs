namespace SuperBackendNR85IA.Repositories
{
    public interface ICarTrackRepository
    {
        Task<Services.CarTrackData> GetAsync(string carPath, string trackName);
        Task UpdateAsync(Services.CarTrackData data);
    }
}

