import { computed, ref, watch } from 'vue'
import type { Ref } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { api } from '@/services/api'
import type { ApiWrapper, SectionResponseDto, LessonResponseDto } from '@/types/api'

// Načte postupně všechny stránky z API.
async function fetchAllPages<T extends { id: number }>(url: string): Promise<T[]> {
  const pageSize = 50
  const allItems: T[] = []
  const seenIds = new Set<number>()
  let page = 1

  while (true) {
    const { data } = await api.get<ApiWrapper<T[]>>(url, {
      params: { Page: page, PageSize: pageSize },
    })

    const items = data.data ?? []

    for (const item of items) {
      if (seenIds.has(item.id)) {
        throw new Error('API returned duplicate items during pagination')
      }
      seenIds.add(item.id)
    }

    allItems.push(...items)

    if (items.length < pageSize) {
      break
    }

    page++
  }

  return allItems
}

export function useCourseContent(courseId: Ref<number>) {
  const selectedSectionId = ref<number | null>(null)
  const selectedLessonId = ref<number | null>(null)

  const sectionsQuery = useQuery({
    queryKey: computed(() => ['course-sections', courseId.value]),
    queryFn: () => fetchAllPages<SectionResponseDto>(`/api/courses/${courseId.value}/sections`),
    enabled: computed(() => courseId.value > 0),
  })

  const sections = computed(() =>
    [...(sectionsQuery.data.value ?? [])].sort((a, b) => a.order - b.order),
  )

  watch(
    sections,
    (items) => {
      if (!items.some((s) => s.id === selectedSectionId.value)) {
        selectedSectionId.value = items[0]?.id ?? null
        selectedLessonId.value = null
      }
    },
    { immediate: true },
  )

  const lessonsQuery = useQuery({
    queryKey: computed(() => ['section-lessons', selectedSectionId.value]),
    queryFn: () =>
      fetchAllPages<LessonResponseDto>(`/api/sections/${selectedSectionId.value}/lessons`),
    enabled: computed(() => selectedSectionId.value !== null),
  })

  const lessons = computed(() =>
    [...(lessonsQuery.data.value ?? [])].sort((a, b) => a.order - b.order),
  )

  const lessonQuery = useQuery({
    queryKey: computed(() => ['lesson-detail', selectedLessonId.value]),
    queryFn: async () => {
      const { data } = await api.get<ApiWrapper<LessonResponseDto>>(
        `/api/lessons/${selectedLessonId.value}`,
      )
      return data
    },
    enabled: computed(() => selectedLessonId.value !== null),
  })

  const selectedLesson = computed(() => lessonQuery.data.value?.data)

  function selectSection(id: number) {
    selectedSectionId.value = id
    selectedLessonId.value = null
  }

  function openLesson(id: number) {
    selectedLessonId.value = id
  }

  function startLearning() {
    if (lessons.value.length > 0) {
      openLesson(lessons.value[0]!.id)
    }
  }

  watch(courseId, () => {
    selectedSectionId.value = null
    selectedLessonId.value = null
  })

  return {
    sections,
    lessons,
    selectedLesson,
    selectedSectionId,
    selectedLessonId,
    sectionsLoading: sectionsQuery.isLoading,
    sectionsIsError: sectionsQuery.isError,
    lessonsLoading: lessonsQuery.isLoading,
    lessonsIsError: lessonsQuery.isError,
    lessonLoading: lessonQuery.isLoading,
    lessonIsError: lessonQuery.isError,
    selectSection,
    openLesson,
    startLearning,
  }
}
