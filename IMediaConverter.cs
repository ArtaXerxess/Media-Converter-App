namespace Nana_Format_Factory
{
    public interface IMediaConverter
    {
        Task<string?> ConvertAsync(string inputPath);
    }
}
