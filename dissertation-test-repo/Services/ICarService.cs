using dissertation_test_repo.DTOs;

namespace dissertation_test_repo.Services
{
    public interface ICarService
    {
        Task<IEnumerable<CarResponseDto>> GetAllCarsAsync();
        Task<CarResponseDto?> GetCarByIdAsync(int id);
        Task<CarResponseDto> CreateCarAsync(CarCreateDto carDto);
        Task<CarResponseDto?> UpdateCarAsync(int id, CarUpdateDto carDto);
        Task<bool> DeleteCarAsync(int id);
        Task<IEnumerable<CarResponseDto>> GetCarsByMakeAsync(string make);
    }
}
