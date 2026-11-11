<script setup lang="ts">
defineProps<{
  modelValue: string
  filter: 'all' | 'title' | 'description'
}>()

const emit = defineEmits<{
  'update:modelValue': [value: string]
  'update:filter': [value: 'all' | 'title' | 'description']
}>()

const { t } = useI18n()
const filterOptions = ['all', 'title', 'description'] as const

function updateSearch(event: Event) {
  const input = event.target as HTMLInputElement
  emit('update:modelValue', input.value)
}
</script>

<template>
  <div class="space-y-3">
    <!-- Vyhledávací pole -->
    <div
      class="flex items-center gap-3 max-w-xl px-4 py-3 rounded-xl border border-surface-300 dark:border-surface-700 bg-surface-0 dark:bg-surface-900 focus-within:border-primary"
    >
      <i class="oi oi-search text-surface-400" aria-hidden="true" />

      <input
        type="search"
        :value="modelValue"
        :placeholder="t('courses.searchPlaceholder')"
        :aria-label="t('courses.searchPlaceholder')"
        class="flex-1 min-w-0 bg-transparent outline-none text-surface-900 dark:text-surface-0 placeholder:text-surface-400"
        @input="updateSearch"
      />

      <button
        v-if="modelValue"
        type="button"
        :aria-label="t('courses.clearSearch')"
        class="text-surface-400 hover:text-primary cursor-pointer"
        @click="emit('update:modelValue', '')"
      >
        <span aria-hidden="true">×</span>
      </button>
    </div>

    <!-- Filtry -->
    <div class="flex flex-wrap items-center gap-2">
      <span class="text-sm text-surface-500 mr-1">
        <i class="oi oi-filter mr-1" aria-hidden="true" />
        {{ t('courses.filterBy') }}
      </span>

      <Button
        v-for="option in filterOptions"
        :key="option"
        :label="t(`courses.filter.${option}`)"
        :outlined="filter !== option"
        :severity="filter === option ? 'primary' : 'secondary'"
        size="small"
        :aria-pressed="filter === option"
        @click="emit('update:filter', option)"
      />
    </div>
  </div>
</template>
