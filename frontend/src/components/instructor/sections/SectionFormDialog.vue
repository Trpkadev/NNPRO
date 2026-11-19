<script setup lang="ts">
import { getApiErrorMessage } from '@/services/api'
import type { SectionResponseDto } from '@/types/api'

const visible = defineModel<boolean>('visible', { default: false })

const props = defineProps<{
  courseId: number
  section?: SectionResponseDto | null
  defaultOrder?: number
}>()

const { t } = useI18n()

const createSectionMutation = useCreateSectionMutation()
const updateSectionMutation = useUpdateSectionMutation()

const sectionTitle = ref('')
const sectionOrder = ref(1)
const sectionError = ref<string | null>(null)

const isSaving = computed(
  () => createSectionMutation.isPending.value || updateSectionMutation.isPending.value,
)

watch(visible, (isOpen) => {
  if (isOpen) {
    sectionTitle.value = props.section?.title ?? ''
    sectionOrder.value = props.section?.order ?? props.defaultOrder ?? 1
    sectionError.value = null
  }
})

async function handleSaveSection() {
  sectionError.value = null
  if (!sectionTitle.value.trim()) {
    sectionError.value = t('validation.required')
    return
  }

  const payload = {
    title: sectionTitle.value.trim(),
    order: Math.max(1, Number(sectionOrder.value) || 1),
    courseId: props.courseId,
  }

  try {
    if (props.section) {
      await updateSectionMutation.mutateAsync({
        sectionId: props.section.id,
        payload,
      })
    } else {
      await createSectionMutation.mutateAsync(payload)
    }
    visible.value = false
  } catch (err) {
    sectionError.value = getApiErrorMessage(err)
  }
}
</script>

<template>
  <Dialog
    v-model:visible="visible"
    modal
    :header="section ? t('instructor.editSection') : t('instructor.addSection')"
    :style="{ width: '28rem', maxWidth: '95vw' }"
  >
    <form class="space-y-4" @submit.prevent="handleSaveSection">
      <Message v-if="sectionError" severity="error" :closable="false">
        {{ sectionError }}
      </Message>

      <div class="space-y-1">
        <label for="section-title-input" class="block text-sm font-medium">
          {{ t('instructor.sectionTitleLabel') }}
        </label>
        <InputText id="section-title-input" v-model="sectionTitle" class="w-full" required />
      </div>

      <div class="space-y-1">
        <label for="section-order-input" class="block text-sm font-medium">
          {{ t('instructor.sectionOrderLabel') }}
        </label>
        <InputNumber
          input-id="section-order-input"
          v-model="sectionOrder"
          :min="1"
          class="w-full"
        />
      </div>

      <div class="flex justify-end gap-2">
        <Button
          type="button"
          :label="t('common.cancel')"
          severity="secondary"
          text
          @click="visible = false"
        />
        <Button type="submit" :label="t('common.save')" icon="oi oi-check" :loading="isSaving" />
      </div>
    </form>
  </Dialog>
</template>
