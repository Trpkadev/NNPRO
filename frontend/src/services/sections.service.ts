import { api, serializeQueryParams } from './api'
import type {
  CreateSectionRequestDto,
  QueryParameters,
  SectionResponseDto,
  UpdateSectionRequestDto,
  WrapperOfLessonResponseDtoList,
  WrapperOfQuizResponseDtoList,
  WrapperOfSectionResponseDto,
  WrapperOfUint,
} from '@/types/api'

export const sectionsService = {
  /**
   * Detail sekce podle ID (GET /api/sections/{sectionId})
   */
  async getById(sectionId: number): Promise<SectionResponseDto> {
    const { data } = await api.get<WrapperOfSectionResponseDto>(`/api/sections/${sectionId}`)
    return data.data
  },

  /**
   * Seznam lekcí v sekci (GET /api/sections/{sectionId}/lessons)
   */
  async getLessons(
    sectionId: number,
    params?: QueryParameters,
  ): Promise<WrapperOfLessonResponseDtoList> {
    const { data } = await api.get<WrapperOfLessonResponseDtoList>(
      `/api/sections/${sectionId}/lessons`,
      {
        params: serializeQueryParams(params),
      },
    )
    return data
  },

  /**
   * Seznam kvízů v sekci (GET /api/sections/{sectionId}/quizzes)
   */
  async getQuizzes(
    sectionId: number,
    params?: QueryParameters,
  ): Promise<WrapperOfQuizResponseDtoList> {
    const { data } = await api.get<WrapperOfQuizResponseDtoList>(
      `/api/sections/${sectionId}/quizzes`,
      {
        params: serializeQueryParams(params),
      },
    )
    return data
  },

  /**
   * Vytvoření nové sekce v kurzu (POST /api/sections)
   */
  async create(payload: CreateSectionRequestDto): Promise<number> {
    const { data } = await api.post<WrapperOfUint>('/api/sections', payload)
    return Number(data.data)
  },

  /**
   * Úprava sekce (PUT /api/sections/{sectionId})
   */
  async update(sectionId: number, payload: UpdateSectionRequestDto): Promise<void> {
    await api.put(`/api/sections/${sectionId}`, payload)
  },

  /**
   * Smazání sekce (DELETE /api/sections/{sectionId})
   */
  async remove(sectionId: number): Promise<void> {
    await api.delete(`/api/sections/${sectionId}`)
  },
}
