<script setup lang="ts">
import { formatDateTime } from '@/utils/formatters'
import { getApiErrorMessage } from '@/services/api'

const { t } = useI18n()

const progressPage = ref(1)

const {
  data: progressResponse,
  isLoading: progressLoading,
  isError: progressError,
  error: progressErrorObj,
} = useMyProgressQuery(
  computed(() => ({
    Page: progressPage.value,
    PageSize: 10,
  })),
)

const progressList = computed(() => progressResponse.value?.data ?? [])
const progressPagination = computed(() => progressResponse.value?.metadata?.pagination ?? null)

const totalProgress = computed(
  () => progressPagination.value?.totalItems ?? progressList.value.length,
)

const completedOnPage = computed(() => progressList.value.filter((item) => item.isCompleted).length)
</script>

<template>
  <div class="space-y-6">
    <!-- Studentské statistiky -->
    <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
      <div
        class="p-5 rounded-2xl bg-surface-0 dark:bg-surface-900 border border-surface-200 dark:border-surface-800 flex items-center gap-4"
      >
        <div
          class="w-12 h-12 rounded-xl bg-primary/10 text-primary flex items-center justify-center"
        >
          <i class="oi oi-book text-xl" />
        </div>

        <div>
          <p class="text-sm text-surface-500">
            {{ t('profile.totalProgressRecords') }}
          </p>
          <Skeleton v-if="progressLoading" width="4rem" height="2rem" />
          <p v-else class="text-2xl font-bold">
            {{ progressError ? '—' : totalProgress }}
          </p>
        </div>
      </div>

      <div
        class="p-5 rounded-2xl bg-surface-0 dark:bg-surface-900 border border-surface-200 dark:border-surface-800 flex items-center gap-4"
      >
        <div
          class="w-12 h-12 rounded-xl bg-green-500/10 text-green-600 dark:text-green-400 flex items-center justify-center"
        >
          <i class="oi oi-check-circle text-xl" />
        </div>

        <div>
          <p class="text-sm text-surface-500">
            {{ t('profile.completedOnPage') }}
          </p>
          <Skeleton v-if="progressLoading" width="4rem" height="2rem" />
          <p v-else class="text-2xl font-bold">
            {{ progressError ? '—' : completedOnPage }}
          </p>
        </div>
      </div>
    </div>

    <!-- Studijní postup -->
    <section
      class="rounded-2xl border border-surface-200 dark:border-surface-800 bg-surface-0 dark:bg-surface-900 p-5 sm:p-6 space-y-5"
    >
      <div class="flex items-center gap-3">
        <div
          class="w-10 h-10 rounded-xl bg-primary/10 text-primary flex items-center justify-center"
        >
          <i class="oi oi-book text-lg" />
        </div>

        <div>
          <h2 class="text-lg font-bold">
            {{ t('profile.learningProgressTitle') }}
          </h2>
          <p class="text-xs text-surface-500">
            {{ t('profile.learningProgressDescription') }}
          </p>
        </div>
      </div>

      <div v-if="progressLoading" class="space-y-3">
        <Skeleton v-for="n in 4" :key="n" height="5rem" class="rounded-xl" />
      </div>

      <Message v-else-if="progressError" severity="error" :closable="false">
        {{ getApiErrorMessage(progressErrorObj) }}
      </Message>

      <div v-else-if="progressList.length === 0" class="text-center py-10 space-y-3">
        <i class="oi oi-book text-3xl text-surface-400" />
        <p class="text-sm text-surface-500">
          {{ t('profile.noProgressYet') }}
        </p>
      </div>

      <div v-else class="space-y-3">
        <RouterLink
          v-for="item in progressList"
          :key="`${item.courseId}-${item.lessonId}`"
          :to="{
            name: 'lesson-detail',
            params: {
              courseId: item.courseId,
              lessonId: item.lessonId,
            },
          }"
          class="group flex flex-col sm:flex-row sm:items-center gap-3 p-4 rounded-xl border border-surface-200 dark:border-surface-700 hover:border-primary/50 hover:bg-surface-50 dark:hover:bg-surface-800/50 transition-colors no-underline"
        >
          <div
            class="w-10 h-10 rounded-xl flex items-center justify-center shrink-0"
            :class="
              item.isCompleted
                ? 'bg-green-500/10 text-green-600 dark:text-green-400'
                : 'bg-surface-100 dark:bg-surface-800 text-surface-500'
            "
          >
            <i :class="item.isCompleted ? 'oi oi-check' : 'oi oi-book'" />
          </div>

          <div class="min-w-0 flex-1 space-y-1">
            <p
              class="font-semibold text-sm text-surface-900 dark:text-surface-0 group-hover:text-primary break-words"
            >
              {{ item.lessonTitle }}
            </p>

            <p v-if="item.completedAtUtc" class="text-xs text-surface-500">
              {{ t('profile.completedAt') }}:
              {{ formatDateTime(item.completedAtUtc) }}
            </p>
          </div>

          <div class="flex items-center gap-3">
            <Tag
              :value="item.isCompleted ? t('lessons.completed') : t('lessons.incomplete')"
              :severity="item.isCompleted ? 'success' : 'secondary'"
            />
            <i class="oi oi-chevron-right text-surface-400" />
          </div>
        </RouterLink>
      </div>

      <PaginationControls
        v-if="!progressError && progressPagination"
        :pagination="progressPagination"
        :loading="progressLoading"
        @update:page="progressPage = $event"
      />
    </section>
  </div>
</template>
