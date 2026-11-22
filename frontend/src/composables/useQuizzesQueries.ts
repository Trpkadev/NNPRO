import type { MaybeRefOrGetter } from 'vue'
import { quizzesService } from '@/services/quizzes.service'
import { questionsService } from '@/services/questions.service'
import { queryKeys } from './queryKeys'
import type {
  AnswerOptionResponseDto,
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

export function useQuizQuestionsQuery(
  quizId: MaybeRefOrGetter<number>,
  params?: MaybeRefOrGetter<QueryParameters | undefined>,
  enabled: MaybeRefOrGetter<boolean> = true,
) {
  return useQuery({
    queryKey: computed(() => queryKeys.quizzes.questions(toValue(quizId), toValue(params))),
    queryFn: () => quizzesService.getQuestions(toValue(quizId), toValue(params)),
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

export function useMyQuizAttemptsQuery(
  quizId: MaybeRefOrGetter<number>,
  params?: MaybeRefOrGetter<QueryParameters | undefined>,
  enabled: MaybeRefOrGetter<boolean> = true,
) {
  return useQuery({
    queryKey: computed(() => queryKeys.quizzes.myAttempts(toValue(quizId), toValue(params))),
    queryFn: () => quizzesService.getMyAttempts(toValue(quizId), toValue(params)),
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

// Načtení možností odpovědí pro historický pokus
export function useQuizAnswerOptionsQuery(
  quizId: MaybeRefOrGetter<number>,
  questionIds: MaybeRefOrGetter<number[]>,
  enabled: MaybeRefOrGetter<boolean> = true,
) {
  return useQuery({
    queryKey: computed(() =>
      queryKeys.quizzes.answerOptions(toValue(quizId), toValue(questionIds)),
    ),
    enabled: computed(
      () => toValue(enabled) && toValue(quizId) > 0 && toValue(questionIds).length > 0,
    ),
    queryFn: async () => {
      const entries = await Promise.all(
        toValue(questionIds).map(async (questionId) => {
          const options: AnswerOptionResponseDto[] = []

          for (let page = 1; page <= 20; page++) {
            const response = await questionsService.getOptions(questionId, {
              Page: page,
              PageSize: 100,
            })

            options.push(...response.data)

            const pagination = response.metadata?.pagination

            if (!pagination?.hasNextPage) {
              return [questionId, options] as const
            }
          }

          throw new Error('Answer options pagination limit exceeded')
        }),
      )

      return Object.fromEntries(entries) as Record<number, AnswerOptionResponseDto[]>
    },
    staleTime: 60_000,
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
      queryClient.invalidateQueries({
        queryKey: [...queryKeys.quizzes.all, variables.quizId, 'my-attempts'],
      })
    },
  })
}

export function useEndQuizAttemptMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: EndAttemptRequestDto) => quizzesService.endAttempt(payload),
    onSuccess: (result) => {
      queryClient.invalidateQueries({
        queryKey: queryKeys.quizzes.attemptDetail(result.id),
      })
      queryClient.invalidateQueries({
        queryKey: [...queryKeys.quizzes.all, result.quizId, 'attempts'],
      })
      queryClient.invalidateQueries({
        queryKey: [...queryKeys.quizzes.all, result.quizId, 'my-attempts'],
      })
    },
  })
}
