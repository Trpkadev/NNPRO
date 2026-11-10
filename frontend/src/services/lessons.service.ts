import { api, serializeQueryParams } from './api'
import type {
  AttachmentRequestDto,
  CreateLessonRequestDto,
  LessonResponseDto,
  QueryParameters,
  UpdateLessonRequestDto,
  UpdateProgressDto,
  WrapperOfAttachmentResponseDtoList,
  WrapperOfLessonResponseDto,
  WrapperOfUint,
} from '@/types/api'

export const lessonsService = {
  /**
   * Detail lekce podle ID (GET /api/lessons/{id})
   */
  async getById(id: number): Promise<LessonResponseDto> {
    const { data } = await api.get<WrapperOfLessonResponseDto>(`/api/lessons/${id}`)
    return data.data
  },

  /**
   * Seznam příloh lekce (GET /api/lessons/{lessonId}/attachments)
   */
  async getAttachments(
    lessonId: number,
    params?: QueryParameters,
  ): Promise<WrapperOfAttachmentResponseDtoList> {
    const { data } = await api.get<WrapperOfAttachmentResponseDtoList>(
      `/api/lessons/${lessonId}/attachments`,
      {
        params: serializeQueryParams(params),
      },
    )
    return data
  },

  /**
   * Vytvoření nové lekce (POST /api/lessons)
   */
  async create(payload: CreateLessonRequestDto): Promise<number> {
    const { data } = await api.post<WrapperOfUint>('/api/lessons', payload)
    return Number(data.data)
  },

  /**
   * Úprava existující lekce (PUT /api/lessons/{lessonId})
   */
  async update(lessonId: number, payload: UpdateLessonRequestDto): Promise<void> {
    await api.put(`/api/lessons/${lessonId}`, payload)
  },

  /**
   * Smazání lekce (DELETE /api/lessons/{lessonId})
   */
  async remove(lessonId: number): Promise<void> {
    await api.delete(`/api/lessons/${lessonId}`)
  },

  /**
   * Aktualizace stavu dokončení lekce (PUT /api/lessons/{lessonId}/progress)
   */
  async updateProgress(lessonId: number, payload: UpdateProgressDto): Promise<void> {
    await api.put(`/api/lessons/${lessonId}/progress`, payload)
  },

  /**
   * Přidání přílohy k lekci (POST /api/lessons/attachments)
   */
  async addAttachment(payload: AttachmentRequestDto): Promise<number> {
    const { data } = await api.post<WrapperOfUint>('/api/lessons/attachments', payload)
    return Number(data.data)
  },

  /**
   * Smazání přílohy lekce (DELETE /api/lessons/{lessonId}/attachments/{attachmentId})
   */
  async removeAttachment(lessonId: number, attachmentId: number): Promise<void> {
    await api.delete(`/api/lessons/${lessonId}/attachments/${attachmentId}`)
  },
}
