import { computed, ref, toValue } from 'vue'
import type { MaybeRefOrGetter } from 'vue'
import { useStorage } from '@vueuse/core'
import { useUpdateLessonProgressMutation } from '@/composables/useLessonsQueries'

export function useLessonProgress(
  lessonIds: MaybeRefOrGetter<number[]>,
  courseId: MaybeRefOrGetter<number>,
) {
  const authStore = useAuthStore()

  const storageKey = computed(
    () => `nnpro-progress-${authStore.user?.username ?? 'guest'}-${toValue(courseId)}`,
  )

  const completedLessons = useStorage<Record<number, boolean>>(storageKey, {})

  const pendingLessonId = ref<number | null>(null)
  const progressError = ref(false)
  const mutation = useUpdateLessonProgressMutation()

  const completedCount = computed(
    () => toValue(lessonIds).filter((id) => completedLessons.value[id] === true).length,
  )

  const progressPercent = computed(() => {
    const total = toValue(lessonIds).length

    return total === 0 ? 0 : Math.round((completedCount.value / total) * 100)
  })

  async function toggleLesson(lessonId: number) {
    if (pendingLessonId.value !== null) return

    const isCompleted = !completedLessons.value[lessonId]
    pendingLessonId.value = lessonId
    progressError.value = false

    try {
      await mutation.mutateAsync({
        lessonId,
        payload: { isCompleted },
      })

      completedLessons.value = {
        ...completedLessons.value,
        [lessonId]: isCompleted,
      }
    } catch {
      progressError.value = true
    } finally {
      pendingLessonId.value = null
    }
  }

  return {
    completedLessons,
    completedCount,
    progressPercent,
    pendingLessonId,
    progressError,
    toggleLesson,
  }
}
