import { api, serializeQueryParams } from './api'
import type {
  CreateUserRequestDto,
  QueryParameters,
  UpdateUserRequestDto,
  UserResponseDto,
  WrapperOfUint,
  WrapperOfUserResponseDto,
  WrapperOfUserResponseDtoList,
} from '@/types/api'

export const usersService = {
  /**
   * Seznam všech uživatelů se stránkováním a vyhledáváním (GET /api/users)
   */
  async getAll(params?: QueryParameters): Promise<WrapperOfUserResponseDtoList> {
    const { data } = await api.get<WrapperOfUserResponseDtoList>('/api/users', {
      params: serializeQueryParams(params),
    })
    return data
  },

  /**
   * Detail uživatele podle ID (GET /api/users/{userId})
   */
  async getById(userId: number): Promise<UserResponseDto> {
    const { data } = await api.get<WrapperOfUserResponseDto>(`/api/users/${userId}`)
    return data.data
  },

  /**
   * Vytvoření uživatele správcem (POST /api/users)
   */
  async create(payload: CreateUserRequestDto): Promise<number> {
    const { data } = await api.post<WrapperOfUint>('/api/users', payload)
    return Number(data.data)
  },

  /**
   * Úprava uživatele správcem (PUT /api/users/{userId})
   */
  async update(userId: number, payload: UpdateUserRequestDto): Promise<void> {
    await api.put(`/api/users/${userId}`, payload)
  },

  /**
   * Smazání uživatele správcem (DELETE /api/users/{userId})
   */
  async remove(userId: number): Promise<void> {
    await api.delete(`/api/users/${userId}`)
  },
}

