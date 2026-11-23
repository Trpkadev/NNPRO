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
</script>

<template>
  <div class="space-y-6">
    <!-- Nadpis profilu -->
    <div class="space-y-2">
      <h1 class="text-2xl font-bold">
        {{ t('profile.title') }}
      </h1>

      <p class="text-sm text-surface-500">
        {{ t('profile.subtitle') }}
      </p>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6 items-start">
      <!-- Nastavení účtu -->
      <div class="min-w-0">
        <ProfileAccountCard />
      </div>

      <!-- Studijní postup -->
      <div
        class="lg:col-span-2 min-w-0 p-4 sm:p-6 rounded-2xl bg-surface-0 dark:bg-surface-900 border border-surface-200 dark:border-surface-800 space-y-5"
      >
        <div class="flex flex-wrap items-center justify-between gap-3">
          <h2 class="text-lg font-bold">
            {{ t('profile.learningProgressTitle') }}
          </h2>

          <Tag
            v-if="!progressLoading && !progressError"
            :value="String(totalProgress)"
            severity="secondary"
            rounded
          />
        </div>

        <!-- Načítání -->
        <div v-if="progressLoading" class="space-y-3">
          <Skeleton v-for="n in 4" :key="n" height="4rem" class="rounded-xl" />
        </div>

        <!-- Chyba API -->
        <Message v-else-if="progressError" severity="error" :closable="false">
          {{ getApiErrorMessage(progressErrorObj) }}
        </Message>

        <!-- Prázdný seznam -->
        <div v-else-if="progressList.length === 0" class="text-center py-10 space-y-3">
          <i class="oi oi-book text-3xl text-surface-400"></i>

          <p class="text-sm text-surface-500">
            {{ t('profile.noProgressYet') }}
          </p>
        </div>

        <template v-else>
          <!-- Mobilní zobrazení -->
          <div class="space-y-3 md:hidden">
            <div
              v-for="item in progressList"
              :key="item.lessonId"
              class="p-4 rounded-xl border border-surface-200 dark:border-surface-700 space-y-3"
            >
              <RouterLink
                :to="{
                  name: 'lesson-detail',
                  params: {
                    courseId: item.courseId,
                    lessonId: item.lessonId,
                  },
                }"
                class="block font-semibold text-primary hover:underline break-words"
              >
                {{ item.lessonTitle }}
              </RouterLink>

              <div class="flex flex-wrap items-center justify-between gap-2">
                <Tag
                  :value="item.isCompleted ? t('lessons.completed') : t('lessons.incomplete')"
                  :severity="item.isCompleted ? 'success' : 'secondary'"
                />

                <span v-if="item.completedAtUtc" class="text-xs text-surface-500">
                  {{ formatDateTime(item.completedAtUtc) }}
                </span>
              </div>
            </div>
          </div>

          <!-- Desktopové zobrazení -->
          <div class="hidden md:block min-w-0">
            <DataTable :value="progressList" striped-rows responsive-layout="scroll" class="w-full">
              <Column field="lessonTitle" :header="t('lessons.lesson')">
                <template #body="{ data }">
                  <RouterLink
                    :to="{
                      name: 'lesson-detail',
                      params: {
                        courseId: data.courseId,
                        lessonId: data.lessonId,
                      },
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
                  <span v-if="data.completedAtUtc" class="text-xs text-surface-500">
                    {{ formatDateTime(data.completedAtUtc) }}
                  </span>

                  <span v-else class="text-surface-400"> — </span>
                </template>
              </Column>
            </DataTable>
          </div>
        </template>

        <!-- Stránkování -->
        <PaginationControls
          v-if="!progressError && progressPagination"
          :pagination="progressPagination"
          :loading="progressLoading"
          @update:page="progressPage = $event"
        />
      </div>
    </div>
  </div>
</template>
