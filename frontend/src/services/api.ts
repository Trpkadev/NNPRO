import axios, { type AxiosError, type InternalAxiosRequestConfig } from 'axios'
import { jwtDecode } from 'jwt-decode'
import router from '@/router'
import { useAuthStore } from '@/stores/auth'
import { UserRoles, type ProblemDetails, type QueryParameters, type UserRole } from '@/types/api'
import type { AuthenticatedUser, JwtPayload } from '@/types/auth'

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'https://localhost:8081',
  timeout: 15000,
  withCredentials: true,
  headers: {
    'Content-Type': 'application/json',
    Accept: 'application/json',
  },
})

/**
 * Ověří platnost JWT tokenu na základě exp claimu
 */
export function isTokenValid(token: string | null | undefined, bufferSeconds = 10): boolean {
  if (!token) return false
  try {
    const decoded = jwtDecode<JwtPayload>(token)
    if (typeof decoded.exp !== 'number' || !Number.isFinite(decoded.exp)) return false
    const currentTime = Date.now() / 1000
    return decoded.exp > currentTime + bufferSeconds
  } catch {
    return false
  }
}

/**
 * Normalizuje řetězec role na typ UserRole
 */
export function normalizeUserRole(rawRole: string | undefined | null): UserRole | null {
  if (!rawRole) return null
  if (
    rawRole === UserRoles.Admin ||
    rawRole === UserRoles.Instructor ||
    rawRole === UserRoles.Student
  ) {
    return rawRole
  }
  const normalized = rawRole.trim().toLowerCase()
  if (normalized === 'admin') return UserRoles.Admin
  if (normalized === 'instructor') return UserRoles.Instructor
  if (normalized === 'student') return UserRoles.Student
  return null
}

/**
 * Dekóduje informace o uživateli z JWT tokenu podle struktury ELearningApi
 */
export function decodeJwtUser(token: string | null | undefined): AuthenticatedUser | null {
  if (!token) return null
  try {
    const payload = jwtDecode<JwtPayload>(token)

    const parsedId = payload.sub !== undefined ? Number(payload.sub) : NaN
    const id = Number.isFinite(parsedId) ? parsedId : null
    const username = typeof payload.name === 'string' ? payload.name.trim() : ''
    const role = normalizeUserRole(payload.role)

    if (id === null || !username || !role) {
      return null
    }

    return {
      id,
      username,
      role,
    }
  } catch {
    return null
  }
}

/**
 * Rozhodne, zda chyba při obnově tokenu znamená neplatnou autentizaci (400/401/403)
 */
export function isAuthInvalidError(error: unknown): boolean {
  if (axios.isAxiosError(error)) {
    const status = error.response?.status
    return status === 400 || status === 401 || status === 403
  }
  return false
}

/**
 * Připraví query parametry pro Axios (převede objekt Filters na JSON řetězec)
 */
export function serializeQueryParams<T extends QueryParameters>(
  params?: T,
): Record<string, unknown> | undefined {
  if (!params) return undefined
  const result: Record<string, unknown> = { ...params } as Record<string, unknown>
  if (params.Filters && typeof params.Filters === 'object') {
    result.Filters = JSON.stringify(params.Filters)
  }
  return result
}

/**
 * Získá čitelnou chybovou zprávu z API odpovědi (ProblemDetails)
 */
export function getApiErrorMessage(
  error: unknown,
  fallbackMessage = 'Došlo k neočekávané chybě při komunikaci se serverem.',
): string {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data as ProblemDetails | undefined
    if (data) {
      if (data.errors && typeof data.errors === 'object') {
        const firstErrorList = Object.values(data.errors)[0]
        if (Array.isArray(firstErrorList) && firstErrorList[0]) {
          return firstErrorList[0]
        }
      }
      if (data.detail) return data.detail
      if (data.title) return data.title
    }
    if (error.message) return error.message
  }
  if (error instanceof Error) {
    return error.message
  }
  return fallbackMessage
}

// ============================================================================
// Axios Interceptory (JWT v paměti + Refresh Token Queue)
// ============================================================================

interface RetryableRequestConfig extends InternalAxiosRequestConfig {
  _retry?: boolean
}

interface FailedQueueItem {
  resolve: (token: string) => void
  reject: (error: unknown) => void
}

let isRefreshing = false
let failedQueue: FailedQueueItem[] = []

function processQueue(error: unknown, token: string | null = null) {
  failedQueue.forEach((prom) => {
    if (error || !token) {
      prom.reject(error ?? new Error('Refresh failed'))
    } else {
      prom.resolve(token)
    }
  })
  failedQueue = []
}

api.interceptors.request.use((config) => {
  const authStore = useAuthStore()
  if (authStore.token) {
    config.headers.Authorization = `Bearer ${authStore.token}`
  }
  return config
})

api.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as RetryableRequestConfig | undefined
    if (!originalRequest) {
      return Promise.reject(error)
    }

    const requestUrl = originalRequest.url ?? ''
    const isAuthEndpoint =
      requestUrl.includes('/api/auth/login') ||
      requestUrl.includes('/api/auth/register') ||
      requestUrl.includes('/api/auth/refresh') ||
      requestUrl.includes('/api/auth/logout')

    if (error.response?.status === 401 && !originalRequest._retry && !isAuthEndpoint) {
      if (isRefreshing) {
        return new Promise<string>((resolve, reject) => {
          failedQueue.push({ resolve, reject })
        }).then((newToken) => {
          originalRequest.headers.Authorization = `Bearer ${newToken}`
          return api(originalRequest)
        })
      }

      originalRequest._retry = true
      isRefreshing = true

      try {
        const authStore = useAuthStore()
        const authData = await authStore.refreshTokens()
        if (!authData) {
          throw error
        }

        processQueue(null, authData.accessToken)
        originalRequest.headers.Authorization = `Bearer ${authData.accessToken}`
        return api(originalRequest)
      } catch (refreshError) {
        processQueue(refreshError, null)
        const authStore = useAuthStore()
        if (!authStore.token || isAuthInvalidError(refreshError)) {
          authStore.clearAuth()
          const currentRoute = router.currentRoute.value
          if (currentRoute.name !== 'login') {
            await router.push({
              name: 'login',
              query: currentRoute.fullPath !== '/' ? { redirect: currentRoute.fullPath } : undefined,
            })
          }
        }
        return Promise.reject(refreshError)
      } finally {
        isRefreshing = false
      }
    }

    return Promise.reject(error)
  },
)
