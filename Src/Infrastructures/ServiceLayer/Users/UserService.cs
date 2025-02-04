using Application.Aggregates.UserAggregate.Commands;
using Application.Aggregates.UserAggregate.Queries;
using Application.Aggregates.UserAuthAggregate;
using Application.Common.Models;
using Application.Repositories;
using Domain.Entities;
using Domain.Enums;

namespace ServiceLayer.Users;

public class UserService : IUserService
{

    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }


    public async Task<CustomResult<Guid>> AddUser(CreateUserRequest createUserRequest)
    {
        var newUser = new User()
        {
            FullName = createUserRequest.FullName,
            UserEmail = createUserRequest.UserEmail,
            UserTypeId = createUserRequest.UserType,
            RegisterTokenExpieryTime = DateTime.UtcNow.AddHours(2)
        };

        var newUserRepo = await _userRepository.AddAsync(newUser);

        if (newUserRepo == null) return CustomResult<Guid>.Failure(new CustomError(false, "Not saved"));

        var newUser1 = await _userRepository.GetByIdAsync(newUserRepo.Id);

        return CustomResult<Guid>.Success(newUser1.RegisterToken);
    }

    public async Task<CustomResult> UpdateUser(int Id, UpdateUserRequest updateUserRequest)
    {
        var currentUser = await _userRepository.GetByIdAsync(Id);

        if (currentUser == null) return CustomResult.Failure("Not found user");

        currentUser.FullName = updateUserRequest.FullName;

        return await _userRepository.UpdateAsync(currentUser);
    }

    public async Task<CustomResult<Guid>> RefreshRegisterToken(int Id)
    {
        var currentUser = await _userRepository.GetByIdAsync(Id);

        if (currentUser == null) return CustomResult<Guid>.Failure(new CustomError(false, "Not found user"));

        var newGuid = Guid.NewGuid();
        currentUser.RegisterTokenExpieryTime = DateTime.UtcNow.AddHours(2);
        currentUser.RegisterToken = newGuid;

        await _userRepository.UpdateAsync(currentUser);

        return CustomResult<Guid>.Success(newGuid);
    }

    public Task<CustomResult<UserLoginResponse>> GetUserByAccessTokenAsync(string accessToken)
    {
        throw new NotImplementedException();
    }

    public async Task<CustomResult<UserDto>> GetUserByAspId(string AspId)
    {
        return await _userRepository.GetUserByAspId(AspId);
    }

    public async Task<CustomResult<UserDto>> GetUserById(int Id)
    {
        return await _userRepository.GetUserById(Id);
    }

    public async Task<IEnumerable<UserDto>> GetUsers(bool IsActive, UserType UserType)
    {
        return await _userRepository.GetUsers(IsActive, UserType);
    }

    public async Task<IEnumerable<SelectListItem>> GetTaskUserSelectList()
    {
        return await _userRepository.GetTaskUserSelectList();
    }


    public async Task<PagingResponse<UserDto>> GetActiveUsersWithPagination(PagingParameters pagingParameters, CancellationToken cancellationToken)
    {
        return await _userRepository.GetActiveUsersWithPagination(pagingParameters, cancellationToken);
    }

    public async Task<bool> UpdateRefreshTokenAsync(int UserId, string refreshToken, DateTime refreshTokenExpiery)
    {
        return await _userRepository.UpdateRefreshTokenAsync(UserId, refreshToken, refreshTokenExpiery);
    }

    public async Task<CustomError> CheckRefreshTokenOfUser(Guid userGuidId, string refreshToken)
    {
        return await _userRepository.CheckRefreshTokenOfUser(userGuidId, refreshToken);
    }

    public async Task<CustomResult<UserDto>> GetUserByUserGuidId(Guid userGuidId)
    {
        return await _userRepository.GetUserByUserGuidId(userGuidId);
    }
}
