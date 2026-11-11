import { api, serializeQueryParams } from './api'
import type {
  CourseCreateRequestDto,
  CourseResponseDto,
  CourseThumbnailQueryParameters,
  CourseUpdateRequestDto,
  QueryParameters,
  WrapperOfCourseInstructorResponseDtoList,
  WrapperOfCourseResponseDto,
  WrapperOfCourseResponseDtoList,
  WrapperOfCourseThumbnailResponseDtoList,
  WrapperOfSectionResponseDtoList,
  WrapperOfUint,
} from '@/types/api'

export const coursesService = {
  /**
   * Seznam všech kurzů s podporou stránkování, řazení a filtrování (GET /api/courses)
   */
  async getAll(params?: QueryParameters): Promise<WrapperOfCourseResponseDtoList> {
    const { data } = await api.get<WrapperOfCourseResponseDtoList>('/api/courses', {
      params: serializeQueryParams(params),
    })
    return data
  },

  /**
   * Seznam náhledů kurzů pro katalog (GET /api/courses/thumbnails)
   */
  async getThumbnails(
    params?: CourseThumbnailQueryParameters,
  ): Promise<WrapperOfCourseThumbnailResponseDtoList> {
    const { data } = await api.get<WrapperOfCourseThumbnailResponseDtoList>(
      '/api/courses/thumbnails',
      {
        params: serializeQueryParams(params),
      },
    )
    return data
  },

  /**
   * Detail kurzu podle ID (GET /api/courses/{id})
   */
  async getById(id: number): Promise<CourseResponseDto> {
    const { data } = await api.get<WrapperOfCourseResponseDto>(`/api/courses/${id}`)
    return data.data
  },

  /**
   * Seznam lektorů přiřazených ke kurzu (GET /api/courses/{courseId}/instructors)
   */
  async getInstructors(
    courseId: number,
    params?: QueryParameters,
  ): Promise<WrapperOfCourseInstructorResponseDtoList> {
    const { data } = await api.get<WrapperOfCourseInstructorResponseDtoList>(
      `/api/courses/${courseId}/instructors`,
      {
        params: serializeQueryParams(params),
      },
    )
    return data
  },

  /**
   * Seznam sekcí kurzu (GET /api/courses/{courseId}/sections)
   */
  async getSections(
    courseId: number,
    params?: QueryParameters,
  ): Promise<WrapperOfSectionResponseDtoList> {
    const { data } = await api.get<WrapperOfSectionResponseDtoList>(
      `/api/courses/${courseId}/sections`,
      {
        params: serializeQueryParams(params),
      },
    )
    return data
  },

  /**
   * Vytvoření nového kurzu (POST /api/courses)
   */
  async create(payload: CourseCreateRequestDto): Promise<number> {
    const { data } = await api.post<WrapperOfUint>('/api/courses', payload)
    return Number(data.data)
  },

  /**
   * Úprava existujícího kurzu (PUT /api/courses/{id})
   */
  async update(id: number, payload: CourseUpdateRequestDto): Promise<void> {
    await api.put(`/api/courses/${id}`, payload)
  },

  /**
   * Smazání kurzu (DELETE /api/courses/{id})
   */
  async remove(id: number): Promise<void> {
    await api.delete(`/api/courses/${id}`)
  },
}
