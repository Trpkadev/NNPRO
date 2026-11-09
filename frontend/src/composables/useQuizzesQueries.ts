import type { MaybeRefOrGetter } from 'vue'
import { quizzesService } from '@/services/quizzes.service'
import { queryKeys } from './queryKeys'
import type {
  CreateQuizRequestDto,
  EndAttemptRequestDto,
  QueryParameters,
  StartAttemptRequestDto,
  UpdateQuizRequestDto,
} from '@/types/api'

export function useQuizDetailQuery(
  quizId: MaybeRefOrGetter<number>,
  enabled: MaybeRefOrGetter<boolean> = true,
) {
  return useQuery({
    queryKey: computed(() => queryKeys.quizzes.detail(toValue(quizId))),
    queryFn: () => quizzesService.getById(toValue(quizId)),
    enabled: computed(() => toValue(enabled) && Number(toValue(quizId)) > 0),
  })
}

export function useQuizEditorQuery(
  quizId: MaybeRefOrGetter<number>,
  enabled: MaybeRefOrGetter<boolean> = true,
) {
  return useQuery({
    queryKey: computed(() => queryKeys.quizzes.editor(toValue(quizId))),
    queryFn: () => quizzesService.getEditorData(toValue(quizId)),
    enabled: computed(() => toValue(enabled) && Number(toValue(quizId)) > 0),
  })
}

export function useCreateQuizMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: CreateQuizRequestDto) => quizzesService.create(payload),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.quizzes.all })
      queryClient.invalidateQueries({ queryKey: queryKeys.sections.detail(variables.sectionId) })
    },
  })
}

export function useUpdateQuizMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ quizId, payload }: { quizId: number; payload: UpdateQuizRequestDto }) =>
      quizzesService.update(quizId, payload),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.quizzes.detail(variables.quizId) })
      queryClient.invalidateQueries({ queryKey: queryKeys.quizzes.editor(variables.quizId) })
      queryClient.invalidateQueries({
        queryKey: queryKeys.sections.detail(variables.payload.sectionId),
      })
    },
  })
}

export function useDeleteQuizMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (quizId: number) => quizzesService.remove(quizId),
    onSuccess: (_data, quizId) => {
      queryClient.removeQueries({ queryKey: queryKeys.quizzes.detail(quizId) })
      queryClient.invalidateQueries({ queryKey: queryKeys.quizzes.all })
      queryClient.invalidateQueries({ queryKey: queryKeys.sections.all })
    },
  })
}

export function useQuizAttemptsQuery(
  quizId: MaybeRefOrGetter<number>,
  params?: MaybeRefOrGetter<QueryParameters | undefined>,
  enabled: MaybeRefOrGetter<boolean> = true,
) {
  return useQuery({
    queryKey: computed(() => queryKeys.quizzes.attempts(toValue(quizId), toValue(params))),
    queryFn: () => quizzesService.getAttempts(toValue(quizId), toValue(params)),
    enabled: computed(() => toValue(enabled) && Number(toValue(quizId)) > 0),
  })
}

export function useQuizAttemptDetailQuery(
  attemptId: MaybeRefOrGetter<number>,
  enabled: MaybeRefOrGetter<boolean> = true,
) {
  return useQuery({
    queryKey: computed(() => queryKeys.quizzes.attemptDetail(toValue(attemptId))),
    queryFn: () => quizzesService.getAttemptById(toValue(attemptId)),
    enabled: computed(() => toValue(enabled) && Number(toValue(attemptId)) > 0),
  })
}

export function useStartQuizAttemptMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: StartAttemptRequestDto) => quizzesService.startAttempt(payload),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({
        queryKey: [...queryKeys.quizzes.all, variables.quizId, 'attempts'],
      })
    },
  })
}

export function useEndQuizAttemptMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: EndAttemptRequestDto) => quizzesService.endAttempt(payload),
    onSuccess: (result) => {
      queryClient.setQueryData(queryKeys.quizzes.attemptDetail(result.id), result)
      queryClient.invalidateQueries({
        queryKey: [...queryKeys.quizzes.all, result.quizId, 'attempts'],
      })
    },
  })
}

