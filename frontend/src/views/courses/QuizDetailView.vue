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

function formatDateTime(iso: string | null | undefined): string {
  if (!iso) return '-'
  try {
    return new Date(iso).toLocaleString()
  } catch {
    return iso
  }
}
</script>

<template>
  <div class="space-y-6">
    <!-- Návrat na detail kurzu -->
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
      <!-- Aktivní průchod kvízem -->
      <QuizPlayer
        v-if="activeAttemptId !== null"
        :quiz="quiz"
        :attempt-id="activeAttemptId"
        :questions="questions"
        @completed="handleAttemptCompleted"
        @cancel="activeAttemptId = null"
      />

      <!-- Okamžité vyhodnocení a zpětná vazba po odevzdání -->
      <QuizResultSummary
        v-else-if="lastResult"
        :quiz="quiz"
        :attempt="lastResult.attempt"
        :questions="questions"
        :selected-answers="lastResult.selectedAnswers"
        :options-by-question="lastResult.optionsByQuestion"
        @back="lastResult = null"
      />

      <!-- Přehled kvízu + Historie pokusů studenta -->
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

        <!-- Historie pokusů studenta -->
        <section
          class="p-5 rounded-2xl bg-surface-0 dark:bg-surface-900 border border-surface-200 dark:border-surface-800 space-y-4"
        >
          <h2 class="text-lg font-bold">{{ t('quizzes.myAttemptsTitle') }}</h2>

          <div v-if="attemptsLoading" class="space-y-2">
            <Skeleton v-for="n in 3" :key="n" height="3rem" class="rounded-xl" />
          </div>

          <p
            v-else-if="myAttempts.length === 0"
            class="text-sm text-surface-500 text-center py-6"
          >
            {{ t('quizzes.noAttemptsYet') }}
          </p>

          <DataTable v-else :value="myAttempts" striped-rows responsive-layout="scroll">
            <Column field="id" header="#">
              <template #body="{ data }">
                <span class="font-semibold">{{ t('quizzes.attemptNumber', { id: data.id }) }}</span>
              </template>
            </Column>
            <Column :header="t('quizzes.startedAt')">
              <template #body="{ data }">
                {{ formatDateTime(data.startedAtUtc) }}
              </template>
            </Column>
            <Column :header="t('quizzes.submittedAt')">
              <template #body="{ data }">
                {{ data.submittedAtUtc ? formatDateTime(data.submittedAtUtc) : t('quizzes.inProgress') }}
              </template>
            </Column>
            <Column :header="t('quizzes.score')">
              <template #body="{ data }">
                <span class="font-bold">{{ Math.round(data.scorePercent * 10) / 10 }} %</span>
              </template>
            </Column>
            <Column :header="t('common.actions')">
              <template #body="{ data }">
                <Tag
                  :value="data.isPassed ? t('quizzes.passed') : t('quizzes.failed')"
                  :severity="data.isPassed ? 'success' : 'danger'"
                />
              </template>
            </Column>
          </DataTable>

          <PaginationControls
            :pagination="attemptsPagination"
            :loading="attemptsLoading"
            @update:page="attemptsPage = $event"
          />
        </section>
      </template>
    </template>
  </div>
</template>
