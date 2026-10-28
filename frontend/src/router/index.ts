import { createRouter, createWebHistory } from 'vue-router'
import { routes } from '@/router/routes.ts'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: routes,
})

router.beforeEach((to) => {
  if (to.meta.requiresAuth) {
    const token = localStorage.getItem('access_token')
    if (!token || !isTokenValid(token)) {
      return { name: 'home' } // TODO: přesměrování na login
    }
  }
})

export default router
