
<script setup lang="ts">
import type { SectionResponseDto } from '@/types/api'

defineProps<{
  sections: SectionResponseDto[]
  selectedSectionId: number | null
}>()

const emit = defineEmits<{
  select: [id: number]
}>()

const { t } = useI18n()
</script>

<template>
  <div class="space-y-4">
    <h2 class="text-xl font-semibold">
      {{ t('courses.courseContent') }}
    </h2>

    <div class="flex flex-wrap gap-2">
      <Button
        v-for="section in sections"
        :key="section.id"
        :label="section.title"
        :outlined="selectedSectionId !== section.id"
        severity="secondary"
        size="small"
        @click="emit('select', section.id)"
      />
    </div>
  </div>
</template>
