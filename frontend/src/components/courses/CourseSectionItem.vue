<script setup lang="ts">
import type { SectionResponseDto } from '@/types/api'

const props = defineProps<{
  section: SectionResponseDto
  courseId: number
  completedLessonIds: Set<number>
  initiallyExpanded?: boolean
}>()

const emit = defineEmits<{
  lessonsLoaded: [sectionId: number, lessonIds: number[]]
}>()

const { t } = useI18n()

const isExpanded = ref(props.initiallyExpanded ?? true)

// Lekce načítáme pro všechny sekce kurzu, aby byl výpočet celkového postupu kurzem vždy přesný
const { data: lessonsResponse, isLoading: lessonsLoading } = useSectionLessonsQuery(
  computed(() => props.section.id),
  { Page: 1, PageSize: 50, SortBy: 'Order', Descending: false },
)

// Kvízy sekce načítáme líně (lazy loading) až při rozbalení sekce
const { data: quizzesResponse, isLoading: quizzesLoading } = useSectionQuizzesQuery(
  computed(() => props.section.id),
  { Page: 1, PageSize: 50 },
  isExpanded,
)

const lessons = computed(() => {
  const list = [...(lessonsResponse.value?.data ?? [])]
  return list.sort((a, b) => a.order - b.order)
})

const completedInSectionCount = computed(
  () => lessons.value.filter((l) => props.completedLessonIds.has(l.id)).length,
)

const quizzes = computed(() => quizzesResponse.value?.data ?? [])

watch(
  [lessonsResponse, lessons],
  ([res, list]) => {
    if (res) {
      emit(
        'lessonsLoaded',
        props.section.id,
        list.map((l) => l.id),
      )
    }
  },
  { immediate: true },
)
</script>

<template>
  <div
    class="rounded-2xl border border-surface-200 dark:border-surface-800 bg-surface-0 dark:bg-surface-900 overflow-hidden"
  >
    <!-- Hlavička sekce -->
    <button
      type="button"
      :aria-expanded="isExpanded"
      class="w-full px-5 py-4 flex items-center justify-between gap-4 text-left hover:bg-surface-50 dark:hover:bg-surface-800/60 transition-colors cursor-pointer"
      @click="isExpanded = !isExpanded"
    >
      <div class="flex items-center gap-3 min-w-0">
        <span
          class="w-7 h-7 rounded-lg bg-primary/10 text-primary text-xs font-bold flex items-center justify-center shrink-0"
        >
          {{ section.order }}
        </span>
        <h3 class="font-semibold text-base text-surface-900 dark:text-surface-0 truncate">
          {{ section.title }}
        </h3>
      </div>

      <div class="flex items-center gap-3 shrink-0">
        <span
          v-if="!lessonsLoading && lessons.length > 0"
          class="text-xs font-medium text-surface-500"
        >
          {{ completedInSectionCount }} / {{ lessons.length }}
        </span>
        <i
          class="oi text-surface-400 transition-transform"
          :class="isExpanded ? 'oi-chevron-up' : 'oi-chevron-down'"
        />
      </div>
    </button>

    <!-- Obsah sekce (lekce + kvízy) -->
    <div
      v-if="isExpanded"
      class="px-5 pb-4 pt-2 border-t border-surface-100 dark:border-surface-800 space-y-4"
    >
      <!-- Lekce -->
      <div class="space-y-2">
        <p class="text-xs font-semibold uppercase tracking-wider text-surface-400">
          {{ t('lessons.lessons') }}
        </p>

        <div v-if="lessonsLoading" class="space-y-2">
          <Skeleton v-for="n in 2" :key="n" height="2.75rem" class="rounded-xl" />
        </div>

        <p
          v-else-if="lessons.length === 0"
          class="text-xs text-surface-500 italic py-1"
        >
          {{ t('lessons.noLessons') }}
        </p>

        <div v-else class="space-y-1.5">
          <RouterLink
            v-for="lesson in lessons"
            :key="lesson.id"
            :to="{
              name: 'lesson-detail',
              params: { courseId, lessonId: lesson.id },
            }"
            class="px-3.5 py-2.5 rounded-xl border border-surface-200/70 dark:border-surface-800 flex items-center justify-between gap-3 hover:border-primary/50 hover:bg-surface-50/70 dark:hover:bg-surface-800/40 transition-colors no-underline text-surface-900 dark:text-surface-0"
          >
            <div class="flex items-center gap-3 min-w-0">
              <i
                class="oi text-base shrink-0"
                :class="
                  completedLessonIds.has(lesson.id)
                    ? 'oi-check-circle text-emerald-500'
                    : 'oi-file text-surface-400'
                "
              />
              <span class="text-sm font-medium truncate">
                {{ lesson.order }}. {{ lesson.title }}
              </span>
              <Tag
                v-if="lesson.videoUrl"
                value="Video"
                severity="info"
                class="text-xs"
              />
            </div>

            <div class="flex items-center gap-2 shrink-0">
              <Tag
                v-if="completedLessonIds.has(lesson.id)"
                :value="t('lessons.completed')"
                severity="success"
                class="text-xs"
              />
              <i class="oi oi-chevron-right text-xs text-surface-400" />
            </div>
          </RouterLink>
        </div>
      </div>

      <!-- Kvízy sekce -->
      <div class="space-y-2">
        <p class="text-xs font-semibold uppercase tracking-wider text-surface-400">
          {{ t('quizzes.quizzes') }}
        </p>

        <div v-if="quizzesLoading" class="space-y-2">
          <Skeleton height="2.75rem" class="rounded-xl" />
        </div>

        <p
          v-else-if="quizzes.length === 0"
          class="text-xs text-surface-500 italic py-1"
        >
          {{ t('quizzes.noQuizzes') }}
        </p>

        <div v-else class="space-y-1.5">
          <RouterLink
            v-for="quiz in quizzes"
            :key="quiz.id"
            :to="{
              name: 'quiz-detail',
              params: { courseId, quizId: quiz.id },
            }"
            class="px-3.5 py-2.5 rounded-xl border border-amber-500/30 bg-amber-500/5 flex flex-wrap items-center justify-between gap-3 hover:border-amber-500/60 transition-colors no-underline text-surface-900 dark:text-surface-0"
          >
            <div class="flex items-center gap-3">
              <i class="oi oi-question-circle text-amber-500 text-base" />
              <span class="text-sm font-semibold">{{ quiz.title }}</span>
            </div>

            <div class="flex items-center gap-2 text-xs text-surface-500">
              <Tag
                :value="t('quizzes.passingThreshold', { percent: quiz.passingThresholdPercent })"
                severity="warn"
              />
              <span v-if="quiz.maxAttempts">
                {{ t('quizzes.maxAttempts', { count: quiz.maxAttempts }) }}
              </span>
              <i class="oi oi-chevron-right text-xs text-surface-400" />
            </div>
          </RouterLink>
        </div>
      </div>
    </div>
  </div>
</template>
