using Pharmacy.WinForms.Models;

namespace Pharmacy.WinForms.Services;

internal sealed class UserService
{
    private readonly ApiClient _apiClient;

    public UserService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<UsersLoadState> LoadUsersAsync(CancellationToken cancellationToken = default)
    {
        _apiClient.EnsureSessionAuthorization();

        var result = await _apiClient.GetAsync<List<UserListItemApiModel>>(
            "api/v1/users",
            "users/list",
            cancellationToken).ConfigureAwait(false);

        if (!result.Success)
        {
            return new UsersLoadState
            {
                Success = false,
                ErrorMessage = result.ErrorMessage ?? "تعذر تحميل المستخدمين.",
                IsConnectionError = result.IsConnectionError
            };
        }

        var users = result.Data?.Select(UserListItemView.FromApi).ToList() ?? [];
        return new UsersLoadState
        {
            Success = true,
            Users = users
        };
    }

    public async Task<UserListItemView?> LoadUserDetailsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        _apiClient.EnsureSessionAuthorization();

        var result = await _apiClient.GetAsync<UserDetailsApiModel>(
            $"api/v1/users/{userId}",
            "users/details",
            cancellationToken).ConfigureAwait(false);

        return result.Success && result.Data is not null
            ? UserListItemView.FromDetails(result.Data)
            : null;
    }

    public async Task<RolesLoadResult> LoadRolesAsync(CancellationToken cancellationToken = default)
    {
        _apiClient.EnsureSessionAuthorization();

        var result = await _apiClient.GetAsync<List<RoleListItemApiModel>>(
            "api/v1/roles",
            "roles/list",
            cancellationToken).ConfigureAwait(false);

        if (!result.Success)
        {
            return new RolesLoadResult
            {
                Success = false,
                ErrorMessage = result.ErrorMessage ?? "تعذر تحميل الأدوار."
            };
        }

        var roles = result.Data?.Select(RoleListItemView.FromApi).ToList() ?? [];
        return new RolesLoadResult
        {
            Success = true,
            Roles = roles
        };
    }

    public async Task<MutateUserResult> CreateUserAsync(
        CreateUserApiRequest request,
        CancellationToken cancellationToken = default)
    {
        _apiClient.EnsureSessionAuthorization();

        var result = await _apiClient.PostAsync<CreateUserApiRequest, UserDetailsApiModel>(
            "api/v1/users",
            request,
            "users/create",
            cancellationToken).ConfigureAwait(false);

        if (!result.Success)
        {
            return new MutateUserResult
            {
                Success = false,
                ErrorMessage = result.ErrorMessage ?? "تعذر إضافة المستخدم.",
                IsConnectionError = result.IsConnectionError
            };
        }

        return new MutateUserResult
        {
            Success = true,
            User = result.Data is not null ? UserListItemView.FromDetails(result.Data) : null
        };
    }

    public async Task<MutateUserResult> UpdateUserAsync(
        Guid userId,
        UpdateUserApiRequest request,
        CancellationToken cancellationToken = default)
    {
        _apiClient.EnsureSessionAuthorization();

        var result = await _apiClient.PutAsync<UpdateUserApiRequest>(
            $"api/v1/users/{userId}",
            request,
            "users/update",
            cancellationToken).ConfigureAwait(false);

        if (!result.Success)
        {
            return new MutateUserResult
            {
                Success = false,
                ErrorMessage = result.ErrorMessage ?? "تعذر تحديث المستخدم.",
                IsConnectionError = result.IsConnectionError
            };
        }

        var refreshed = await LoadUserDetailsAsync(userId, cancellationToken).ConfigureAwait(false);
        return new MutateUserResult
        {
            Success = refreshed is not null,
            User = refreshed,
            ErrorMessage = refreshed is null ? "تم التحديث لكن تعذر تحميل بيانات المستخدم." : null
        };
    }

    public async Task<MutateUserResult> SetUserActiveAsync(
        UserListItemView user,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var details = await LoadUserDetailsAsync(user.Id, cancellationToken).ConfigureAwait(false);
        if (details is null)
        {
            return new MutateUserResult
            {
                Success = false,
                ErrorMessage = "تعذر تحميل بيانات المستخدم قبل التحديث."
            };
        }

        return await UpdateUserAsync(
            user.Id,
            new UpdateUserApiRequest
            {
                FullName = details.DisplayName,
                Email = details.Email,
                Phone = details.Phone,
                RoleId = details.RoleId,
                IsActive = isActive
            },
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<MutateUserResult> DeleteUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        _apiClient.EnsureSessionAuthorization();

        var result = await _apiClient.DeleteAsync(
            $"api/v1/users/{userId}",
            "users/delete",
            cancellationToken).ConfigureAwait(false);

        return new MutateUserResult
        {
            Success = result.Success,
            ErrorMessage = result.ErrorMessage ?? (result.Success ? null : "تعذر حذف المستخدم."),
            IsConnectionError = result.IsConnectionError
        };
    }
}
