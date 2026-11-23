<script setup lang="ts">
import { getApiErrorMessage } from '@/services/api'
import { parsePositiveId } from '@/utils/route'
import type { AnswerOptionResponseDto, QuizAttemptResponseDto } from '@/types/api'
import {
  useQuizAttemptDetailQuery,
  useQuizAnswerOptionsQuery,
} from '@/composables/useQuizzesQueries'

const { t } = useI18n()
const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

const courseId = computed(() => parsePositiveId(route.params.courseId))
const quizId = computed(() => parsePositiveId(route.params.quizId))
const validIds = computed(() => courseId.value > 0 && quizId.value > 0)

// Studentská data načítáme jen pro studenta.
const studentEnabled = computed(() => validIds.value && authStore.isStudent)

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
} = useQuizQuestionsQuery(quizId, { Page: 1, PageSize: 100 }, studentEnabled)

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
  studentEnabled,
)

const startAttemptMutation = useStartQuizAttemptMutation()

const questions = computed(() => questionsResponse.value?.data ?? [])
const myAttempts = computed(() => myAttemptsResponse.value?.data ?? [])
const attemptsPagination = computed(() => myAttemptsResponse.value?.metadata?.pagination ?? null)

const totalMyAttempts = computed(
  () => attemptsPagination.value?.totalItems ?? myAttempts.value.length,
)

const hasReachedMaxAttempts = computed(() => {
  if (!authStore.isStudent || !quiz.value?.maxAttempts) return false
  return totalMyAttempts.value >= quiz.value.maxAttempts
})

const canStartQuiz = computed(
  () =>
    studentEnabled.value &&
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
const viewedAttemptId = ref<number | null>(null)

const {
  data: viewedAttempt,
  isLoading: viewedAttemptLoading,
  isError: viewedAttemptError,
  error: viewedAttemptErrorObj,
} = useQuizAttemptDetailQuery(
  computed(() => viewedAttemptId.value ?? 0),
  computed(() => studentEnabled.value && viewedAttemptId.value !== null),
)

// Texty odpovědí historického pokusu.
const questionIds = computed(() => questions.value.map((question) => question.id))

const {
  data: historicalOptions,
  isLoading: historicalOptionsLoading,
  isError: historicalOptionsError,
  error: historicalOptionsErrorObj,
} = useQuizAnswerOptionsQuery(
  quizId,
  questionIds,
  computed(
    () =>
      studentEnabled.value &&
      viewedAttemptId.value !== null &&
      viewedAttempt.value?.quizId === quizId.value &&
      !!viewedAttempt.value?.submittedAtUtc,
  ),
)

const historicalSelectedAnswers = computed<Record<number, number[]>>(() => {
  const result: Record<number, number[]> = {}

  for (const [questionId, selectedIds] of Object.entries(
    viewedAttempt.value?.answerIdsByQuestionId ?? {},
  )) {
    result[Number(questionId)] = selectedIds ?? []
  }

  return result
})

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
  viewedAttemptId.value = null

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

function handleViewAttempt(attemptId: number) {
  if (!authStore.isStudent || !Number.isSafeInteger(attemptId) || attemptId <= 0) return

  lastResult.value = null
  viewedAttemptId.value = attemptId
}

function handleBackToOverview() {
  viewedAttemptId.value = null
  lastResult.value = null
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

    <Message v-if="courseId === 0" severity="error" :closable="false">
      {{ t('courses.courseNotFound') }}
    </Message>

    <Message v-else-if="quizId === 0" severity="error" :closable="false">
      {{ t('quizzes.quizNotFound') }}
    </Message>

    <div v-else-if="quizLoading || (authStore.isStudent && questionsLoading)" class="space-y-4">
      <Skeleton height="8rem" class="rounded-2xl" />
      <Skeleton height="14rem" class="rounded-2xl" />
    </div>

    <Message v-else-if="quizError" severity="error" :closable="false">
      {{ getApiErrorMessage(quizErrorObj) }}
    </Message>

    <Message v-else-if="authStore.isStudent && questionsError" severity="error" :closable="false">
      {{ getApiErrorMessage(questionsErrorObj) }}
    </Message>

    <template v-else-if="quiz">
      <!-- Aktivní pokus studenta -->
      <QuizPlayer
        v-if="authStore.isStudent && activeAttemptId !== null"
        :quiz="quiz"
        :attempt-id="activeAttemptId"
        :questions="questions"
        @completed="handleAttemptCompleted"
        @cancel="activeAttemptId = null"
      />

      <!-- Výsledek právě dokončeného pokusu -->
      <QuizResultSummary
        v-else-if="authStore.isStudent && lastResult"
        :quiz="quiz"
        :attempt="lastResult.attempt"
        :questions="questions"
        :selected-answers="lastResult.selectedAnswers"
        :options-by-question="lastResult.optionsByQuestion"
        @back="handleBackToOverview"
      />

      <!-- Detail historického pokusu studenta -->
      <div v-else-if="authStore.isStudent && viewedAttemptId !== null" class="space-y-4">
        <div
          v-if="viewedAttemptLoading || (viewedAttempt?.submittedAtUtc && historicalOptionsLoading)"
          class="space-y-3"
        >
          <Skeleton height="8rem" class="rounded-2xl" />
          <Skeleton height="6rem" class="rounded-2xl" />
        </div>

        <Message v-else-if="viewedAttemptError" severity="error" :closable="false">
          {{ getApiErrorMessage(viewedAttemptErrorObj) }}
        </Message>

        <Message v-else-if="historicalOptionsError" severity="error" :closable="false">
          {{ getApiErrorMessage(historicalOptionsErrorObj) }}
        </Message>

        <Message
          v-else-if="!viewedAttempt || viewedAttempt.quizId !== quizId"
          severity="error"
          :closable="false"
        >
          {{ t('quizzes.quizNotFound') }}
        </Message>

        <QuizResultSummary
          v-else-if="viewedAttempt.submittedAtUtc && historicalOptions"
          :quiz="quiz"
          :attempt="viewedAttempt"
          :questions="questions"
          :selected-answers="historicalSelectedAnswers"
          :options-by-question="historicalOptions"
          @back="handleBackToOverview"
        />

        <Message v-else-if="!viewedAttempt.submittedAtUtc" severity="info" :closable="false">
          {{ t('quizzes.inProgress') }}
        </Message>

        <Button
          v-if="
            viewedAttemptError ||
            historicalOptionsError ||
            !viewedAttempt ||
            !viewedAttempt.submittedAtUtc ||
            viewedAttempt.quizId !== quizId
          "
          :label="t('quizzes.backToQuizOverview')"
          icon="oi oi-arrow-left"
          severity="secondary"
          outlined
          @click="handleBackToOverview"
        />
      </div>

      <!-- Přehled kvízu -->
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

        <!-- Studentská část -->
        <template v-if="authStore.isStudent">
          <Message v-if="hasReachedMaxAttempts" severity="warn" :closable="false">
            {{ t('quizzes.maxAttemptsReached') }}
          </Message>

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
            @view-attempt="handleViewAttempt"
          />
        </template>

        <!-- Nestudentská část -->
        <div
          v-else
          class="p-5 rounded-2xl border border-surface-200 dark:border-surface-800 bg-surface-0 dark:bg-surface-900 space-y-3"
        >
          <p class="text-sm text-surface-500">
            {{ t('quizzes.instructorManagementHint') }}
          </p>

          <Button
            :label="t('nav.instructorZone')"
            icon="oi oi-arrow-right"
            severity="secondary"
            outlined
            @click="router.push('/instructor')"
          />
        </div>
      </template>
    </template>

    <Message v-else severity="error" :closable="false">
      {{ t('quizzes.quizNotFound') }}
    </Message>
  </div>
</template>
