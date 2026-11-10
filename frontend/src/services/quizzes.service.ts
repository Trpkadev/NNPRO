import { api, serializeQueryParams } from './api'
import type {
  CreateQuizRequestDto,
  EndAttemptRequestDto,
  QueryParameters,
  QuizAttemptResponseDto,
  QuizResponseDto,
  StartAttemptRequestDto,
  UpdateQuizRequestDto,
  WrapperOfMyQuizAttemptResponseDtoList,
  WrapperOfQuestionResponseDtoList,
  WrapperOfQuizAttemptResponseDto,
  WrapperOfQuizAttemptResponseDtoList,
  WrapperOfQuizResponseDto,
  WrapperOfUint,
} from '@/types/api'

export const quizzesService = {
  /**
   * Detail kvízu podle ID (GET /api/quizzes/{quizId})
   */
  async getById(quizId: number): Promise<QuizResponseDto> {
    const { data } = await api.get<WrapperOfQuizResponseDto>(`/api/quizzes/${quizId}`)
    return data.data
  },

  /**
   * Detail kvízu pro editor (GET /api/quizzes/{quizId}/editor)
   */
  async getEditorData(quizId: number): Promise<QuizResponseDto> {
    const { data } = await api.get<WrapperOfQuizResponseDto>(`/api/quizzes/${quizId}/editor`)
    return data.data
  },

  /**
   * Seznam otázek kvízu (GET /api/quizzes/{quizId}/questions)
   */
  async getQuestions(
    quizId: number,
    params?: QueryParameters,
  ): Promise<WrapperOfQuestionResponseDtoList> {
    const { data } = await api.get<WrapperOfQuestionResponseDtoList>(
      `/api/quizzes/${quizId}/questions`,
      {
        params: serializeQueryParams(params),
      },
    )
    return data
  },

  /**
   * Vytvoření nového kvízu (POST /api/quizzes)
   */
  async create(payload: CreateQuizRequestDto): Promise<number> {
    const { data } = await api.post<WrapperOfUint>('/api/quizzes', payload)
    return Number(data.data)
  },

  /**
   * Úprava kvízu (PUT /api/quizzes/{quizId})
   */
  async update(quizId: number, payload: UpdateQuizRequestDto): Promise<void> {
    await api.put(`/api/quizzes/${quizId}`, payload)
  },

  /**
   * Smazání kvízu (DELETE /api/quizzes/{quizId})
   */
  async remove(quizId: number): Promise<void> {
    await api.delete(`/api/quizzes/${quizId}`)
  },

  /**
   * Seznam všech pokusů u daného kvízu (GET /api/quizzes/{quizId}/attempts)
   */
  async getAttempts(
    quizId: number,
    params?: QueryParameters,
  ): Promise<WrapperOfQuizAttemptResponseDtoList> {
    const { data } = await api.get<WrapperOfQuizAttemptResponseDtoList>(
      `/api/quizzes/${quizId}/attempts`,
      {
        params: serializeQueryParams(params),
      },
    )
    return data
  },

  /**
   * Seznam pokusů přihlášeného uživatele u daného kvízu (GET /api/quizzes/{quizId}/my-attempts)
   */
  async getMyAttempts(
    quizId: number,
    params?: QueryParameters,
  ): Promise<WrapperOfMyQuizAttemptResponseDtoList> {
    const { data } = await api.get<WrapperOfMyQuizAttemptResponseDtoList>(
      `/api/quizzes/${quizId}/my-attempts`,
      {
        params: serializeQueryParams(params),
      },
    )
    return data
  },

  /**
   * Detail konkrétního pokusu o kvíz (GET /api/quizzes/attempt/{attemptId})
   */
  async getAttemptById(attemptId: number): Promise<QuizAttemptResponseDto> {
    const { data } = await api.get<WrapperOfQuizAttemptResponseDto>(
      `/api/quizzes/attempt/${attemptId}`,
    )
    return data.data
  },

  /**
   * Zahájení pokusu o kvíz (POST /api/quizzes/attempt/start)
   */
  async startAttempt(payload: StartAttemptRequestDto): Promise<number> {
    const { data } = await api.post<WrapperOfUint>('/api/quizzes/attempt/start', payload)
    return Number(data.data)
  },

  /**
   * Odevzdání a ukončení pokusu o kvíz (POST /api/quizzes/attempt/end)
   */
  async endAttempt(payload: EndAttemptRequestDto): Promise<QuizAttemptResponseDto> {
    const { data } = await api.post<WrapperOfQuizAttemptResponseDto>(
      '/api/quizzes/attempt/end',
      payload,
    )
    return data.data
  },
}
