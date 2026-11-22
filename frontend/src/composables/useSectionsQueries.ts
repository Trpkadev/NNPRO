import type { MaybeRefOrGetter } from 'vue'
import { sectionsService } from '@/services/sections.service'
import { queryKeys } from './queryKeys'
import type { CreateSectionRequestDto, QueryParameters, UpdateSectionRequestDto } from '@/types/api'

export function useSectionDetailQuery(
  sectionId: MaybeRefOrGetter<number>,
  enabled: MaybeRefOrGetter<boolean> = true,
) {
  return useQuery({
    queryKey: computed(() => queryKeys.sections.detail(toValue(sectionId))),
    queryFn: () => sectionsService.getById(toValue(sectionId)),
    enabled: computed(() => toValue(enabled) && Number(toValue(sectionId)) > 0),
  })
}

export function useSectionLessonsQuery(
  sectionId: MaybeRefOrGetter<number>,
  params?: MaybeRefOrGetter<QueryParameters | undefined>,
  enabled: MaybeRefOrGetter<boolean> = true,
) {
  return useQuery({
    queryKey: computed(() => queryKeys.sections.lessons(toValue(sectionId), toValue(params))),
    queryFn: () => sectionsService.getLessons(toValue(sectionId), toValue(params)),
    enabled: computed(() => toValue(enabled) && Number(toValue(sectionId)) > 0),
  })
}

// Všechny lekce sekce pro navigaci Předchozí / Další
export function useAllSectionLessonsQuery(sectionId: MaybeRefOrGetter<number>) {
  return useQuery({
    queryKey: computed(() => queryKeys.sections.allLessons(toValue(sectionId))),
    enabled: computed(() => Number(toValue(sectionId)) > 0),
    staleTime: 60_000,
    queryFn: async () => {
      const allLessons: Awaited<ReturnType<typeof sectionsService.getLessons>>['data'] = []
      const maxPages = 100

      for (let page = 1; page <= maxPages; page++) {
        const response = await sectionsService.getLessons(toValue(sectionId), {
          Page: page,
          PageSize: 50,
          SortBy: 'Order',
          Descending: false,
        })

        const pagination = response.metadata?.pagination
        if (!pagination || typeof pagination.hasNextPage !== 'boolean') {
          throw new Error('Missing lesson pagination metadata')
        }

        allLessons.push(...response.data)

        if (!pagination.hasNextPage) {
          return allLessons
        }

        if (pagination.totalPages != null && page >= pagination.totalPages) {
          throw new Error('Invalid lesson pagination')
        }
      }

      throw new Error('Lesson pagination limit exceeded')
    },
  })
}

export function useSectionQuizzesQuery(
  sectionId: MaybeRefOrGetter<number>,
  params?: MaybeRefOrGetter<QueryParameters | undefined>,
  enabled: MaybeRefOrGetter<boolean> = true,
) {
  return useQuery({
    queryKey: computed(() => queryKeys.sections.quizzes(toValue(sectionId), toValue(params))),
    queryFn: () => sectionsService.getQuizzes(toValue(sectionId), toValue(params)),
    enabled: computed(() => toValue(enabled) && Number(toValue(sectionId)) > 0),
  })
}

export function useCreateSectionMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: CreateSectionRequestDto) => sectionsService.create(payload),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.sections.all })
      queryClient.invalidateQueries({ queryKey: queryKeys.courses.detail(variables.courseId) })
    },
  })
}

export function useUpdateSectionMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ sectionId, payload }: { sectionId: number; payload: UpdateSectionRequestDto }) =>
      sectionsService.update(sectionId, payload),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.sections.detail(variables.sectionId) })
      queryClient.invalidateQueries({
        queryKey: queryKeys.courses.detail(variables.payload.courseId),
      })
    },
  })
}

export function useDeleteSectionMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (sectionId: number) => sectionsService.remove(sectionId),
    onSuccess: (_data, sectionId) => {
      queryClient.removeQueries({ queryKey: queryKeys.sections.detail(sectionId) })
      queryClient.invalidateQueries({ queryKey: queryKeys.sections.all })
      queryClient.invalidateQueries({ queryKey: queryKeys.courses.all })
    },
  })
}
