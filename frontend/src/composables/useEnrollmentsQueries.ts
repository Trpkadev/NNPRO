import type { MaybeRefOrGetter } from 'vue'
import { enrollmentsService } from '@/services/enrollments.service'
import { queryKeys } from './queryKeys'
import type { EnrollmentRequestDto, ForceEnrollmentRequestDto, QueryParameters } from '@/types/api'

export function useCourseEnrollmentsQuery(
  courseId: MaybeRefOrGetter<number>,
  params?: MaybeRefOrGetter<QueryParameters | undefined>,
  enabled: MaybeRefOrGetter<boolean> = true,
) {
  return useQuery({
    queryKey: computed(() => queryKeys.enrollments.byCourse(toValue(courseId), toValue(params))),
    queryFn: () => enrollmentsService.getByCourse(toValue(courseId), toValue(params)),
    enabled: computed(() => toValue(enabled) && Number(toValue(courseId)) > 0),
  })
}

export function useEnrollCourseMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: EnrollmentRequestDto) => enrollmentsService.enroll(payload),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.enrollments.all })
      queryClient.invalidateQueries({ queryKey: queryKeys.courses.detail(variables.courseId) })
      queryClient.invalidateQueries({ queryKey: queryKeys.courses.lists() })
      queryClient.invalidateQueries({ queryKey: queryKeys.users.all })
    },
  })
}

export function useForceEnrollMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: ForceEnrollmentRequestDto) => enrollmentsService.forceEnroll(payload),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.enrollments.all })
      queryClient.invalidateQueries({ queryKey: queryKeys.admin.userDetail(variables.studentId) })
      queryClient.invalidateQueries({ queryKey: queryKeys.users.all })
    },
  })
}

export function useUnenrollCourseMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (courseId: number) => enrollmentsService.unenroll(courseId),
    onSuccess: (_data, courseId) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.enrollments.all })
      queryClient.invalidateQueries({ queryKey: queryKeys.courses.detail(courseId) })
      queryClient.invalidateQueries({ queryKey: queryKeys.users.all })
    },
  })
}

export function useRemoveStudentEnrollmentMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ courseId, studentId }: { courseId: number; studentId: number }) =>
      enrollmentsService.removeStudent(courseId, studentId),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.enrollments.all })
      queryClient.invalidateQueries({ queryKey: queryKeys.admin.userDetail(variables.studentId) })
      queryClient.invalidateQueries({ queryKey: queryKeys.users.all })
    },
  })
}
