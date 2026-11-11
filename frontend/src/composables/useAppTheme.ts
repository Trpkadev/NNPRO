import { computed, watch } from 'vue'
import { useStorage } from '@vueuse/core'

const theme = useStorage<'light' | 'dark'>('nnpro-theme', 'dark')

export function useAppTheme() {
  const isDark = computed(() => theme.value === 'dark')

  watch(
    theme,
    (value) => {
      document.documentElement.classList.toggle('dark', value === 'dark')
    },
    {
      immediate: true,
      flush: 'sync',
    },
  )

  function toggleDark() {
    theme.value = isDark.value ? 'light' : 'dark'
  }

  return {
    isDark,
    toggleDark,
  }
}
