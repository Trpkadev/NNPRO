<script setup lang="ts">
import { getApiErrorMessage } from '@/services/api'
import type { QuestionResponseDto, QuizResponseDto } from '@/types/api'

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

const deleteQuestionMutation = useDeleteQuestionMutation()

const questions = computed(() => questionsResponse.value?.data ?? [])

const editingQuestion = ref<QuestionResponseDto | null>(null)
const deletingQuestionId = ref<number | null>(null)
const errorMessage = ref<string | null>(null)

async function handleDeleteQuestion(questionId: number) {
  errorMessage.value = null
  deletingQuestionId.value = questionId
  try {
    await deleteQuestionMutation.mutateAsync(questionId)
    if (editingQuestion.value?.id === questionId) {
      editingQuestion.value = null
    }
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

      <QuestionFormCard
        v-if="quiz"
        :quiz-id="quiz.id"
        :editing-question="editingQuestion"
        @saved="editingQuestion = null"
        @cancel-edit="editingQuestion = null"
      />

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
                @click="editingQuestion = q"
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
