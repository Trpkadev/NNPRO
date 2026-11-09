import type { MaybeRefOrGetter } from 'vue'
import { questionsService } from '@/services/questions.service'
import { queryKeys } from './queryKeys'
import type { CreateQuestionRequestDto, UpdateQuestionRequestDto } from '@/types/api'

export function useQuestionDetailQuery(
  questionId: MaybeRefOrGetter<number>,
  enabled: MaybeRefOrGetter<boolean> = true,
) {
  return useQuery({
    queryKey: computed(() => queryKeys.questions.detail(toValue(questionId))),
    queryFn: () => questionsService.getById(toValue(questionId)),
    enabled: computed(() => toValue(enabled) && Number(toValue(questionId)) > 0),
  })
}

export function useCreateQuestionMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (payload: CreateQuestionRequestDto) => questionsService.create(payload),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.questions.all })
      queryClient.invalidateQueries({ queryKey: queryKeys.quizzes.detail(variables.quizId) })
      queryClient.invalidateQueries({ queryKey: queryKeys.quizzes.editor(variables.quizId) })
    },
  })
}

export function useUpdateQuestionMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({
      questionId,
      payload,
    }: {
      questionId: number
      payload: UpdateQuestionRequestDto
    }) => questionsService.update(questionId, payload),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({
        queryKey: queryKeys.questions.detail(variables.questionId),
      })
      queryClient.invalidateQueries({
        queryKey: queryKeys.quizzes.detail(variables.payload.quizId),
      })
      queryClient.invalidateQueries({
        queryKey: queryKeys.quizzes.editor(variables.payload.quizId),
      })
    },
  })
}

export function useDeleteQuestionMutation() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (questionId: number) => questionsService.remove(questionId),
    onSuccess: (_data, questionId) => {
      queryClient.removeQueries({ queryKey: queryKeys.questions.detail(questionId) })
      queryClient.invalidateQueries({ queryKey: queryKeys.questions.all })
      queryClient.invalidateQueries({ queryKey: queryKeys.quizzes.all })
    },
  })
}

