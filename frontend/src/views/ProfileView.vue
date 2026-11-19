<script setup lang="ts">
import { formatDateTime } from '@/utils/formatters'

const { t } = useI18n()

const progressPage = ref(1)
const { data: progressResponse, isLoading: progressLoading } = useMyProgressQuery(
  computed(() => ({
    Page: progressPage.value,
    PageSize: 10,
  })),
)

const progressList = computed(() => progressResponse.value?.data ?? [])
const progressPagination = computed(() => progressResponse.value?.metadata?.pagination ?? null)
</script>

<template>
  <div class="space-y-8">
    <div>
      <h1 class="text-2xl font-bold">{{ t('profile.title') }}</h1>
      <p class="text-sm text-surface-500">{{ t('profile.subtitle') }}</p>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
      <ProfileAccountCard />

      <div
        class="lg:col-span-2 p-5 rounded-2xl bg-surface-0 dark:bg-surface-900 border border-surface-200 dark:border-surface-800 space-y-4"
      >
        <h2 class="text-base font-bold">{{ t('profile.learningProgressTitle') }}</h2>

        <div v-if="progressLoading" class="space-y-2">
          <Skeleton v-for="n in 4" :key="n" height="3rem" class="rounded-xl" />
        </div>

        <p v-else-if="progressList.length === 0" class="text-sm text-surface-500 text-center py-8">
          {{ t('profile.noProgressYet') }}
        </p>

        <DataTable v-else :value="progressList" striped-rows responsive-layout="scroll">
          <Column field="lessonTitle" :header="t('lessons.lesson')">
            <template #body="{ data }">
              <RouterLink
                :to="{
                  name: 'lesson-detail',
                  params: { courseId: data.courseId, lessonId: data.lessonId },
                }"
                class="font-semibold text-primary hover:underline"
              >
                {{ data.lessonTitle }}
              </RouterLink>
            </template>
          </Column>
          <Column :header="t('common.actions')">
            <template #body="{ data }">
              <Tag
                :value="data.isCompleted ? t('lessons.completed') : t('lessons.incomplete')"
                :severity="data.isCompleted ? 'success' : 'secondary'"
              />
            </template>
          </Column>
          <Column :header="t('profile.completedAt')">
            <template #body="{ data }">
              <span class="text-xs text-surface-500">{{
                formatDateTime(data.completedAtUtc)
              }}</span>
            </template>
          </Column>
        </DataTable>

        <PaginationControls
          :pagination="progressPagination"
          :loading="progressLoading"
          @update:page="progressPage = $event"
        />
      </div>
    </div>
  </div>
</template>
