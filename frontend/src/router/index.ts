import { createRouter, createWebHistory } from 'vue-router'
import { routes } from '@/router/routes.ts'
import { useAuthStore } from '@/stores/auth'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
})

router.beforeEach(async (to) => {
  const authStore = useAuthStore()

  // Pokud vypršel access token, zkusíme tichý refresh pomocí HttpOnly refreshToken cookie
  if (!authStore.isAuthenticated && authStore.token) {
    await authStore.refreshTokens()
  }

  // Routa vyžaduje přihlášení
  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    return {
      name: 'login',
      query: to.fullPath !== '/' ? { redirect: to.fullPath } : undefined,
    }
  }

  // Routa je pouze pro nepřihlášené (login, register)
  if (to.meta.guestOnly && authStore.isAuthenticated) {
    return { name: 'home' }
  }

  // Kontrola oprávnění podle rolí (RBAC)
  if (to.meta.roles && to.meta.roles.length > 0) {
    if (!authStore.hasAnyRole(to.meta.roles)) {
      return { name: 'forbidden' }
    }
  }
})

export default router
