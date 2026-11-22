<script setup lang="ts">
import { getApiErrorMessage } from '@/services/api'
import type { AnswerOptionResponseDto, QuizAttemptResponseDto } from '@/types/api'

const { t } = useI18n()
const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

// Validace ID z URL
function parsePositiveId(value: unknown): number {
  if (typeof value !== 'string' || !/^[1-9]\d*$/.test(value)) return 0

  const id = Number(value)
  return Number.isSafeInteger(id) ? id : 0
}

const courseId = computed(() => parsePositiveId(route.params.courseId))
const quizId = computed(() => parsePositiveId(route.params.quizId))
const validIds = computed(() => courseId.value > 0 && quizId.value > 0)

const {
  data: quiz,
  isLoading: quizLoading,
  isError: quizError,
  error: quizErrorObj,
} = useQuizDetailQuery(quizId, validIds)

const {
  data: questionsResponse,
  isLoading: questionsLoading,
  isError: questionsError,
  error: questionsErrorObj,
} = useQuizQuestionsQuery(quizId, { Page: 1, PageSize: 100 }, validIds)

const attemptsPage = ref(1)
const {
  data: myAttemptsResponse,
  isLoading: attemptsLoading,
  isError: attemptsError,
  error: attemptsErrorObj,
} = useMyQuizAttemptsQuery(
  quizId,
  computed(() => ({
    Page: attemptsPage.value,
    PageSize: 10,
  })),
  validIds,
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

const canStartQuiz = computed(
  () =>
    validIds.value &&
    !!quiz.value &&
    !quizLoading.value &&
    !quizError.value &&
    !questionsLoading.value &&
    !questionsError.value &&
    questions.value.length > 0 &&
    !attemptsLoading.value &&
    !attemptsError.value &&
    !!myAttemptsResponse.value &&
    !hasReachedMaxAttempts.value,
)

const activeAttemptId = ref<number | null>(null)

const lastResult = ref<{
  attempt: QuizAttemptResponseDto
  selectedAnswers: Record<number, number[]>
  optionsByQuestion: Record<number, AnswerOptionResponseDto[]>
} | null>(null)

const startError = ref<string | null>(null)

async function handleStartQuiz() {
  if (!canStartQuiz.value || startAttemptMutation.isPending.value) return

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
        :disabled="courseId === 0"
        @click="router.push({ name: 'course-detail', params: { id: courseId } })"
      />
    </div>

    <!-- Neplatné ID kurzu -->
    <Message v-if="courseId === 0" severity="error" :closable="false">
      {{ t('courses.courseNotFound') }}
    </Message>

    <!-- Neplatné ID kvízu -->
    <Message v-else-if="quizId === 0" severity="error" :closable="false">
      {{ t('quizzes.quizNotFound') }}
    </Message>

    <div v-else-if="quizLoading || questionsLoading" class="space-y-4">
      <Skeleton height="8rem" class="rounded-2xl" />
      <Skeleton height="14rem" class="rounded-2xl" />
    </div>

    <Message v-else-if="quizError" severity="error" :closable="false">
      {{ getApiErrorMessage(quizErrorObj) }}
    </Message>

    <!-- Chyba načítání otázek -->
    <Message v-else-if="questionsError" severity="error" :closable="false">
      {{ getApiErrorMessage(questionsErrorObj) }}
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
              :disabled="!canStartQuiz"
              :loading="startAttemptMutation.isPending.value || attemptsLoading"
              @click="handleStartQuiz"
            />
          </div>
        </div>

        <Message v-if="hasReachedMaxAttempts" severity="warn" :closable="false">
          {{ t('quizzes.maxAttemptsReached') }}
        </Message>

        <!-- Chyba načítání pokusů -->
        <Message v-if="attemptsError" severity="error" :closable="false">
          {{ getApiErrorMessage(attemptsErrorObj) }}
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

    <!-- Kvíz nebyl nalezen -->
    <Message v-else severity="error" :closable="false">
      {{ t('quizzes.quizNotFound') }}
    </Message>
  </div>
</template>
