<script setup lang="ts">
import { formatDateTime } from '@/utils/formatters'
import type { QuizResponseDto } from '@/types/api'

const visible = defineModel<boolean>('visible', { default: false })

const props = defineProps<{
  quiz: QuizResponseDto | null
}>()

const { t } = useI18n()

const attemptsPage = ref(1)

watch(
  () => props.quiz?.id,
  () => {
    attemptsPage.value = 1
  },
)

const { data: quizAttemptsResponse, isLoading: attemptsLoading } = useQuizAttemptsQuery(
  computed(() => props.quiz?.id ?? 0),
  computed(() => ({ Page: attemptsPage.value, PageSize: 10 })),
  computed(() => visible.value && !!props.quiz),
)
</script>

<template>
  <Dialog
    v-model:visible="visible"
    modal
    :header="t('instructor.attemptsDialogTitle', { title: quiz?.title ?? '' })"
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
        <Column :header="t('quizzes.startedAt')">
          <template #body="{ data }">
            {{ formatDateTime(data.startedAtUtc) }}
          </template>
        </Column>

        <Column :header="t('quizzes.submittedAt')">
          <template #body="{ data }">
            {{
              data.submittedAtUtc ? formatDateTime(data.submittedAtUtc) : t('quizzes.inProgress')
            }}
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
        :pagination="quizAttemptsResponse?.metadata?.pagination"
        :loading="attemptsLoading"
        @update:page="attemptsPage = $event"
      />
    </div>
  </Dialog>
</template>
