import type { RouteRecordRaw } from 'vue-router'
import { UserRoles } from '@/types/api'
import '@/types/auth'

export const routes: RouteRecordRaw[] = [
  // Přihlášení a registrace
  {
    path: '/auth',
    component: () => import('@/layouts/AuthLayout.vue'),
    meta: { requiresAuth: false, guestOnly: true, layout: 'auth' },
    children: [
      {
        path: '/login',
        name: 'login',
        component: () => import('@/views/auth/LoginView.vue'),
        meta: { requiresAuth: false, guestOnly: true, layout: 'auth' },
      },
      {
        path: '/register',
        name: 'register',
        component: () => import('@/views/auth/RegisterView.vue'),
        meta: { requiresAuth: false, guestOnly: true, layout: 'auth' },
      },
    ],
  },

  // Hlavní aplikace
  {
    path: '/',
    component: () => import('@/layouts/MainLayout.vue'),
    meta: { requiresAuth: true, layout: 'main' },
    children: [
      // Přesměrování na homepage
      {
        path: '',
        redirect: { name: 'home' },
      },
      {
        path: 'home',
        name: 'home',
        component: () => import('@/views/HomeView.vue'),
        meta: { requiresAuth: true, layout: 'main' },
      },
      // Detail kurzu
      {
        path: 'courses/:id',
        name: 'course-detail',
        component: () => import('@/views/courses/CourseDetailView.vue'),
        meta: { requiresAuth: true, layout: 'main' },
      },

      // Lektorská sekce
      {
        path: 'instructor',
        component: () => import('@/layouts/InstructorLayout.vue'),
        meta: {
          requiresAuth: true,
          roles: [UserRoles.Instructor, UserRoles.Admin],
          layout: 'instructor',
        },
        children: [
          {
            path: '',
            name: 'instructor-dashboard',
            component: () => import('@/views/instructor/InstructorDashboardView.vue'),
            meta: {
              requiresAuth: true,
              roles: [UserRoles.Instructor, UserRoles.Admin],
              layout: 'instructor',
            },
          },
        ],
      },

      // Administrátorská sekce
      {
        path: 'admin',
        component: () => import('@/layouts/AdminLayout.vue'),
        meta: {
          requiresAuth: true,
          roles: [UserRoles.Admin],
          layout: 'admin',
        },
        children: [
          {
            path: '',
            name: 'admin-dashboard',
            component: () => import('@/views/admin/AdminDashboardView.vue'),
            meta: {
              requiresAuth: true,
              roles: [UserRoles.Admin],
              layout: 'admin',
            },
          },
        ],
      },

      // Přístup odepřen
      {
        path: 'forbidden',
        name: 'forbidden',
        component: () => import('@/views/ForbiddenView.vue'),
        meta: { requiresAuth: false, layout: 'main' },
      },
    ],
  },

  // Stránka nenalezena
  {
    path: '/:pathMatch(.*)*',
    name: 'not-found',
    component: () => import('@/views/NotFoundView.vue'),
    meta: { requiresAuth: false, layout: 'empty' },
  },
]
