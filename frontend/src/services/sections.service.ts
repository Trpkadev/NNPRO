import { api } from './api'
import type {
  CreateSectionRequestDto,
  SectionResponseDto,
  UpdateSectionRequestDto,
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

