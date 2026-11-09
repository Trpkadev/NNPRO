import {
  ACCESS_TOKEN_KEY,
  REFRESH_TOKEN_KEY,
  TOKEN_EXPIRES_KEY,
  decodeJwtUser,
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

export const useAuthStore = defineStore('auth', () => {
  const token = useStorage<string | null>(ACCESS_TOKEN_KEY, null)
  const refreshToken = useStorage<string | null>(REFRESH_TOKEN_KEY, null)
  const expiresAtUtc = useStorage<string | null>(TOKEN_EXPIRES_KEY, null)

  const isAuthenticated = computed(() => !!token.value && isTokenValid(token.value))

  const user = computed<AuthenticatedUser | null>(() => {
    if (!token.value) return null
    return decodeJwtUser(token.value)
  })

  const userRole = computed<UserRole | null>(() => user.value?.role ?? null)
  const userRoles = computed<UserRole[]>(() => user.value?.roles ?? [])

  const isAdmin = computed(() => userRoles.value.includes(UserRoles.Admin))
  const isInstructor = computed(() => userRoles.value.includes(UserRoles.Instructor))
  const isStudent = computed(() => userRoles.value.includes(UserRoles.Student))

  function hasRole(role: UserRole): boolean {
    return userRoles.value.includes(role)
  }

  function hasAnyRole(roles: readonly UserRole[]): boolean {
    if (roles.length === 0) return true
    return roles.some((role) => userRoles.value.includes(role))
  }

  function setToken(newToken: string | null) {
    token.value = newToken
  }

  function setAuth(authData: AuthResponseDto) {
    token.value = authData.accessToken
    refreshToken.value = authData.refreshToken
    expiresAtUtc.value = authData.expiresAtUtc
  }

  function clearAuth() {
    token.value = null
    refreshToken.value = null
    expiresAtUtc.value = null
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
    if (!token.value || !refreshToken.value) {
      clearAuth()
      return null
    }
    try {
      const authData = await authService.refresh({
        accessToken: token.value,
        refreshToken: refreshToken.value,
      })
      setAuth(authData)
      return authData
    } catch {
      clearAuth()
      return null
    }
  }

  async function logout(): Promise<void> {
    const currentRefreshToken = refreshToken.value
    clearAuth()
    if (currentRefreshToken) {
      try {
        await authService.logout({ refreshToken: currentRefreshToken })
      } catch {
        // Ignorujeme chybu při odhlašování na serveru, lokální session je již smazána
      }
    }
  }

  return {
    token,
    refreshToken,
    expiresAtUtc,
    user,
    userRole,
    userRoles,
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
    logout,
  }
})
