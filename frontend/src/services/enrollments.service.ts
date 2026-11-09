import { api, serializeQueryParams } from './api'
import type {
  EnrollmentRequestDto,
  ForceEnrollmentRequestDto,
  QueryParameters,
  WrapperOfEnrollmentResponseDto,
  WrapperOfUint,
} from '@/types/api'

export const enrollmentsService = {
  /**
   * Seznam zapsaných studentů v kurzu (GET /api/enrollments/{courseId})
   */
  async getByCourse(
    courseId: number,
    params?: QueryParameters,
  ): Promise<WrapperOfEnrollmentResponseDto> {
    const { data } = await api.get<WrapperOfEnrollmentResponseDto>(
      `/api/enrollments/${courseId}`,
      {
        params: serializeQueryParams(params),
      },
    )
    return data
  },

  /**
   * Zápis přihlášeného studenta do kurzu (POST /api/enrollments)
   */
  async enroll(payload: EnrollmentRequestDto): Promise<number> {
    const { data } = await api.post<WrapperOfUint>('/api/enrollments', payload)
    return Number(data.data)
  },

  /**
   * Vynucený zápis studenta do kurzu lektorem/správcem (POST /api/enrollments/forceenroll)
   */
  async forceEnroll(payload: ForceEnrollmentRequestDto): Promise<number> {
    const { data } = await api.post<WrapperOfUint>('/api/enrollments/forceenroll', payload)
    return Number(data.data)
  },

  /**
   * Odhlášení přihlášeného studenta z kurzu (DELETE /api/enrollments/{courseId})
   */
  async unenroll(courseId: number): Promise<void> {
    await api.delete(`/api/enrollments/${courseId}`)
  },

  /**
   * Odebrání studenta z kurzu lektorem/správcem (DELETE /api/enrollments/{courseId}/{studentId})
   */
  async removeStudent(courseId: number, studentId: number): Promise<void> {
    await api.delete(`/api/enrollments/${courseId}/${studentId}`)
  },
}

