<script setup lang="ts">
import { useToast } from 'openvue/usetoast'
import { getApiErrorMessage } from '@/services/api'
import type { CourseResponseDto } from '@/types/api'

const props = defineProps<{
  visible: boolean
  course?: CourseResponseDto | null
}>()

const emit = defineEmits<{
  'update:visible': [value: boolean]
  saved: [courseId: number]
}>()

const { t } = useI18n()
const toast = useToast()

const createMutation = useCreateCourseMutation()
const updateMutation = useUpdateCourseMutation()

const title = ref('')
const description = ref('')
const accessKey = ref('')
const errorMessage = ref<string | null>(null)

const isEditing = computed(() => !!props.course)
const isSaving = computed(
  () => createMutation.isPending.value || updateMutation.isPending.value,
)

watch(
  () => props.visible,
  (isOpen) => {
    if (isOpen) {
      title.value = props.course?.title ?? ''
      description.value = props.course?.description ?? ''
      accessKey.value = ''
      errorMessage.value = null
    }
  },
)

async function handleSubmit() {
  errorMessage.value = null
  const trimmedTitle = title.value.trim()
  const trimmedDesc = description.value.trim()
  if (!trimmedTitle || !trimmedDesc) {
    errorMessage.value = t('validation.required')
    return
  }

  const payload = {
    title: trimmedTitle,
    description: trimmedDesc,
    accessKey: accessKey.value.trim() ? accessKey.value.trim() : null,
  }

  try {
    let courseId: number
    if (props.course) {
      await updateMutation.mutateAsync({ id: props.course.id, payload })
      courseId = props.course.id
    } else {
      courseId = await createMutation.mutateAsync(payload)
    }

    toast.add({
      severity: 'success',
      summary: t('instructor.courseSaved'),
      life: 3000,
    })

    emit('saved', courseId)
    emit('update:visible', false)
  } catch (err) {
    errorMessage.value = getApiErrorMessage(err)
  }
}
</script>

<template>
  <Dialog
    :visible="visible"
    modal
    :header="isEditing ? t('instructor.editCourse') : t('instructor.createCourse')"
    :style="{ width: '32rem', maxWidth: '95vw' }"
    @update:visible="emit('update:visible', $event)"
  >
    <form class="space-y-4" @submit.prevent="handleSubmit">
      <Message v-if="errorMessage" severity="error" :closable="false">
        {{ errorMessage }}
      </Message>

      <div class="space-y-1.5">
        <label class="block text-sm font-medium">{{ t('instructor.courseTitleLabel') }}</label>
        <InputText v-model="title" class="w-full" required />
      </div>

      <div class="space-y-1.5">
        <label class="block text-sm font-medium">{{ t('instructor.courseDescLabel') }}</label>
        <Textarea v-model="description" rows="4" class="w-full" required />
      </div>

      <div class="space-y-1.5">
        <label class="block text-sm font-medium">{{ t('instructor.courseAccessKeyLabel') }}</label>
        <InputText
          v-model="accessKey"
          :placeholder="t('courses.accessKeyPlaceholder')"
          class="w-full"
        />
      </div>

      <div class="flex justify-end gap-2 pt-2">
        <Button
          type="button"
          :label="t('common.cancel')"
          severity="secondary"
          text
          @click="emit('update:visible', false)"
        />
        <Button
          type="submit"
          :label="t('common.save')"
          icon="oi oi-check"
          :loading="isSaving"
        />
      </div>
    </form>
  </Dialog>
</template>
