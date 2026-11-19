<script setup lang="ts">
import { getApiErrorMessage } from '@/services/api'
import { QuestionTypes, type QuestionResponseDto, type QuestionType } from '@/types/api'

const props = defineProps<{
  quizId: number
  editingQuestion: QuestionResponseDto | null
}>()

const emit = defineEmits<{
  saved: []
  cancelEdit: []
}>()

const { t } = useI18n()

const createQuestionMutation = useCreateQuestionMutation()
const updateQuestionMutation = useUpdateQuestionMutation()

const text = ref('')
const imageUrl = ref('')
const type = ref<QuestionType>(QuestionTypes.SingleChoice)
const points = ref<number | null>(1)
const errorMessage = ref<string | null>(null)

const questionTypeOptions = computed(() => [
  { label: t('quizzes.questionType.SingleChoice'), value: QuestionTypes.SingleChoice },
  { label: t('quizzes.questionType.MultipleChoice'), value: QuestionTypes.MultipleChoice },
  { label: t('quizzes.questionType.TrueFalse'), value: QuestionTypes.TrueFalse },
])

watch(
  () => props.editingQuestion,
  (q) => {
    text.value = q?.text ?? ''
    imageUrl.value = q?.imageUrl ?? ''
    type.value = q?.type ?? QuestionTypes.SingleChoice
    points.value = q?.points ?? 1
    errorMessage.value = null
  },
  { immediate: true },
)

async function handleSave() {
  errorMessage.value = null
  if (!text.value.trim()) {
    errorMessage.value = t('validation.required')
    return
  }

  if (
    points.value === null ||
    !Number.isInteger(points.value) ||
    points.value < 1 ||
    points.value > 100
  ) {
    errorMessage.value = t('validation.invalidNumberRange', { min: 1, max: 100 })
    return
  }

  const payload = {
    text: text.value.trim(),
    imageUrl: imageUrl.value.trim() ? imageUrl.value.trim() : null,
    type: type.value,
    points: points.value,
    quizId: props.quizId,
  }

  try {
    if (props.editingQuestion) {
      await updateQuestionMutation.mutateAsync({
        questionId: props.editingQuestion.id,
        payload,
      })
    } else {
      await createQuestionMutation.mutateAsync(payload)
    }
    text.value = ''
    imageUrl.value = ''
    type.value = QuestionTypes.SingleChoice
    points.value = 1
    emit('saved')
  } catch (err) {
    errorMessage.value = getApiErrorMessage(err)
  }
}
</script>

<template>
  <form
    class="p-4 rounded-2xl bg-surface-50 dark:bg-surface-800/50 border border-surface-200 dark:border-surface-800 space-y-3"
    @submit.prevent="handleSave"
  >
    <h4 class="text-sm font-bold">
      {{ editingQuestion ? t('instructor.editQuestion') : t('instructor.addQuestion') }}
    </h4>

    <Message v-if="errorMessage" severity="error" :closable="false">
      {{ errorMessage }}
    </Message>

    <div class="space-y-1">
      <label for="question-text-input" class="block text-xs font-medium">
        {{ t('instructor.questionTextLabel') }}
      </label>
      <Textarea id="question-text-input" v-model="text" rows="2" class="w-full" required />
    </div>

    <div class="grid grid-cols-1 sm:grid-cols-3 gap-3">
      <div class="space-y-1">
        <label for="question-type-select" class="block text-xs font-medium">
          {{ t('instructor.questionTypeLabel') }}
        </label>
        <Select
          label-id="question-type-select"
          v-model="type"
          :options="questionTypeOptions"
          option-label="label"
          option-value="value"
          size="small"
          class="w-full"
        />
      </div>

      <div class="space-y-1">
        <label for="question-points-input" class="block text-xs font-medium">
          {{ t('instructor.questionPointsLabel') }}
        </label>
        <InputNumber
          input-id="question-points-input"
          v-model="points"
          :min="1"
          :max="100"
          size="small"
          class="w-full"
        />
      </div>

      <div class="space-y-1">
        <label for="question-image-input" class="block text-xs font-medium">
          {{ t('instructor.questionImageUrlLabel') }}
        </label>
        <InputText id="question-image-input" v-model="imageUrl" size="small" class="w-full" />
      </div>
    </div>

    <div class="flex justify-end gap-2">
      <Button
        v-if="editingQuestion"
        type="button"
        :label="t('common.cancel')"
        severity="secondary"
        text
        size="small"
        @click="emit('cancelEdit')"
      />
      <Button
        type="submit"
        :label="editingQuestion ? t('common.save') : t('instructor.addQuestion')"
        icon="oi oi-check"
        size="small"
        :loading="createQuestionMutation.isPending.value || updateQuestionMutation.isPending.value"
      />
    </div>
  </form>
</template>
