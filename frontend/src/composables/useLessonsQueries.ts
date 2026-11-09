import type { MaybeRefOrGetter } from 'vue'
import { lessonsService } from '@/services/lessons.service'
import { queryKeys } from './queryKeys'
import type {
  AttachmentRequestDto,
  CreateLessonRequestDto,
  UpdateLessonRequestDto,
  UpdateProgressDto,
} from '@/types/api'

export function useLessonDetailQuery(
  lessonId: MaybeRefOrGetter<number>,
  enabled: MaybeRefOrGetter<boolean> = true,
) {
  return useQuery({
    queryKey: computed(() => queryKeys.lessons.detail(toValue(lessonId))),
    queryFn: () => lessonsService.getById(toValue(lessonId)),
    enabled: computed(() => toValue(enabled) && Number(toValue(lessonId)) > 0),
  })
}

export function useCreateLessonMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: CreateLessonRequestDto) => lessonsService.create(payload),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.lessons.all })
      queryClient.invalidateQueries({ queryKey: queryKeys.sections.detail(variables.sectionId) })
    },
  })
}

export function useUpdateLessonMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ lessonId, payload }: { lessonId: number; payload: UpdateLessonRequestDto }) =>
      lessonsService.update(lessonId, payload),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.lessons.detail(variables.lessonId) })
      queryClient.invalidateQueries({
        queryKey: queryKeys.sections.detail(variables.payload.sectionId),
      })
    },
  })
}

export function useDeleteLessonMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (lessonId: number) => lessonsService.remove(lessonId),
    onSuccess: (_data, lessonId) => {
      queryClient.removeQueries({ queryKey: queryKeys.lessons.detail(lessonId) })
      queryClient.invalidateQueries({ queryKey: queryKeys.lessons.all })
      queryClient.invalidateQueries({ queryKey: queryKeys.sections.all })
    },
  })
}

export function useUpdateLessonProgressMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ lessonId, payload }: { lessonId: number; payload: UpdateProgressDto }) =>
      lessonsService.updateProgress(lessonId, payload),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.lessons.detail(variables.lessonId) })
      queryClient.invalidateQueries({ queryKey: queryKeys.courses.all })
    },
  })
}

export function useAddLessonAttachmentMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: AttachmentRequestDto) => lessonsService.addAttachment(payload),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.lessons.detail(variables.lessonId) })
    },
  })
}

export function useDeleteLessonAttachmentMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ lessonId, attachmentId }: { lessonId: number; attachmentId: number }) =>
      lessonsService.removeAttachment(lessonId, attachmentId),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.lessons.detail(variables.lessonId) })
    },
  })
}

