<script setup lang="ts">
import { sanitizeHtml } from '@/utils/html'
import type {
  AnswerOptionResponseDto,
  QuestionResponseDto,
  QuizAttemptResponseDto,
  QuizResponseDto,
} from '@/types/api'

const props = defineProps<{
  quiz: QuizResponseDto
  attempt: QuizAttemptResponseDto
  questions: QuestionResponseDto[]
  selectedAnswers: Record<number, number[]>
  optionsByQuestion: Record<number, AnswerOptionResponseDto[]>
}>()

const emit = defineEmits<{
  back: []
}>()

const { t } = useI18n()

function getSelectedOptionTexts(questionId: number): string[] {
  const chosenIds = props.selectedAnswers[questionId] ?? []
  const options = props.optionsByQuestion[questionId] ?? []
  return options.filter((o) => chosenIds.includes(o.id)).map((o) => o.text)
}
</script>

<template>
  <div class="space-y-6">
    <!-- Souhrn výsledku pokusu -->
    <div
      class="p-6 rounded-2xl border flex flex-col sm:flex-row sm:items-center justify-between gap-4"
      :class="
        attempt.isPassed
          ? 'border-emerald-500/40 bg-emerald-500/10'
          : 'border-red-500/40 bg-red-500/10'
      "
    >
      <div class="space-y-1">
        <div class="flex items-center gap-2">
          <Tag
            :value="attempt.isPassed ? t('quizzes.passed') : t('quizzes.failed')"
            :severity="attempt.isPassed ? 'success' : 'danger'"
          />
          <span class="text-xs text-surface-500">
            {{ t('quizzes.attemptNumber', { id: attempt.id }) }}
          </span>
        </div>
        <h2 class="text-xl font-bold">{{ t('quizzes.feedbackTitle') }}</h2>
        <p class="text-sm text-surface-600 dark:text-surface-300">
          {{ t('quizzes.passingThreshold', { percent: quiz.passingThresholdPercent }) }}
        </p>
      </div>

      <div class="text-left sm:text-right">
        <span class="text-xs uppercase tracking-wider text-surface-500 block">
          {{ t('quizzes.score') }}
        </span>
        <span
          class="text-3xl font-extrabold"
          :class="attempt.isPassed ? 'text-emerald-600 dark:text-emerald-400' : 'text-red-600 dark:text-red-400'"
        >
          {{ Math.round(attempt.scorePercent * 10) / 10 }} %
        </span>
      </div>
    </div>

    <!-- Přehled odpovědí studenta u jednotlivých otázek -->
    <div class="space-y-3">
      <div
        v-for="(question, idx) in questions"
        :key="question.id"
        class="p-4 rounded-2xl border border-surface-200 dark:border-surface-800 bg-surface-0 dark:bg-surface-900 space-y-2"
      >
        <div class="flex items-center justify-between gap-2">
          <span class="text-xs font-bold text-primary">#{{ idx + 1 }}</span>
          <Tag
            :value="t('quizzes.pointsLabel', { points: question.points })"
            severity="secondary"
            class="text-xs"
          />
        </div>

        <div
          class="text-sm font-medium"
          v-html="sanitizeHtml(question.text)"
        />

        <div class="pt-2 border-t border-surface-100 dark:border-surface-800 text-xs space-y-1">
          <span class="font-semibold text-surface-500">{{ t('quizzes.yourAnswer') }}:</span>
          <div v-if="getSelectedOptionTexts(question.id).length > 0" class="flex flex-wrap gap-1.5">
            <span
              v-for="(ans, i) in getSelectedOptionTexts(question.id)"
              :key="i"
              class="px-2.5 py-1 rounded-lg bg-surface-100 dark:bg-surface-800 text-surface-800 dark:text-surface-200"
              v-html="sanitizeHtml(ans)"
            />
          </div>
          <p v-else class="italic text-surface-400">
            {{ t('quizzes.noAnswerSelected') }}
          </p>
        </div>
      </div>
    </div>

    <div>
      <Button
        :label="t('quizzes.backToQuizOverview')"
        icon="oi oi-arrow-left"
        severity="secondary"
        outlined
        @click="emit('back')"
      />
    </div>
  </div>
</template>
