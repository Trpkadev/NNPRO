<script setup lang="ts">
import type { UserRole } from '@/types/api'

const props = withDefaults(
  defineProps<{
    roles?: UserRole[]
    requireAuth?: boolean
  }>(),
  {
    roles: () => [],
    requireAuth: true,
  },
)

const authStore = useAuthStore()

const isAllowed = computed(() => {
  if (props.requireAuth && !authStore.isAuthenticated) {
    return false
  }
  if (props.roles.length > 0) {
    return authStore.hasAnyRole(props.roles)
  }
  return true
})
</script>

<template>
  <slot v-if="isAllowed" />
  <slot v-else name="fallback" />
</template>
