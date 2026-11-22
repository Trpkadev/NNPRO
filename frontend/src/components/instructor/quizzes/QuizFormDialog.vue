<script setup lang="ts">
import { getApiErrorMessage } from '@/services/api'
import type { QuizResponseDto } from '@/types/api'

const visible = defineModel<boolean>('visible', { default: false })

const props = defineProps<{
  sectionId: number
  quiz?: QuizResponseDto | null
}>()

const { t } = useI18n()

const createQuizMutation = useCreateQuizMutation()
const updateQuizMutation = useUpdateQuizMutation()

const quizTitle = ref('')
const quizPassingThreshold = ref<number | null>(75)
const quizMaxAttempts = ref<number | null>(null)
const quizTimeLimitHours = ref<number | null>(null)
const quizTimeLimitMinutes = ref<number | null>(null)
const quizTimeLimitSeconds = ref<number | null>(null)
const quizRandomizeQuestions = ref(false)
const quizRandomizeAnswers = ref(false)
const quizError = ref<string | null>(null)

const isSaving = computed(
  () => createQuizMutation.isPending.value || updateQuizMutation.isPending.value,
)

watch(visible, (isOpen) => {
  if (isOpen) {
    quizTitle.value = props.quiz?.title ?? ''
    quizPassingThreshold.value = props.quiz?.passingThresholdPercent ?? 75
    quizMaxAttempts.value = props.quiz?.maxAttempts ?? null

    const seconds = props.quiz?.timeLimitSeconds

    quizTimeLimitHours.value = seconds != null ? Math.floor(seconds / 3600) : null
    quizTimeLimitMinutes.value = seconds != null ? Math.floor((seconds % 3600) / 60) : null
    quizTimeLimitSeconds.value = seconds != null ? seconds % 60 : null

    quizRandomizeQuestions.value = props.quiz?.randomizeQuestions ?? false
    quizRandomizeAnswers.value = props.quiz?.randomizeAnswers ?? false
    quizError.value = null
  }
})

async function handleSaveQuiz() {
  quizError.value = null

  if (!quizTitle.value.trim()) {
    quizError.value = t('validation.required')
    return
  }

  const threshold = quizPassingThreshold.value

  if (threshold === null || !Number.isFinite(threshold) || threshold < 0 || threshold > 100) {
    quizError.value = t('validation.invalidNumberRange', { min: 0, max: 100 })
    return
  }

  if (
    quizMaxAttempts.value !== null &&
    (!Number.isInteger(quizMaxAttempts.value) ||
      quizMaxAttempts.value < 1 ||
      quizMaxAttempts.value > 100)
  ) {
    quizError.value = t('validation.invalidNumberRange', { min: 1, max: 100 })
    return
  }

  const hours = quizTimeLimitHours.value
  const minutes = quizTimeLimitMinutes.value
  const seconds = quizTimeLimitSeconds.value

  if (
    (hours !== null && (!Number.isInteger(hours) || hours < 0 || hours > 24)) ||
    (minutes !== null && (!Number.isInteger(minutes) || minutes < 0 || minutes > 59)) ||
    (seconds !== null && (!Number.isInteger(seconds) || seconds < 0 || seconds > 59))
  ) {
    quizError.value = t('instructor.quizTimeLimitInvalid')
    return
  }

  const timeLimitSeconds =
    hours === null && minutes === null && seconds === null
      ? null
      : (hours ?? 0) * 3600 + (minutes ?? 0) * 60 + (seconds ?? 0)

  if (timeLimitSeconds !== null && (timeLimitSeconds < 1 || timeLimitSeconds > 86400)) {
    quizError.value = t('instructor.quizTimeLimitInvalid')
    return
  }

  const payload = {
    title: quizTitle.value.trim(),
    sectionId: props.sectionId,
    maxAttempts: quizMaxAttempts.value,
    timeLimitSeconds,
    passingThresholdPercent: threshold,
    randomizeQuestions: quizRandomizeQuestions.value,
    randomizeAnswers: quizRandomizeAnswers.value,
  }

  try {
    if (props.quiz) {
      await updateQuizMutation.mutateAsync({
        quizId: props.quiz.id,
        payload,
      })
    } else {
      await createQuizMutation.mutateAsync(payload)
    }

    visible.value = false
  } catch (err) {
    quizError.value = getApiErrorMessage(err)
  }
}
</script>

<template>
  <Dialog
    v-model:visible="visible"
    modal
    :header="quiz ? t('instructor.editQuiz') : t('instructor.addQuiz')"
    :style="{ width: '44rem', maxWidth: '95vw' }"
  >
    <form class="space-y-3 sm:space-y-5 min-w-0" @submit.prevent="handleSaveQuiz">
      <Message v-if="quizError" severity="error" :closable="false">
        {{ quizError }}
      </Message>

      <div class="space-y-2 min-w-0">
        <label for="quiz-title-input" class="block text-sm font-medium">
          {{ t('instructor.quizTitleLabel') }}
        </label>

        <InputText id="quiz-title-input" v-model="quizTitle" class="w-full" required />
      </div>

      <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
        <div class="space-y-2 min-w-0">
          <label for="quiz-threshold-input" class="block text-sm font-medium">
            {{ t('instructor.quizPassingThresholdLabel') }}
          </label>

          <InputNumber
            input-id="quiz-threshold-input"
            v-model="quizPassingThreshold"
            :min="0"
            :max="100"
            class="w-full"
            :input-style="{ width: '100%' }"
          />
        </div>

        <div class="space-y-2 min-w-0">
          <label for="quiz-max-attempts-input" class="block text-sm font-medium">
            {{ t('instructor.quizMaxAttemptsLabel') }}
          </label>

          <InputNumber
            input-id="quiz-max-attempts-input"
            v-model="quizMaxAttempts"
            :min="1"
            :max="100"
            class="w-full"
            :input-style="{ width: '100%' }"
          />
        </div>
      </div>

      <div class="space-y-2">
        <label class="block text-sm font-medium">
          {{ t('instructor.quizTimeLimitLabel') }}
        </label>

        <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
          <div class="space-y-2 min-w-0">
            <label for="quiz-hours-input" class="block text-sm">
              {{ t('instructor.quizTimeLimitHours') }}
            </label>

            <InputNumber
              input-id="quiz-hours-input"
              v-model="quizTimeLimitHours"
              :min="0"
              :max="24"
              :max-fraction-digits="0"
              class="w-full"
              :input-style="{ width: '100%' }"
            />
          </div>

          <div class="space-y-2 min-w-0">
            <label for="quiz-minutes-input" class="block text-sm">
              {{ t('instructor.quizTimeLimitMinutes') }}
            </label>

            <InputNumber
              input-id="quiz-minutes-input"
              v-model="quizTimeLimitMinutes"
              :min="0"
              :max="59"
              :max-fraction-digits="0"
              class="w-full"
              :input-style="{ width: '100%' }"
            />
          </div>

          <div class="space-y-2 min-w-0">
            <label for="quiz-seconds-input" class="block text-sm">
              {{ t('instructor.quizTimeLimitSeconds') }}
            </label>

            <InputNumber
              input-id="quiz-seconds-input"
              v-model="quizTimeLimitSeconds"
              :min="0"
              :max="59"
              :max-fraction-digits="0"
              class="w-full"
              :input-style="{ width: '100%' }"
            />
          </div>
        </div>

        <small class="text-surface-500">
          {{ t('instructor.quizTimeLimitHint') }}
        </small>
      </div>

      <div class="flex flex-wrap gap-4 pt-2">
        <label class="flex items-center gap-2 text-sm cursor-pointer">
          <Checkbox v-model="quizRandomizeQuestions" binary />
          <span>{{ t('instructor.quizRandomizeQuestionsLabel') }}</span>
        </label>

        <label class="flex items-center gap-2 text-sm cursor-pointer">
          <Checkbox v-model="quizRandomizeAnswers" binary />
          <span>{{ t('instructor.quizRandomizeAnswersLabel') }}</span>
        </label>
      </div>

      <div class="flex flex-wrap justify-end gap-2 pt-3">
        <Button
          type="button"
          :label="t('common.cancel')"
          severity="secondary"
          text
          @click="visible = false"
        />

        <Button type="submit" :label="t('common.save')" icon="oi oi-check" :loading="isSaving" />
      </div>
    </form>
  </Dialog>
</template>
