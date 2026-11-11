import { useAuthStore } from '@/stores/auth'
import { UserRoles, type UserRole } from '@/types/api'

/**
 * Composable pro řízení přístupu na základě rolí (RBAC - Správce, Lektor, Student)
 */
export function usePermissions() {
  const authStore = useAuthStore()

  const isAuthenticated = computed(() => authStore.isAuthenticated)
  const isAdmin = computed(() => authStore.isAdmin)
  const isInstructor = computed(() => authStore.isInstructor)
  const isStudent = computed(() => authStore.isStudent)

  const canManageUsers = computed(() => authStore.isAdmin)
  const canManageCourses = computed(() => authStore.isAdmin || authStore.isInstructor)
  const canManageContent = computed(() => authStore.isAdmin || authStore.isInstructor)
  const canForceEnroll = computed(() => authStore.isAdmin || authStore.isInstructor)
  const canEnrollInCourses = computed(() => authStore.isAuthenticated)

  function hasRole(role: UserRole): boolean {
    return authStore.hasRole(role)
  }

  function hasAnyRole(roles: readonly UserRole[]): boolean {
    return authStore.hasAnyRole(roles)
  }

  return {
    UserRoles,
    isAuthenticated,
    isAdmin,
    isInstructor,
    isStudent,
    canManageUsers,
    canManageCourses,
    canManageContent,
    canForceEnroll,
    canEnrollInCourses,
    hasRole,
    hasAnyRole,
  }
}
