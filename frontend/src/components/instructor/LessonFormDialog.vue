<script setup lang="ts">
import { getApiErrorMessage } from '@/services/api'
import type { LessonResponseDto } from '@/types/api'

const visible = defineModel<boolean>('visible', { default: false })

const props = defineProps<{
  sectionId: number
  lesson?: LessonResponseDto | null
  defaultOrder?: number
}>()

const { t } = useI18n()

const createLessonMutation = useCreateLessonMutation()
const updateLessonMutation = useUpdateLessonMutation()

const lessonTitle = ref('')
const lessonOrder = ref(1)
const lessonVideoUrl = ref('')
const lessonContent = ref('')
const lessonError = ref<string | null>(null)

const isSaving = computed(
  () => createLessonMutation.isPending.value || updateLessonMutation.isPending.value,
)

watch(visible, (isOpen) => {
  if (isOpen) {
    lessonTitle.value = props.lesson?.title ?? ''
    lessonOrder.value = props.lesson?.order ?? props.defaultOrder ?? 1
    lessonVideoUrl.value = props.lesson?.videoUrl ?? ''
    lessonContent.value = props.lesson?.content ?? ''
    lessonError.value = null
  }
})

async function handleSaveLesson() {
  lessonError.value = null
  if (!lessonTitle.value.trim() || !lessonContent.value.trim()) {
    lessonError.value = t('validation.required')
    return
  }

  const payload = {
    title: lessonTitle.value.trim(),
    content: lessonContent.value,
    videoUrl: lessonVideoUrl.value.trim() ? lessonVideoUrl.value.trim() : null,
    order: Math.max(1, Number(lessonOrder.value) || 1),
    sectionId: props.sectionId,
  }

  try {
    if (props.lesson) {
      await updateLessonMutation.mutateAsync({
        lessonId: props.lesson.id,
        payload,
      })
    } else {
      await createLessonMutation.mutateAsync(payload)
    }
    visible.value = false
  } catch (err) {
    lessonError.value = getApiErrorMessage(err)
  }
}
</script>

<template>
  <Dialog
    v-model:visible="visible"
    modal
    :header="lesson ? t('instructor.editLesson') : t('instructor.addLesson')"
    :style="{ width: '38rem', maxWidth: '95vw' }"
  >
    <form class="space-y-4" @submit.prevent="handleSaveLesson">
      <Message v-if="lessonError" severity="error" :closable="false">
        {{ lessonError }}
      </Message>

      <div class="grid grid-cols-1 sm:grid-cols-4 gap-3">
        <div class="sm:col-span-3 space-y-1">
          <label for="lesson-title-input" class="block text-sm font-medium">
            {{ t('instructor.lessonTitleLabel') }}
          </label>
          <InputText id="lesson-title-input" v-model="lessonTitle" class="w-full" required />
        </div>
        <div class="space-y-1">
          <label for="lesson-order-input" class="block text-sm font-medium">
            {{ t('instructor.lessonOrderLabel') }}
          </label>
          <InputNumber
            input-id="lesson-order-input"
            v-model="lessonOrder"
            :min="1"
            class="w-full"
          />
        </div>
      </div>

      <div class="space-y-1">
        <label for="lesson-video-input" class="block text-sm font-medium">
          {{ t('instructor.lessonVideoLabel') }}
        </label>
        <InputText
          id="lesson-video-input"
          v-model="lessonVideoUrl"
          placeholder="https://www.youtube.com/watch?v=..."
          class="w-full"
        />
      </div>

      <div class="space-y-1">
        <label for="lesson-content-input" class="block text-sm font-medium">
          {{ t('instructor.lessonContentLabel') }}
        </label>
        <Textarea
          id="lesson-content-input"
          v-model="lessonContent"
          rows="7"
          class="w-full"
          required
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
