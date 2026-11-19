<script setup lang="ts">
import { formatDateTime } from '@/utils/formatters'
import type { MyQuizAttemptResponseDto, PaginationMetadata } from '@/types/api'

defineProps<{
  attempts: MyQuizAttemptResponseDto[]
  pagination: PaginationMetadata | null
  loading: boolean
}>()

const emit = defineEmits<{
  'update:page': [page: number]
}>()

const { t } = useI18n()
</script>

<template>
  <section
    class="p-5 rounded-2xl bg-surface-0 dark:bg-surface-900 border border-surface-200 dark:border-surface-800 space-y-4"
  >
    <h2 class="text-lg font-bold">{{ t('quizzes.myAttemptsTitle') }}</h2>

    <div v-if="loading" class="space-y-2">
      <Skeleton v-for="n in 3" :key="n" height="3rem" class="rounded-xl" />
    </div>

    <p v-else-if="attempts.length === 0" class="text-sm text-surface-500 text-center py-6">
      {{ t('quizzes.noAttemptsYet') }}
    </p>

    <DataTable v-else :value="attempts" striped-rows responsive-layout="scroll">
      <Column field="id" header="#">
        <template #body="{ data }">
          <span class="font-semibold">{{ t('quizzes.attemptNumber', { id: data.id }) }}</span>
        </template>
      </Column>
      <Column :header="t('quizzes.startedAt')">
        <template #body="{ data }">
          {{ formatDateTime(data.startedAtUtc) }}
        </template>
      </Column>
      <Column :header="t('quizzes.submittedAt')">
        <template #body="{ data }">
          {{ data.submittedAtUtc ? formatDateTime(data.submittedAtUtc) : t('quizzes.inProgress') }}
        </template>
      </Column>
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
      :pagination="pagination"
      :loading="loading"
      @update:page="emit('update:page', $event)"
    />
  </section>
</template>
