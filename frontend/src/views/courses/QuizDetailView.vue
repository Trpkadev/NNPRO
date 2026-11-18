<script setup lang="ts">
import { getApiErrorMessage } from '@/services/api'
import type { AnswerOptionResponseDto, QuizAttemptResponseDto } from '@/types/api'

const { t } = useI18n()
const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

const courseId = computed(() => Number(route.params.courseId))
const quizId = computed(() => Number(route.params.quizId))

const {
  data: quiz,
  isLoading: quizLoading,
  isError: quizError,
  error: quizErrorObj,
} = useQuizDetailQuery(quizId)

const { data: questionsResponse, isLoading: questionsLoading } = useQuizQuestionsQuery(quizId, {
  Page: 1,
  PageSize: 100,
})

const attemptsPage = ref(1)
const { data: myAttemptsResponse, isLoading: attemptsLoading } = useMyQuizAttemptsQuery(
  quizId,
  computed(() => ({
    Page: attemptsPage.value,
    PageSize: 10,
  })),
)

const startAttemptMutation = useStartQuizAttemptMutation()

const questions = computed(() => questionsResponse.value?.data ?? [])
const myAttempts = computed(() => myAttemptsResponse.value?.data ?? [])
const attemptsPagination = computed(() => myAttemptsResponse.value?.metadata?.pagination ?? null)
const totalMyAttempts = computed(
  () => attemptsPagination.value?.totalItems ?? myAttempts.value.length,
)

const hasReachedMaxAttempts = computed(() => {
  if (!quiz.value?.maxAttempts) return false
  return totalMyAttempts.value >= quiz.value.maxAttempts
})

const activeAttemptId = ref<number | null>(null)
const lastResult = ref<{
  attempt: QuizAttemptResponseDto
  selectedAnswers: Record<number, number[]>
  optionsByQuestion: Record<number, AnswerOptionResponseDto[]>
} | null>(null)
const startError = ref<string | null>(null)

async function handleStartQuiz() {
  startError.value = null
  lastResult.value = null
  try {
    const attemptId = await startAttemptMutation.mutateAsync({
      quizId: quizId.value,
    })
    activeAttemptId.value = attemptId
  } catch (err) {
    startError.value = getApiErrorMessage(err)
  }
}

function handleAttemptCompleted(payload: {
  attempt: QuizAttemptResponseDto
  selectedAnswers: Record<number, number[]>
  optionsByQuestion: Record<number, AnswerOptionResponseDto[]>
}) {
  activeAttemptId.value = null
  lastResult.value = payload
}
</script>

<template>
  <div class="space-y-6">
    <div>
      <Button
        :label="t('lessons.backToCourse')"
        icon="oi oi-arrow-left"
        severity="secondary"
        text
        size="small"
        @click="router.push({ name: 'course-detail', params: { id: courseId } })"
      />
    </div>

    <div v-if="quizLoading || questionsLoading" class="space-y-4">
      <Skeleton height="8rem" class="rounded-2xl" />
      <Skeleton height="14rem" class="rounded-2xl" />
    </div>

    <Message v-else-if="quizError" severity="error" :closable="false">
      {{ getApiErrorMessage(quizErrorObj) }}
    </Message>

    <template v-else-if="quiz">
      <QuizPlayer
        v-if="activeAttemptId !== null"
        :quiz="quiz"
        :attempt-id="activeAttemptId"
        :questions="questions"
        @completed="handleAttemptCompleted"
        @cancel="activeAttemptId = null"
      />

      <QuizResultSummary
        v-else-if="lastResult"
        :quiz="quiz"
        :attempt="lastResult.attempt"
        :questions="questions"
        :selected-answers="lastResult.selectedAnswers"
        :options-by-question="lastResult.optionsByQuestion"
        @back="lastResult = null"
      />

      <template v-else>
        <div
          class="p-6 rounded-2xl bg-surface-0 dark:bg-surface-900 border border-surface-200 dark:border-surface-800 flex flex-col sm:flex-row sm:items-center justify-between gap-4 shadow-xs"
        >
          <div class="space-y-2">
            <div class="flex flex-wrap items-center gap-2">
              <Tag
                :value="t('quizzes.passingThreshold', { percent: quiz.passingThresholdPercent })"
                severity="warn"
              />
              <Tag
                :value="
                  quiz.maxAttempts
                    ? t('quizzes.maxAttempts', { count: quiz.maxAttempts })
                    : t('quizzes.unlimitedAttempts')
                "
                severity="secondary"
              />
              <Tag
                :value="
                  quiz.timeLimitSeconds
                    ? t('quizzes.timeLimit', { seconds: quiz.timeLimitSeconds })
                    : t('quizzes.noTimeLimit')
                "
                severity="info"
              />
            </div>

            <h1 class="text-2xl font-bold">{{ quiz.title }}</h1>
          </div>

          <div v-if="authStore.isStudent" class="shrink-0">
            <Button
              :label="t('quizzes.startQuiz')"
              icon="oi oi-play"
              :disabled="hasReachedMaxAttempts || questions.length === 0"
              :loading="startAttemptMutation.isPending.value"
              @click="handleStartQuiz"
            />
          </div>
        </div>

        <Message v-if="hasReachedMaxAttempts" severity="warn" :closable="false">
          {{ t('quizzes.maxAttemptsReached') }}
        </Message>

        <Message v-if="startError" severity="error" :closable="false">
          {{ startError }}
        </Message>

        <QuizAttemptsTable
          :attempts="myAttempts"
          :pagination="attemptsPagination"
          :loading="attemptsLoading"
          @update:page="attemptsPage = $event"
        />
      </template>
    </template>
  </div>
</template>
