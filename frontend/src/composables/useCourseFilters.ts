import { computed, ref, toValue } from 'vue'
import type { MaybeRefOrGetter } from 'vue'

type SearchableCourse = {
  id: number
  title: string
  shortDescription?: string | null
}

export function useCourseFilters<T extends SearchableCourse>(courses: MaybeRefOrGetter<T[]>) {
  const search = ref('')
  const filter = ref<'all' | 'title' | 'description'>('all')

  const filteredCourses = computed(() => {
    const query = search.value.trim().toLocaleLowerCase()

    if (!query) return toValue(courses)

    return toValue(courses).filter((course) => {
      const title = course.title.toLocaleLowerCase()
      const description = course.shortDescription?.toLocaleLowerCase() ?? ''

      if (filter.value === 'title') {
        return title.includes(query)
      }

      if (filter.value === 'description') {
        return description.includes(query)
      }

      return title.includes(query) || description.includes(query)
    })
  })

  return {
    search,
    filter,
    filteredCourses,
  }
}
