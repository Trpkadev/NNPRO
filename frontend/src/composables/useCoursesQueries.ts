import type { MaybeRefOrGetter } from 'vue'
import { coursesService } from '@/services/courses.service'
import { queryKeys } from './queryKeys'
import type {
  CourseCreateRequestDto,
  CourseThumbnailQueryParameters,
  CourseUpdateRequestDto,
  QueryParameters,
} from '@/types/api'

export function useCoursesQuery(params?: MaybeRefOrGetter<QueryParameters | undefined>) {
  return useQuery({
    queryKey: computed(() => queryKeys.courses.list(toValue(params))),
    queryFn: () => coursesService.getAll(toValue(params)),
  })
}

export function useCourseThumbnailsQuery(
  params?: MaybeRefOrGetter<CourseThumbnailQueryParameters | undefined>,
) {
  return useQuery({
    queryKey: computed(() => queryKeys.courses.thumbnailList(toValue(params))),
    queryFn: () => coursesService.getThumbnails(toValue(params)),
  })
}

export function useCourseDetailQuery(
  courseId: MaybeRefOrGetter<number>,
  enabled: MaybeRefOrGetter<boolean> = true,
) {
  return useQuery({
    queryKey: computed(() => queryKeys.courses.detail(toValue(courseId))),
    queryFn: () => coursesService.getById(toValue(courseId)),
    enabled: computed(() => toValue(enabled) && Number(toValue(courseId)) > 0),
  })
}

export function useCreateCourseMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: CourseCreateRequestDto) => coursesService.create(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.courses.all })
    },
  })
}

export function useUpdateCourseMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, payload }: { id: number; payload: CourseUpdateRequestDto }) =>
      coursesService.update(id, payload),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.courses.detail(variables.id) })
      queryClient.invalidateQueries({ queryKey: queryKeys.courses.lists() })
      queryClient.invalidateQueries({ queryKey: queryKeys.courses.thumbnails() })
    },
  })
}

export function useDeleteCourseMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: number) => coursesService.remove(id),
    onSuccess: (_data, id) => {
      queryClient.removeQueries({ queryKey: queryKeys.courses.detail(id) })
      queryClient.invalidateQueries({ queryKey: queryKeys.courses.all })
    },
  })
}
