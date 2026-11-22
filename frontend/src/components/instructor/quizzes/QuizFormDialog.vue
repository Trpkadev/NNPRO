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
const quizTimeLimitMinutes = ref<number | null>(null)
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
    quizTimeLimitMinutes.value =
      props.quiz?.timeLimitSeconds != null ? props.quiz.timeLimitSeconds / 60 : null
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
  if (
    threshold === null ||
    typeof threshold !== 'number' ||
    !Number.isFinite(threshold) ||
    threshold < 0 ||
    threshold > 100
  ) {
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

  const timeLimitSeconds =
    quizTimeLimitMinutes.value !== null ? Math.round(quizTimeLimitMinutes.value * 60) : null

  if (
    timeLimitSeconds !== null &&
    (!Number.isFinite(timeLimitSeconds) || timeLimitSeconds < 1 || timeLimitSeconds > 86400)
  ) {
    quizError.value = t('validation.invalidNumberRange', { min: 1, max: 1440 })
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
    :style="{ width: '34rem', maxWidth: '95vw' }"
  >
    <form class="space-y-4" @submit.prevent="handleSaveQuiz">
      <Message v-if="quizError" severity="error" :closable="false">
        {{ quizError }}
      </Message>

      <div class="space-y-1">
        <label for="quiz-title-input" class="block text-sm font-medium">
          {{ t('instructor.quizTitleLabel') }}
        </label>
        <InputText id="quiz-title-input" v-model="quizTitle" class="w-full" required />
      </div>

      <div class="grid grid-cols-1 sm:grid-cols-3 gap-3">
        <div class="space-y-1">
          <label for="quiz-threshold-input" class="block text-xs font-medium">
            {{ t('instructor.quizPassingThresholdLabel') }}
          </label>
          <InputNumber
            input-id="quiz-threshold-input"
            v-model="quizPassingThreshold"
            :min="0"
            :max="100"
            class="w-full"
          />
        </div>

        <div class="space-y-1">
          <label for="quiz-max-attempts-input" class="block text-xs font-medium">
            {{ t('instructor.quizMaxAttemptsLabel') }}
          </label>
          <InputNumber
            input-id="quiz-max-attempts-input"
            v-model="quizMaxAttempts"
            :min="1"
            :max="100"
            class="w-full"
          />
        </div>

        <div class="space-y-1">
          <label for="quiz-time-limit-input" class="block text-xs font-medium">
            {{ t('instructor.quizTimeLimitLabel') }} (min)
          </label>
          <InputNumber
            input-id="quiz-time-limit-input"
            v-model="quizTimeLimitMinutes"
            :min="0"
            :max="1440"
            :min-fraction-digits="0"
            :max-fraction-digits="4"
            class="w-full"
          />
        </div>
      </div>

      <div class="flex flex-wrap gap-6 pt-1">
        <label class="flex items-center gap-2 text-sm cursor-pointer">
          <Checkbox v-model="quizRandomizeQuestions" binary />
          <span>{{ t('instructor.quizRandomizeQuestionsLabel') }}</span>
        </label>
        <label class="flex items-center gap-2 text-sm cursor-pointer">
          <Checkbox v-model="quizRandomizeAnswers" binary />
          <span>{{ t('instructor.quizRandomizeAnswersLabel') }}</span>
        </label>
      </div>

      <div class="flex justify-end gap-2 pt-2">
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
