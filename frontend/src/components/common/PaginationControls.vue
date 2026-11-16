<script setup lang="ts">
import type { PaginationMetadata } from '@/types/api'

const props = defineProps<{
  pagination?: PaginationMetadata | null
  loading?: boolean
}>()

const emit = defineEmits<{
  'update:page': [page: number]
}>()

const { t } = useI18n()

const currentPage = computed(() => props.pagination?.page ?? 1)
const totalItems = computed(() => props.pagination?.totalItems ?? 0)
const pageSize = computed(() => props.pagination?.pageSize ?? 20)
const totalPages = computed(() => {
  if (props.pagination?.totalPages && props.pagination.totalPages > 0) {
    return props.pagination.totalPages
  }
  return totalItems.value > 0 ? Math.ceil(totalItems.value / Math.max(1, pageSize.value)) : 1
})

const canPrev = computed(() => {
  if (props.pagination?.hasPreviousPage !== undefined) {
    return props.pagination.hasPreviousPage || currentPage.value > 1
  }
  return currentPage.value > 1
})

const canNext = computed(() => {
  if (props.pagination?.hasNextPage !== undefined) {
    return props.pagination.hasNextPage || currentPage.value < totalPages.value
  }
  return currentPage.value < totalPages.value
})

function goToPage(page: number) {
  if (page < 1 || page > totalPages.value || page === currentPage.value) return
  emit('update:page', page)
}
</script>

<template>
  <div
    v-if="totalPages > 1 || totalItems > pageSize"
    class="flex flex-col sm:flex-row items-center justify-between gap-3 pt-4 border-t border-surface-200 dark:border-surface-800"
  >
    <span class="text-xs text-surface-500 dark:text-surface-400">
      {{
        t('common.pageInfo', {
          page: currentPage,
          totalPages,
          totalItems,
        })
      }}
    </span>

    <div class="flex items-center gap-2">
      <Button
        :label="t('common.prevPage')"
        icon="oi oi-chevron-left"
        severity="secondary"
        outlined
        size="small"
        :disabled="!canPrev || loading"
        @click="goToPage(currentPage - 1)"
      />
      <span
        class="px-3 py-1 rounded-lg bg-surface-100 dark:bg-surface-800 text-xs font-semibold"
      >
        {{ currentPage }} / {{ totalPages }}
      </span>
      <Button
        :label="t('common.nextPage')"
        icon="oi oi-chevron-right"
        icon-pos="right"
        severity="secondary"
        outlined
        size="small"
        :disabled="!canNext || loading"
        @click="goToPage(currentPage + 1)"
      />
    </div>
  </div>
</template>
