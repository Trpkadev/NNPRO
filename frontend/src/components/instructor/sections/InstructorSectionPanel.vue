<script setup lang="ts">
import { useConfirm } from 'openvue/useconfirm'
import { useToast } from 'openvue/usetoast'
import { getApiErrorMessage } from '@/services/api'
import type { LessonResponseDto, QuizResponseDto, SectionResponseDto } from '@/types/api'

const props = defineProps<{
  section: SectionResponseDto
}>()

const emit = defineEmits<{
  editSection: [section: SectionResponseDto]
  deleteSection: [section: SectionResponseDto]
}>()

const { t } = useI18n()
const confirm = useConfirm()
const toast = useToast()

const sectionId = computed(() => props.section.id)

const { data: lessonsResponse, isLoading: lessonsLoading } = useSectionLessonsQuery(sectionId, {
  Page: 1,
  PageSize: 50,
  SortBy: 'Order',
  Descending: false,
})

const { data: quizzesResponse, isLoading: quizzesLoading } = useSectionQuizzesQuery(sectionId, {
  Page: 1,
  PageSize: 50,
})

const deleteLessonMutation = useDeleteLessonMutation()
const deleteQuizMutation = useDeleteQuizMutation()

const lessons = computed(() =>
  [...(lessonsResponse.value?.data ?? [])].sort((a, b) => a.order - b.order),
)
const quizzes = computed(() => quizzesResponse.value?.data ?? [])

// Stavy modálních dialogů
const lessonDialogVisible = ref(false)
const editingLesson = ref<LessonResponseDto | null>(null)

const attachmentsDialogVisible = ref(false)
const attachmentsTargetLesson = ref<LessonResponseDto | null>(null)

const quizDialogVisible = ref(false)
const editingQuiz = ref<QuizResponseDto | null>(null)

const questionsDialogVisible = ref(false)
const questionsTargetQuiz = ref<QuizResponseDto | null>(null)

const attemptsDialogVisible = ref(false)
const attemptsTargetQuiz = ref<QuizResponseDto | null>(null)

function openAddLesson() {
  editingLesson.value = null
  lessonDialogVisible.value = true
}

function openEditLesson(lesson: LessonResponseDto) {
  editingLesson.value = lesson
  lessonDialogVisible.value = true
}

function confirmDeleteLesson(lesson: LessonResponseDto) {
  confirm.require({
    header: t('common.confirmDeleteTitle'),
    message: t('instructor.deleteLessonConfirm', { title: lesson.title }),
    acceptLabel: t('common.delete'),
    rejectLabel: t('common.cancel'),
    acceptProps: { severity: 'danger' },
    accept: async () => {
      try {
        await deleteLessonMutation.mutateAsync(lesson.id)
      } catch (err) {
        toast.add({ severity: 'error', summary: getApiErrorMessage(err), life: 4000 })
      }
    },
  })
}

function openAddQuiz() {
  editingQuiz.value = null
  quizDialogVisible.value = true
}

function openEditQuiz(quiz: QuizResponseDto) {
  editingQuiz.value = quiz
  quizDialogVisible.value = true
}

function confirmDeleteQuiz(quiz: QuizResponseDto) {
  confirm.require({
    header: t('common.confirmDeleteTitle'),
    message: t('instructor.deleteQuizConfirm', { title: quiz.title }),
    acceptLabel: t('common.delete'),
    rejectLabel: t('common.cancel'),
    acceptProps: { severity: 'danger' },
    accept: async () => {
      try {
        await deleteQuizMutation.mutateAsync(quiz.id)
      } catch (err) {
        toast.add({ severity: 'error', summary: getApiErrorMessage(err), life: 4000 })
      }
    },
  })
}
</script>

<template>
  <div
    class="p-5 rounded-2xl border border-surface-200 dark:border-surface-800 bg-surface-0 dark:bg-surface-900 space-y-4"
  >
    <!-- Hlavička sekce -->
    <div
      class="flex flex-wrap items-center justify-between gap-3 pb-3 border-b border-surface-100 dark:border-surface-800"
    >
      <div class="flex items-center gap-3">
        <span
          class="w-8 h-8 rounded-lg bg-primary/10 text-primary text-xs font-bold flex items-center justify-center"
        >
          {{ section.order }}
        </span>
        <h3 class="font-bold text-base">{{ section.title }}</h3>
      </div>

      <div class="flex flex-wrap items-center gap-2">
        <Button
          :label="t('instructor.addLesson')"
          icon="oi oi-plus"
          size="small"
          severity="secondary"
          outlined
          @click="openAddLesson"
        />
        <Button
          :label="t('instructor.addQuiz')"
          icon="oi oi-question-circle"
          size="small"
          severity="warn"
          outlined
          @click="openAddQuiz"
        />
        <Button
          icon="oi oi-pencil"
          severity="secondary"
          text
          rounded
          size="small"
          :aria-label="t('instructor.editSection')"
          @click="emit('editSection', section)"
        />
        <Button
          icon="oi oi-trash"
          severity="danger"
          text
          rounded
          size="small"
          :aria-label="t('common.delete')"
          @click="emit('deleteSection', section)"
        />
      </div>
    </div>

    <!-- Lekce sekce -->
    <div class="space-y-2">
      <p class="text-xs font-semibold uppercase tracking-wider text-surface-400">
        {{ t('lessons.lessons') }}
      </p>

      <div v-if="lessonsLoading" class="space-y-2">
        <Skeleton height="2.75rem" class="rounded-xl" />
      </div>

      <p v-else-if="lessons.length === 0" class="text-xs text-surface-500 italic">
        {{ t('lessons.noLessons') }}
      </p>

      <div v-else class="space-y-2">
        <div
          v-for="lesson in lessons"
          :key="lesson.id"
          class="px-3.5 py-2.5 rounded-xl border border-surface-200 dark:border-surface-800 flex flex-wrap items-center justify-between gap-2"
        >
          <div class="flex items-center gap-2.5 min-w-0">
            <span class="text-xs font-bold text-surface-400">#{{ lesson.order }}</span>
            <span class="text-sm font-medium truncate">{{ lesson.title }}</span>
            <Tag v-if="lesson.videoUrl" value="Video" severity="info" class="text-xs" />
          </div>

          <div class="flex items-center gap-1.5">
            <Button
              :label="t('instructor.manageAttachments')"
              icon="oi oi-paperclip"
              size="small"
              severity="secondary"
              outlined
              @click="
                () => {
                  attachmentsTargetLesson = lesson
                  attachmentsDialogVisible = true
                }
              "
            />
            <Button
              icon="oi oi-pencil"
              severity="secondary"
              text
              rounded
              size="small"
              :aria-label="t('instructor.editLesson')"
              @click="openEditLesson(lesson)"
            />
            <Button
              icon="oi oi-trash"
              severity="danger"
              text
              rounded
              size="small"
              :aria-label="t('common.delete')"
              @click="confirmDeleteLesson(lesson)"
            />
          </div>
        </div>
      </div>
    </div>

    <!-- Kvízy sekce -->
    <div class="space-y-2 pt-2">
      <p class="text-xs font-semibold uppercase tracking-wider text-surface-400">
        {{ t('quizzes.quizzes') }}
      </p>

      <div v-if="quizzesLoading" class="space-y-2">
        <Skeleton height="2.75rem" class="rounded-xl" />
      </div>

      <p v-else-if="quizzes.length === 0" class="text-xs text-surface-500 italic">
        {{ t('quizzes.noQuizzes') }}
      </p>

      <div v-else class="space-y-2">
        <div
          v-for="quiz in quizzes"
          :key="quiz.id"
          class="px-3.5 py-2.5 rounded-xl border border-amber-500/30 bg-amber-500/5 flex flex-wrap items-center justify-between gap-2"
        >
          <div class="flex items-center gap-2.5">
            <i class="oi oi-question-circle text-amber-500" />
            <span class="text-sm font-semibold">{{ quiz.title }}</span>
            <Tag :value="`${quiz.passingThresholdPercent} %`" severity="warn" class="text-xs" />
          </div>

          <div class="flex flex-wrap items-center gap-1.5">
            <Button
              :label="t('instructor.manageQuestions')"
              icon="oi oi-list"
              size="small"
              severity="secondary"
              outlined
              @click="
                () => {
                  questionsTargetQuiz = quiz
                  questionsDialogVisible = true
                }
              "
            />
            <Button
              :label="t('instructor.viewAllAttempts')"
              icon="oi oi-chart-bar"
              size="small"
              severity="secondary"
              outlined
              @click="
                () => {
                  attemptsTargetQuiz = quiz
                  attemptsDialogVisible = true
                }
              "
            />
            <Button
              icon="oi oi-pencil"
              severity="secondary"
              text
              rounded
              size="small"
              :aria-label="t('instructor.editQuiz')"
              @click="openEditQuiz(quiz)"
            />
            <Button
              icon="oi oi-trash"
              severity="danger"
              text
              rounded
              size="small"
              :aria-label="t('common.delete')"
              @click="confirmDeleteQuiz(quiz)"
            />
          </div>
        </div>
      </div>
    </div>

    <!-- Modální dialogy -->
    <LessonFormDialog
      v-model:visible="lessonDialogVisible"
      :section-id="section.id"
      :lesson="editingLesson"
      :default-order="lessons.length + 1"
    />

    <LessonAttachmentsDialog
      v-model:visible="attachmentsDialogVisible"
      :lesson="attachmentsTargetLesson"
    />

    <QuizFormDialog
      v-model:visible="quizDialogVisible"
      :section-id="section.id"
      :quiz="editingQuiz"
    />

    <QuizQuestionsDialog v-model:visible="questionsDialogVisible" :quiz="questionsTargetQuiz" />

    <QuizAttemptsDialog v-model:visible="attemptsDialogVisible" :quiz="attemptsTargetQuiz" />
  </div>
</template>
