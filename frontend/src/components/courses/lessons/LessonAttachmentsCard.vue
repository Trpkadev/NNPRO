<script setup lang="ts">
import { formatBytes } from '@/utils/formatters'

const props = defineProps<{
  lessonId: number
}>()

const { t } = useI18n()

const { data: attachmentsResponse, isLoading: attachmentsLoading } = useLessonAttachmentsQuery(
  computed(() => props.lessonId),
  { Page: 1, PageSize: 50 },
)

const attachments = computed(() => attachmentsResponse.value?.data ?? [])
</script>

<template>
  <section
    class="p-5 rounded-2xl bg-surface-0 dark:bg-surface-900 border border-surface-200 dark:border-surface-800 space-y-3"
  >
    <h2 class="text-base font-bold flex items-center gap-2">
      <i class="oi oi-paperclip text-primary" />
      <span>{{ t('lessons.attachmentsSection') }}</span>
    </h2>

    <div v-if="attachmentsLoading" class="space-y-2">
      <Skeleton height="3rem" class="rounded-xl" />
    </div>

    <p v-else-if="attachments.length === 0" class="text-xs text-surface-500 italic">
      {{ t('lessons.noAttachments') }}
    </p>

    <div v-else class="grid grid-cols-1 sm:grid-cols-2 gap-3">
      <a
        v-for="att in attachments"
        :key="att.id"
        :href="att.url"
        target="_blank"
        rel="noopener noreferrer"
        class="p-3.5 rounded-xl border border-surface-200 dark:border-surface-800 hover:border-primary/60 flex items-center justify-between gap-3 no-underline transition-colors"
      >
        <div class="flex items-center gap-3 min-w-0">
          <div
            class="w-9 h-9 rounded-lg bg-primary/10 text-primary flex items-center justify-center shrink-0"
          >
            <i class="oi oi-download" />
          </div>
          <div class="min-w-0">
            <p class="text-sm font-semibold text-surface-900 dark:text-surface-0 truncate">
              {{ att.fileName }}
            </p>
            <p class="text-xs text-surface-500">
              {{ att.contentType || 'Document' }}
              <span v-if="att.sizeBytes">&bull; {{ formatBytes(att.sizeBytes) }}</span>
            </p>
          </div>
        </div>

        <span class="text-xs font-semibold text-primary shrink-0">
          {{ t('lessons.downloadAttachment') }}
        </span>
      </a>
    </div>
  </section>
</template>
