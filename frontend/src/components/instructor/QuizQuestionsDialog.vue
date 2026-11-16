<script setup lang="ts">
import { getApiErrorMessage } from '@/services/api'
import {
  QuestionTypes,
  type QuestionResponseDto,
  type QuestionType,
  type QuizResponseDto,
} from '@/types/api'

const visible = defineModel<boolean>('visible', { default: false })

const props = defineProps<{
  quiz: QuizResponseDto | null
}>()

const { t } = useI18n()

const quizId = computed(() => props.quiz?.id ?? 0)
const isEnabled = computed(() => visible.value && quizId.value > 0)

const { data: questionsResponse, isLoading } = useQuizQuestionsQuery(
  quizId,
  { Page: 1, PageSize: 50 },
  isEnabled,
)

const createQuestionMutation = useCreateQuestionMutation()
const updateQuestionMutation = useUpdateQuestionMutation()
const deleteQuestionMutation = useDeleteQuestionMutation()

const questions = computed(() => questionsResponse.value?.data ?? [])

const editingQuestionId = ref<number | null>(null)
const deletingQuestionId = ref<number | null>(null)
const text = ref('')
const imageUrl = ref('')
const type = ref<QuestionType>(QuestionTypes.SingleChoice)
const points = ref(1)
const errorMessage = ref<string | null>(null)

const questionTypeOptions = computed(() => [
  { label: t('quizzes.questionType.SingleChoice'), value: QuestionTypes.SingleChoice },
  { label: t('quizzes.questionType.MultipleChoice'), value: QuestionTypes.MultipleChoice },
  { label: t('quizzes.questionType.TrueFalse'), value: QuestionTypes.TrueFalse },
])

function startEditQuestion(q: QuestionResponseDto) {
  editingQuestionId.value = q.id
  text.value = q.text
  imageUrl.value = q.imageUrl ?? ''
  type.value = q.type
  points.value = q.points
}

function resetQuestionForm() {
  editingQuestionId.value = null
  text.value = ''
  imageUrl.value = ''
  type.value = QuestionTypes.SingleChoice
  points.value = 1
  errorMessage.value = null
}

async function handleSaveQuestion() {
  if (!props.quiz) return
  errorMessage.value = null
  if (!text.value.trim()) {
    errorMessage.value = t('validation.required')
    return
  }

  const payload = {
    text: text.value.trim(),
    imageUrl: imageUrl.value.trim() ? imageUrl.value.trim() : null,
    type: type.value,
    points: Math.max(1, Number(points.value) || 1),
    quizId: props.quiz.id,
  }

  try {
    if (editingQuestionId.value) {
      await updateQuestionMutation.mutateAsync({
        questionId: editingQuestionId.value,
        payload,
      })
    } else {
      await createQuestionMutation.mutateAsync(payload)
    }
    resetQuestionForm()
  } catch (err) {
    errorMessage.value = getApiErrorMessage(err)
  }
}

async function handleDeleteQuestion(questionId: number) {
  errorMessage.value = null
  deletingQuestionId.value = questionId
  try {
    await deleteQuestionMutation.mutateAsync(questionId)
  } catch (err) {
    errorMessage.value = getApiErrorMessage(err)
  } finally {
    deletingQuestionId.value = null
  }
}
</script>

<template>
  <Dialog
    v-model:visible="visible"
    modal
    :header="t('instructor.questionsDialogTitle', { title: quiz?.title ?? '' })"
    :style="{ width: '48rem', maxWidth: '96vw' }"
  >
    <div class="space-y-6">
      <Message v-if="errorMessage" severity="error" :closable="false">
        {{ errorMessage }}
      </Message>

      <!-- Formulář pro vytvoření / úpravu otázky -->
      <form
        class="p-4 rounded-2xl bg-surface-50 dark:bg-surface-800/50 border border-surface-200 dark:border-surface-800 space-y-3"
        @submit.prevent="handleSaveQuestion"
      >
        <h4 class="text-sm font-bold">
          {{ editingQuestionId ? t('instructor.editQuestion') : t('instructor.addQuestion') }}
        </h4>

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
            v-if="editingQuestionId"
            type="button"
            :label="t('common.cancel')"
            severity="secondary"
            text
            size="small"
            @click="resetQuestionForm"
          />
          <Button
            type="submit"
            :label="editingQuestionId ? t('common.save') : t('instructor.addQuestion')"
            icon="oi oi-check"
            size="small"
            :loading="
              createQuestionMutation.isPending.value || updateQuestionMutation.isPending.value
            "
          />
        </div>
      </form>

      <!-- Seznam otázek s editorem odpovědí -->
      <div class="space-y-3">
        <div v-if="isLoading" class="space-y-2">
          <Skeleton v-for="n in 2" :key="n" height="6rem" class="rounded-2xl" />
        </div>

        <div
          v-for="(q, idx) in questions"
          :key="q.id"
          class="p-4 rounded-2xl border border-surface-200 dark:border-surface-800 space-y-3"
        >
          <div class="flex items-start justify-between gap-3">
            <div class="space-y-1">
              <div class="flex items-center gap-2">
                <span class="text-xs font-bold text-primary">#{{ idx + 1 }}</span>
                <Tag
                  :value="t(`quizzes.questionType.${q.type}`)"
                  severity="secondary"
                  class="text-xs"
                />
                <Tag
                  :value="t('quizzes.pointsLabel', { points: q.points })"
                  severity="info"
                  class="text-xs"
                />
              </div>
              <p class="text-sm font-semibold">{{ q.text }}</p>
            </div>

            <div class="flex items-center gap-1 shrink-0">
              <Button
                icon="oi oi-pencil"
                severity="secondary"
                text
                rounded
                size="small"
                :aria-label="t('common.edit')"
                @click="startEditQuestion(q)"
              />
              <Button
                icon="oi oi-trash"
                severity="danger"
                text
                rounded
                size="small"
                :aria-label="t('common.delete')"
                :loading="deleteQuestionMutation.isPending.value && deletingQuestionId === q.id"
                @click="handleDeleteQuestion(q.id)"
              />
            </div>
          </div>

          <QuestionOptionsEditorItem :question="q" />
        </div>
      </div>
    </div>
  </Dialog>
</template>
