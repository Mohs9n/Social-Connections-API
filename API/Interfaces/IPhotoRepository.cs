using API.DTOs;
using API.Entities;

namespace API.Interfaces;

public interface IPhotoRepository
{
    Task<ICollection<PhotoForApprovalDTO>> GetUnapprovedPhotos();
    Task<Photo> GetPhotoByIdAsync(int id);
    void RemovePhoto(Photo photo);
}
