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
</script>

<template>
  <div class="space-y-3">
    <!-- Vyhledávací pole -->
    <div class="relative max-w-xl">
      <i
        class="oi oi-search absolute left-4 top-1/2 -translate-y-1/2 text-surface-400 pointer-events-none"
      />

      <InputText
        :model-value="modelValue"
        :placeholder="t('courses.searchPlaceholder')"
        class="w-full !pl-11 !pr-11 !py-3 rounded-xl"
        @update:model-value="emit('update:modelValue', String($event ?? ''))"
      />

      <button
        v-if="modelValue"
        type="button"
        class="absolute right-4 top-1/2 -translate-y-1/2 text-surface-400 hover:text-primary"
        :aria-label="t('courses.clearSearch')"
        @click="emit('update:modelValue', '')"
      >
        <i class="oi oi-x" />
      </button>
    </div>

    <!-- Filtry -->
    <div class="flex flex-wrap items-center gap-2">
      <span class="text-sm text-surface-500 mr-1">
        <i class="oi oi-filter mr-1" />
        {{ t('courses.filterBy') }}
      </span>

      <Button
        v-for="option in ['all', 'title', 'description'] as const"
        :key="option"
        :label="t(`courses.filter.${option}`)"
        :outlined="filter !== option"
        :severity="filter === option ? 'primary' : 'secondary'"
        size="small"
        @click="emit('update:filter', option)"
      />
    </div>
  </div>
</template>
