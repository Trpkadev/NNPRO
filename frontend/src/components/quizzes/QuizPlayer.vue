<script setup lang="ts">
import { useToast } from 'openvue/usetoast'
import { getApiErrorMessage } from '@/services/api'
import type {
  AnswerOptionResponseDto,
  QuestionResponseDto,
  QuizAttemptResponseDto,
  QuizResponseDto,
} from '@/types/api'

const props = defineProps<{
  quiz: QuizResponseDto
  attemptId: number
  questions: QuestionResponseDto[]
}>()

const emit = defineEmits<{
  completed: [
    payload: {
      attempt: QuizAttemptResponseDto
      selectedAnswers: Record<number, number[]>
      optionsByQuestion: Record<number, AnswerOptionResponseDto[]>
    },
  ]
  cancel: []
}>()

const { t } = useI18n()
const toast = useToast()
const endAttemptMutation = useEndQuizAttemptMutation()

const selectedAnswers = reactive<Record<number, number[]>>({})
const optionsByQuestion = reactive<Record<number, AnswerOptionResponseDto[]>>({})
const remainingSeconds = ref<number | null>(props.quiz.timeLimitSeconds ?? null)
const errorMessage = ref<string | null>(null)
let timerInterval: ReturnType<typeof setInterval> | null = null

function formatTime(seconds: number): string {
  const mins = Math.floor(seconds / 60)
  const secs = seconds % 60
  return `${String(mins).padStart(2, '0')}:${String(secs).padStart(2, '0')}`
}

function handleOptionsLoaded(questionId: number, options: AnswerOptionResponseDto[]) {
  optionsByQuestion[questionId] = options
}

async function submitAttempt() {
  if (endAttemptMutation.isPending.value) return
  errorMessage.value = null

  const answersPayload = props.questions.map((q) => ({
    questionId: q.id,
    selectedOptionIds: selectedAnswers[q.id] ?? [],
  }))

  try {
    const result = await endAttemptMutation.mutateAsync({
      attemptId: props.attemptId,
      answersJson: JSON.stringify(answersPayload),
    })

    emit('completed', {
      attempt: result,
      selectedAnswers: { ...selectedAnswers },
      optionsByQuestion: { ...optionsByQuestion },
    })
  } catch (err) {
    errorMessage.value = getApiErrorMessage(err)
  }
}

onMounted(() => {
  if (remainingSeconds.value !== null && remainingSeconds.value > 0) {
    timerInterval = setInterval(() => {
      if (remainingSeconds.value !== null) {
        if (remainingSeconds.value <= 1) {
          remainingSeconds.value = 0
          if (timerInterval) clearInterval(timerInterval)
          toast.add({
            severity: 'warn',
            summary: t('quizzes.timeExpiredAutoSubmit'),
            life: 4000,
          })
          submitAttempt()
        } else {
          remainingSeconds.value -= 1
        }
      }
    }, 1000)
  }
})

onBeforeUnmount(() => {
  if (timerInterval) clearInterval(timerInterval)
})
</script>

<template>
  <div class="space-y-6">
    <!-- Horní lišta s časovačem a stavem odpovědí -->
    <div
      class="sticky top-18 z-20 p-4 rounded-2xl bg-surface-0/95 dark:bg-surface-900/95 backdrop-blur border border-surface-200 dark:border-surface-800 flex flex-wrap items-center justify-between gap-4 shadow-sm"
    >
      <div class="space-y-0.5">
        <h2 class="font-bold text-base">{{ quiz.title }}</h2>
        <p class="text-xs text-surface-500">
          {{ t('quizzes.attemptNumber', { id: attemptId }) }} &bull;
          {{ t('quizzes.passingThreshold', { percent: quiz.passingThresholdPercent }) }}
        </p>
      </div>

      <div class="flex items-center gap-3">
        <div
          v-if="remainingSeconds !== null"
          class="px-3.5 py-1.5 rounded-xl font-mono text-sm font-bold flex items-center gap-2"
          :class="
            remainingSeconds <= 30
              ? 'bg-red-500/15 text-red-600 dark:text-red-400 animate-pulse'
              : 'bg-surface-100 dark:bg-surface-800 text-surface-800 dark:text-surface-100'
          "
        >
          <i class="oi oi-clock" />
          <span>{{ formatTime(remainingSeconds) }}</span>
        </div>

        <Button
          :label="t('quizzes.submitQuiz')"
          icon="oi oi-check"
          size="small"
          :loading="endAttemptMutation.isPending.value"
          @click="submitAttempt"
        />
      </div>
    </div>

    <Message v-if="errorMessage" severity="error" :closable="false">
      {{ errorMessage }}
    </Message>

    <!-- Seznam otázek -->
    <div class="space-y-4">
      <QuizQuestionItem
        v-for="(question, idx) in questions"
        :key="question.id"
        :question="question"
        :index="idx"
        :selected-option-ids="selectedAnswers[question.id] ?? []"
        @update:selected-option-ids="selectedAnswers[question.id] = $event"
        @options-loaded="handleOptionsLoaded"
      />
    </div>

    <div class="flex justify-end pt-2">
      <Button
        :label="t('quizzes.submitQuiz')"
        icon="oi oi-check-circle"
        :loading="endAttemptMutation.isPending.value"
        @click="submitAttempt"
      />
    </div>
  </div>
</template>
