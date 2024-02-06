using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class PhotoRepository : IPhotoRepository
{
    private readonly DataContext _dataContext;
    private readonly IMapper _mapper;

    public PhotoRepository(DataContext dataContext, IMapper mapper)
    {
        _dataContext = dataContext;
        _mapper = mapper;
    }
    public async Task<Photo> GetPhotoByIdAsync(int id)
    {
        return await _dataContext.Photos.IgnoreQueryFilters().Where(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task<ICollection<PhotoForApprovalDTO>> GetUnapprovedPhotos()
    {
        return await _dataContext.Photos
            .Where(x => x.IsApproved == false)
            .IgnoreQueryFilters()
            .Select(x => new PhotoForApprovalDTO
            {
                Username = x.AppUser.UserName,
                Url = x.Url,
                PhotoId = x.Id,
                IsApproved = x.IsApproved
            })
            .ToListAsync();
        // .ProjectTo<PhotoForApprovalDTO>(_mapper.ConfigurationProvider)
    }

    public void RemovePhoto(Photo photo)
    {
        _dataContext.Photos.Remove(photo);
    }
}

