import { computed, ref, toValue } from 'vue'
import type { MaybeRefOrGetter } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { usersService } from '@/services/users.service'
import { useUpdateLessonProgressMutation } from '@/composables/useLessonsQueries'
import type { UserLessonProgressResponseDto } from '@/types/api'

export function useLessonProgress(
  lessonIds: MaybeRefOrGetter<number[]>,
  courseId: MaybeRefOrGetter<number>,
) {
  const authStore = useAuthStore()
  const pendingLessonId = ref<number | null>(null)
  const progressError = ref(false)
  const mutation = useUpdateLessonProgressMutation()

  const progressQuery = useQuery({
    queryKey: computed(() => ['my-lesson-progress', authStore.user?.username]),
    queryFn: async () => {
      const allProgress: UserLessonProgressResponseDto[] = []
      const pageSize = 50
      let page = 1

      while (true) {
        const response = await usersService.getMyProgress({
          Page: page,
          PageSize: pageSize,
        })

        allProgress.push(...response.data)

        const pagination = response.metadata?.pagination
        const hasNextPage = pagination ? pagination.hasNextPage : response.data.length === pageSize

        if (!hasNextPage) break
        page++
      }

      return allProgress
    },
    enabled: computed(() => Boolean(authStore.user?.username)),
  })

  const completedLessons = computed<Record<number, boolean>>(() => {
    const result: Record<number, boolean> = {}

    for (const item of progressQuery.data.value ?? []) {
      if (item.courseId === toValue(courseId)) {
        result[item.lessonId] = item.isCompleted
      }
    }

    return result
  })

  const completedCount = computed(
    () => toValue(lessonIds).filter((id) => completedLessons.value[id] === true).length,
  )

  const progressPercent = computed(() => {
    const total = toValue(lessonIds).length

    return total === 0 ? 0 : Math.round((completedCount.value / total) * 100)
  })

  async function toggleLesson(lessonId: number) {
    console.log('Kliknutí na Dokončit:', lessonId)
    console.log('Stav:', {
      pendingLessonId: pendingLessonId.value,
      progressPending: progressQuery.isPending.value,
    })

    if (pendingLessonId.value !== null || progressQuery.isPending.value) return
    const isCompleted = !completedLessons.value[lessonId]
    pendingLessonId.value = lessonId
    progressError.value = false

    try {
      await mutation.mutateAsync({
        lessonId,
        payload: { isCompleted },
      })

      const result = await progressQuery.refetch()

      console.log('Odeslaný stav:', { lessonId, isCompleted })
      console.log('Načtený progress:', result.data)

      if (result.isError) progressError.value = true
    } catch (error) {
  console.error('Chyba při ukládání progressu:', error)
  progressError.value = true
}
    finally {
      pendingLessonId.value = null
    }
  }

  return {
    completedLessons,
    completedCount,
    progressPercent,
    pendingLessonId,
    progressError,
    progressLoading: progressQuery.isPending,
    toggleLesson,
  }
}
