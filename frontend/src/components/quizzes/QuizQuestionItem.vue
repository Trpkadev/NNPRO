<script setup lang="ts">
import { sanitizeHtml } from '@/utils/html'
import { QuestionTypes, type AnswerOptionResponseDto, type QuestionResponseDto } from '@/types/api'

const props = defineProps<{
  question: QuestionResponseDto
  index: number
  selectedOptionIds: number[]
}>()

const emit = defineEmits<{
  'update:selectedOptionIds': [optionIds: number[]]
  optionsLoaded: [questionId: number, options: AnswerOptionResponseDto[]]
}>()

const { t } = useI18n()

const { data: optionsResponse, isLoading } = useQuestionOptionsQuery(
  computed(() => props.question.id),
  { Page: 1, PageSize: 50 },
)

const options = computed(() => optionsResponse.value?.data ?? [])

watch(
  options,
  (loaded) => {
    if (loaded.length > 0) {
      emit('optionsLoaded', props.question.id, loaded)
    }
  },
  { immediate: true },
)

const isMultiple = computed(() => props.question.type === QuestionTypes.MultipleChoice)

function toggleOption(optionId: number) {
  if (isMultiple.value) {
    const exists = props.selectedOptionIds.includes(optionId)
    const next = exists
      ? props.selectedOptionIds.filter((id) => id !== optionId)
      : [...props.selectedOptionIds, optionId]
    emit('update:selectedOptionIds', next)
  } else {
    emit('update:selectedOptionIds', [optionId])
  }
}
</script>

<template>
  <div
    class="p-5 rounded-2xl border border-surface-200 dark:border-surface-800 bg-surface-0 dark:bg-surface-900 space-y-4 shadow-xs"
  >
    <!-- Hlavička otázky -->
    <div class="flex flex-wrap items-center justify-between gap-2">
      <div class="flex items-center gap-2">
        <span
          class="w-7 h-7 rounded-lg bg-primary/10 text-primary text-xs font-bold flex items-center justify-center"
        >
          {{ index + 1 }}
        </span>
        <Tag
          :value="t(`quizzes.questionType.${question.type}`)"
          severity="secondary"
          class="text-xs"
        />
      </div>

      <Tag
        :value="t('quizzes.pointsLabel', { points: question.points })"
        severity="info"
        class="text-xs"
      />
    </div>

    <!-- Text otázky (sanitizované HTML) a případný obrázek -->
    <div
      class="prose dark:prose-invert max-w-none text-sm font-medium text-surface-900 dark:text-surface-0"
      v-html="sanitizeHtml(question.text)"
    />

    <div v-if="question.imageUrl" class="rounded-xl overflow-hidden max-w-md border border-surface-200 dark:border-surface-800">
      <img
        :src="question.imageUrl"
        :alt="t('quizzes.questionImageAlt', { number: index + 1 })"
        class="w-full h-auto object-contain max-h-64"
        loading="lazy"
      />
    </div>

    <!-- Možnosti odpovědí -->
    <div v-if="isLoading" class="space-y-2">
      <Skeleton v-for="n in 3" :key="n" height="3rem" class="rounded-xl" />
    </div>

    <div v-else class="space-y-2">
      <button
        v-for="option in options"
        :key="option.id"
        type="button"
        class="w-full p-3.5 rounded-xl border text-left transition-all flex items-start gap-3 cursor-pointer"
        :class="
          selectedOptionIds.includes(option.id)
            ? 'border-primary bg-primary/5 ring-1 ring-primary/40'
            : 'border-surface-200 dark:border-surface-800 hover:border-surface-300 dark:hover:border-surface-700'
        "
        @click="toggleOption(option.id)"
      >
        <div
          class="mt-0.5 w-5 h-5 flex items-center justify-center border shrink-0 transition-colors"
          :class="[
            isMultiple ? 'rounded-md' : 'rounded-full',
            selectedOptionIds.includes(option.id)
              ? 'border-primary bg-primary text-primary-contrast'
              : 'border-surface-300 dark:border-surface-600',
          ]"
        >
          <i v-if="selectedOptionIds.includes(option.id)" class="oi oi-check text-[10px]" />
        </div>

        <div class="flex-1 space-y-2 min-w-0">
          <div
            class="text-sm text-surface-800 dark:text-surface-100"
            v-html="sanitizeHtml(option.text)"
          />
          <img
            v-if="option.imageUrl"
            :src="option.imageUrl"
            :alt="t('quizzes.optionImageAlt')"
            class="max-h-36 rounded-lg border border-surface-200 dark:border-surface-800"
            loading="lazy"
          />
        </div>
      </button>
    </div>
  </div>
</template>
