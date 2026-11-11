import { api } from './api'
import type {
  CreateQuestionRequestDto,
  QuestionResponseDto,
  UpdateQuestionRequestDto,
  WrapperOfQuestionResponseDto,
  WrapperOfUint,
} from '@/types/api'

export const questionsService = {
  /**
   * Detail otázky podle ID (GET /api/questions/{questionId})
   */
  async getById(questionId: number): Promise<QuestionResponseDto> {
    const { data } = await api.get<WrapperOfQuestionResponseDto>(`/api/questions/${questionId}`)
    return data.data
  },

  /**
   * Vytvoření nové otázky (POST /api/questions)
   */
  async create(payload: CreateQuestionRequestDto): Promise<number> {
    const { data } = await api.post<WrapperOfUint>('/api/questions', payload)
    return Number(data.data)
  },

  /**
   * Úprava otázky (PUT /api/questions/{questionId})
   */
  async update(questionId: number, payload: UpdateQuestionRequestDto): Promise<void> {
    await api.put(`/api/questions/${questionId}`, payload)
  },

  /**
   * Smazání otázky (DELETE /api/questions/{questionId})
   */
  async remove(questionId: number): Promise<void> {
    await api.delete(`/api/questions/${questionId}`)
  },
}
