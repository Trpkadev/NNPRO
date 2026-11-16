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

const createLessonMutation = useCreateLessonMutation()
const updateLessonMutation = useUpdateLessonMutation()
const deleteLessonMutation = useDeleteLessonMutation()

const createQuizMutation = useCreateQuizMutation()
const updateQuizMutation = useUpdateQuizMutation()
const deleteQuizMutation = useDeleteQuizMutation()

const lessons = computed(() =>
  [...(lessonsResponse.value?.data ?? [])].sort((a, b) => a.order - b.order),
)
const quizzes = computed(() => quizzesResponse.value?.data ?? [])

// Stav dialogu pro lekci
const lessonDialogVisible = ref(false)
const editingLesson = ref<LessonResponseDto | null>(null)
const lessonTitle = ref('')
const lessonOrder = ref(1)
const lessonVideoUrl = ref('')
const lessonContent = ref('')
const lessonError = ref<string | null>(null)

// Stav dialogu příloh lekce
const attachmentsDialogVisible = ref(false)
const attachmentsTargetLesson = ref<LessonResponseDto | null>(null)

// Stav dialogu pro kvíz
const quizDialogVisible = ref(false)
const editingQuiz = ref<QuizResponseDto | null>(null)
const quizTitle = ref('')
const quizPassingThreshold = ref(75)
const quizMaxAttempts = ref<number | null>(null)
const quizTimeLimit = ref<number | null>(null)
const quizRandomizeQuestions = ref(false)
const quizRandomizeAnswers = ref(false)
const quizError = ref<string | null>(null)

// Stav dialogu otázek kvízu a pokusů kvízu
const questionsDialogVisible = ref(false)
const questionsTargetQuiz = ref<QuizResponseDto | null>(null)
const attemptsDialogVisible = ref(false)
const attemptsTargetQuiz = ref<QuizResponseDto | null>(null)
const attemptsPage = ref(1)

const { data: quizAttemptsResponse, isLoading: attemptsLoading } = useQuizAttemptsQuery(
  computed(() => attemptsTargetQuiz.value?.id ?? 0),
  computed(() => ({ Page: attemptsPage.value, PageSize: 10 })),
  computed(() => attemptsDialogVisible.value && !!attemptsTargetQuiz.value),
)

function openAddLesson() {
  editingLesson.value = null
  lessonTitle.value = ''
  lessonOrder.value = lessons.value.length + 1
  lessonVideoUrl.value = ''
  lessonContent.value = ''
  lessonError.value = null
  lessonDialogVisible.value = true
}

function openEditLesson(lesson: LessonResponseDto) {
  editingLesson.value = lesson
  lessonTitle.value = lesson.title
  lessonOrder.value = lesson.order
  lessonVideoUrl.value = lesson.videoUrl ?? ''
  lessonContent.value = lesson.content
  lessonError.value = null
  lessonDialogVisible.value = true
}

async function handleSaveLesson() {
  lessonError.value = null
  if (!lessonTitle.value.trim() || !lessonContent.value.trim()) {
    lessonError.value = t('validation.required')
    return
  }

  const payload = {
    title: lessonTitle.value.trim(),
    content: lessonContent.value,
    videoUrl: lessonVideoUrl.value.trim() ? lessonVideoUrl.value.trim() : null,
    order: Math.max(1, Number(lessonOrder.value) || 1),
    sectionId: props.section.id,
  }

  try {
    if (editingLesson.value) {
      await updateLessonMutation.mutateAsync({
        lessonId: editingLesson.value.id,
        payload,
      })
    } else {
      await createLessonMutation.mutateAsync(payload)
    }
    lessonDialogVisible.value = false
  } catch (err) {
    lessonError.value = getApiErrorMessage(err)
  }
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
  quizTitle.value = ''
  quizPassingThreshold.value = 75
  quizMaxAttempts.value = null
  quizTimeLimit.value = null
  quizRandomizeQuestions.value = false
  quizRandomizeAnswers.value = false
  quizError.value = null
  quizDialogVisible.value = true
}

function openEditQuiz(quiz: QuizResponseDto) {
  editingQuiz.value = quiz
  quizTitle.value = quiz.title
  quizPassingThreshold.value = quiz.passingThresholdPercent
  quizMaxAttempts.value = quiz.maxAttempts
  quizTimeLimit.value = quiz.timeLimitSeconds
  quizRandomizeQuestions.value = false
  quizRandomizeAnswers.value = false
  quizError.value = null
  quizDialogVisible.value = true
}

async function handleSaveQuiz() {
  quizError.value = null
  if (!quizTitle.value.trim()) {
    quizError.value = t('validation.required')
    return
  }

  const payload = {
    title: quizTitle.value.trim(),
    sectionId: props.section.id,
    maxAttempts: quizMaxAttempts.value ? Number(quizMaxAttempts.value) : null,
    timeLimitSeconds: quizTimeLimit.value ? Number(quizTimeLimit.value) : null,
    passingThresholdPercent: Number(quizPassingThreshold.value) || 0,
    randomizeQuestions: quizRandomizeQuestions.value,
    randomizeAnswers: quizRandomizeAnswers.value,
  }

  try {
    if (editingQuiz.value) {
      await updateQuizMutation.mutateAsync({
        quizId: editingQuiz.value.id,
        payload,
      })
    } else {
      await createQuizMutation.mutateAsync(payload)
    }
    quizDialogVisible.value = false
  } catch (err) {
    quizError.value = getApiErrorMessage(err)
  }
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
    <div class="flex flex-wrap items-center justify-between gap-3 pb-3 border-b border-surface-100 dark:border-surface-800">
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
          @click="emit('editSection', section)"
        />
        <Button
          icon="oi oi-trash"
          severity="danger"
          text
          rounded
          size="small"
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
              @click="openEditLesson(lesson)"
            />
            <Button
              icon="oi oi-trash"
              severity="danger"
              text
              rounded
              size="small"
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
            <Tag
              :value="`${quiz.passingThresholdPercent} %`"
              severity="warn"
              class="text-xs"
            />
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
                  attemptsPage = 1
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
              @click="openEditQuiz(quiz)"
            />
            <Button
              icon="oi oi-trash"
              severity="danger"
              text
              rounded
              size="small"
              @click="confirmDeleteQuiz(quiz)"
            />
          </div>
        </div>
      </div>
    </div>

    <!-- Dialog pro vytvoření / úpravu lekce -->
    <Dialog
      v-model:visible="lessonDialogVisible"
      modal
      :header="editingLesson ? t('instructor.editLesson') : t('instructor.addLesson')"
      :style="{ width: '38rem', maxWidth: '95vw' }"
    >
      <form class="space-y-4" @submit.prevent="handleSaveLesson">
        <Message v-if="lessonError" severity="error" :closable="false">
          {{ lessonError }}
        </Message>

        <div class="grid grid-cols-1 sm:grid-cols-4 gap-3">
          <div class="sm:col-span-3 space-y-1">
            <label class="block text-sm font-medium">{{ t('instructor.lessonTitleLabel') }}</label>
            <InputText v-model="lessonTitle" class="w-full" required />
          </div>
          <div class="space-y-1">
            <label class="block text-sm font-medium">{{ t('instructor.lessonOrderLabel') }}</label>
            <InputNumber v-model="lessonOrder" :min="1" class="w-full" />
          </div>
        </div>

        <div class="space-y-1">
          <label class="block text-sm font-medium">{{ t('instructor.lessonVideoLabel') }}</label>
          <InputText v-model="lessonVideoUrl" placeholder="https://www.youtube.com/watch?v=..." class="w-full" />
        </div>

        <div class="space-y-1">
          <label class="block text-sm font-medium">{{ t('instructor.lessonContentLabel') }}</label>
          <Textarea v-model="lessonContent" rows="7" class="w-full" required />
        </div>

        <div class="flex justify-end gap-2">
          <Button
            type="button"
            :label="t('common.cancel')"
            severity="secondary"
            text
            @click="lessonDialogVisible = false"
          />
          <Button
            type="submit"
            :label="t('common.save')"
            icon="oi oi-check"
            :loading="createLessonMutation.isPending.value || updateLessonMutation.isPending.value"
          />
        </div>
      </form>
    </Dialog>

    <!-- Dialog příloh lekce -->
    <LessonAttachmentsDialog
      v-model:visible="attachmentsDialogVisible"
      :lesson="attachmentsTargetLesson"
    />

    <!-- Dialog pro vytvoření / úpravu kvízu -->
    <Dialog
      v-model:visible="quizDialogVisible"
      modal
      :header="editingQuiz ? t('instructor.editQuiz') : t('instructor.addQuiz')"
      :style="{ width: '34rem', maxWidth: '95vw' }"
    >
      <form class="space-y-4" @submit.prevent="handleSaveQuiz">
        <Message v-if="quizError" severity="error" :closable="false">
          {{ quizError }}
        </Message>

        <div class="space-y-1">
          <label class="block text-sm font-medium">{{ t('instructor.quizTitleLabel') }}</label>
          <InputText v-model="quizTitle" class="w-full" required />
        </div>

        <div class="grid grid-cols-1 sm:grid-cols-3 gap-3">
          <div class="space-y-1">
            <label class="block text-xs font-medium">{{ t('instructor.quizPassingThresholdLabel') }}</label>
            <InputNumber v-model="quizPassingThreshold" :min="0" :max="100" class="w-full" />
          </div>
          <div class="space-y-1">
            <label class="block text-xs font-medium">{{ t('instructor.quizMaxAttemptsLabel') }}</label>
            <InputNumber v-model="quizMaxAttempts" :min="1" :max="100" class="w-full" />
          </div>
          <div class="space-y-1">
            <label class="block text-xs font-medium">{{ t('instructor.quizTimeLimitLabel') }}</label>
            <InputNumber v-model="quizTimeLimit" :min="10" class="w-full" />
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
            @click="quizDialogVisible = false"
          />
          <Button
            type="submit"
            :label="t('common.save')"
            icon="oi oi-check"
            :loading="createQuizMutation.isPending.value || updateQuizMutation.isPending.value"
          />
        </div>
      </form>
    </Dialog>

    <!-- Dialog otázek kvízu -->
    <QuizQuestionsDialog
      v-model:visible="questionsDialogVisible"
      :quiz="questionsTargetQuiz"
    />

    <!-- Dialog přehledu všech pokusů studentů u kvízu -->
    <Dialog
      v-model:visible="attemptsDialogVisible"
      modal
      :header="t('instructor.attemptsDialogTitle', { title: attemptsTargetQuiz?.title ?? '' })"
      :style="{ width: '44rem', maxWidth: '96vw' }"
    >
      <div class="space-y-4">
        <div v-if="attemptsLoading" class="space-y-2">
          <Skeleton v-for="n in 3" :key="n" height="2.75rem" class="rounded-xl" />
        </div>

        <p
          v-else-if="(quizAttemptsResponse?.data ?? []).length === 0"
          class="text-sm text-surface-500 text-center py-4"
        >
          {{ t('quizzes.noAttemptsYet') }}
        </p>

        <DataTable
          v-else
          :value="quizAttemptsResponse?.data ?? []"
          striped-rows
          responsive-layout="scroll"
        >
          <Column field="id" header="ID" />
          <Column field="studentId" :header="t('instructor.studentIdColumn')" />
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
          :pagination="quizAttemptsResponse?.metadata?.pagination"
          :loading="attemptsLoading"
          @update:page="attemptsPage = $event"
        />
      </div>
    </Dialog>
  </div>
</template>
