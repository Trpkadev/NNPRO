import {
  decodeJwtUser,
  isAuthInvalidError,
  isTokenValid,
} from '@/services/api'
import { authService } from '@/services/auth.service'
import {
  UserRoles,
  type AuthResponseDto,
  type LoginRequestDto,
  type RegisterRequestDto,
  type UserRole,
} from '@/types/api'
import type { AuthenticatedUser } from '@/types/auth'

export const HAS_SESSION_KEY = 'has_session'

let refreshPromise: Promise<AuthResponseDto | null> | null = null

export const useAuthStore = defineStore('auth', () => {
  // Access token držíme výhradně v paměti (ne v localStorage)
  const token = ref<string | null>(null)
  const expiresAtUtc = ref<string | null>(null)
  const isInitialized = ref(false)

  const user = computed<AuthenticatedUser | null>(() => {
    if (!token.value) return null
    return decodeJwtUser(token.value)
  })

  const isAuthenticated = computed(
    () => !!token.value && isTokenValid(token.value) && user.value !== null,
  )

  const userRole = computed<UserRole | null>(() => user.value?.role ?? null)

  const isAdmin = computed(() => userRole.value === UserRoles.Admin)
  const isInstructor = computed(() => userRole.value === UserRoles.Instructor)
  const isStudent = computed(() => userRole.value === UserRoles.Student)

  function hasRole(role: UserRole): boolean {
    return userRole.value === role
  }

  function hasAnyRole(roles: readonly UserRole[]): boolean {
    if (roles.length === 0) return true
    return userRole.value !== null && roles.includes(userRole.value)
  }

  function setToken(newToken: string | null) {
    token.value = newToken
  }

  function setAuth(authData: AuthResponseDto) {
    token.value = authData.accessToken
    expiresAtUtc.value = authData.expiresAtUtc
    isInitialized.value = true
    localStorage.setItem(HAS_SESSION_KEY, '1')
  }

  function clearAuth() {
    token.value = null
    expiresAtUtc.value = null
    localStorage.removeItem(HAS_SESSION_KEY)
  }

  async function login(payload: LoginRequestDto): Promise<AuthResponseDto> {
    const authData = await authService.login(payload)
    setAuth(authData)
    return authData
  }

  async function register(payload: RegisterRequestDto): Promise<AuthResponseDto> {
    const authData = await authService.register(payload)
    setAuth(authData)
    return authData
  }

  async function refreshTokens(): Promise<AuthResponseDto | null> {
    if (refreshPromise) return refreshPromise

    refreshPromise = (async () => {
      try {
        const authData = await authService.refresh()
        setAuth(authData)
        return authData
      } catch (error) {
        if (isAuthInvalidError(error)) {
          clearAuth()
        }
        return null
      } finally {
        isInitialized.value = true
        refreshPromise = null
      }
    })()

    return refreshPromise
  }

  /**
   * Po obnovení stránky (F5) se jednou pokusí obnovit access token pomocí HttpOnly refreshToken cookie,
   * pokud má uživatel příznak aktivní relace v localStorage.
   */
  async function initializeAuth(): Promise<void> {
    if (isInitialized.value) return
    const hasSessionFlag = localStorage.getItem(HAS_SESSION_KEY) === '1'
    if (!isAuthenticated.value && hasSessionFlag) {
      await refreshTokens()
    } else {
      isInitialized.value = true
    }
  }

  async function logout(): Promise<void> {
    try {
      await authService.logout()
    } finally {
      clearAuth()
    }
  }

  return {
    token,
    expiresAtUtc,
    isInitialized,
    user,
    userRole,
    isAuthenticated,
    isAdmin,
    isInstructor,
    isStudent,
    hasRole,
    hasAnyRole,
    setToken,
    setAuth,
    clearAuth,
    login,
    register,
    refreshTokens,
    initializeAuth,
    logout,
  }
})
