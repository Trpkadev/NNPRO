import { api, serializeQueryParams } from './api'
import type {
  AdminUserDetailsDto,
  QueryParameters,
  UpdateUserRoleRequestDto,
  WrapperOfAdminSessionResponseDtoList,
  WrapperOfAdminUserDetailsDto,
  WrapperOfUserResponseDtoList,
} from '@/types/api'

export const adminService = {
  /**
   * Seznam všech uživatelů pro administraci (GET /api/admin/users)
   */
  async getUsers(params?: QueryParameters): Promise<WrapperOfUserResponseDtoList> {
    const { data } = await api.get<WrapperOfUserResponseDtoList>('/api/admin/users', {
      params: serializeQueryParams(params),
    })
    return data
  },

  /**
   * Detail uživatele pro administraci (GET /api/admin/users/{userId})
   */
  async getUserById(userId: number): Promise<AdminUserDetailsDto> {
    const { data } = await api.get<WrapperOfAdminUserDetailsDto>(`/api/admin/users/${userId}`)
    return data.data
  },

  /**
   * Změna role uživatele (PUT /api/admin/users/{userId}/role)
   */
  async updateUserRole(userId: number, payload: UpdateUserRoleRequestDto): Promise<void> {
    await api.put(`/api/admin/users/${userId}/role`, payload)
  },

  /**
   * Seznam aktivních relací uživatele (GET /api/admin/users/{userId}/sessions)
   */
  async getUserSessions(
    userId: number,
    params?: QueryParameters,
  ): Promise<WrapperOfAdminSessionResponseDtoList> {
    const { data } = await api.get<WrapperOfAdminSessionResponseDtoList>(
      `/api/admin/users/${userId}/sessions`,
      {
        params: serializeQueryParams(params),
      },
    )
    return data
  },

  /**
   * Revokace všech relací uživatele (DELETE /api/admin/users/{userId}/sessions)
   */
  async revokeAllUserSessions(userId: number): Promise<void> {
    await api.delete(`/api/admin/users/${userId}/sessions`)
  },

  /**
   * Revokace konkrétní relace uživatele (DELETE /api/admin/users/{userId}/sessions/{sessionId})
   */
  async revokeUserSession(userId: number, sessionId: string): Promise<void> {
    await api.delete(`/api/admin/users/${userId}/sessions/${sessionId}`)
  },
}

