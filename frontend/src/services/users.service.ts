import { api, serializeQueryParams } from './api'
import type {
  QueryParameters,
  UpdateUserRequestDto,
  WrapperOfUserCourseResponseDtoList,
  WrapperOfUserLessonProgressResponseDtoList,
} from '@/types/api'

export const usersService = {
  /**
   * Seznam zapsaných kurzů aktuálně přihlášeného uživatele (GET /api/users/me/courses)
   */
  async getMyCourses(params?: QueryParameters): Promise<WrapperOfUserCourseResponseDtoList> {
    const { data } = await api.get<WrapperOfUserCourseResponseDtoList>('/api/users/me/courses', {
      params: serializeQueryParams(params),
    })
    return data
  },

  /**
   * Studijní postup aktuálně přihlášeného uživatele (GET /api/users/me/progress)
   */
  async getMyProgress(
    params?: QueryParameters,
  ): Promise<WrapperOfUserLessonProgressResponseDtoList> {
    const { data } = await api.get<WrapperOfUserLessonProgressResponseDtoList>(
      '/api/users/me/progress',
      {
        params: serializeQueryParams(params),
      },
    )
    return data
  },

  /**
   * Úprava aktuálně přihlášeného uživatele (PUT /api/users)
   */
  async updateCurrent(payload: UpdateUserRequestDto): Promise<void> {
    await api.put('/api/users', payload)
  },

  /**
   * Smazání aktuálně přihlášeného uživatele (DELETE /api/users)
   */
  async removeCurrent(): Promise<void> {
    await api.delete('/api/users')
  },

  /**
   * Úprava uživatele podle ID (PUT /api/users/{userId})
   */
  async update(userId: number, payload: UpdateUserRequestDto): Promise<void> {
    await api.put(`/api/users/${userId}`, payload)
  },

  /**
   * Smazání uživatele podle ID (DELETE /api/users/{userId})
   */
  async remove(userId: number): Promise<void> {
    await api.delete(`/api/users/${userId}`)
  },
}
