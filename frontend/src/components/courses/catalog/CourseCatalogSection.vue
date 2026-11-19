<script setup lang="ts">
import { getApiErrorMessage } from '@/services/api'

defineProps<{
  enrolledCourseIds: Set<number>
  canEnrollAsStudent: boolean
}>()

const emit = defineEmits<{
  requestEnroll: [course: { id: number; title: string }]
}>()

const { t } = useI18n()
const router = useRouter()

const searchQuery = ref('')
const debouncedSearch = refDebounced(searchQuery, 350)
const sortBy = ref<'Id' | 'Title'>('Id')
const descending = ref(false)

const queryParams = computed(() => ({
  PageSize: 9,
  MaxDescriptionLength: 140,
  SortBy: sortBy.value,
  Descending: descending.value,
  Filters: debouncedSearch.value.trim() ? { title: debouncedSearch.value.trim() } : undefined,
}))

const { data, isLoading, isError, error, hasNextPage, isFetchingNextPage, fetchNextPage } =
  useInfiniteCourseThumbnailsQuery(queryParams)

const courses = computed(() => data.value?.pages.flatMap((page) => page.data ?? []) ?? [])
const totalItems = computed(
  () => data.value?.pages[0]?.metadata?.pagination?.totalItems ?? courses.value.length,
)
const errorMessage = computed(() => getApiErrorMessage(error.value, t('courses.loadError')))

const loadMoreTrigger = useTemplateRef<HTMLElement>('loadMoreTrigger')

useIntersectionObserver(loadMoreTrigger, ([entry]) => {
  if (entry?.isIntersecting && hasNextPage.value && !isFetchingNextPage.value) {
    fetchNextPage()
  }
})

function clearFilters() {
  searchQuery.value = ''
  sortBy.value = 'Id'
  descending.value = false
}
</script>

<template>
  <section class="space-y-4">
    <!-- Hlavička a vyhledávací/řadicí lišta -->
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-3">
      <div class="flex items-center gap-2.5">
        <div class="w-8 h-8 rounded-lg bg-primary/10 text-primary flex items-center justify-center">
          <i class="oi oi-book" />
        </div>
        <div>
          <h2 class="text-lg font-bold">{{ t('nav.courses') }}</h2>
          <p v-if="!isLoading" class="text-xs text-surface-500">
            {{ t('courses.totalCount', { count: totalItems }) }}
          </p>
        </div>
      </div>

      <div class="flex flex-wrap items-center gap-2">
        <div class="relative flex-1 sm:w-64">
          <InputText
            v-model="searchQuery"
            :placeholder="t('courses.searchPlaceholder')"
            size="small"
            class="w-full"
          />
        </div>

        <Button
          :label="sortBy === 'Title' ? t('courses.sortTitle') : t('courses.sortId')"
          icon="oi oi-sort-alt"
          severity="secondary"
          outlined
          size="small"
          @click="sortBy = sortBy === 'Id' ? 'Title' : 'Id'"
        />

        <Button
          :icon="descending ? 'oi oi-sort-amount-down' : 'oi oi-sort-amount-up'"
          severity="secondary"
          outlined
          size="small"
          :aria-label="descending ? t('common.sortDesc') : t('common.sortAsc')"
          @click="descending = !descending"
        />

        <Button
          v-if="searchQuery || sortBy !== 'Id' || descending"
          :label="t('common.clearFilters')"
          icon="oi oi-times"
          severity="secondary"
          text
          size="small"
          @click="clearFilters"
        />
      </div>
    </div>

    <!-- Načítání první stránky -->
    <div v-if="isLoading" class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
      <Skeleton v-for="n in 6" :key="n" height="11rem" class="rounded-2xl" />
    </div>

    <Message v-else-if="isError" severity="error" :closable="false">
      {{ errorMessage }}
    </Message>

    <div
      v-else-if="courses.length === 0"
      class="p-8 rounded-2xl bg-surface-0 dark:bg-surface-900 border border-surface-200 dark:border-surface-800 text-center text-surface-500 text-sm"
    >
      {{ t('courses.empty') }}
    </div>

    <!-- Mřížka kurzů s Infinite Scrollingem -->
    <div v-else class="space-y-6">
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
        <div
          v-for="course in courses"
          :key="course.id"
          class="p-5 rounded-2xl bg-surface-0 dark:bg-surface-900 border border-surface-200 dark:border-surface-800 flex flex-col justify-between gap-4 hover:border-primary/50 transition-colors shadow-xs"
        >
          <div class="space-y-2">
            <div class="flex items-start justify-between gap-2">
              <h3 class="font-bold text-base text-surface-900 dark:text-surface-0 line-clamp-1">
                {{ course.title }}
              </h3>
              <Tag
                v-if="enrolledCourseIds.has(course.id)"
                :value="t('courses.enrolledBadge')"
                severity="success"
                class="shrink-0"
              />
            </div>

            <p class="text-sm text-surface-600 dark:text-surface-400 line-clamp-3">
              {{ course.shortDescription }}
            </p>
          </div>

          <div
            class="flex items-center gap-2 pt-3 border-t border-surface-100 dark:border-surface-800"
          >
            <Button
              :label="t('courses.openCourse')"
              icon="oi oi-eye"
              severity="secondary"
              outlined
              size="small"
              class="flex-1"
              @click="router.push({ name: 'course-detail', params: { id: course.id } })"
            />
            <Button
              v-if="canEnrollAsStudent && !enrolledCourseIds.has(course.id)"
              :label="t('courses.enrollBtn')"
              icon="oi oi-plus"
              size="small"
              class="flex-1"
              @click="emit('requestEnroll', { id: course.id, title: course.title })"
            />
          </div>
        </div>
      </div>

      <!-- Sentinel pro Infinite Scroll a tlačítko Načíst další -->
      <div ref="loadMoreTrigger" class="flex justify-center py-2">
        <Button
          v-if="hasNextPage"
          :label="t('common.loadMore')"
          icon="oi oi-refresh"
          severity="secondary"
          outlined
          size="small"
          :loading="isFetchingNextPage"
          @click="fetchNextPage()"
        />
        <span v-else-if="courses.length > 9" class="text-xs text-surface-400 dark:text-surface-500">
          {{ t('common.allLoaded') }}
        </span>
      </div>
    </div>
  </section>
</template>
