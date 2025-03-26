namespace MyBookingsWebApi.Services
{
    public interface ICsvUploadService
    {
        Task UploadMembersAsync(Stream csvStream);
        Task UploadInventoryAsync(Stream csvStream);
    }

}
