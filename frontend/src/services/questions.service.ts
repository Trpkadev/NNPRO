import { api, serializeQueryParams } from './api'
import type {
  CreateAnswerOptionRequestDto,
  CreateQuestionRequestDto,
  QueryParameters,
  QuestionResponseDto,
  UpdateAnswerOptionRequestDto,
  UpdateQuestionRequestDto,
  WrapperOfAnswerOptionEditorResponseDtoList,
  WrapperOfAnswerOptionResponseDtoList,
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

  /**
   * Možnosti odpovědí u otázky bez informace o správnosti (GET /api/questions/{questionId}/options)
   */
  async getOptions(
    questionId: number,
    params?: QueryParameters,
  ): Promise<WrapperOfAnswerOptionResponseDtoList> {
    const { data } = await api.get<WrapperOfAnswerOptionResponseDtoList>(
      `/api/questions/${questionId}/options`,
      {
        params: serializeQueryParams(params),
      },
    )
    return data
  },

  /**
   * Možnosti odpovědí u otázky pro editor včetně správnosti (GET /api/questions/{questionId}/options/editor)
   */
  async getOptionsForEditor(
    questionId: number,
    params?: QueryParameters,
  ): Promise<WrapperOfAnswerOptionEditorResponseDtoList> {
    const { data } = await api.get<WrapperOfAnswerOptionEditorResponseDtoList>(
      `/api/questions/${questionId}/options/editor`,
      {
        params: serializeQueryParams(params),
      },
    )
    return data
  },

  /**
   * Vytvoření možnosti odpovědi u otázky (POST /api/questions/{questionId}/options)
   */
  async createOption(questionId: number, payload: CreateAnswerOptionRequestDto): Promise<number> {
    const { data } = await api.post<WrapperOfUint>(
      `/api/questions/${questionId}/options`,
      payload,
    )
    return Number(data.data)
  },

  /**
   * Úprava možnosti odpovědi u otázky (PUT /api/questions/{questionId}/options/{optionId})
   */
  async updateOption(
    questionId: number,
    optionId: number,
    payload: UpdateAnswerOptionRequestDto,
  ): Promise<void> {
    await api.put(`/api/questions/${questionId}/options/${optionId}`, payload)
  },

  /**
   * Smazání možnosti odpovědi u otázky (DELETE /api/questions/{questionId}/options/{optionId})
   */
  async removeOption(questionId: number, optionId: number): Promise<void> {
    await api.delete(`/api/questions/${questionId}/options/${optionId}`)
  },
}
